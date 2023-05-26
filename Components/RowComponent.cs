using Godot;
using Godot.Sharp.Extras;

namespace DialogueEditor.Components;

public partial class RowComponent : HBoxContainer {
	[Signal]
	public delegate void RemoveRowPressedEventHandler(RowComponent row);

	[Signal]
	public delegate void ContentChangedEventHandler(RowComponent row);

	[NodePath("Content")]
	public LineEdit Content;

	[NodePath("RemoveRow")]
	public Button RemoveRow;

	public override void _Ready() {
		this.OnReady();
		Content.TextSubmitted += text => EmitSignal(SignalName.ContentChanged, this);
		RemoveRow.Pressed += () => EmitSignal(SignalName.RemoveRowPressed, this);
	}
}