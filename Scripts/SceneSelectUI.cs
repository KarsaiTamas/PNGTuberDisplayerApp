using Godot;
using System;

public partial class SceneSelectUI : Control
{
    public int sceneID;
    public Button selectSceneButton;
    public Button deleteSceneButton;
    public override void _EnterTree()
    {
        selectSceneButton = GetNode<Button>("panel/MG/HC/SelectScene");
        deleteSceneButton = GetNode<Button>("panel/MG/HC/DeleteScene");
    }
    public override void _Ready()
    {
        deleteSceneButton.ButtonUp += RemoveScenePressed;
        selectSceneButton.ButtonUp += SelectScenePressed;
    }

    private void SelectScenePressed()
    {

        if (ProgramHandler.isSceneModified)
        {
            ConfirmUI.Instance.SetConfirm($"You have unsaved changes. Would you like to save before switching scenes? ",
                SelectSceneAndSave,SelectScene);
            return;
        }
        ProgramHandler.instance.ChangeScene(sceneID);
    }
    void SelectScene()
    { 
        ProgramHandler.instance.ChangeScene(sceneID);

    }
    void SelectSceneAndSave()
    {
        ProgramHandler.instance.SaveScene();
        ProgramHandler.instance.ChangeScene(sceneID);

    }
    void RemoveScenePressed()
    {
        ConfirmUI.Instance.SetConfirm($"Delete {selectSceneButton.Text}", RemoveScene);
    
    }
    void RemoveScene()
    {
        GD.Print(sceneID);
        if(!ProgramHandler.instance.DeleteScene(sceneID))return;
        QueueFree();
    }

}
