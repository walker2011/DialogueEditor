using DialogueEditor.Components;
using DialogueEditor.Data;
using DialogueEditor.Nodes;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System;
using System.Collections.Generic;
using Array = Godot.Collections.Array;

namespace DialogueEditor;

public partial class DialogueGraph : GraphEdit {

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

	[NodePath("Panel")]
	private MenuPanel _panel;

	[NodePath("HBoxContainer/AddSpeaker")]
	private Button _addSpeaker;

	[NodePath("HBoxContainer/AddFunc")]
	private Button _addFunction;

	[NodePath("HBoxContainer/AddVar")]
	private Button _addVar;
	
	[NodePath("HBoxContainer/LoadDialogue")]
	private Button _loadDialogue;
	
	[NodePath("HBoxContainer/SaveDialogue")]
	private Button _saveDialogue;

	private ulong _nodeId;

	private static System.Collections.Generic.Dictionary<ENodeType, string> NodeType2Url = new() {
		{ ENodeType.CallNode, "res://Nodes/call_node.tscn" },
		{ ENodeType.DialogueNode, "res://Nodes/dialogue_node.tscn" },
		{ ENodeType.SetVarNode, "res://Nodes/start_node.tscn" },
		{ ENodeType.StartNode, "res://Nodes/set_var_node.tscn" },
	};

	public override void _Ready() {
		this.OnReady();

		GlobalData.I.SetGraph(this);

		_addStartNode.Pressed += () => AddGraphNode<StartNode>(ENodeType.StartNode);
		_addDialogueNode.Pressed += () => AddGraphNode<DialogueNode>(ENodeType.DialogueNode);
		_addCallNode.Pressed += () => AddGraphNode<CallNode>(ENodeType.CallNode);
		_addSetVarNode.Pressed += () => AddGraphNode<SetVarNode>(ENodeType.SetVarNode);
		_addSpeaker.Pressed += OnAddSpeaker;
		_addFunction.Pressed += OnAddFunction;
		_addVar.Pressed += OnAddVariable;
		_saveDialogue.Pressed += OnSaveDialogue;
		_loadDialogue.Pressed += OnLoadDialogue;

		ConnectionRequest += OnConnectionRequest;
		DisconnectionRequest += OnDisconnectionRequest;
		DeleteNodesRequest += OnDeleteNodesRequest;
		PopupRequest += OnPopupRequest;
		ConnectionToEmpty += OnConnectionToEmpty;
		ChildEnteredTree += OnChildEnteredTree;
		ChildExitingTree += OnChildExitingTree;
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
		var fileDialog = new FileDialog();
		fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		fileDialog.AddFilter("*.json");
		fileDialog.FileSelected += path => {
			GlobalData.I.ReadDialogue(path);
		};
		fileDialog.Show();
	}

	private void OnSaveDialogue() {
		var fileDialog = new FileDialog();
		fileDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
		fileDialog.AddFilter("*.json");
		fileDialog.FileSelected += path => {
			GlobalData.I.SaveDialogue(path);
		};
		fileDialog.Show();
	}

	private void OnAddSpeaker() {
		_panel.SetData(GlobalData.I.GetSetting(ESettingType.Speaker));
		_panel.GlobalPosition = _addSpeaker.GlobalPosition - new Vector2(0, _panel.Size.Y);
	}

	private void OnAddFunction() {
		_panel.SetData(GlobalData.I.GetSetting(ESettingType.Function));
		_panel.GlobalPosition = _addFunction.GlobalPosition - new Vector2(0, _panel.Size.Y);
	}

	private void OnAddVariable() {
		_panel.SetData(GlobalData.I.GetSetting(ESettingType.Variable));
		_panel.GlobalPosition = _addVar.GlobalPosition - new Vector2(0, _panel.Size.Y);
	}

	private void OnConnectionToEmpty(StringName fromNode, long fromPort, Vector2 releasePosition) {
		GlobalData.I.RemoveAllOutput(fromNode, fromPort);
	}

	private void OnResizeRequest<T>(Vector2 minSize, T node) where T : SerializeGraphNode {
		if (UseSnap) {
			minSize = minSize.Snapped(Vector2.One * SnapDistance);
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

	public void TryConnectNode(StringName fromNode, long fromPort, StringName toNode, long toPort) {
		var error = ConnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
		if (error == Error.Ok) {
			GlobalData.I.OnConnectNode(fromNode, fromPort, toNode, toPort);
		} else {
			GD.PrintErr($"Error connecting node: {error}");
		}
	}

	public void TryDisconnectNode(StringName fromNode, long fromPort, StringName toNode, long toPort) {
		DisconnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
		GlobalData.I.OnDisconnectNode(fromNode, fromPort, toNode, toPort);
	}

	private T AddGraphNode<T>(ENodeType nodeType) where T : SerializeGraphNode {
		var nodeCls = GD.Load<PackedScene>(NodeType2Url[nodeType]);
		var node = nodeCls.Instantiate<T>();
		node.Name = typeof(T).Name + "_" + (_nodeId++);
		node.CloseRequest += () => OnCloseRequest(node);
		node.ResizeRequest += minSize => OnResizeRequest(minSize, node);
		AddChild(node);
		node.PositionOffset = _popMenu.Position;
		_popMenu.Visible = false;
		return node;
	}

	public override void _GuiInput(InputEvent @event) {
		if (@event is InputEventMouseButton mb) {
			if (mb.ButtonIndex == MouseButton.Left && mb.Pressed) {
				HideAll();
			}
		}
	}

	private void HideAll() {
		_popMenu.Visible = false;
		_panel.HideAndInvalidateData();
	}

	public void DeserializeData(List<System.Collections.Generic.Dictionary<string, string>> graphNodes, Array<Dictionary> graphConnections) {
		System.Collections.Generic.Dictionary<string, string> oldName2NewName = new();
		foreach (var graphNode in graphNodes) {
			SerializeGraphNode node;
			switch ((ENodeType)graphNode["NodeType"].ToInt()) {
				case ENodeType.CallNode:
					node = AddGraphNode<CallNode>(ENodeType.CallNode);
					break;
				case ENodeType.DialogueNode:
					node = AddGraphNode<DialogueNode>(ENodeType.DialogueNode);
					break;
				case ENodeType.SetVarNode:
					node = AddGraphNode<SetVarNode>(ENodeType.SetVarNode);
					break;
				case ENodeType.StartNode:
					node = AddGraphNode<StartNode>(ENodeType.StartNode);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			node.FromJson(graphNode);
			oldName2NewName.Add(graphNode["Name"], node.Name);
		}

		foreach (var connection in graphConnections) {
			if (oldName2NewName.TryGetValue(connection["from"].AsString(), out var fromNodeName) &&
			    oldName2NewName.TryGetValue(connection["to"].AsString(), out var toNodeName)) {
				TryConnectNode(fromNodeName, connection["from_port"].AsInt64(), toNodeName, connection["to_port"].AsInt64());
			}
		}
	}
}