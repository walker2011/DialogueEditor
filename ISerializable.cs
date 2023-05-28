using System.Collections.Generic;
using DialogueEditor.Data.NodeMo;

namespace DialogueEditor;

public interface ISerializable {

	CheckSerializeResult CheckCanSerialize();
	void ToJson(SerializeNodeMo mo);
	void FromJson(SerializeNodeMo mo);

}