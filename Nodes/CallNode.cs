using DialogueEditor.Data;
using DialogueEditor.Data.NodeMo;
using Godot;
using Godot.Sharp.Extras;

namespace DialogueEditor.Nodes;

public partial class CallNode : SerializeGraphNode {
	[NodePath("FuncName")]
	private OptionButton _funcName;

	public override void _Ready() {
		this.OnReady();
		GlobalData.I.ReloadConfigs += OnReloadConfigs;
		OnReloadConfigs();
	}

	private void OnReloadConfigs() {
		var functionConfig = GlobalData.I.FunctionConfig;
		if (!string.IsNullOrEmpty(_funcName.Text)) {
			if (!functionConfig.MoDict.ContainsKey(_funcName.Text)) {
				_funcName.Text = "";
			}
		}

		_funcName.Clear();
		for (var i = 0; i < functionConfig.MoArray.Count; i++) {
			var functionMo = functionConfig.MoArray[i];
			var id = i;
			_funcName.AddItem(functionMo.Name, id);
			_funcName.SetItemTooltip(id, functionMo.Description);
		}
	}

	public override CheckSerializeResult CheckCanSerialize() {
		if (string.IsNullOrEmpty(_funcName.Text)) {
			return new CheckSerializeResult { IsFailed = true, Reason = "Function Name Can't Be Empty." };
		}

		return new CheckSerializeResult { IsFailed = false };
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