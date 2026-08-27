using Godot;
using System;

public partial class SavedCharacters 
{
    public string ID {  get; set; }
    public string filePath {  get; set; }

    public SavedCharacters(string iD, string filePath)
    {
        ID = iD;
        this.filePath = filePath;
    }
}
