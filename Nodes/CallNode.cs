using DialogueEditor.Data;
using DialogueEditor.Data.NodeMo;
using Godot;
using Godot.Sharp.Extras;

namespace DialogueEditor.Nodes;

public partial class CallNode : SerializeGraphNode {
	[NodePath("FuncName")] private OptionButton _funcName;

	public override void _Ready() {
		this.OnReady();
		GlobalData.I.OneSettingDataChanged += OnOneSettingDataChanged;
		GlobalData.I.AllSettingDataChanged += OnAllSettingDataChanged;
		OnAllSettingDataChanged();
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
			return new CheckSerializeResult {IsFailed = true, Reason = "Function Name Can't Be Empty."};
		}

		return new CheckSerializeResult {IsFailed = false};
	}

	public override void ToJson(SerializeNodeMo mo) {
		base.ToJson(mo);

		mo.NodeType = ENodeType.CallNode.ToString();
		mo.FuncName = _funcName.Text;
		var targetNode = GlobalData.I.GetLinkTo(Name);
		if (targetNode != null) {
			mo.LinkNext = targetNode.Uuid.ToString();
		}
	}

	public override void FromJson(SerializeNodeMo mo) {
		base.FromJson(mo);
		_funcName.Text = mo.FuncName;
	}
}
