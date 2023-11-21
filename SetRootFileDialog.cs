using DialogueEditor.Data;
using Godot;

namespace DialogueEditor; 

public partial class SetRootFileDialog : FileDialog {


	/// <inheritdoc />
	public override void _Ready() {
		FileMode = FileModeEnum.OpenDir;
		Access = AccessEnum.Filesystem;
		DialogCloseOnEscape = false;
		
		var cancelBtn = GetCancelButton();
		cancelBtn.Text = "取消";
		var okBtn = GetOkButton();
		okBtn.Text = "确定";
		
		CloseRequested += OnCloseRequested;
	}
	
	private void OnCloseRequested() {
		if (GlobalData.I.IsDialogueRootInvalid(CurrentDir)) {
			
		}
	}


}