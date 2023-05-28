using System.Collections.Generic;
using DialogueEditor.Data.ConnectionMo;
using DialogueEditor.Data.NodeMo;
using DialogueEditor.Nodes;

namespace DialogueEditor.Data;

public class DialogueGraphSerializeMo {
    public List<SerializeNodeMo> NodeMos;
    public List<ConnectionSerializeMo> ConnectionMos;
}