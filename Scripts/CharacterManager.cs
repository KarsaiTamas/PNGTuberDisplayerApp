using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Godot.Time;
using SLM = SaveLoadManager;
 
public partial class CharacterManager : Control
{
    public static CharacterManager instance;
    public static int IDLoaded;
    public Character characterInEdit;
    //public Dictionary<int, (Button outfitChangerB, TextureRect outfitLook)> availableOutfits;
    public Dictionary<int, AnimUI> availableAnimations;
    public Button addToSceneButton;
    public Button sendToOnlineFriend;
    public Button closeCharacterEditorButton;
    public OptionButton outfits;
    public SpinBox blinkFrequency;
    public HSlider noiseGateHS;
    private LineEdit CharacterNameLE;
    //private LineEdit outfitNameLE;
    //public Node Outfits;
    public bool isEdited=false;
    public int animClicked=0;
    int soundChannelPicked;
    public override void _EnterTree()
    {
        Init();
    }
    void Init()
    {
        instance = this;
        //availableOutfits =new Dictionary<int, (Button,TextureRect)>();
        availableAnimations=new Dictionary<int, AnimUI> ();
        outfits = GetNode<OptionButton>("HC/VC/HC/CharacterAnimations/SC/VC/Outfit/VC/OutfitsOB");
        characterInEdit = GetNode<Character>("HC/VC/HC/CharacterLook");
        closeCharacterEditorButton = GetNode<Button>("HC/VC/ButtonsPC/HC/CloseButton");
        CharacterNameLE = GetNode<LineEdit>("HC/VC/ButtonsPC/HC/CharacterNameLE");
        //outfitNameLE = GetNode<LineEdit>("HC/VC/HC/CharacterAnimations/SC/VC/Outfit/VC/OutifNameLE");
        addToSceneButton = GetNode<Button>("HC/VC/ButtonsPC/HC/AddToSceneButton");
        sendToOnlineFriend = GetNode<Button>("HC/VC/ButtonsPC/HC/SendToOnlineFriendButton");
        blinkFrequency = GetNode<SpinBox>("HC/VC/HC/CharacterAnimations/SC/VC/Eyes/VC/BlinkFrequencyHC2/SpinBox");
        noiseGateHS = GetNode<HSlider>("HC/VC/HC/CharacterAnimations/SC/VC/Mouth/VC/NoiseGateHC2/HSlider");
        closeCharacterEditorButton.ButtonDown += UIManager.instance.CloseCharacterEditor;
        addToSceneButton.ButtonDown += AddCharacterToScene;
        sendToOnlineFriend.ButtonDown += SendOnlineCharacter;
        soundChannelPicked = 0;
        /*AddOutfit(
            "OutfitButton",
            () => { GD.Print("Gimmi function"); });
        //Outfits = GetNode<Node>("HC/Outfits/SC/VC");
        */
        UIManager.instance.ToggleAddToSceneButton(false);
        AddAnimation(EBaseAnims.Base.ToString(),false, ()=> { UIManager.instance.FileDialogueShowForAnimation(EBaseAnims.Base.ToString()); });
        AddAnimation(EBaseAnims.Eyes.ToString(), false, ()=> { UIManager.instance.FileDialogueShowForAnimation(EBaseAnims.Eyes.ToString()); });
        AddAnimation(EBaseAnims.Mouth.ToString(), false, ()=> { UIManager.instance.FileDialogueShowForAnimation(EBaseAnims.Mouth.ToString()); });
        AddAnimation(EBaseAnims.Outfit.ToString(), false, ()=> { UIManager.instance.FileDialogueShowForAnimation(EBaseAnims.Outfit.ToString()); });
        var mouth = availableAnimations[(int)EBaseAnims.Mouth];
        mouth.keyActionOB.AddItem("Custom");
        mouth.keyActionOB.Select(0);
        mouth.keyActionOB.ItemSelected += SoundChannelPicked;
        
        foreach (var item in AudioManager.GetInputDevices())
        {
            mouth.keyActionOB.AddItem(item);
        }
        foreach (var item in AudioManager.GetOutputDevices())
        {
            mouth.keyActionOB.AddItem(item);

        }
        outfits.ItemSelected += OutfitSelected;

    }/*
    public void AddOutfit(string buttonName,Action outfitButtonAction)
    {
        availableOutfits.Add(availableOutfits.Count,
            (GetNode<Button>($"HC/Outfits/SC/VC/{buttonName}/Button"),
            GetNode<TextureRect>($"HC/Outfits/SC/VC/{buttonName}/Button/TextureRect")));
        availableOutfits[availableOutfits.Count-1].outfitChangerB.ButtonDown += outfitButtonAction;

    }
    */
    private void OutfitSelected(long index)
    {
        UIManager.instance.OutfitSwitching((int)index);
        if (index == 0) return;
        availableAnimations[(int)EBaseAnims.Outfit].animNameLE.Text = characterInEdit.data.outfits[(int)index].outfitName;
    }

