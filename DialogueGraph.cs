using DialogueEditor.Data;
using DialogueEditor.Nodes;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System;
using System.IO;
using Array = Godot.Collections.Array;
using FileAccess = Godot.FileAccess;

namespace DialogueEditor;

public partial class DialogueGraph : Control, IDialogueGraph {

	[NodePath("GraphEdit")]
	public GraphEdit Graph;

	[NodePath("VBoxContainer/AddStartNode")]
	private Button _addStartNode;

	[NodePath("VBoxContainer/AddDialogueNode")]
	private Button _addDialogueNode;

	[NodePath("VBoxContainer/AddCallNode")]
	private Button _addCallNode;

	[NodePath("VBoxContainer/AddSetVarNode")]
	private Button _addSetVarNode;

	[NodePath("VBoxContainer")]
	private VBoxContainer _popMenu;

	// [NodePath("Panel"))]
	// private MenuPanel _panel;

	[NodePath("HBoxContainer/LoadDialogue")]
	private Button _loadDialogue;

	[NodePath("HBoxContainer/SaveDialogue")]
	private Button _saveDialogue;

	[NodePath("HBoxContainer/SetRoot")]
	private Button _setRoot;

	[NodePath("OpenFileDialog")]
	private FileDialog _openFileDialog;

	[NodePath("SaveFileDialog")]
	private FileDialog _saveFileDialog;

	[NodePath("AcceptDialog")]
	private AcceptDialog _acceptDialog;

	[NodePath("SetRootFileDialog")]
	private FileDialog _setRootFileDialog;

	private ulong _nodeId;

	private static readonly System.Collections.Generic.Dictionary<ENodeType, string> NodeType2Url = new() {
		{ ENodeType.CallNode, "res://Nodes/call_node.tscn" },
		{ ENodeType.DialogueNode, "res://Nodes/dialogue_node.tscn" },
		{ ENodeType.SetVarNode, "res://Nodes/set_var_node.tscn" },
		{ ENodeType.StartNode, "res://Nodes/start_node.tscn" },
	};

	public override void _Ready() {
		this.OnReady();

		GlobalData.I.SetGraph(this);

		_addStartNode.Pressed += () => AddGraphNode<StartNode>(ENodeType.StartNode);
		_addDialogueNode.Pressed += () => AddGraphNode<DialogueNode>(ENodeType.DialogueNode);
		_addCallNode.Pressed += () => AddGraphNode<CallNode>(ENodeType.CallNode);
		_addSetVarNode.Pressed += () => AddGraphNode<SetVarNode>(ENodeType.SetVarNode);
		_saveDialogue.Pressed += OnSaveDialogue;
		_loadDialogue.Pressed += OnLoadDialogue;
		_setRoot.Pressed += OnSetRoot;

		Graph.ConnectionRequest += OnConnectionRequest;
		Graph.DisconnectionRequest += OnDisconnectionRequest;
		Graph.DeleteNodesRequest += OnDeleteNodesRequest;
		Graph.PopupRequest += OnPopupRequest;
		Graph.ConnectionToEmpty += OnConnectionToEmpty;
		Graph.GuiInput += @event => {
			if (@event is InputEventMouseButton mb) {
				if (mb.ButtonIndex == MouseButton.Left && mb.Pressed) {
					HideAll();
				}
			}
		};
		Graph.ChildEnteredTree += OnChildEnteredTree;
		Graph.ChildExitingTree += OnChildExitingTree;
		_openFileDialog.FileSelected += OnOpenFileSelected;
		_saveFileDialog.FileSelected += OnSaveFileSelected;
		_setRootFileDialog.DirSelected += OnSetRootDirSelected;

		if (!GlobalData.I.IsDialogueRootInvalid(GlobalData.I.GlobalSettingMo.DialogueRoot)) {
			_setRootFileDialog.Show();
		}
	}

	private void OnOpenFileSelected(string path) {
		if (FileAccess.FileExists(path)) {
			SetCurrentDirFromPath(path);
			GlobalData.I.LoadDialogue(path);
		}
	}

	private void OnSaveFileSelected(string path) {
		if (FileAccess.FileExists(path)) {
			foreach (var pair in GlobalData.I.GraphNodes) {
				var result = pair.Value.CheckCanSerialize();
				if (result.IsFailed) {
					ShowMessage(result.Reason);
					return;
				}
			}
			SetCurrentDirFromPath(path);
			GlobalData.I.SaveDialogue(path);
		}
	}

	private void OnSetRootDirSelected(string path) {
		if (GlobalData.I.IsDialogueRootInvalid(path)) {
			SetCurrentDirFromPath(path);
			GlobalData.I.SetDialogueRoot(path);
		} else {
			_setRootFileDialog.Show();
			ShowMessage("Can not find Npcs.json in this directory. Invalid Dialogue root.");
		}
	}

	private void SetCurrentDirFromPath(string path) {
		DirectoryInfo dictionaryInfo = null;
		if (Path.HasExtension(path)) {
			var fileInfo = new FileInfo(path);
			dictionaryInfo = fileInfo.Directory;
		}
		if (dictionaryInfo != null) {
			GlobalData.I.SetCurrentDir(dictionaryInfo.FullName);
		}
	}

	private void OnChildExitingTree(Node node) {
		if (node is SerializeGraphNode graphNode) {
			GlobalData.I.RemoveGraphNode(graphNode);
		}
	}

