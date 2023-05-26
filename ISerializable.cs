using System.Collections.Generic;

namespace DialogueEditor;

public interface ISerializable {

	CheckSerializeResult CheckCanSerialize();
	void ToJson(Dictionary<string, string> json);
	void FromJson(Dictionary<string, string> json);

}