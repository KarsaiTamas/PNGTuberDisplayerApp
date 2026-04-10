using Godot;
using Godot.Collections;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class DataHandler
{

    public static int GetCharacterType(Dictionary character)
    {
		try
		{
			if(character == null)return 0;
			return character[DataBaseHandler.cCharacterType].AsInt32();

        }
		catch (Exception)
		{
			GD.Print("Something went wrong while loading the character type using default instead");
			return 0;
		}
    }
	public static void LoadOutfitsToUI()
	{
        var caUI= ProgramHandler.instance.characterAnimationsPanel;
        caUI.data.outfitsUI.Clear();
        caUI.AddDefaultOutfit();
        var outfits = DataBaseHandler.GetOutfits(caUI.data.characterID);
        var outfit = outfits[0].AsGodotDictionary();
        
        caUI.data.outfitsUI[0].SetupOutfit(caUI.data.characterID, outfit["id"].AsInt32());
        caUI.data.simpleOutfitLocation = (string)outfit[DataBaseHandler.outfitLocation];
        caUI.simpleOutfitTextureRect.Texture = FileLoaderHandler.GetCharacterAnim(caUI.data.simpleOutfitLocation);
        for (int i = 1; i < outfits.Count; i++)
        {

            outfit = outfits[i].AsGodotDictionary();
            var outFitUI = (OutfitButtonUI)SpawnHandler.Spawn(SpawnableScenes.OutfitButton, caUI.outfitsHolder);
            outFitUI.SetupOutfit(caUI.data.characterID, outfit["id"].AsInt32());
            caUI.data.outfitsUI.Add(outFitUI);
            var anims = DataBaseHandler.GetCharacterAnimations(caUI.data.characterID, outfit["id"].AsInt32());
            outFitUI.ChangeButtonLook(outfit[DataBaseHandler.oIsSimple].AsBool() ?
                outfit[DataBaseHandler.outfitLocation].AsString() :
                anims[0].AsGodotDictionary()[DataBaseHandler.oSpriteFile].AsString());
        }
        caUI.data.outfitsUI[0].LoadOutfit();
    }

    public static void LoadAnimationsToUI()
    {
        var caUI= ProgramHandler.instance.characterAnimationsPanel;
        var anims = DataBaseHandler.GetCharacterAnimations(caUI.data.characterID, caUI.data.selectedOutfit);
        if (anims == null) return;
        for (int i = 3; i < anims.Count; i++)
        {
            AnimUI animUI = (AnimUI)SpawnHandler.Spawn(SpawnableScenes.CharacterAnimUI, caUI.animationsHolder);
            caUI.data.anims.Add(animUI);
        }
        caUI.simpleOutfitTextureRect.Texture = FileLoaderHandler.GetCharacterAnim(caUI.data.simpleOutfitLocation);
        for (int i = 0; i < anims.Count; i++)
        {
            var dbData = anims[i].AsGodotDictionary();
            GD.Print(dbData);
            caUI.data.anims[i].data =
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
                bool deviceSelected = false;
                for (int j = 0; j < caUI.devices.Count; j++)
                {
                    if (caUI.devices[j].Equals(dbData[DataBaseHandler.oAnimExtraInfo].AsString()))
                    {
                        caUI.currentCharacterTalkButton.Select(j);
                        deviceSelected = true;
                        break;
                    }
                }
                if (!deviceSelected)
                {
                    caUI.currentCharacterTalkButton.Select(caUI.devices.Count); ;
                    caUI.specificProgramLineEdit.Text = dbData[DataBaseHandler.oAnimExtraInfo].AsString();
                }
            }
            caUI.data.anims[i].LoadDataIntoTheUI();
        }
    }
    public static void LoadOutfitToUI(int outfitToLoad)
    {
        var caUI= ProgramHandler.instance.characterAnimationsPanel;
        caUI.data.selectedOutfit = outfitToLoad;
        if (outfitToLoad == 1) caUI.removeOutfitButton.Hide();
        else caUI.removeOutfitButton.Show();
        var cData = DataBaseHandler.GetCharacterInSaves(ProgramHandler.selectedCharacterID.ToString());
        var oData = DataBaseHandler.GetOutfit(ProgramHandler.selectedCharacterID, outfitToLoad);
        GD.Print(ProgramHandler.selectedCharacterID.ToString());
        caUI.characterName.Text = cData == null ? "Default name" : cData["name"].AsString();
        caUI.outfitName.Text = oData == null ? "Default outfit" : oData["name"].AsString();
        caUI.data.simpleOutfitLocation = oData == null ? "No outfit here" : (string)oData[DataBaseHandler.outfitLocation];
        caUI.SetSimpleOutfit(oData == null ? false : oData[DataBaseHandler.oIsSimple].AsBool());
        caUI.ChangeAnimations();
        GD.Print(oData == null ? "null data outfit" : "not null data");
        caUI.simpleOutfit.SetupSimpleAnim(outfitToLoad);
        caUI.simpleOutfitToggle.ButtonPressed = oData == null ? false : oData[DataBaseHandler.oIsSimple].AsBool();
        caUI.characterTypeButton.Select(GetCharacterType(cData));
        caUI.simpleOutfitToggle.Visible = outfitToLoad != 1;
        caUI.data.characterType = (CharacterType) GetCharacterType(cData);
    }

    public static void SaveOutfit()
    {
        var caUI = ProgramHandler.instance.characterAnimationsPanel;
        caUI.PutUIOutfitDataIntoData();
        DataBaseHandler.UpdateCharacter(caUI.data.characterID.ToString(), caUI.data.characterName, (int)caUI.data.characterType);
        var cData = ProgramHandler.instance.GetCharacter(caUI.data.characterID);
        cData.selectCharacterButton.Text = caUI.data.characterName;
        var oData = caUI.data.GetOutfitByID(caUI.data.selectedOutfit);
        oData.ChangeToThisOutFitButton.TooltipText = caUI.data.outfitName;

        DataBaseHandler.UpdateOutfitInCharacter(
            caUI.data.selectedOutfit,
            caUI.data.characterID,
            caUI.data.outfitName,
            caUI.data.isSimple,
            caUI.simpleOutfit.data.spriteLoc);
        foreach (var anim in caUI.data.anims)
        {
            anim.PutUIDataIntoData();
            if (anim.Name.Equals("TalkAnim"))
            {
                if (caUI.currentCharacterTalkButton.GetItemText(caUI.currentCharacterTalkButton.Selected).Equals("CUSTOM"))
                {
                    anim.data.extraAnimInfo = caUI.specificProgramLineEdit.Text;
                }
                else
                {
                    anim.data.extraAnimInfo = caUI.currentCharacterTalkButton.GetItemText(caUI.currentCharacterTalkButton.Selected);

                }
            }
            DataBaseHandler.UpdateAnimationInCharacter(
                anim.data.animID,
                caUI.data.selectedOutfit,
                caUI.data.characterID,
                anim.data.animName,
                anim.data.spriteLoc,
                anim.data.animLenght,
                anim.data.animCount,
                (int)anim.data.animType,
                anim.data.extraAnimInfo);
        }
        oData.ChangeButtonLook(caUI.data.anims[0].data.spriteLoc);

    }

}
