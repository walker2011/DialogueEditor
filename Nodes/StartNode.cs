using DialogueEditor.Data;
using Godot;
using Godot.Sharp.Extras;
using System.Collections.Generic;

namespace DialogueEditor.Nodes;

public partial class StartNode : SerializeGraphNode {

	[NodePath("DialogueId")]
	private LineEdit _dialogueId;

	public override void _Ready() {
		this.OnReady();
	}

	public override CheckSerializeResult CheckCanSerialize() {
		if (string.IsNullOrEmpty(_dialogueId.Text)) {
			return new CheckSerializeResult { IsFailed = true, Reason = "Dialogue Id Can't Be Empty." };
		}

		return new CheckSerializeResult { IsFailed = false };
	}

	public override void ToJson(Dictionary<string, string> json) {
		base.ToJson(json);
		json["nodeType"] = ENodeType.StartNode.ToString();
		json["DialogueId"] = _dialogueId.Text;
	}

	public override void FromJson(Dictionary<string, string> json) {
		base.FromJson(json);
		_dialogueId.Text = json["DialogueId"];
	}

}