using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class UIManager : Node
{
    #region Variables
    public static UIManager instance;
    public FileDialog fileLoader;
    public string fileWithPathSelected="";
    #endregion
    #region Enums
    private enum ESceneMenus
    {
        CreateScene,
        OpenScene,
        SaveScene,
        DeleteScene,
    }

    private enum ECharacterMenus
    {
        CreateCharacter,
        OpenCharacter,
        SaveCharacter,
        CreateOutfit,
        AddAnimation,
        DeleteCharacter,
        DeleteOutfit,
    }
    private enum EMultiplayerMenu
    { 
        Host, 
        Join,
        Invite,
        CopyInviteCode,
    }
    private enum EHelpMenu
    {
        Controls,
        CreateScene,
        CreateCharacter
    }
    #endregion

    #region UI Elements
    private MenuButton sceneMB;
    private PopupMenu sceneOptionsPM;
    private MenuButton characterMB;
    private PopupMenu characterOptionsPM;
    private MenuButton networkMB;
    private MenuButton helpMB;
    public CharacterManager characterEditor;
    public Control controlsUI;
    public List<Character> charactersOnScene;
    #endregion

    public override void _EnterTree()
    {
        instance = this;
        InitMenuButtons();
        InitCharacterEditor();
    }

    public override void _Ready()
    {
        SaveLoadManager.LoadSavedScenes();
        SaveLoadManager.LoadSavedCharacters();
        CharacterEditorVisibility(false);
    }
    #region UI Initializing
    private void InitMenuButtons()
    {
        sceneMB = GetNode<MenuButton>("/root/Control/UICL/TopBarMenu/HC/SceneMB");
        characterMB = GetNode<MenuButton>("/root/Control/UICL/TopBarMenu/HC/CharacterMB");
        networkMB = GetNode<MenuButton>("/root/Control/UICL/TopBarMenu/HC/NetworkMB");
        helpMB = GetNode<MenuButton>("/root/Control/UICL/TopBarMenu/HC/HelpMB");
        controlsUI = GetNode<Control>("/root/Control/UICL/ControlsPC");
        sceneOptionsPM = new PopupMenu();
        sceneOptionsPM.Name = "SceneOptionsPM"; 
        characterOptionsPM = new PopupMenu();
        characterOptionsPM.Name = "CharacterOptionsPM";
        fileLoader = GetNode<FileDialog>("FileDialog");
        /*
         
        
         */
        GetNode<Button>("/root/Control/UICL/ControlsPC/VBoxContainer/Button").ButtonDown += ToggleControlsUI;
        fileLoader.FileSelected += FileDialogueFileSelect;
        controlsUI.Visible = false;
        AddItemsToPopup(sceneMB.GetPopup(), 
            ("Create Scene", false), 
            ("Open Scene", sceneOptionsPM), 
            ("Save Scene", true),
            ("Delete Scene", true));
        sceneMB.GetPopup().IdPressed += SceneMenuBPressed;

        AddItemsToPopup(characterMB.GetPopup(), 
            ("Create Character", false), 
            ("Open Character", characterOptionsPM), 
            ("Save Character", true), 
            ("Create Outfit", true),
            ("Add animation", true),
            ("Delete Character", true), 
            ("Delete Outfit", true));
        characterMB.GetPopup().IdPressed += CharacterMenuBPressed;

        AddItemsToPopup(networkMB.GetPopup(), 
            ("Host", false),
            ("Join Via code in clipboard", false),
            ("Invite", true),
            ("Copy Invite code", true));
        networkMB.GetPopup().IdPressed += NetworkMenuBPressed;

        AddItemsToPopup(helpMB.GetPopup(), 
            ("Show Controls", false), 
            ("How to make scene", false),
            ("How to make character", false));
        helpMB.GetPopup().IdPressed += HelpMenuBPressed;

        sceneOptionsPM.IdPressed += SceneOptionsPressed;
        AddItemsToPopup(sceneOptionsPM, ("Select scene from directory",false));
        characterOptionsPM.IdPressed += CharacterOptionsPressed;
        AddItemsToPopup(characterOptionsPM, ("Select character from directory",false));
    }
    private void InitCharacterEditor()
    {
        characterEditor = GetNode<CharacterManager>("/root/Control/UICL/CharacterEditorPC");
    }
     
    #endregion
    #region Toggle UI 
    #endregion
    #region UI Functionality 
    private void SceneMenuBPressed(long id)
    {
        GD.Print($"{id} ID pressed In scene");
        switch ((ESceneMenus)id)
        {
            case ESceneMenus.CreateScene:
            default:
                SceneManager.instance.CreateScene();
                SceneManager.instance.LoadScene(SaveLoadManager.scenesToLoad.Count);
                break;
            case ESceneMenus.SaveScene:
                SceneManager.instance.SaveScene();
                break;
            case ESceneMenus.DeleteScene:
                ConfirmUI.Instance.ShowConfirm("Do you want to delete this open scene?", SceneManager.instance.DeleteScene );
                break;
        }
    }

    private void CharacterMenuBPressed(long id)
    {
        GD.Print($"{id} ID pressed In Character");
        switch ((ECharacterMenus)id)
        {
            case ECharacterMenus.CreateCharacter:
            default:
                characterEditor.CreateCharacter();
                CharacterOptionsPressed(SaveLoadManager.charactersToLoad.Count);
                break;
            case ECharacterMenus.SaveCharacter:
                characterEditor.SaveCharacter();
                break;
            case ECharacterMenus.DeleteCharacter:
                ConfirmUI.Instance.ShowConfirm("Do you want to delete this open character?", characterEditor.DeleteCharacter);
                break;
            case ECharacterMenus.CreateOutfit:
                characterEditor.CreateOutfit();
                break;
            case ECharacterMenus.DeleteOutfit:
                ConfirmUI.Instance.ShowConfirm("Do you want to delete this open outfit?", characterEditor.DeleteOutfit);
                break;
        }
    }
    private void NetworkMenuBPressed(long id)
    {
        GD.Print($"{id} ID pressed In Network");
        switch ((EMultiplayerMenu)id)
        {

            case EMultiplayerMenu.Join:
            default:
                NetworkManager.instance.JoinToLobby();
                break;
            case EMultiplayerMenu.Host:
                NetworkManager.instance.HostLobby();
                break;
            case EMultiplayerMenu.Invite:
                break;
            case EMultiplayerMenu.CopyInviteCode:
                DisplayServer.ClipboardSet(NetworkManager.instance.GetJoinCode().ToString());
                break; 
        }
    }
    private void HelpMenuBPressed(long id)
    {
        GD.Print($"{id} ID pressed In help");
        switch ((EHelpMenu)id)
        {
            case EHelpMenu.Controls:
            default:
                ToggleControlsUI();
                break;
            case EHelpMenu.CreateScene:
                break;
            case EHelpMenu.CreateCharacter:
                break;
        }
    }
    private void SceneOptionsPressed(long id)
    {
        if((int)id == 0)
        {
            SaveLoadManager.OpenFileDialogueForScene();
            return;
        }
        SaveLoadManager.LoadScene((int)id);
    }

    private void CharacterOptionsPressed(long id)
    {
        if (id == 0)
        {
            OpenFileDialogueForCharacter();
            return;
        }
        GD.Print("we pressed character with ID: "+id);
        GD.Print(SaveLoadManager.charactersToLoad[(int)id - 1].filePath);
        characterEditor.LoadCharacter(id);
    }

    public void OpenFileDialogueForCharacter()
    {
        FileDialogueShowSavedData("character");
    }
    void FileDialogueFileSelect(string file)
    {
        //fileLoader.FileSelected -= FileDialogueConfirmedCharacterData; 
        //Handle Image selection
        if(fileLoader.Filters[0].Contains("png"))
        {
            GD.Print(file);
            if (CharacterManager.instance.animClicked == 3)
            {
                GD.Print("This should do the outfit change");
                characterEditor.UpdateOutfit(file);
                return;
            }
            characterEditor.UpdateAnim(CharacterManager.instance.animClicked,file);
            return;
        }
        //Handle character selection
        if (fileLoader.Title.Contains("character"))
        {

            return;
        }
        //

    }
    public void ToggleSceneMenuButtons(bool isDisabled)
    {
        ToggleMenuButtonOptions(sceneMB.GetPopup(), isDisabled,(int)ESceneMenus.SaveScene, (int)ESceneMenus.DeleteScene);
    }

    public void ToggleCharacterMenuButtons(bool isDisabled)
    {
        ToggleMenuButtonOptions(characterMB.GetPopup(), isDisabled, 
            (int)ECharacterMenus.SaveCharacter, 
            (int)ECharacterMenus.DeleteCharacter,
            (int)ECharacterMenus.CreateOutfit, 
            (int)ECharacterMenus.DeleteOutfit);
    }
    public void ToggleNetworkMenuButtons(bool isDisabled)
    {
        ToggleMenuButtonOptions(networkMB.GetPopup(), isDisabled,
            (int)EMultiplayerMenu.Invite,
            (int)EMultiplayerMenu.CopyInviteCode);
    }
    public void ToggleNetworkConnectionButtons(bool isDisabled) 
    {
        ToggleMenuButtonOptions(networkMB.GetPopup(), isDisabled,
            (int)EMultiplayerMenu.Join,
            (int)EMultiplayerMenu.Host);

    }

    public void ToggleAddToSceneButton(bool isDisabled)
    {
        characterEditor.addToSceneButton.Disabled = !isDisabled;
    }
    public void ToggleControlsUI()
    {
        controlsUI.Visible = !controlsUI.Visible;
    }
    public void ResetCharacterEditor()
    {
        GD.PrintErr("Reset Character Editor is not set in UIManager 237");
    }

    public void AddSceneToSceneOptions(string sceneName)
    {
        AddItemsToPopup(sceneOptionsPM, sceneName);
    }

    public void AddCharacterToCharacterOptions(string characterName)
    {
        AddItemsToPopup(characterOptionsPM, characterName);

    }

    public void FileDialogueShowForAnimation(string animName)
    {
        ShowFileDialogue($"Select first frame of {animName} animation.", "*.png; PNG animation frame");
    }

    public void FileDialogueShowSavedData(string saveType)
    {
        ShowFileDialogue($"Select a {saveType} save file.", "*.json; JSON file");
    }
    #endregion
    #region CharacterUI
    public void CharacterOutfitToggle(Character character,bool isVisible)
    {
        character.deleteCharacterFromScene.Visible = isVisible;
        character.selectedOutfit.Visible = isVisible;
        character.uiVisible = isVisible;
    }

    #endregion
    #region SceneManager

    public void OpenScene(int sceneToLoad)
    {  
        GD.Print("Opened scene");
        ToggleSceneUI(true);
        SceneManager.instance.isEdited = false;
    }
    public void CloseScene()
    {

        if (!SceneManager.instance.isEdited) 
        { 
            ClosingSceneItems();
        }
        else 
        { 
            ConfirmUI.Instance.ShowConfirm("You have unsaved changes, would you like to save before closing?", 
                ()=> {
                    SceneManager.instance.SaveScene(); 
                    ClosingSceneItems();
                }, 
                () =>
                {
                    ClosingSceneItems();
                    SceneManager.instance.isEdited = false;

                });
        }
    }
    public void ClosingSceneItems()
    {
        SceneManager.instance.RemoveCharacters();
        ToggleSceneUI(false);
        SceneManager.IDLoadedScene = -1;
    }
    void ToggleSceneUI(bool visible)
    {
        SceneManager.instance.SceneUIPanel.Visible=visible;
        ToggleMenuButtonOptions(sceneMB.GetPopup(), !visible, (int)ESceneMenus.SaveScene, (int)ESceneMenus.DeleteScene);
        ToggleAddToSceneButton(visible);
    }

    public void LoadDataIntoSceneUI()
    {
        //SceneManager.instance.openSceneData;
        SceneManager.instance.sceneNameLE.Text = SceneManager.instance.data.sceneName;
         
        for (int i = 0; i < SceneManager.instance.data.charactersSceneData.Count; i++)
        {
            var characterData = SceneManager.instance.data.charactersSceneData[i];
            GD.Print(characterData.positionX);
            var character = SpawnManager.SpawnCharacterToScene(SaveLoadManager.GetCharacterIntID(characterData.ID));
            character.SpawnInit(characterData);
        }
    }

    internal void RemoveSceneFromPopupMenu(int id)
    {
        RemoveItemFromPopup(sceneOptionsPM, id);
    }

    public void RenameSceneInPopupMenu(int id, string newName)
    {
        RenameItemInPopup(sceneOptionsPM, id, newName);
    }
    #endregion
    #region Character Editor UI

    public void CloseCharacterEditor()
    {
        if (!characterEditor.isEdited)
        {
            CharacterEditorVisibility(false);
            ResetCharacterEditor();
        }
        else
        {
            ConfirmUI.Instance.ShowConfirm("You didn't saved yet, would you like to save the character before closing?",
                ()=> 
                { 
                    characterEditor.SaveCharacter(); 
                    ResetCharacterEditor();
                    CharacterEditorVisibility(false);
                    CharacterManager.instance.isEdited = false;

                },
                () =>
                {
                    CharacterEditorVisibility(false);
                    ResetCharacterEditor();
                    CharacterManager.instance.isEdited = false;
                });
        }
    }

    public void CharacterEditorVisibility(bool visible)
    {
        characterEditor.Visible = visible;
        ToggleMenuButtonOptions(characterMB.GetPopup(), !visible, 
            (int)ECharacterMenus.SaveCharacter, 
            (int)ECharacterMenus.DeleteCharacter,
            (int)ECharacterMenus.CreateOutfit,
            (int)ECharacterMenus.AddAnimation);
        OutfitSwitching(0);

    }
    public void OutfitSwitching(int outfit)
    {
        ToggleMenuButtonOptions(characterMB.GetPopup(), outfit==0, (int)ECharacterMenus.DeleteOutfit);
        characterEditor.ToggleOutfitUI(outfit != 0);
        characterEditor.LoadOutfitToUI();
        characterEditor.characterInEdit.ChangeOutfit(outfit);

    }

    public void RemoveCharacterFromPopupMenu(int id)
    {
        RemoveItemFromPopup(characterOptionsPM, id);
    }
    public void RenameCharacterInPopupMenu(int id,string newName)
    {
        RenameItemInPopup(characterOptionsPM, id, newName);
    } 
    #endregion
    #region Helper Functions
    /// <summary>
    /// Adds items to a menu button's popup and disable them if need be
    /// <br>Usage:</br> 
    /// <br>To add item popup do(string,bool)</br>
    /// <br>To add item popup have item turn on default(string)</br>
    /// <br>To add Submenu do(string,PopupMenu)</br>
    /// </summary>
    /// <param name="popupMenu"></param>
    /// <param name="items">(name, isDisabled) or (name, submenu) (name) </param>
    private void AddItemsToPopup(PopupMenu popupMenu, params object[] items)
    {
        foreach (var itemOption in items)
        {
            switch (itemOption)
            {
                case (string name):
                    popupMenu.AddItem(name);
                    popupMenu.SetItemDisabled(popupMenu.ItemCount - 1,false);
                    break;
                case (string name, bool disabled):
                    popupMenu.AddItem(name);
                    popupMenu.SetItemDisabled(popupMenu.ItemCount - 1, disabled);
                    break;
                case (string name, PopupMenu submenu):
                    popupMenu.AddSubmenuNodeItem(name, submenu);
                    break;
            }
        }
    }
    public void RemoveItemFromPopup(PopupMenu popupMenu, int id)
    {
        popupMenu.RemoveItem(id + 1);
    }
    public void RenameItemInPopup(PopupMenu popupMenu,int id, string newName)
    {
        popupMenu.SetItemText(id+1, newName);
    }
    public void ToggleMenuButtonOptions(PopupMenu popup,bool isDisabled, params int[] popupsToToggle)
    {
        foreach (int menuItem in popupsToToggle)
        {
            popup.SetItemDisabled(menuItem, isDisabled);
        }
    }

    public void SaveBeforeQuitPopup()
    {    
        ConfirmUI.Instance.ShowConfirm("You have unsaved changes. Would you like to save before quiting?",
        () => 
        {
            if (SceneManager.instance.isEdited)
                SceneManager.instance.SaveScene();
            if (CharacterManager.instance.isEdited)
                characterEditor.SaveCharacter();
            GetTree().Quit();
        },
        () => { GetTree().Quit(); }
        );
    }

    void ShowFileDialogue(string title,params string[] filters)
    {
        fileLoader.Title = title;
        fileLoader.Filters = filters;
        fileLoader.PopupCentered();
    }
    #endregion
}
