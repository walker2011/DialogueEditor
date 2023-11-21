using Newtonsoft.Json.Linq;

namespace DialogueEditor.Data;

public class FunctionMo : IDialogueConfig<string> {
	public string Name;
	public string Description;

	/// <inheritdoc />
	public string Key => Name;

	/// <inheritdoc />
	public string GetDescription() {
		return Description;
	}

	/// <inheritdoc />
	public void Parse(JObject data) {
		Name = data.GetValue(nameof(Name))?.Value<string>();
		Description = data.GetValue(nameof(Description))?.Value<string>();
	}
}