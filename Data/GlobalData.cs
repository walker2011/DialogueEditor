using System.Collections.Generic;
using System.Data;
using System.Linq;
using DialogueEditor.Data.ConnectionMo;
using DialogueEditor.Data.NodeMo;
using DialogueEditor.Nodes;
using Godot;
using Newtonsoft.Json;

namespace DialogueEditor.Data;

public partial class GlobalData : Node {
	private static readonly string SettingPath = "./dialogue_settings.json";

	public static GlobalData I { get; private set; }

	[Signal]
	public delegate void OneSettingDataChangedEventHandler(SettingMo mo);

	[Signal]
	public delegate void AllSettingDataChangedEventHandler();

	public readonly Dictionary<string, SerializeGraphNode> GraphNodes = new();

	/// <summary>
	/// fromNodeName->from_port->toNodeName->to_port->boolean
	/// </summary>
	public readonly Dictionary<string, Dictionary<long, Dictionary<string, Dictionary<long, bool>>>> GraphConnections =
		new();

	public DialogueGraph Dialogue;

	private readonly Dictionary<ESettingType, SettingMo> _settingMap;

	public GlobalData() {
		if (I != null) {
			throw new SyntaxErrorException("duplicated GlobalData instance");
		}

		I = this;
		if (!Engine.IsEditorHint()) {
			if (FileAccess.FileExists(SettingPath)) {
				var fileAccess = FileAccess.Open(SettingPath, FileAccess.ModeFlags.Read);
				var settings = fileAccess.GetAsText();
				fileAccess.Close();
				_settingMap = JsonConvert.DeserializeObject<Dictionary<ESettingType, SettingMo>>(settings);
			} else {
				_settingMap = new Dictionary<ESettingType, SettingMo>();
			}

			EmitSignal(SignalName.AllSettingDataChanged);
		}
	}

	public void SetGraph(DialogueGraph dialogueGraph) {
		Dialogue = dialogueGraph;
	}

	public void RemoveAllOutput(string fromName) {
		var connectionList = Dialogue.Graph.GetConnectionList();
		foreach (var connectionDict in connectionList) {
			if (connectionDict["from"].AsString().Equals(fromName)) {
				Dialogue.TryDisconnectNode(connectionDict["from"].AsString(), connectionDict["from_port"].AsInt32(),
					connectionDict["to"].AsString(), connectionDict["to_port"].AsInt32());
			}
		}
	}

	public void RemoveAllOutput(string fromName, long fromPort) {
		var connectList = Dialogue.Graph.GetConnectionList();
		foreach (var connectionDict in connectList) {
			if (connectionDict["from"].AsString().Equals(fromName) &&
			    connectionDict["from_port"].AsInt64() == fromPort) {
				Dialogue.TryDisconnectNode(connectionDict["from"].AsString(), connectionDict["from_port"].AsInt32(),
					connectionDict["to"].AsString(), connectionDict["to_port"].AsInt32());
			}
		}
	}

	public void InvalidateSetting(SettingMo mo) {
		var setting = JsonConvert.SerializeObject(_settingMap);
		var fileAccess = FileAccess.Open(SettingPath, FileAccess.ModeFlags.Write);
		fileAccess.StoreString(setting);
		fileAccess.Flush();
		fileAccess.Close();
		EmitSignal(SignalName.OneSettingDataChanged, mo);
	}

	public SettingMo GetSetting(ESettingType settingType) {
		if (!_settingMap.ContainsKey(settingType)) {
			_settingMap.Add(settingType, new SettingMo { SettingType = settingType, Data = new List<string>() });
		}

		return _settingMap[settingType];
	}

	public void SaveDialogue(string path) {
		var fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.WriteRead);
		var graphMo = new DialogueGraphSerializeMo {
			ConnectionMos = new List<ConnectionSerializeMo>(),
			NodeMos = new List<SerializeNodeMo>()
		};
		foreach (var pair in GraphNodes) {
			var nodeMo =new SerializeNodeMo();
			pair.Value.ToJson(nodeMo);
			nodeMo.Name = pair.Value.Name;
			graphMo.NodeMos.Add(nodeMo);
		}

		var connectionList = Dialogue.Graph.GetConnectionList();
		foreach (var connectionDict in connectionList) {
			var connectionMo = new ConnectionSerializeMo() {
				FromNodeName = connectionDict["from"].AsString(),
				FromPort = connectionDict["from_port"].AsString(),
				ToNodeName = connectionDict["to"].AsString(),
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

			foreach (var pair in GraphNodes) {
				Dialogue.RemoveChild(pair.Value);
				pair.Value.QueueFree();
			}
			

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