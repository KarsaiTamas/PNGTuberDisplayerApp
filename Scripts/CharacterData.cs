using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterData  
{
    public int characterID;
    public int selectedOutfit;
    public bool isSimple;
    public string simpleOutfitLocation;
    public string outfitName;
    public string characterName;
    public CharacterType characterType;
    public List<AnimUI> anims;
    public List<OutfitButtonUI> outfitsUI;

    public CharacterData(int characterID, int selectedOutfit)
    {
        this.characterID = characterID;
        this.selectedOutfit = selectedOutfit;
        this.anims = new List<AnimUI>();
        this.outfitsUI = new List<OutfitButtonUI>();
        characterType = CharacterType.character_png;
    }
    public OutfitButtonUI GetOutfitByID(int id)
    {
        var outfit = outfitsUI.Where(e => e.SameOutfit(id)).ToList();
        if (outfit.Count == 0) return null;
        return outfit[0];
    }
    public AnimUI GetAnimByID(int id)
    {
        var outfit = anims.Where(e => e.data.animID==id).ToList();
        if (outfit.Count == 0) return null;
        return outfit[0];
    }
}
