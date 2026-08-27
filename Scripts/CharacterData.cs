using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public enum EBaseAnims
{
    Base,
    Eyes,
    Mouth,
    Outfit,
}
public class CharacterData
{
    public string ID {  get; set; }
    public string characterName { get; set; }
    public float positionX { get; set; }
    public float positionY { get; set; }
    public float sizeX {  get; set; }
    public float sizeY { get; set; }
    [JsonConverter(typeof(IntFromDoubleConverter))]
    public int layer {  get; set; }
    public bool mirrored {  get; set; }
    public float blinkFrequency {  get; set; }
    public float talkingMinVolume {  get; set; }
    [JsonConverter(typeof(IntFromDoubleConverter))]
    public int selectedOutfit {  get; set; }
    [JsonConverter(typeof(IntFromDoubleConverter))]
    public int selectedSoundChannel { get; set; }
    public List<AnimData> animData {  get; set; }
    public List<OutfitData> outfits { get; set; }

     

    public CharacterData(string ID, string characterName)
    {
        this.characterName = characterName;
        this.ID = ID;
        DefaultValues();
    }
    public CharacterData(string ID)
    {
        characterName = "default";
        this.ID = ID;
        DefaultValues();
    }
    public CharacterData()
    {
        characterName = "default";
        this.ID = "0";
        DefaultValues();
    }
    void DefaultValues()
    {

        position = new Vector2(0, 0);
        size = new Vector2(512, 512);
        layer = 1;
        mirrored = false;
        talkingMinVolume = 0;
        blinkFrequency = 2;
        selectedOutfit = 0;
        animData = new List<AnimData>();
        outfits = new List<OutfitData>();
        outfits.Add(new OutfitData( "No outfit"));
        animData.Add(new AnimData());
        animData.Add(new AnimData());
        animData.Add(new AnimData());
    }

    [JsonIgnore]
    public Vector2 position
    {
        get => new Vector2((float)positionX, (float)positionY);
        set { positionX = value.X;positionY = value.Y; }
    }
    [JsonIgnore]
    public Vector2 size
    {
        get => new Vector2((float)sizeX, (float)sizeY);
        set { sizeX = value.X; sizeY = value.Y; }
    }
    public void GiveSceneData(CharacterData cData)
    {
        cData.position = position;
        cData.size = size;
        cData.layer = layer;
        cData.mirrored = mirrored;
        cData.selectedOutfit = selectedOutfit;
    }
    public void GiveCharacterData(CharacterData cData)
    {
        cData.blinkFrequency = blinkFrequency;
        cData.animData = animData;
        cData.outfits = outfits;
        cData.characterName = characterName;
        cData.talkingMinVolume = talkingMinVolume;
        cData.ID = ID;
    }
}
