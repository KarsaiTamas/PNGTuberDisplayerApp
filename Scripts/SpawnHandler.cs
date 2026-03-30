using Godot; 

public static class SpawnHandler
{
    //https://forum.godotengine.org/t/how-to-import-all-files-from-a-folder/65619/5
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

    public static Node Spawn(SpawnableScenes objectToSpawn, Node parent)
    {
        var spawnedObj = ProgramHandler.nodesToSpawn[objectToSpawn.ToString()].Instantiate<Node>();
        parent.AddChild(spawnedObj);
        return spawnedObj;
    }
    public static Control SpawnUI(SpawnableScenes objectToSpawn, Node parent)
    {
        var spawnedObj = ProgramHandler.nodesToSpawn[objectToSpawn.ToString()].Instantiate<Control>();
        parent.AddChild(spawnedObj);
        return spawnedObj;
    }
    public static void SpawnPopup(string lable, System.Action extraAccept)
    {
        ConfirmUI.Instance.SetConfirm(lable, extraAccept);
    }
    public static void SpawnSceneUI(int id, string name)
    {
        var sceneUI = (SceneSelectUI)SpawnUI(SpawnableScenes.SceneSelectButton, ProgramHandler.instance.scenesContainer);
        sceneUI.sceneID = id;
        sceneUI.selectSceneButton.Text = name;
        ProgramHandler.instance.loadedScenes.Add(sceneUI);
    }

    public static void SpawnCharacterUI(int id, string name)
    {
        var characterUI = (CharacterSelectUI)SpawnUI(SpawnableScenes.CharacterSelectButton, ProgramHandler.instance.charactersContainer);
        characterUI.characterID = id;
        characterUI.selectCharacterButton.Text = name;
        ProgramHandler.instance.loadedCharacters.Add(characterUI);
    }
}
