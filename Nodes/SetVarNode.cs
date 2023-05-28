using DialogueEditor.Data;
using DialogueEditor.Data.NodeMo;
using Godot;
using Godot.Sharp.Extras;

namespace DialogueEditor.Nodes;

public partial class SetVarNode : SerializeGraphNode {
	[NodePath("VarName")] private OptionButton _varName;

	[NodePath("VarValue")] private LineEdit _varValue;

	public override void _Ready() {
		this.OnReady();
		GlobalData.I.OneSettingDataChanged += OnOneSettingDataChanged;
		GlobalData.I.AllSettingDataChanged += OnAllSettingDataChanged;
		OnAllSettingDataChanged();
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
			return new CheckSerializeResult {IsFailed = true, Reason = "Var Name Can't Be Empty."};
		}

		if (string.IsNullOrEmpty(_varValue.Text)) {
			return new CheckSerializeResult {IsFailed = true, Reason = "Var Value Can't Be Empty."};
		}

		return new CheckSerializeResult {IsFailed = false};
	}

	public override void ToJson(SerializeNodeMo mo) {
		base.ToJson(mo);
		mo.NodeType = ENodeType.SetVarNode.ToString();
		mo.VarName = _varName.Text;
		mo.VarValue = _varValue.Text;
		var targetNode = GlobalData.I.GetLinkTo(Name);
		if (targetNode != null) {
			mo.LinkNext = targetNode.Uuid.ToString();
		}
	}

	public override void FromJson(SerializeNodeMo mo) {
		base.FromJson(mo);
		_varName.Text = mo.VarName;
		_varValue.Text = mo.VarValue;
	}
}
