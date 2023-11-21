using DialogueEditor.Data.ConnectionMo;
using DialogueEditor.Data.NodeMo;
using DialogueEditor.Nodes;
using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DialogueEditor.Data;

public partial class GlobalData : Node {
	public static GlobalData I { get; private set; }

	[Signal]
	public delegate void ReloadConfigsEventHandler();

	public readonly Dictionary<string, SerializeGraphNode> GraphNodes = new();

	/// <summary>
	/// fromNodeName->from_port->toNodeName->to_port->boolean
	/// </summary>
	public readonly Dictionary<string, Dictionary<long, Dictionary<string, Dictionary<long, bool>>>> GraphConnections = new();

	public IDialogueGraph Dialogue;

	public GlobalSettingMo GlobalSettingMo { get; private set; }
	public DialogueConfigMos<ulong, NpcMo> NpcConfig;
	public DialogueConfigMos<string, VariableMo> VariableConfig;
	public DialogueConfigMos<string, FunctionMo> FunctionConfig;

	public GlobalData() {
		if (I != null) {
			throw new SyntaxErrorException("duplicated GlobalData instance");
		}

		I = this;
		if (!Engine.IsEditorHint()) {
			GlobalSettingMo = FileAccess.FileExists(GlobalSettingMo.GlobalSettingPath)
				? ResourceLoader.Load<GlobalSettingMo>(GlobalSettingMo.GlobalSettingPath)
				: new GlobalSettingMo();

			TryReloadConfigs();
		}
	}

	public void SetCurrentDir(string path) {
		GlobalSettingMo.CurrentDir = path;
	}

	public void SetDialogueRoot(string path) {
		GlobalSettingMo.DialogueRoot = path;
		TryReloadConfigs();
	}

	public void TryReloadConfigs() {
		NpcConfig = new DialogueConfigMos<ulong, NpcMo>(GlobalSettingMo.DialogueRoot);
		VariableConfig = new DialogueConfigMos<string, VariableMo>(GlobalSettingMo.DialogueRoot);
		FunctionConfig = new DialogueConfigMos<string, FunctionMo>(GlobalSettingMo.DialogueRoot);
		EmitSignal(SignalName.ReloadConfigs);
	}
	
	public bool IsDialogueRootInvalid(string path) {
		return DirAccess.DirExistsAbsolute(path) && FileAccess.FileExists($"{path}/Npcs.json");
	}

	public void SetGraph(IDialogueGraph dialogueGraph) {
		Dialogue = dialogueGraph;
	}

	public void RemoveAllOutput(string fromName) {
		var connectionList = Dialogue.GetConnectionList();
		foreach (var connectionDict in connectionList) {
			if (connectionDict["from_node"].AsString().Equals(fromName)) {
				Dialogue.TryDisconnectNode(connectionDict["from_node"].AsString(), connectionDict["from_port"].AsInt32(), connectionDict["to_node"].AsString(),
					connectionDict["to_port"].AsInt32());
			}
		}
	}

	public void RemoveAllOutput(string fromName, long fromPort) {
		var connectList = Dialogue.GetConnectionList();
		foreach (var connectionDict in connectList) {
			if (connectionDict["from_node"].AsString().Equals(fromName) && connectionDict["from_port"].AsInt64() == fromPort) {
				Dialogue.TryDisconnectNode(connectionDict["from_node"].AsString(), connectionDict["from_port"].AsInt32(), connectionDict["to_node"].AsString(),
					connectionDict["to_port"].AsInt32());
			}
		}
	}

	public void SaveDialogue(string path) {
		var fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.WriteRead);
		var graphMo = new DialogueGraphSerializeMo {
			ConnectionMos = new List<ConnectionSerializeMo>(),
			NodeMos = new List<SerializeNodeMo>()
		};
		foreach (var pair in GraphNodes) {
			var nodeMo = new SerializeNodeMo();
			pair.Value.ToJson(nodeMo);
			nodeMo.Name = pair.Value.Name;
			graphMo.NodeMos.Add(nodeMo);
		}

		var connectionList = Dialogue.GetConnectionList();
		foreach (var connectionDict in connectionList) {
			var connectionMo = new ConnectionSerializeMo() {
				FromNodeName = connectionDict["from_node"].AsString(),
				FromPort = connectionDict["from_port"].AsString(),
				ToNodeName = connectionDict["to_node"].AsString(),
				ToNodePort = connectionDict["to_port"].AsString(),
			};
			graphMo.ConnectionMos.Add(connectionMo);
		}
		fileAccess.StoreString(JsonConvert.SerializeObject(graphMo));
		fileAccess.Flush();
		fileAccess.Close();
	}

	public void LoadDialogue(string path) {
		if (FileAccess.FileExists(path)) {
			var fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			var jsonText = fileAccess.GetAsText();
			var graphMo = JsonConvert.DeserializeObject<DialogueGraphSerializeMo>(jsonText);
			fileAccess.Close();
			
			Dialogue.DeserializeData(graphMo);
		}
	}

	public void AddGraphNode(SerializeGraphNode graphNode) {
		GraphNodes.Add(graphNode.Name, graphNode);
	}

	public void RemoveGraphNode(SerializeGraphNode graphNode) {
		GraphNodes.Remove(graphNode.Name);
	}

	public void OnConnectNode(StringName fromNode, long fromPort, StringName toNode, long toPort) {
		var dict1 = GraphConnections.GetValueOrDefault(fromNode, new());
		GraphConnections[fromNode] = dict1;
		var dict2 = dict1.GetValueOrDefault(fromPort, new());
		dict1[fromPort] = dict2;
		var dict3 = dict2.GetValueOrDefault(toNode, new());
		dict2[toNode] = dict3;
		dict3[toPort] = true;
	}

	public void OnDisconnectNode(StringName fromNode, long fromPort, StringName toNode, long toPort) {
		if (GraphConnections.TryGetValue(fromNode, out var dict1)) {
			if (dict1.TryGetValue(fromPort, out var dict2)) {
				if (dict2.TryGetValue(toNode, out var dict3)) {
					dict3.Remove(toPort);
				}
			}
		}
	}

	public SerializeGraphNode GetLinkTo(string fromNode) {
		if (GraphConnections.TryGetValue(fromNode, out var dict)) {
			var targetNodeName = dict.Values.First()?.Keys.First();
			if (!string.IsNullOrEmpty(targetNodeName)) {
				return GraphNodes.TryGetValue(targetNodeName, out var targetNode) ? targetNode : null;
			}
		}

		return null;
	}
	public List<KeyValuePair<long, Dictionary<string, Dictionary<long, bool>>>> GetLinkTos(string fromNode) {
		if (GraphConnections.TryGetValue(fromNode, out var dict)) {
			return dict.ToList();
		}

		return null;
	}

}