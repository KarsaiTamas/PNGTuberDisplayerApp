using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public partial class ProgramHandler : Node
{
    public static ProgramHandler instance;
    public static NetworkManager network;
    public Vector2 previousMouseLocation = Vector2.Zero;
    public Vector2 currentMouseLocation = Vector2.Zero;
    public static System.Collections.Generic.Dictionary<string, PackedScene> nodesToSpawn;
    public Character selectedCharacter; 
    private Button scenesButton;
    private Button newSceneButton;
    private Button newCharacterButton;
    private Button charactersButton;
    private Button saveSceneButton;
    private LineEdit sceneNameLE;
    private Panel scenesPanel;
    private Panel charactersPanel;
    private Node loadedScene;
    public Node scenesContainer;
    public Node charactersContainer;
    private SceneHandler openScene;
    public CharacterAnimsUI characterAnimationsPanel;
    //private LineEdit characterKey;
    /*
    public Action<Node> OnSceneClosed;
    public Action<int> OnSceneOpened;
    public Action<int> OnSceneDeleted;
    public Action<int> OnCharacterSelected;*/
    string keyToPress="";
    public static int sceneID;
    public static int sceneOBJID; 
    public static int openSceneID;
    public static int characterID;
    public static int selectedCharacterID;
    public static bool isCharacterModified;
    public static bool isSceneModified;
    public List<SceneSelectUI> loadedScenes;
    public List<CharacterSelectUI> loadedCharacters;
    public SceneSelectUI currentSceneUI;
    public CharacterSelectUI currentCharacterUI;
    public Action<int> OnSceneChange;
    public static GodotObject DB;
    public FileDialog fileDialog; 
    public AnimUI selectedImage;
    public override void _EnterTree()
    {

        instance = this;
        DB = GetNode<Node>("DB");
        loadedCharacters= new List<CharacterSelectUI>();
        loadedScenes = new List<SceneSelectUI>();
        scenesButton = GetNode<Button>("PC/VBC/SceneC/Button");
        charactersButton = GetNode<Button>("PC/VBC/CharacterC/Button");
        newSceneButton = GetNode<Button>("ScenesP/VBC/PC/MC/VBC/NewSceneB/Button");
        newCharacterButton = GetNode<Button>("CharactersP/VBC/PC/MC/VBC/NewCharacterB/Button");
        saveSceneButton = GetNode<Button>("ScenesP/VBC/PC/MC/VBC/SaveScene/Button");
        sceneNameLE = GetNode<LineEdit>("ScenesP/VBC/PC/MC/VBC/SceneName/LineEdit");
        loadedScene = GetNode("LoadedScene");
        //
        scenesPanel = GetNode<Panel>("ScenesP");
        charactersPanel = GetNode<Panel>("CharactersP");
        nodesToSpawn = SpawnHandler.LoadScenes("res://Scenes/SpawnableScenes/");
        scenesContainer = GetNode("ScenesP/VBC/ScenesListHolder/SC/ScenesCon");
        charactersContainer = GetNode("CharactersP/VBC/PC2/SC/CharactersCon");
        characterAnimationsPanel = GetNode<CharacterAnimsUI>("CharacterAnimations");
        fileDialog = GetNode<FileDialog>("FileDialog");
        //fileDialog.Confirmed += FileFoundAction;
        //characterKey = GetNode<LineEdit>("CharacterAnimations/VC/VCAnimations/PBaseAnim/ScrollContainer/VC/MCActionName/LineEdit");
        //characterKey.TextChanged += (e) => { keyToPress = e.ToUpper(); };
        //CharacterAnimations/VC/VCAnimations/PBaseAnim/ScrollContainer/VC/TalkAnim/VC/SoundInput/OptionButton
        //CharacterAnimations/VC/VCAnimations/PBaseAnim/ScrollContainer/VC/BlinkAnim/VC/SoundInput/OptionButton
        /*
         *  Array AudioServer.capture_get_device_list()
            String AudioServer.capture_get_device()
            AudioServer.capture_set_device()*/
        ((Control)saveSceneButton.GetParent()).Hide();
        ((Control)sceneNameLE.GetParent()).Hide();
        fileDialog.FileSelected += AnimationChosen;



        //SetupPopup();
        sceneID = 0;
        openSceneID = -1;
        characterID = 0;
        selectedCharacterID = -1;

        isCharacterModified = false;
        isSceneModified = false;
    }


    public override void _Ready()
    {

        SetupButtons();
        scenesPanel.Hide();
        charactersPanel.Hide();
        characterID=DataBaseHandler.GetNextIDForTable(DataBaseHandler.Table.characters);
        LoadDatabaseData();
        network = new NetworkManager();
        network.Name = "NetworkManager";
        AddChild(network);
        //DataBaseHandler.CreateCharacter("sajt", CharacterType.character_png);
    }


    public override void _Input(InputEvent e)
    {

        if (e is InputEventMouse mouse)
        {
            currentMouseLocation = mouse.Position;
            HandleCharacter(mouse);
            previousMouseLocation = mouse.Position;
        }
        if (e is InputEventKey key)
        { 
            if (key.AsText().ToUpper().Equals(keyToPress)) GD.Print(key.AsText()); 
            if (key.Keycode.Equals(Key.F) && key.IsReleased())
            {
                MirrorCharacter();
            }
        }
    }
    void HandleCharacter(InputEventMouse e)
    {
        if (selectedCharacter == null) return;
        selectedCharacter.CharacterActions(e, currentMouseLocation, previousMouseLocation);

    }
    void MirrorCharacter()
    {
        if (selectedCharacter == null) return;
        var sc = selectedCharacter;
        sc.mirrored = !sc.mirrored;
        sc.Flip(sc.mirrored);
    }
    void SetupButtons()
    {
        scenesButton.ButtonUp += () =>
        {
            if (scenesPanel.Visible) 
            { 
                scenesPanel.Hide(); 

            }
            else 
            { 
                scenesPanel.Show();
                charactersPanel.Hide();

            }

        };
        newSceneButton.ButtonUp += AddNewScene;
        newCharacterButton.ButtonUp += AddNewCharacter;
        charactersButton.ButtonUp += () =>
        {
            if (charactersPanel.Visible) 
            {
                charactersPanel.Hide(); 
            
            }
            else 
            {
                charactersPanel.Show();
                scenesPanel.Hide();

            }

        };
        saveSceneButton.ButtonUp += SaveScene;
        //OnCharacterSelected += OpenCharacterSettings;
        //OnSceneDeleted += DeleteScene;
    }
    public SceneSelectUI GetScene(int id)
    {
        GD.Print("Getting scene");
        GD.Print(loadedScenes[0].sceneID);
        GD.Print(id);

        return loadedScenes.Where(sc => sc.sceneID == id).First();
    }
    public CharacterSelectUI GetCharacter(int id)
    {
        return loadedCharacters.Where(sc => sc.characterID== id).First();
    }

    void SetCurrentSceneUI(int id)
    {
        currentSceneUI = GetScene(id); 
    }

    void SetCurrentCharacterUI()
    {
        currentCharacterUI = GetCharacter(characterID);
    }

    #region Spawning
    
    void AddNewScene()
    { 
        AddSceneToDB();
        SpawnHandler.SpawnSceneUI(sceneID, $"Scene name {sceneID}");
 
    }
    private void SpawnCharacterToScene(int sceneDataid)
    {
        DataBaseHandler.GetCharacterInScene(sceneDataid.ToString());

    }
    private void AddNewCharacter()
    {
        AddCharacterToDB();
        SpawnHandler.SpawnCharacterUI(characterID, $"Character name {characterID}");
    } 
    public void SetCloseScene()
    {
        if(openScene==null) return; 
        ConfirmUI.Instance.SetConfirm("Are you sure you want to close this scene?",CloseScene);
        
    }

    public void OpenScene(int id)
    {
        if (openSceneID == -1) return;
        openScene=(SceneHandler)SpawnHandler.Spawn(SpawnableScenes.BaseScene, loadedScene);
        sceneNameLE.Text=currentSceneUI.selectSceneButton.Text;
        openScene.sceneID = id;
        openScene.LoadScene();

    }
    public void AnimationChosen(string path)
    {
        selectedImage.GetAnimationLocation(path);
        if (selectedImage.data.animID == 1)
        {
            var outfit= characterAnimationsPanel.data.GetOutfitByID(selectedImage.data.outfitID);
            outfit.baseOutfitLook.Texture=FileLoaderHandler.GetCharacterAnim(path);
        }
    }

    public void CancelDialogue()
    {
        selectedImage.data.spriteLoc = "default";
        selectedImage.animStartFrameImg.Texture = FileLoaderHandler.GetCharacterAnim("default");
    }

    public void AddCharacterToScene(int id)
    {
        GD.Print($"Character with ID of: {id} should be added to scene");
        int sceneDataID = DataBaseHandler.GetNextIDForTable(DataBaseHandler.Table.scene_data);
        DataBaseHandler.InsertIntoSceneData(
            sceneDataID,
            id, 1);
        SceneHandler.instance.charactersInScene.Add(new SceneData(sceneDataID, id, 1, 0, 0, 256, false));
        SceneHandler.instance.AddingCharacter(
            SceneHandler.instance.charactersInScene[SceneHandler.instance.charactersInScene.Count-1]);

    }
    #endregion
    #region UI
    public void ChangeScene(int id)
    {
        OnSceneChange.Invoke(id == openSceneID?-1:id);
        if (openScene!=null)
        {
            bool sameClicked = id == openSceneID;
            CloseScene();
            if (sameClicked) return;
        }
        openSceneID=id;
        
        GD.Print("Should open the selected scene!");
        ((Control)saveSceneButton.GetParent()).Show();
        ((Control)sceneNameLE.GetParent()).Show();
        SetCurrentSceneUI(id);
        OpenScene(id);
    }
    public void CloseScene()
    {
        ((Control)saveSceneButton.GetParent()).Hide();
        ((Control)sceneNameLE.GetParent()).Hide();
        openSceneID = -1;
        foreach (var character in openScene.charactersInScene)
        {
            character.character.UnsubingFromAudioHandling();
        }
        openScene.QueueFree();
        openScene=null;

        GD.Print("Should close the selected scene!");
    }
    public void OpenCloseCharacterSettings(int obj)
    {
        if (obj == selectedCharacterID)
        {
            selectedCharacterID = -1;
            characterAnimationsPanel.Hide();
            return;
        }
        selectedCharacterID = obj;
        characterAnimationsPanel.Show(); 
        characterAnimationsPanel.data.characterID=selectedCharacterID;
        characterAnimationsPanel.RemoveOutfits();
        characterAnimationsPanel.ChangeOutfit(1);
        characterAnimationsPanel.LoadOutfits();
    }
    private void LoadCharacters()
    {
        foreach (Dictionary item in DataBaseHandler.GetCreatedCharacters())
        {
            SpawnHandler.SpawnCharacterUI(item["id"].AsInt32(), item["name"].AsString());
        }
    }
    private void LoadScenes()
    {
        foreach (Dictionary item in DataBaseHandler.GetCreatedScenes())
        {
            GD.Print(item["id"]+" great scene id");
            SpawnHandler.SpawnSceneUI(item["id"].AsInt32(), item["name"].AsString());

        }
    }

    #endregion

    #region Database

    private void LoadDatabaseData()
    {
        GD.Print($"This should laod every scene and character uis from the database");
        LoadCharacters();
        LoadScenes();
    } 

    public void AddSceneToDB()
    {
        GD.Print($"This should add a scene to the database");
        sceneID = DataBaseHandler.GetNextIDForTable(DataBaseHandler.Table.scenes);
        DataBaseHandler.CreateScene(sceneID.ToString(), $"Scene name {sceneID}");

    }
    public void AddCharacterToDB()
    {
        GD.Print($"This should add a character to the database");
        characterID = DataBaseHandler.GetNextIDForTable(DataBaseHandler.Table.characters);
        DataBaseHandler.CreateCharacter(characterID.ToString(), $"Character name {characterID}", CharacterType.character_png);
        DataBaseHandler.InsertOutfitIntoCharacter(1, characterID, "Normal", false, "default");
        AddAnimation(characterID, 1, "Idle", AnimType.other,1);
        AddAnimation(characterID, 1, "Talk", AnimType.other,2);
        AddAnimation(characterID, 1, "Blink", AnimType.other,3); 
    }
    public void AddOutfit(int characterId)
    {
        GD.Print($"This should add a outfit to the character with this id:{characterID}");
        int oID = DataBaseHandler.GetNextIDForTable(DataBaseHandler.Table.outfits,characterId);
        DataBaseHandler.InsertOutfitIntoCharacter(oID,characterId,$"outfit{oID}",true,"default");
        
    }
    public void AddAnimation(int characterId,int outfitId,string animName,AnimType anim, int aID=-1)
    {
       
        aID =aID==-1? DataBaseHandler.GetNextIDForTable(DataBaseHandler.Table.outfit_animations):aID;

        DataBaseHandler.InsertAnimationIntoCharacter(aID, outfitId, characterId, animName, "default",1f,1,(int)anim,"default");
        GD.Print($"This should add an animation to the character with this id:{characterID} and with this outfit id:{outfitId} ");
    }
    public bool DeleteScene(int id)
    {
        GD.Print("Scene deleted");
        if (openSceneID == id) CloseScene();
        if(!DataBaseHandler.DeleteScene(id))return false;
        loadedScenes.Remove(GetScene(id));
        DataBaseHandler.CloseUsedScene();
        GD.Print($"Scene with ID of: {id} should be deleted");
        return true;
    }
    public void DeleteCharacter(int id)
    {
        if (selectedCharacterID==id) OpenCloseCharacterSettings(id);
        loadedCharacters.Remove(GetCharacter(id));
        DataBaseHandler.DeleteCharacter(id.ToString());
        GD.Print($"Character with ID of: {id} should be deleted");
        //delete the database file for the character
        //delete the character from the characters table using the id


    }
    public void DeleteAnimation(int id)
    {
        GD.Print($"Animation with ID of: {id} should be deleted");
        //delete the animation from selected character and from the selected outfit
    }
    public void DeleteOutfit(int id)
    {
        GD.Print($"Outfit with ID of: {id} should be deleted");
    }
    public void SaveCharacter(int id)
    {
        GD.Print($"Save character with ID of: {id} this should save outfits,animations");

    }
    public void SaveScene()
    {
        //use openSceneID
        GD.Print($"Save scene with ID of: {openSceneID} ");
        currentSceneUI.selectSceneButton.Text = sceneNameLE.Text;
        DataBaseHandler.UpdateScene(openSceneID.ToString(), sceneNameLE.Text);
        openScene.SaveScene();
    }
    #endregion



    #region Online
    #endregion
}