	private void OnChildEnteredTree(Node node) {
		if (node is SerializeGraphNode graphNode) {
			GlobalData.I.AddGraphNode(graphNode);
		}
	}

	private void OnLoadDialogue() {
		_openFileDialog.CurrentDir = GlobalData.I.GlobalSettingMo.CurrentDir;
		_openFileDialog.Show();
	}

	private void OnSetRoot() {
		_setRootFileDialog.CurrentDir = GlobalData.I.GlobalSettingMo.CurrentDir;
		_setRootFileDialog.Show();
	}

	private void OnSaveDialogue() {
		_saveFileDialog.CurrentDir = GlobalData.I.GlobalSettingMo.CurrentDir;
		_saveFileDialog.Show();
	}

	private void OnConnectionToEmpty(StringName fromNode, long fromPort, Vector2 releasePosition) {
		GlobalData.I.RemoveAllOutput(fromNode, fromPort);
	}

	private void OnResizeRequest<T>(Vector2 minSize, T node) where T : SerializeGraphNode {
		if (Graph.UseSnap) {
			minSize = minSize.Snapped(Vector2.One * Graph.SnapDistance);
		}

		node.Size = minSize;
	}

	private void OnCloseRequest(SerializeGraphNode node) {
		GlobalData.I.RemoveAllOutput(node.Name);
		node.QueueFree();
	}

	private void OnPopupRequest(Vector2 position) {
		_popMenu.Visible = true;
		_popMenu.Position = position;
	}

	private void OnDeleteNodesRequest(Array nodes) {
		foreach (var value in nodes) {
			var node = GetNode<Node>(value.AsString());
			node?.QueueFree();
		}
	}

	private void OnDisconnectionRequest(StringName fromNode, long fromPort, StringName toNode, long toPort) {
		TryDisconnectNode(fromNode, fromPort, toNode, toPort);
	}

	private void OnConnectionRequest(StringName fromNode, long fromPort, StringName toNode, long toPort) {
		if (fromNode == toNode) {
			return;
		}

		GlobalData.I.RemoveAllOutput(fromNode, fromPort);

		TryConnectNode(fromNode, fromPort, toNode, toPort);
	}

	private void TryConnectNode(StringName fromNode, long fromPort, StringName toNode, long toPort) {
		var error = Graph.ConnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
		if (error == Error.Ok) {
			GlobalData.I.OnConnectNode(fromNode, fromPort, toNode, toPort);
		} else {
			GD.PrintErr($"Error connecting node: {error}");
		}
	}

	private T AddGraphNode<T>(ENodeType nodeType) where T : SerializeGraphNode {
		var nodeCls = GD.Load<PackedScene>(NodeType2Url[nodeType]);
		var node = nodeCls.Instantiate<T>();
		node.Name = typeof(T).Name + "_" + (_nodeId++);
		node.CloseRequest += () => OnCloseRequest(node);
		node.ResizeRequest += minSize => OnResizeRequest(minSize, node);
		Graph.AddChild(node);
		node.PositionOffset = (_popMenu.Position + Graph.ScrollOffset) / Graph.Zoom;
		_popMenu.Visible = false;
		return node;
	}

	private void HideAll() {
		_popMenu.Visible = false;
		// _panel.Visible = false;
		// _panel.HideAndInvalidateData();
	}

	private void ShowMessage(string message) {
		_acceptDialog.DialogText = message;
		_acceptDialog.Show();
	}

	/// <inheritdoc />
	public void DeserializeData(DialogueGraphSerializeMo graphMo) {
		foreach (var pair in GlobalData.I.GraphNodes) {
			RemoveChild(pair.Value);
			pair.Value.QueueFree();
		}
		System.Collections.Generic.Dictionary<string, string> oldName2NewName = new();
		foreach (var graphNodeMo in graphMo.NodeMos) {
			Enum.TryParse<ENodeType>(graphNodeMo.NodeType, out var nodeType);
			SerializeGraphNode node = nodeType switch {
				ENodeType.CallNode => AddGraphNode<CallNode>(ENodeType.CallNode),
				ENodeType.DialogueNode => AddGraphNode<DialogueNode>(ENodeType.DialogueNode),
				ENodeType.SetVarNode => AddGraphNode<SetVarNode>(ENodeType.SetVarNode),
				ENodeType.StartNode => AddGraphNode<StartNode>(ENodeType.StartNode),
				_ => throw new ArgumentOutOfRangeException()
			};

			node.FromJson(graphNodeMo);
			oldName2NewName.Add(graphNodeMo.Name, node.Name);
		}

		foreach (var connectionMo in graphMo.ConnectionMos) {
			if (oldName2NewName.TryGetValue(connectionMo.FromNodeName, out var fromNodeName) && oldName2NewName.TryGetValue(connectionMo.ToNodeName, out var toNodeName)) {
				TryConnectNode(fromNodeName, connectionMo.FromPort.ToInt(), toNodeName, connectionMo.ToNodePort.ToInt());
			}
		}
	}

	/// <inheritdoc />
	public Array<Dictionary> GetConnectionList() {
		return Graph.GetConnectionList();
	}

	/// <inheritdoc />
	public void TryDisconnectNode(StringName fromNode, long fromPort, StringName toNode, long toPort) {
		Graph.DisconnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
		GlobalData.I.OnDisconnectNode(fromNode, fromPort, toNode, toPort);
	}
}