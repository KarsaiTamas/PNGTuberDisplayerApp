using Godot;
using System;
using System.Collections.Generic;

public class SceneData 
{
    public string sceneName { get; set; }
    public List<CharacterData> charactersSceneData { get; set; }

    public SceneData(string name)
    {
        sceneName = name;
        this.charactersSceneData = new List<CharacterData>();
    }
    public SceneData() 
    {
        sceneName = ProgramManager.VALUENOTSET;
        this.charactersSceneData = new List<CharacterData>();

    }
}
