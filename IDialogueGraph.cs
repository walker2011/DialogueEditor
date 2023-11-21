using DialogueEditor.Data;
using Godot;
using Godot.Collections;

namespace DialogueEditor; 

public interface IDialogueGraph {

	Array<Dictionary> GetConnectionList();

	void TryDisconnectNode(StringName fromNode, long fromPort, StringName toNode, long toPort);

	void DeserializeData(DialogueGraphSerializeMo graphMo);
}