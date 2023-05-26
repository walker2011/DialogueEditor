using DialogueEditor.Data;
using Godot;
using Godot.Sharp.Extras;
using System.Collections.Generic;

namespace DialogueEditor.Components;

public partial class MenuPanel : Panel {

	[NodePath("VBoxContainer/HBoxContainer/AddRow")]
	private Button _addRow;

	[NodePath("VBoxContainer/HBoxContainer/Close")]
	private Button _close;

	[NodePath("VBoxContainer/ScrollContainer/RowContainer")]
	private VBoxContainer _rowContainer;

	private SettingMo _mo;

	public override void _Ready() {
		this.OnReady();
		_close.Pressed += () => {
			Visible = false;
			InvalidateData();
		};
		_addRow.Pressed += OnAddRow;
	}

	public void SetData(SettingMo mo) {
		InvalidateData();
		_mo = mo;
		for (int i = _rowContainer.GetChildCount() - 1; i >= _mo.Data.Count; i--) {
			var child = _rowContainer.GetChild(i);
			_rowContainer.RemoveChild(child);
			child.QueueFree();
		}

		for (int i = _rowContainer.GetChildCount(); i < _mo.Data.Count; i++) {
			OnAddRow();
		}

		for (int i = 0; i < _rowContainer.GetChildCount(); i++) {
			var child = _rowContainer.GetChild<RowComponent>(i);
			child.Content.Text = _mo.Data[i];
		}

		Visible = true;
	}

	private void OnAddRow() {
		var node = GD.Load<PackedScene>("res://Components/row_component.tscn").Instantiate<RowComponent>();
		node.RemoveRowPressed += OnRemoveRow;
		node.ContentChanged += OnContentChanged;
		_rowContainer.AddChild(node);
	}

	private void OnContentChanged(RowComponent row) {
	}

	private void OnRemoveRow(RowComponent row) {
		row.QueueFree();
	}

	private void InvalidateData() {
		if (_mo != null) {
			var newData = new List<string>();
			foreach (var child in _rowContainer.GetChildren()) {
				if (child is RowComponent row) {
					if (!string.IsNullOrEmpty(row.Content.Text)) {
						newData.Add(row.Content.Text);
					}
				}
			}

			_mo.Data = newData;
			GlobalData.I.InvalidateSetting(_mo);
			_mo = null;
		}
	}

	public void HideAndInvalidateData() {
		InvalidateData();
		Visible = false;
	}
}