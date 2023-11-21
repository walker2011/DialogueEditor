using Newtonsoft.Json.Linq;

namespace DialogueEditor.Data;

public interface IDialogueConfig<TKey> {

	public TKey Key { get; }

	public string GetDescription();

	public void Parse(JObject data);

}