    private void SoundChannelPicked(long index)
    {
        var mouth = characterInEdit.data.animData[(int)EBaseAnims.Mouth];
        mouth.activationType = (int)index;
        soundChannelPicked = (int)index;
 
    }

    public void AddAnimation(string animPanelName,bool isSpawned, Action outfitButtonAction)
    {
        availableAnimations.Add(availableAnimations.Count, 
            GetNode<AnimUI>($"HC/VC/HC/CharacterAnimations/SC/VC/{animPanelName}"));
        availableAnimations[availableAnimations.Count - 1].Init(isSpawned);
        availableAnimations[availableAnimations.Count - 1].animChangeB.ButtonDown += outfitButtonAction;
        int clicked = availableAnimations.Count - 1;
        availableAnimations[availableAnimations.Count - 1].animChangeB.ButtonDown += () => { animClicked = clicked; GD.Print(animClicked); };

    }
    public void PutUIDataToCharacterData()
    {
        characterInEdit.data.characterName= CharacterNameLE.Text;
        for (int i = 0; i < characterInEdit.data.animData.Count; i++)
        {
            characterInEdit.data.animData[i].animationCount=(int)availableAnimations[i].frameCountSB.Value;
            characterInEdit.data.animData[i].animSpeed=(float)availableAnimations[i].frameTimeSB.Value;
            if (availableAnimations[i].keyActionLE == null) continue;
            characterInEdit.data.animData[i].activation=availableAnimations[i].keyActionLE.Text;
        }
        characterInEdit.data.blinkFrequency= (float)blinkFrequency.Value ;
        for (int i = 1; i < outfits.ItemCount; i++)
        {
            characterInEdit.data.outfits[i].outfitName= outfits.GetItemText(i);
        }
            var mouth = characterInEdit.data.animData[(int)EBaseAnims.Mouth];
        mouth.activationType= availableAnimations[(int)EBaseAnims.Mouth].keyActionOB.Selected;
        mouth.activation= mouth.activationType == 0?
            availableAnimations[(int)EBaseAnims.Mouth].keyActionLE.Text:
            availableAnimations[(int)EBaseAnims.Mouth].keyActionOB.GetItemText(mouth.activationType);
        characterInEdit.data.talkingMinVolume=(float) noiseGateHS.Value;
        if(mouth.activation == null || mouth.activation == "")
        {
            mouth.activation = ProgramManager.VALUENOTSET;
        }
        characterInEdit.data.outfits[outfits.Selected].outfitName= availableAnimations[(int)(EBaseAnims.Outfit)].animNameLE.Text;
        characterInEdit.UpdateUIWithData();
    }
    public void PutCharacterDataIntoUI()
    {
        CharacterNameLE.Text = characterInEdit.data.characterName;
        DefaultOutfitsList();
        for (int i = 1; i < characterInEdit.data.outfits.Count; i++)
        {
            outfits.AddItem(characterInEdit.data.outfits[i].outfitName);
        }
        for (int i = 0; i < characterInEdit.data.animData.Count; i++)
        {
            availableAnimations[i].frameCountSB.Value= characterInEdit.data.animData[i].animationCount;
            availableAnimations[i].frameTimeSB.Value = characterInEdit.data.animData[i].animSpeed;
            if (availableAnimations[i].keyActionLE == null) continue;
            availableAnimations[i].keyActionLE.Text = characterInEdit.data.animData[i].activation;
        }
        blinkFrequency.Value=characterInEdit.data.blinkFrequency;
        var mouth = characterInEdit.data.animData[(int)EBaseAnims.Mouth];
        soundChannelPicked = availableAnimations[(int)EBaseAnims.Mouth].keyActionOB.Selected= mouth.activationType;
        if (soundChannelPicked == 0)
        {
            availableAnimations[(int)EBaseAnims.Mouth].keyActionLE.Text= mouth.activation;
        }
         availableAnimations[(int)(EBaseAnims.Outfit)].animNameLE.Text= characterInEdit.data.outfits[outfits.Selected].outfitName;

        noiseGateHS.Value = characterInEdit.data.talkingMinVolume;
    }
    public void AddCharacterToScene()
    {
        var chara=SpawnManager.SpawnCharacterToScene(IDLoaded);

        SceneManager.instance.data.charactersSceneData.Add(chara.data);
        chara.LoadCharacterData(IDLoaded);
        chara.UpdateUIWithData();
        
        SceneManager.instance.isEdited = true;   

    }

    public void UpdateAnim(int clicked,string file)
    {
        characterInEdit.data.animData[clicked].filePath= file;
        availableAnimations[clicked].animLookTR.Texture= SLM.GetCharacterAnim(file);
        isEdited = true;
    }
    void DefaultOutfitsList()
    {
        outfits.Clear();
        outfits.AddItem(ProgramManager.VALUENOTSET);
    }
    internal void CreateOutfit()
    {
        int count = outfits.ItemCount;
        outfits.AddItem("outfit "+ count);
        characterInEdit.data.outfits.Add(new OutfitData("outfit "+count));
        isEdited = true;
    }

