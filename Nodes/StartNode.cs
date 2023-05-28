using DialogueEditor.Data;
using DialogueEditor.Data.NodeMo;
using Godot;
using Godot.Sharp.Extras;

namespace DialogueEditor.Nodes;

public partial class StartNode : SerializeGraphNode {
	[NodePath("DialogueId")] private LineEdit _dialogueId;

	public override void _Ready() {
		this.OnReady();
	}

	public override CheckSerializeResult CheckCanSerialize() {
		if (string.IsNullOrEmpty(_dialogueId.Text)) {
			return new CheckSerializeResult {IsFailed = true, Reason = "Dialogue Id Can't Be Empty."};
		}

		return new CheckSerializeResult {IsFailed = false};
	}

	public override void ToJson(SerializeNodeMo mo) {
		base.ToJson(mo);
		mo.NodeType = ENodeType.StartNode.ToString();
		mo.DialogueId = _dialogueId.Text;
		var targetNode = GlobalData.I.GetLinkTo(Name);
		if (targetNode != null) {
			mo.LinkNext = targetNode.Uuid.ToString();
		}
	}

	public override void FromJson(SerializeNodeMo mo) {
		base.FromJson(mo);
		_dialogueId.Text = mo.ToString();
	}
}
