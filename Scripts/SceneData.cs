using Godot;
using System;

public class SceneData
{
    public int ID;
    public int characterID;
    public int outfitID;
    public float posX;
    public float posY;
    public float scale;
    public bool mirrored;
    public Character character;

    public SceneData(int iD, int characterID, int outfitID, float posX, float posY, float scale, bool mirrored)
    {
        ID = iD;
        this.characterID = characterID;
        this.outfitID = outfitID;
        this.posX = posX;
        this.posY = posY;
        this.scale = scale;
        this.mirrored = mirrored;
    }
    public bool IsSameCharacter(int id)
    {
        return id == ID;
    }

    public bool GetCharacterByPeerID(long peerID)
    {
        return peerID == character.peerId;
    }
}
