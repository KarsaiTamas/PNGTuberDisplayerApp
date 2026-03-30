using Godot;
using Godot.Collections; 

public partial class OutfitButtonUI : MarginContainer
{
    public int cID=1;
    public int outfitID=1;
    public string outfitName;
    public Button ChangeToThisOutFitButton;
    public TextureRect baseOutfitLook;
    public CharacterData dataToLoad;
    public override void _EnterTree()
    {
        ChangeToThisOutFitButton = GetNode<Button>("Button");
        baseOutfitLook = GetNode<TextureRect>("Button/TextureRect");

        ChangeToThisOutFitButton.ButtonUp += LoadOutfit;
    } 

    public void SetupOutfit(int characterID,int newOutfitID)
    {
        outfitID = newOutfitID;
        cID = characterID;
        dataToLoad = new CharacterData(cID, outfitID);
        //LoadOutfit();
    }

    public void ChangeButtonLook(string location)
    {
        baseOutfitLook.Texture = FileLoaderHandler.GetCharacterAnim(location);

    }
    public void LoadOutfit()
    {
        GD.Print($"Loading outfit {outfitID}");
        ProgramHandler.instance.characterAnimationsPanel.ChangeOutfit(outfitID);
        //ProgramHandler.selectedCharacterID,outfitID
        var anims= DataBaseHandler.GetCharacterAnimations(dataToLoad.characterID, outfitID);
        var outfitData = DataBaseHandler.GetOutfit(dataToLoad.characterID, outfitID);
        GD.Print(outfitData);
        GD.Print(anims);
        if (anims == null) return;
        if (anims.Count == 0) return;
        if(outfitData==null) return;
        dataToLoad.selectedOutfit = outfitID;
        outfitName= outfitData["name"].AsString();
        ChangeToThisOutFitButton.TooltipText = outfitName;
        dataToLoad.outfitName = outfitName;
        dataToLoad.isSimple = outfitData[DataBaseHandler.oIsSimple].AsBool();
        dataToLoad.simpleOutfitLocation = outfitData[DataBaseHandler.outfitLocation].AsString();
        baseOutfitLook.Texture = dataToLoad.isSimple ?
        FileLoaderHandler.GetCharacterAnim(dataToLoad.simpleOutfitLocation) :
        FileLoaderHandler.GetCharacterAnim(anims[0].AsGodotDictionary()[DataBaseHandler.oSpriteFile].AsString());

        foreach (Dictionary item in anims)
        {
            dataToLoad.anims.Add(
            new AnimUI(
            new AnimData(
                item["id"].AsInt32(),
                item[DataBaseHandler.oOutfitID].AsInt32(),
                item["name"].AsString(),
                item[DataBaseHandler.oSpriteFile].AsString(),
                (float)item[DataBaseHandler.oAnimLength].AsDouble(),
                item[DataBaseHandler.oAnimCount].AsInt32(),
                item[DataBaseHandler.oAnimType].AsInt32(),
                item[DataBaseHandler.oAnimExtraInfo].AsString()
            )));
        }
        GD.Print(dataToLoad);
    }
    public bool SameOutfit(int id)
    {
        return outfitID== id;
    }
}
