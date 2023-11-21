using Newtonsoft.Json.Linq;

namespace DialogueEditor.Data;

public class NpcMo : IDialogueConfig<ulong> {
	public ulong Id;
	public string Name;

	/// <inheritdoc />
	public ulong Key => Id;

	/// <inheritdoc />
	public string GetDescription() {
		return Name;
	}
	/// <inheritdoc />
	public void Parse(JObject data) {
		Id = (ulong)data.GetValue(nameof(Id))?.Value<ulong>();
		Name = data.GetValue(nameof(Name))?.Value<string>();
	}

	/// <inheritdoc />
	public override string ToString() {
		return $"{Name}#{Id}";
	}

	public static ulong TryParseNpcId(string content) {
		if (!string.IsNullOrEmpty(content)) {
			var array = content.Split("#");
			if (array.Length >= 2) {
				return ulong.Parse(array[1]);
			}
		}
		return 0;
	}
}