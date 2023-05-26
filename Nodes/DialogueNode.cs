using DialogueEditor.Data;
using Godot;
using Godot.Sharp.Extras;
using System.Collections.Generic;

namespace DialogueEditor.Nodes;

public partial class DialogueNode : SerializeGraphNode {

	[NodePath("Speaker")]
	private OptionButton _speaker;

	[NodePath("Content")]
	private TextEdit _content;

	[NodePath("AddOption")]
	private Button _addOption;

	private readonly List<Option.Option> _options = new();

	public override void _Ready() {
		this.OnReady();
		_addOption.Pressed += OnAddOption;

		GlobalData.I.OneSettingDataChanged += OnOneSettingDataChanged;
		GlobalData.I.AllSettingDataChanged += OnAllSettingDataChanged;
	}

	private void OnAllSettingDataChanged() {
		var mo = GlobalData.I.GetSetting(ESettingType.Speaker);
		OnOneSettingDataChanged(mo);
	}

	private void OnOneSettingDataChanged(SettingMo mo) {
		if (mo.SettingType.Equals(ESettingType.Speaker)) {
			if (string.IsNullOrEmpty(_speaker.Text)) {
				if (mo.Data.IndexOf(_speaker.Text) == -1) {
					_speaker.Text = "";
				}
			}

			_speaker.Clear();
			foreach (var content in mo.Data) {
				_speaker.AddItem(content);
			}
		}
	}

	private void OnAddOption() {
		var option = GD.Load<PackedScene>("res://Nodes/Option/Option.tscn").Instantiate<Option.Option>();
		AddChild(option);
		option.OptionRemoved += OnOptionRemoved;
		SetSlot(option.GetIndex(), false, 0, Color.FromHtml("#2177b8"), true, 0, Color.FromHtml("#2177b8"));
		_options.Add(option);
	}

	private void OnOptionRemoved(Option.Option option) {
		if (_options.Count > 0) {
			var index = _options.IndexOf(option);
			if (index >= 0) {
				for (var i = 0; i < _options.Count - 1; i++) {
					_options[i].OptionContent.Text = _options[i + 1].OptionContent.Text;
				}

				// 先全部移除
				GlobalData.I.RemoveAllOutput(Name);

				var lastNode = _options[^1];

				_options.RemoveAt(_options.Count - 1);
				RemoveChild(lastNode);
			}
		}
	}

	public override CheckSerializeResult CheckCanSerialize() {
		if (string.IsNullOrEmpty(_speaker.Text)) {
			return new CheckSerializeResult { IsFailed = true, Reason = "Speaker Can't Be Empty." };
		}

		if (string.IsNullOrEmpty(_content.Text)) {
			return new CheckSerializeResult { IsFailed = true, Reason = "Content Can't Be Empty." };
		}

		return new CheckSerializeResult { IsFailed = false };
	}

	public override void ToJson(Dictionary<string, string> json) {
		base.ToJson(json);
		json["nodeType"] = ENodeType.DialogueNode.ToString();
		json["Speaker"] = _speaker.Text;
		json["Content"] = _content.Text;
	}

	public override void FromJson(Dictionary<string, string> json) {
		base.FromJson(json);
		_speaker.Text = json["Speaker"];
		_content.Text = json["Content"];
	}
}
