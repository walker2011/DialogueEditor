using Godot;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DialogueEditor.Data;

public class DialogueConfigMos<TKey, TValue> where TValue : IDialogueConfig<TKey>, new() {

	public readonly List<TValue> MoArray = new();
	public readonly Dictionary<TKey, TValue> MoDict = new();
	private readonly string _path;

	public DialogueConfigMos(string path) {
		_path = path;
	}

	public void Reload() {
		MoArray.Clear();
		if (FileAccess.FileExists(_path)) {
			var content = FileAccess.GetFileAsString(_path);
			var json = JObject.Parse(content);
			foreach (var data in json["values"]) {
				var mo = new TValue();
				mo.Parse(data as JObject);
				MoArray.Add(mo);
				MoDict.Add(mo.Key, mo);
			}
		} else {
			GD.PrintErr("NpcMos.Reload path is not exists. path=" + _path);
		}
	}

}