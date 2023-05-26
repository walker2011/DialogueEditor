using DialogueEditor.Nodes;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

namespace DialogueEditor.Data;

public partial class GlobalData : Node {
	private static readonly string SettingPath = "./dialogue_settings.json";

	public static GlobalData I { get; private set; }

	[Signal]
	public delegate void OneSettingDataChangedEventHandler(SettingMo mo);

	[Signal]
	public delegate void AllSettingDataChangedEventHandler();

	public readonly System.Collections.Generic.Dictionary<string, SerializeGraphNode> GraphNodes = new();

	/// <summary>
	/// fromNodeName->from_port->toNodeName->to_port->boolean
	/// </summary>
	public readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<long, System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<long, bool>>>> GraphConnections =
		new();

	public DialogueGraph Graph;

	private readonly System.Collections.Generic.Dictionary<ESettingType, SettingMo> _settingMap;

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
				_settingMap = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<ESettingType, SettingMo>>(settings);
			} else {
				_settingMap = new System.Collections.Generic.Dictionary<ESettingType, SettingMo>();
			}

			EmitSignal(SignalName.AllSettingDataChanged);
		}
	}

	public void SetGraph(DialogueGraph dialogueGraph) {
		Graph = dialogueGraph;
	}

	public void RemoveAllOutput(string fromName) {
		var connectionList = Graph.GetConnectionList();
		foreach (var connectionDict in connectionList) {
			if (connectionDict["from"].AsString().Equals(fromName)) {
				Graph.DisconnectNode(connectionDict["from"].AsString(), connectionDict["from_port"].AsInt32(),
					connectionDict["to"].AsString(), connectionDict["to_port"].AsInt32());
			}
		}
	}

	public void RemoveAllOutput(string fromName, long fromPort) {
		var connectList = Graph.GetConnectionList();
		foreach (var connectionDict in connectList) {
			if (connectionDict["from"].AsString().Equals(fromName) &&
			    connectionDict["from_port"].AsInt64() == fromPort) {
				Graph.DisconnectNode(connectionDict["from"].AsString(), connectionDict["from_port"].AsInt32(),
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
		if (FileAccess.FileExists(path)) {
			var fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.Write);
			var graphNodes = new List<System.Collections.Generic.Dictionary<string, string>>();
			foreach (var pair in GraphNodes) {
				var nodeData = new System.Collections.Generic.Dictionary<string, string>();
				pair.Value.ToJson(nodeData);
				nodeData["Name"] = pair.Value.Name;
				graphNodes.Add(nodeData);
			}

			var data = new System.Collections.Generic.Dictionary<string, object> {
				{ "GraphNodes", graphNodes },
				{ "GraphConnections", Graph.GetConnectionList() },
			};
			fileAccess.StoreString(JsonConvert.SerializeObject(data));
			fileAccess.Flush();
			fileAccess.Close();
		}
	}

	public void ReadDialogue(string path) {
		if (FileAccess.FileExists(path)) {
			var fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			var jsonText = fileAccess.GetAsText();
			var data = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(jsonText);
			fileAccess.Close();

			foreach (var pair in GraphNodes) {
				Graph.RemoveChild(pair.Value);
				pair.Value.QueueFree();
			}

			var graphNodes = data["GraphNodes"] as List<System.Collections.Generic.Dictionary<string, string>>;
			var graphConnections =
				data["GraphConnections"] as
					Array<Dictionary>;
			

			Graph.DeserializeData(graphNodes, graphConnections);
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
		var dict2 = dict1.GetValueOrDefault(fromPort, new());
		var dict3 = dict2.GetValueOrDefault(toNode, new());
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
}