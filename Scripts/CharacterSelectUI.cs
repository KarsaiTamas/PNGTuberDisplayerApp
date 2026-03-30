using Godot; 

public partial class CharacterSelectUI : Control
{
    public int characterID;
    public Button selectCharacterButton;
    public Button addToSceneButton;
    public Button deleteCharacterButton;
    public override void _EnterTree()
    {
        selectCharacterButton = GetNode<Button>("panel/MG/VC/SelectCharacter");
        addToSceneButton = GetNode<Button>("panel/MG/VC/HC/AddToSceneButton");
        deleteCharacterButton = GetNode<Button>("panel/MG/VC/HC/DeleteCharacterButton");

    }
    public override void _Ready()
    {
        //characterID = ProgramHandler.characterID;
        //selectCharacterButton.Text = $"Character name {characterID}";
        AddButtonVisibility(ProgramHandler.openSceneID);
        ProgramHandler.instance.OnSceneChange += AddButtonVisibility;
        deleteCharacterButton.ButtonUp += RemoveCharacterPressed;
        selectCharacterButton.ButtonUp += SelectCharacterPressed;
        addToSceneButton.ButtonUp += AddCharacterToScene;
    }
    void SelectCharacterPressed()
    {
        if (ProgramHandler.isCharacterModified)
        {
            ConfirmUI.Instance.SetConfirm($"You have unsaved changes. Would you like to save before changing character? ",
                SelectChartacterAndSave,SelectChartacter);
            return;
        }
        ProgramHandler.instance.OpenCloseCharacterSettings(characterID);
    }
    void SelectChartacter()
    {
        ProgramHandler.instance.OpenCloseCharacterSettings(characterID);
    }
    void SelectChartacterAndSave()
    {
        ProgramHandler.instance.SaveCharacter(characterID);
        ProgramHandler.instance.OpenCloseCharacterSettings(characterID);
    }
    void RemoveCharacterPressed()
    {
        ConfirmUI.Instance.SetConfirm($"Delete {selectCharacterButton.Text}", RemoveCharacter);

    }
    void AddCharacterToScene()
    {
        ProgramHandler.instance.AddCharacterToScene(characterID);
    }
    void RemoveCharacter()
    {
        ProgramHandler.instance.DeleteCharacter(characterID);
        QueueFree();
    }
    void AddButtonVisibility(int visibility)
    {

        if (visibility == -1)
        {
            addToSceneButton.Hide();
        }
        else
        {
            addToSceneButton.Show();
        }
    }
    protected override void Dispose(bool disposing)
    {
        ProgramHandler.instance.OnSceneChange -= AddButtonVisibility;

        base.Dispose(disposing);
    }
}
