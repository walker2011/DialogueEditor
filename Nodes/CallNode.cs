using DialogueEditor.Data;
using Godot;
using Godot.Sharp.Extras;
using System.Collections.Generic;

namespace DialogueEditor.Nodes;

public partial class CallNode : SerializeGraphNode {

	[NodePath("FuncName")]
	private OptionButton _funcName;

	public override void _Ready() {
		this.OnReady();
		GlobalData.I.OneSettingDataChanged += OnOneSettingDataChanged;
		GlobalData.I.AllSettingDataChanged += OnAllSettingDataChanged;
	}

	private void OnAllSettingDataChanged() {
		var mo = GlobalData.I.GetSetting(ESettingType.Function);
		OnOneSettingDataChanged(mo);
	}

	private void OnOneSettingDataChanged(SettingMo mo) {
		if (mo.SettingType.Equals(ESettingType.Function)) {
			if (string.IsNullOrEmpty(_funcName.Text)) {
				if (mo.Data.IndexOf(_funcName.Text) == -1) {
					_funcName.Text = "";
				}
			}

			_funcName.Clear();
			foreach (var content in mo.Data) {
				_funcName.AddItem(content);
			}
		}
	}

	public override CheckSerializeResult CheckCanSerialize() {
		if (string.IsNullOrEmpty(_funcName.Text)) {
			return new CheckSerializeResult { IsFailed = true, Reason = "Function Name Can't Be Empty." };
		}

		return new CheckSerializeResult { IsFailed = false };
	}

	public override void ToJson(Dictionary<string, string> json) {
		base.ToJson(json);
		json["nodeType"] = ENodeType.CallNode.ToString();
		json["FuncName"] = _funcName.Text;
	}

	public override void FromJson(Dictionary<string, string> json) {
		base.FromJson(json);
		_funcName.Text = json["FuncName"];
	}
}