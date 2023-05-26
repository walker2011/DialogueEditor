using Godot;
using System.Collections.Generic;

namespace DialogueEditor.Data; 

public partial class SettingMo : RefCounted {
	public ESettingType SettingType;
	public List<string> Data;
}