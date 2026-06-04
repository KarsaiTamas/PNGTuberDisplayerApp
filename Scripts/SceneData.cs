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
    public string nodeName;
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
    public SceneData(int iD) 
    {
        ID = iD;
        this.outfitID = -1;
        this.characterID = -1;
        this.posX = 0;
        this.posY = 0;
        this.scale = 1;
        this.mirrored = false;
    }
    public SceneData(int iD, string nodeName)
    {
        ID = iD;
        this.outfitID = -1;
        this.characterID = -1;
        this.posX = 0;
        this.posY = 0;
        this.scale = 1;
        this.mirrored = false;
        this.nodeName = nodeName;
    }

    public SceneData(int iD, string nodeName,Character chara)
    {
        ID = iD;
        this.outfitID = -1;
        this.characterID = -1;
        this.posX = 0;
        this.posY = 0;
        this.scale = 1;
        this.mirrored = false;
        this.nodeName = nodeName;
        character = chara;
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
