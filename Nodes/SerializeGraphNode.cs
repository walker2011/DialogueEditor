using DialogueEditor.Data.NodeMo;
using Godot;
using Medo;

namespace DialogueEditor.Nodes;

public partial class SerializeGraphNode : GraphNode, ISerializable {
    public Uuid7 Uuid { get; private set; } = Uuid7.NewUuid7();

    public virtual CheckSerializeResult CheckCanSerialize() {
        return new CheckSerializeResult {IsFailed = false};
    }

    public virtual void ToJson(SerializeNodeMo mo) {
        mo.Uuid = Uuid.ToString();
        mo.SizeX = Size.X.ToString();
        mo.SizeY = Size.Y.ToString();
        mo.PosX = PositionOffset.X.ToString();
        mo.PosY = PositionOffset.Y.ToString();
    }

    public virtual void FromJson(SerializeNodeMo mo) {
        Uuid = Uuid7.FromString(mo.Uuid);
        Size = new(mo.SizeX.ToFloat(), mo.SizeY.ToFloat());
        PositionOffset = new(mo.PosX.ToFloat(), mo.PosY.ToFloat());
    }
}