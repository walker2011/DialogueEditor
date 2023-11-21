using DialogueEditor.Data;
using Godot;
using Godot.Sharp.Extras;

namespace DialogueEditor.Components;

public partial class MenuPanel : Panel {

	// [NodePath("VBoxContainer/HBoxContainer/Close")]
	// private Button _close;
	//
	// [NodePath("VBoxContainer/ScrollContainer/RowContainer")]
	// private VBoxContainer _rowContainer;
	//
	// public override void _Ready() {
	// 	this.OnReady();
	// 	_close.Pressed += () => {
	// 		Visible = false;
	// 	};
	// }
	//
	// public void SetData<TKey, TValue>(DialogueConfigMos<TKey, TValue> mo) where TValue : IDialogueConfig<TKey>, new() {
	// 	for (var i = _rowContainer.GetChildCount() - 1; i >= mo.MoArray.Count; i--) {
	// 		var child = _rowContainer.GetChild(i);
	// 		_rowContainer.RemoveChild(child);
	// 		child.QueueFree();
	// 	}
	//
	// 	for (var i = _rowContainer.GetChildCount(); i < mo.MoArray.Count; i++) {
	// 		AddRow();
	// 	}
	//
	// 	for (var i = 0; i < _rowContainer.GetChildCount(); i++) {
	// 		var child = _rowContainer.GetChild<RowComponent>(i);
	// 		child.Content.Text = mo.MoArray[i].GetDescription();
	// 	}
	//
	// 	Visible = true;
	// }
	//
	// private void AddRow() {
	// 	var node = GD.Load<PackedScene>("res://Components/row_component.tscn").Instantiate<RowComponent>();
	// 	node.RemoveRowPressed += OnRemoveRow;
	// 	node.ContentChanged += OnContentChanged;
	// 	_rowContainer.AddChild(node);
	// }
	//
	// private void OnContentChanged(RowComponent row) {
	// }
	//
	// private void OnRemoveRow(RowComponent row) {
	// 	row.QueueFree();
	// }
	//
	// public void HideAndInvalidateData() {
	// 	Visible = false;
	// }
}