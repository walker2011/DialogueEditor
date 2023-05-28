using System.Collections.Generic;

namespace DialogueEditor.Data.NodeMo; 

public class SerializeNodeMo {
    public string Name;
    public string Uuid;
    public string SizeX;
    public string SizeY;
    public string PosX;
    public string PosY;

    public string NodeType;
    public string LinkNext;
    
    public string FuncName;
    
    public string VarName;
    public string VarValue;
    
    public string DialogueId;
    
    public string Speaker;
    public string Content;
    public List<OptionMo> OptionMos;
}