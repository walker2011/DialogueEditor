using System.Collections.Generic;
using System.Linq;
using DialogueEditor.Data;
using DialogueEditor.Data.NodeMo;
using Godot;
using Godot.Sharp.Extras;

namespace DialogueEditor.Nodes;

public partial class DialogueNode : SerializeGraphNode {
	[NodePath("Speaker")] private OptionButton _speaker;

	[NodePath("Content")] private TextEdit _content;

	[NodePath("AddOption")] private Button _addOption;

	private readonly List<Option.Option> _options = new();

	public override void _Ready() {
		this.OnReady();
		_addOption.Pressed += OnAddOption;

		GlobalData.I.OneSettingDataChanged += OnOneSettingDataChanged;
		GlobalData.I.AllSettingDataChanged += OnAllSettingDataChanged;
		OnAllSettingDataChanged();
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
		AddOption();
	}

	private void AddOption(string content = "") {
		var option = GD.Load<PackedScene>("res://Nodes/Option/Option.tscn").Instantiate<Option.Option>();
		AddChild(option);
		option.OptionRemoved += OnOptionRemoved;
		SetSlot(option.GetIndex(), false, 0, Color.FromHtml("#2177b8"), true, 0, Color.FromHtml("#2177b8"));
		option.OptionContent.Text = content;
		_options.Add(option);
	}

	private void OnOptionRemoved(Option.Option option) {
		if (_options.Count > 0) {
			var index = _options.IndexOf(option);
			if (index >= 0) {
				// var linkTos = GlobalData.I.GetLinkTos(Name);
				// if (linkTos != null) {
				// 	foreach (var linkTo in linkTos) {
				// 		if (linkTo.Value.Count > 0 &&
				// 			GlobalData.I.GraphNodes.TryGetValue(linkTo.Value.First().Key, out var targetNode)) {
				// 			var slotIdx = GetConnectionOutputSlot((int) linkTo.Key);
				// 			if (slotIdx == option.GetIndex()) {
				// 				GlobalData.I.RemoveAllOutput(Name, linkTo.Key);
				// 				break;
				// 			}
				// 		}
				// 	}
				// }
				GlobalData.I.RemoveAllOutput(Name);
				

				_options.RemoveAt(index);
				RemoveChild(option);
			}
		}
	}

	public override CheckSerializeResult CheckCanSerialize() {
		if (string.IsNullOrEmpty(_speaker.Text)) {
			return new CheckSerializeResult {IsFailed = true, Reason = "Speaker Can't Be Empty."};
		}

		if (string.IsNullOrEmpty(_content.Text)) {
			return new CheckSerializeResult {IsFailed = true, Reason = "Content Can't Be Empty."};
		}

		return new CheckSerializeResult {IsFailed = false};
	}

	public override void ToJson(SerializeNodeMo mo) {
		base.ToJson(mo);
		
		mo.NodeType = ENodeType.DialogueNode.ToString();
		mo.Speaker = _speaker.Text;
		mo.Content = _content.Text;
		mo.OptionMos = new List<OptionMo>();
			
		var linkTos = GlobalData.I.GetLinkTos(Name);
		var childIdx2Node = new Dictionary<int, SerializeGraphNode>();
		if (linkTos != null) {
			foreach (var linkTo in linkTos) {
				if (linkTo.Value.Count > 0 &&
					GlobalData.I.GraphNodes.TryGetValue(linkTo.Value.First().Key, out var targetNode)) {
					var slotIdx = GetOutputPortSlot((int)linkTo.Key);
					childIdx2Node.Add(slotIdx, targetNode);
				}
			}
		}

		for (int i = 0; i < _options.Count; i++) {
			var option = _options[i];
			if (childIdx2Node.TryGetValue(option.GetIndex(), out var targetNode)) {
				mo.OptionMos.Add(new OptionMo {
					Content= option.OptionContent.Text,
					LinkNext = targetNode.Uuid.ToString()
				});
			}
		}
	}

	public override void FromJson(SerializeNodeMo mo) {
		base.FromJson(mo);
		_speaker.Text = mo.Speaker;
		_content.Text = mo.Content;
		if (mo.OptionMos != null) {
			foreach (var optionMo in mo.OptionMos) {
				AddOption(optionMo.Content);
			}
		}
		
	}
}
