using Godot;

namespace DialogueEditor.Data;

[GlobalClass]
public partial class GlobalSettingMo : Resource {
	
	public static readonly string GlobalSettingPath = "user://global_setting.tres";

	[Export]
	public string DialogueRoot { get; set; }

	[Export]
	public string CurrentDir { get; set; }

}