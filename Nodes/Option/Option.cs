using Godot;
using Godot.Sharp.Extras;

namespace DialogueEditor.Nodes.Option;

public partial class Option : HBoxContainer {

	[Signal]
	public delegate void OptionRemovedEventHandler(Option option);

	[NodePath("RemoveOption")]
	public Button RemoveOption;

	[NodePath("Content")]
	public LineEdit OptionContent;

	public override void _Ready() {
		this.OnReady();
		RemoveOption.Pressed += () => EmitSignal(SignalName.OptionRemoved, this);
	}

}