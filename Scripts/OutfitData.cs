using Godot;
using System;

public class OutfitData 
{
    public string outfitName {  get; set; }
    public string outfitFilePath {  get; set; }

    public OutfitData()
    {
        this.outfitName = ProgramManager.VALUENOTSET;
        this.outfitFilePath = ProgramManager.VALUENOTSET;
    }

    public OutfitData(string name)
    {
        this.outfitName = name;
        this.outfitFilePath = ProgramManager.VALUENOTSET;
    }

    public OutfitData(string outfitName, string outfitFilePath) 
    {
        this.outfitName = outfitName;
        this.outfitFilePath = outfitFilePath;
    }
}