    public void CreateCharacter()
    {
        string characterName = "Character_" + SLM.DateTimeForSave();
        GD.Print("Creating character");
        var ID = SLM.DateTimeForSave();

        SLM.charactersToLoad.Add(new SavedCharacters(ID, SLM.SAVEDCHARACTERSLOCATION + characterName));
        SLM.Save(SLM.charactersToLoad, SLM.SAVEDCHARACTERSFILE, SLM.SAVEDCHARACTERSLOCATION);
        SLM.Save(new CharacterData(ID, characterName), characterName, SLM.SAVEDCHARACTERSLOCATION);
        UIManager.instance.AddCharacterToCharacterOptions(characterName);

    }
    public void SaveCharacter()
    {
        isEdited = false;
        PutUIDataToCharacterData();
        if (!SLM.RenamePathInList(SLM.charactersToLoad,
            IDLoaded,
            characterInEdit.data.characterName,
            SLM.SAVEDCHARACTERSFILE,
            SLM.SAVEDCHARACTERSLOCATION))
        {
            ConfirmUI.Instance.ShowConfirm("Failed to save character.");
            return;
        }
        UIManager.instance.RenameCharacterInPopupMenu(
            IDLoaded,
            characterInEdit.data.characterName);

        SLM.Save(characterInEdit.data,
            characterInEdit.data.characterName,
            SLM.SAVEDCHARACTERSLOCATION);
    }
    public void LoadCharacter(long id)
    {

        UIManager.instance.CharacterEditorVisibility(true);
        IDLoaded = (int)id - 1;
        characterInEdit.data = SLM.Load<CharacterData>(SLM.charactersToLoad[IDLoaded].filePath);
        for (int i=0; i< characterInEdit.data.animData.Count; i++)
        {
            var anim= characterInEdit.data.animData[i];
            if (anim.filePath.Equals(ProgramManager.VALUENOTSET)) continue;
            UpdateAnim(i,anim.filePath);
        }
        isEdited = false;

        characterInEdit.LoadCharacterData(CharacterManager.IDLoaded);
        characterInEdit.UpdateUIWithData();
        PutCharacterDataIntoUI();
    }
    public void SendOnlineCharacter()
    {
        var character = NetworkManager.instance.joinedPlayers[Multiplayer.GetUniqueId()];
        character.LoadCharacterData(IDLoaded);
        character.UpdateUIWithData();
        character.AddAudioMonitorion();
        foreach (var item in NetworkManager.instance.joinedPlayers)
        {
            if (Multiplayer.MultiplayerPeer.GetUniqueId() == item.Key) continue;
            NetworkManager.instance.SendImageDataInPieces(
                SLM.AnimsToByte(character),
                item.Key,
                Multiplayer.MultiplayerPeer.GetUniqueId());
            NetworkManager.instance.SendDataToPeer(
                SLM.CharacterDataToBytes(characterInEdit.data),
                item.Key, 
                Multiplayer.MultiplayerPeer.GetUniqueId());
 
        }
    }
    public void DeleteCharacter()
    {
        if (!SLM.DeleteFile(SLM.charactersToLoad[IDLoaded].filePath)) return;
        SLM.charactersToLoad.RemoveAt(IDLoaded);

        UIManager.instance.RemoveCharacterFromPopupMenu(IDLoaded);
        SLM.Save(SLM.charactersToLoad, SLM.SAVEDCHARACTERSFILE, SLM.SAVEDCHARACTERSLOCATION);
        UIManager.instance.CloseCharacterEditor();

    }
    public void ToggleOutfitUI(bool visible)
    {
        availableAnimations[(int)EBaseAnims.Outfit].animNameLE.Visible = visible;
        availableAnimations[(int)EBaseAnims.Outfit].animChangeB.Visible = visible;

    }
    public void DeleteOutfit()
    {
        GD.Print("Deleting outfit");
        GD.Print(characterInEdit.data.outfits.Count);
        characterInEdit.data.outfits.RemoveAt(outfits.Selected);
        outfits.RemoveItem(outfits.Selected);
        GD.Print(characterInEdit.data.outfits.Count);
        UIManager.instance.OutfitSwitching(0);
        isEdited = true;
    }

    internal void UpdateOutfit(string file)
    {
        characterInEdit.data.outfits[outfits.Selected].outfitFilePath= file;
        availableAnimations[(int)EBaseAnims.Outfit].animLookTR.Texture = SLM.GetCharacterAnim(file);
        isEdited = true;
    }
    public void LoadOutfitToUI()
    { 
        if (outfits.Selected == 0 || outfits.Selected == -1) return;
        availableAnimations[(int)EBaseAnims.Outfit].animLookTR.Texture = SLM.GetCharacterAnim(characterInEdit.data.outfits[outfits.Selected].outfitFilePath);

    }
}
