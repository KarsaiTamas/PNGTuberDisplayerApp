using Godot;
using System;

public enum ESpawnableScenes
{
    Character,
    CharacterAnimUI,
    OutfitButton,
}

public static class SpawnManager
{
    //https://forum.godotengine.org/t/how-to-import-all-files-from-a-folder/65619/5
    const string SPAWNABLESCENESPATH = "res://Scenes/SpawnableScenes/";
    public static System.Collections.Generic.Dictionary<string, PackedScene> nodesToSpawn;
    public static void Init()
    {
        nodesToSpawn=LoadScenes(SPAWNABLESCENESPATH);
        GD.Print("Spawn manager initialized");
    }
    public static System.Collections.Generic.Dictionary<string, PackedScene> LoadScenes(string path)
    {
        System.Collections.Generic.Dictionary<string, PackedScene> resources = new System.Collections.Generic.Dictionary<string, PackedScene>();

        DirAccess dir_access = DirAccess.Open(path);
        if (dir_access == null) { return null; }

        string[] files = dir_access.GetFiles();
        if (files == null) { return null; }

        foreach (string file_name in files)
        {
            string loadName = file_name.TrimSuffix(".remap");
            PackedScene loaded_resource = GD.Load<PackedScene>(path + loadName);
            //GD.Print(file_name);
            if (loaded_resource == null) { continue; }
            resources.Add(loadName.TrimSuffix(".remap").TrimSuffix(".tscn"), loaded_resource);
        }

        return resources;
    }

    public static T Spawn<T>(ESpawnableScenes objectToSpawn, Node parent) where T:Node
    {
        var spawnedObj = nodesToSpawn[objectToSpawn.ToString()].Instantiate<T>();
        parent.AddChild(spawnedObj);
        return spawnedObj;
    }/*
    public static void SpawnOutfitButton()
    {
        var newOutfitButton = Spawn<Control>(ESpawnableScenes.OutfitButton, UIManager.instance.characterEditor.Outfits);
        var basePath = newOutfitButton.GetPath().ToString();
        UIManager.instance.characterEditor.AddOutfit(
            newOutfitButton.Name,
            () => { GD.Print("Implement Outfit function!"); });
    }*/
    public static Character SpawnCharacterToScene(int ID)
    {
        var chara = SpawnCharacter(ID, SceneManager.instance.sceneCharacters);
        SceneManager.instance.charactersOnScene.Add(chara);
        return chara;
    } 
    /*
    public static Character SpawnCharacterToScene(int charID)
    {
        var spawnedCharacter = SpawnCharacter((int)ESpawnableScenes.Character, SceneManager.instance.sceneCharacters); 
        SceneManager.instance.charactersOnScene.Add(spawnedCharacter);
        return spawnedCharacter;

    }*/
    public static Character SpawnCharacter(int ID,Node parent)
    {
        var spawnedCharacter= Spawn<Character>(ESpawnableScenes.Character, parent);
        spawnedCharacter.LoadCharacterData(ID);
            ProgramManager.instance.spawnedCharacters.Add(spawnedCharacter); 
        return spawnedCharacter;

    }


    public static Character SpawnCharacter()
    {
        var spawnedCharacter = Spawn<Character>(ESpawnableScenes.Character, SceneManager.instance.sceneCharacters);
        ProgramManager.instance.spawnedCharacters.Add(spawnedCharacter);
        return spawnedCharacter;

    }

    /*
    public static Character SpawnCharacter(int charID, Node parent)
    {
        var spawnedCharacter = Spawn<Character>(ESpawnableScenes.Character, parent);
        ProgramManager.instance.spawnedCharacters.Add(spawnedCharacter); 
        
        return spawnedCharacter;

    }*/
}
