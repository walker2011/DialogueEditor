using Godot;
using Medo;
using System.Collections.Generic;

namespace DialogueEditor.Nodes;

public partial class SerializeGraphNode : GraphNode, ISerializable {

	public Uuid7 Uuid { get; private set; } = Uuid7.NewUuid7();

	public virtual CheckSerializeResult CheckCanSerialize() {
		return new CheckSerializeResult { IsFailed = false };
	}

	public virtual void ToJson(Dictionary<string, string> json) {
		json["Uuid"] = Uuid.ToString();
		json["SizeX"] = Size.X.ToString();
		json["SizeY"] = Size.Y.ToString();
		json["PosX"] = Position.X.ToString();
		json["PosY"] = Position.Y.ToString();
	}

	public virtual void FromJson(Dictionary<string, string> json) {
		Uuid = Uuid7.FromString(json["Uuid"]);
		Size = new(json["SizeX"].ToFloat(), json["SizeY"].ToFloat());
		Position = new(json["PosX"].ToFloat(), json["PosY"].ToFloat());
	}

}