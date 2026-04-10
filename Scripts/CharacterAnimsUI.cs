using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class CharacterAnimsUI : Control
{
    public CharacterData data;
    public OptionButton currentCharacterTalkButton;
    public Button AddNewAnimButton;
    public Button AddNewOutfitButton;
    public Button removeOutfitButton;
    public Button saveOutfitButton;
    public OptionButton characterTypeButton;
    public Node animationsHolder;
    public SimpleAnimUI simpleOutfit;
    Control complexOutfit;
    public Node outfitsHolder;
    public LineEdit characterName;
    public LineEdit outfitName;
    public Button simpleOutfitButton;
    public CheckButton simpleOutfitToggle;
    public TextureRect simpleOutfitTextureRect;
    public Button closeButton;
    public LineEdit specificProgramLineEdit;
    public List<string> devices;
    public override void _EnterTree()
    {
        animationsHolder = GetNode<Node>("HC/VCAnims/VCAnimations/PBaseAnim/SC/VC");
        saveOutfitButton = GetNode<Button>("HC/VCAnims/VCAnimations/MCSaveOutfit/Button");
        removeOutfitButton = GetNode<Button>("HC/VCAnims/HC/Remove Outfit/Button");
        AddNewAnimButton = GetNode<Button>("HC/VCAnims/VCAnimations/MCAddNewAnim/Button");
        AddNewOutfitButton = GetNode<Button>("HC/Outfits/VC/Add Outfit/Button");
        simpleOutfitButton = GetNode<Button>("HC/VCAnims/VCAnimations/SimpleOutfit/MCAnimChange/Button");
        simpleOutfitTextureRect = GetNode<TextureRect>("HC/VCAnims/VCAnimations/SimpleOutfit/MCAnimChange/Button/TextureRect");
        simpleOutfit = GetNode<SimpleAnimUI>("HC/VCAnims/VCAnimations/SimpleOutfit");
        complexOutfit = GetNode<Control>("HC/VCAnims/VCAnimations/PBaseAnim");
        simpleOutfitToggle = GetNode<CheckButton>("HC/VCAnims/VCAnimations/IsSimple/CheckButton");
        characterTypeButton=GetNode<OptionButton>("HC/VCAnims/VCAnimations/CharacterType/OptionButton");
        closeButton=GetNode<Button>("HC/VCAnims/HC/Closer/Button");
        //HC/VCAnims/VCAnimations/IsSimple/CheckButton
        //HC/VCAnims/VCAnimations/SimpleOutfit/MCAnimChange/Button
        //HC/VCAnims/CharacterName/LineEdit
        characterName = GetNode<LineEdit>("HC/VCAnims/CharacterName/LineEdit");
        GD.Print(characterName);
        outfitName = GetNode<LineEdit>("HC/VCAnims/OutfitName/LineEdit");
        outfitsHolder = GetNode<Node>("HC/Outfits/VC/OutfitsHolder/SC/VC");
        //HC/Outfits/VC/Add Outfit/Button
        //HC/VCAnims/VCAnimations/PBaseAnim/SC/VC/TalkAnim/VC/SpecificProgramSound/LineEdit
        specificProgramLineEdit = GetNode<LineEdit>("HC/VCAnims/VCAnimations/PBaseAnim/SC/VC/TalkAnim/VC/SpecificProgramSound/LineEdit");
        currentCharacterTalkButton = GetNode<OptionButton>("HC/VCAnims/VCAnimations/PBaseAnim/SC/VC/TalkAnim/VC/SoundInput/OptionButton");
        devices = AudioServer.GetInputDeviceList().ToList();
        foreach (var item in devices)
        {
            currentCharacterTalkButton.AddItem(item);

        } 
        currentCharacterTalkButton.AddItem("CUSTOM", -5);
        
    }

    private void TalkOutputToSelect(long index)
    {
        if (currentCharacterTalkButton.GetItemId((int)index)==-5)
        {
            specificProgramLineEdit.Show();
        }
        else
        {
            specificProgramLineEdit.Hide();
        } 
    }

    public override void _Ready()
    {
        data = new CharacterData(ProgramHandler.selectedCharacterID,1);
        ChangeOutfit(1);
        Hide();
        simpleOutfitToggle.Toggled += SetSimpleOutfit;
        saveOutfitButton.ButtonUp += SaveOutfit;
        removeOutfitButton.ButtonDown += DeleteOutfit;
        AddNewAnimButton.ButtonUp += AddNewAnimation;
        AddNewOutfitButton.ButtonUp += AddNewOutFit;
        simpleOutfitButton.ButtonUp += SetSimpleOutFit;
        closeButton.ButtonUp += CloseCharacterAnims;
        currentCharacterTalkButton.ItemSelected += TalkOutputToSelect;
        characterTypeButton.ItemSelected += CharacterTypeChanged;
    }

    private void CharacterTypeChanged(long index)
    {
        data.characterType=(CharacterType)index;
    }

    public void ChangeOutfit(int newOutfit)
    {
        DataHandler.LoadOutfitToUI(newOutfit);
        /*data.selectedOutfit = newOutfit;
        if (newOutfit == 1) removeOutfitButton.Hide();
        else removeOutfitButton.Show();
        var cData=DataBaseHandler.GetCharacterInSaves(ProgramHandler.selectedCharacterID.ToString());
        var oData = DataBaseHandler.GetOutfit(ProgramHandler.selectedCharacterID, newOutfit);
        GD.Print(ProgramHandler.selectedCharacterID.ToString());
        characterName.Text= cData==null?"Default name":cData["name"].AsString();
        outfitName.Text=oData==null?"Default outfit" : oData["name"].AsString();
        data.simpleOutfitLocation = oData == null ? "No outfit here" :(string)oData[DataBaseHandler.outfitLocation];
        SetSimpleOutfit(oData==null?false:oData[DataBaseHandler.oIsSimple].AsBool());
        ChangeAnimations();
        GD.Print(oData == null ? "null data outfit" : "not null data");
        simpleOutfit.SetupSimpleAnim(newOutfit);
        simpleOutfitToggle.ButtonPressed=oData == null ? false : oData[DataBaseHandler.oIsSimple].AsBool();
        
        simpleOutfitToggle.Visible = newOutfit != 1;
        */
    }
    public void ChangeAnimations()
    {
        for (int i = 3; i < data.anims.Count; i++)
        {
            data.anims[i].quedForDeletion = true;
            data.anims[i].QueueFree();
        }
        data.anims.Clear();
        AddExistingAnims();
        LoadAnims();
    }
    void AddExistingAnims()
    {
        foreach (AnimUI item in animationsHolder.GetChildren())
        { 
            if(!item.quedForDeletion)
            data.anims.Add(item);
        }
    }
    public void RemoveOutfits()
    {
        for(int i = 1; i < data.outfitsUI.Count; i++)
        {
            data.outfitsUI[i].QueueFree();
        }
        data.outfitsUI.Clear();
        AddDefaultOutfit();
    }
    public void AddDefaultOutfit()
    {
        data.outfitsUI.Add((OutfitButtonUI)outfitsHolder.GetChild(0));
    }
    void SaveOutfit()
    {
        PutUIOutfitDataIntoData();
        DataHandler.SaveOutfit();
        /*
        DataBaseHandler.UpdateCharacter(data.characterID.ToString(), data.characterName, data.characterType.ToString());
        var cData= ProgramHandler.instance.GetCharacter(data.characterID);
        cData.selectCharacterButton.Text = data.characterName;
        var oData=data.GetOutfitByID(data.selectedOutfit);
        oData.ChangeToThisOutFitButton.TooltipText = data.outfitName;

        DataBaseHandler.UpdateOutfitInCharacter(
            data.selectedOutfit,
            data.characterID, 
            data.outfitName, 
            data.isSimple, 
            simpleOutfit.data.spriteLoc);
        foreach (var anim in data.anims)
        {
            anim.PutUIDataIntoData();
            if (anim.Name.Equals("TalkAnim"))
            {
                if (currentCharacterTalkButton.GetItemText(currentCharacterTalkButton.Selected).Equals("CUSTOM"))
                {
                    anim.data.extraAnimInfo = specificProgramLineEdit.Text;
                }
                else
                {
                    anim.data.extraAnimInfo = currentCharacterTalkButton.GetItemText(currentCharacterTalkButton.Selected);
                   
                }
            }
            DataBaseHandler.UpdateAnimationInCharacter(
                anim.data.animID,
                data.selectedOutfit,
                data.characterID,
                anim.data.animName,
                anim.data.spriteLoc,
                anim.data.animLenght,
                anim.data.animCount,
                (int)anim.data.animType,
                anim.data.extraAnimInfo);
        }
        oData.ChangeButtonLook(data.anims[0].data.spriteLoc);
        */
    }
    public void DeleteOutfit()
    {
        ConfirmUI.Instance.SetConfirm($"Would you like to delete: {data.outfitName}?",
            () =>
            {
                DataBaseHandler.DeleteOutfit(data.characterID,data.selectedOutfit);
                var oData= data.GetOutfitByID(data.selectedOutfit);
                data.outfitsUI.Remove(oData);
                oData.QueueFree();
                ChangeOutfit(1);
            });
    }
    public void DeleteAnimation(int id)
    {
        DataBaseHandler.DeleteAnimation(data.characterID, id);
        var aData = data.GetAnimByID(id);
        data.anims.Remove(aData);
        aData.QueueFree();
    
    }
    void AddNewAnimation()
    {
        AnimUI animUI= (AnimUI)SpawnHandler.Spawn(SpawnableScenes.CharacterAnimUI, animationsHolder);
        int id = DataBaseHandler.GetNextIDForTable(DataBaseHandler.Table.outfit_animations,data.characterID);
        animUI.data = new AnimData(
            id,
            data.selectedOutfit);
        data.anims.Add(animUI);
        DataBaseHandler.InsertAnimationIntoCharacter(id,
            data.selectedOutfit,
            data.characterID,
            animUI.data.animName,
            animUI.data.spriteLoc,
            animUI.data.animLenght,
            animUI.data.animCount, 
            (int)animUI.data.animType,
            animUI.data.extraAnimInfo);

    }

    void AddNewOutFit()
    {
        var outFitUI = (OutfitButtonUI)SpawnHandler.Spawn(SpawnableScenes.OutfitButton, outfitsHolder);
        data.outfitsUI.Add(outFitUI);

        var oID = DataBaseHandler.GetNextIDForTable(DataBaseHandler.Table.outfits, data.characterID);
        DataBaseHandler.InsertOutfitIntoCharacter(oID, data.characterID, $"outfit{oID}", true, "default");
        outFitUI.SetupOutfit(data.characterID,oID );
        var aID = DataBaseHandler.GetNextIDForTable(DataBaseHandler.Table.outfit_animations, data.characterID);
        outFitUI.SetupOutfit(oID, data.characterID);
        ProgramHandler.instance.AddAnimation(data.characterID, oID, "Idle", AnimType.other, aID);
        ProgramHandler.instance.AddAnimation(data.characterID, oID, "Talk", AnimType.other, aID+1);
        ProgramHandler.instance.AddAnimation(data.characterID, oID, "Blink", AnimType.other, aID+2);
        //DataBaseHandler.
        GD.Print(oID);
    }

    private void SetSimpleOutFit()
    {


    }

    public void LoadOutfits()
    {
        DataHandler.LoadOutfitsToUI();
        /*
         * data.outfitsUI.Clear();
        AddDefaultOutfit();
        var outfits= DataBaseHandler.GetOutfits(data.characterID);
        var outfit = outfits[0].AsGodotDictionary();
        characterTypeButton.Select(DataLoader.LoadCharacterType(data.characterID));
        data.outfitsUI[0].SetupOutfit(data.characterID, outfit["id"].AsInt32());
        data.simpleOutfitLocation = (string)outfit[DataBaseHandler.outfitLocation];
        simpleOutfitTextureRect.Texture= FileLoaderHandler.GetCharacterAnim(data.simpleOutfitLocation);
        for (int i = 1; i < outfits.Count; i++)
        {

            outfit = outfits[i].AsGodotDictionary();
            var outFitUI = (OutfitButtonUI)SpawnHandler.Spawn(SpawnableScenes.OutfitButton, outfitsHolder);
            outFitUI.SetupOutfit(data.characterID, outfit["id"].AsInt32());
            data.outfitsUI.Add(outFitUI);
            var anims= DataBaseHandler.GetCharacterAnimations(data.characterID, outfit["id"].AsInt32());
            outFitUI.ChangeButtonLook(outfit[DataBaseHandler.oIsSimple].AsBool()?
                outfit[DataBaseHandler.outfitLocation].AsString():
                anims[0].AsGodotDictionary()[DataBaseHandler.oSpriteFile].AsString());
        }
        data.outfitsUI[0].LoadOutfit();
        */
    }
    void LoadAnims()
    {
        DataHandler.LoadAnimationsToUI();
        /*
        var anims= DataBaseHandler.GetCharacterAnimations(data.characterID, data.selectedOutfit);
        if (anims == null) return;
        for (int i = 3; i < anims.Count; i++)
        {
            AnimUI animUI = (AnimUI)SpawnHandler.Spawn(SpawnableScenes.CharacterAnimUI, animationsHolder);
            data.anims.Add(animUI);
        }
        simpleOutfitTextureRect.Texture = FileLoaderHandler.GetCharacterAnim(data.simpleOutfitLocation);
        for (int i = 0; i < anims.Count; i++)
        {
            var dbData=anims[i].AsGodotDictionary();
            GD.Print(dbData);
            data.anims[i].data=
                new AnimData(dbData["id"].AsInt32(),
                dbData[DataBaseHandler.oOutfitID].AsInt32(),
                dbData["name"].AsString(),
                dbData[DataBaseHandler.oSpriteFile].AsString(),
                (float)dbData[DataBaseHandler.oAnimLength].AsDouble(),
                dbData[DataBaseHandler.oAnimCount].AsInt32(),
                dbData[DataBaseHandler.oAnimType].AsInt32(),
                dbData[DataBaseHandler.oAnimExtraInfo].AsString()); 
            if (dbData["name"].AsString().Equals("Talk"))
            {
                bool deviceSelected=false;
                for (int j = 0; j < devices.Count; j++)
                { 
                    if (devices[j].Equals(dbData[DataBaseHandler.oAnimExtraInfo].AsString()))
                    { 
                        currentCharacterTalkButton.Select(j);
                        deviceSelected=true;
                        break;
                    }
                }
                if (!deviceSelected)
                {
                    currentCharacterTalkButton.Select(devices.Count); ;
                    specificProgramLineEdit.Text = dbData[DataBaseHandler.oAnimExtraInfo].AsString();
                }
            }
            data.anims[i].LoadDataIntoTheUI();
        }
        */
    }

    public void SetSimpleOutfit(bool isSimple)
    {
        if (isSimple)
        {
            simpleOutfit.Show();
            complexOutfit.Hide();
            AddNewAnimButton.Hide();
        }
        else
        {

            simpleOutfit.Hide();
            complexOutfit.Show();
            AddNewAnimButton.Show();
        }
        data.isSimple = isSimple;
    }

    public void PutUIOutfitDataIntoData()
    {
        data.outfitName= outfitName.Text;
        data.characterName= characterName.Text;
    }
    void CloseCharacterAnims()
    {
        this.Hide();
        ProgramHandler.selectedCharacterID = -1;
    }
}
