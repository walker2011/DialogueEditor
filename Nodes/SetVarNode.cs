using DialogueEditor.Data;
using Godot;
using Godot.Sharp.Extras;
using System.Collections.Generic;

namespace DialogueEditor.Nodes;

public partial class SetVarNode : SerializeGraphNode {

	[NodePath("VarName")]
	private OptionButton _varName;

	[NodePath("VarValue")]
	private LineEdit _varValue;

	public override void _Ready() {
		this.OnReady();
		GlobalData.I.OneSettingDataChanged += OnOneSettingDataChanged;
		GlobalData.I.AllSettingDataChanged += OnAllSettingDataChanged;
	}

	private void OnAllSettingDataChanged() {
		var mo = GlobalData.I.GetSetting(ESettingType.Variable);
		OnOneSettingDataChanged(mo);
	}

	private void OnOneSettingDataChanged(SettingMo mo) {
		if (mo.SettingType.Equals(ESettingType.Variable)) {
			if (string.IsNullOrEmpty(_varName.Text)) {
				if (mo.Data.IndexOf(_varName.Text) == -1) {
					_varName.Text = "";
				}
			}

			_varName.Clear();
			foreach (var content in mo.Data) {
				_varName.AddItem(content);
			}
		}
	}

	public override CheckSerializeResult CheckCanSerialize() {
		if (string.IsNullOrEmpty(_varName.Text)) {
			return new CheckSerializeResult { IsFailed = true, Reason = "Var Name Can't Be Empty." };
		}

		if (string.IsNullOrEmpty(_varValue.Text)) {
			return new CheckSerializeResult { IsFailed = true, Reason = "Var Value Can't Be Empty." };
		}

		return new CheckSerializeResult { IsFailed = false };
	}

	public override void ToJson(Dictionary<string, string> json) {
		base.ToJson(json);
		json["nodeType"] = ENodeType.SetVarNode.ToString();
		json["VarName"] = _varName.Text;
		json["VarValue"] = _varValue.Text;
	}

	public override void FromJson(Dictionary<string, string> json) {
		base.FromJson(json);
		_varName.Text = json["VarName"];
		_varValue.Text = json["VarValue"];
	}
}
