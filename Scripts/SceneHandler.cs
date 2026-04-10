using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SceneHandler : Node
{
    public static SceneHandler instance;
    public int sceneID;
    public List<SceneData> charactersInScene;
    public bool sceneModified=false;
     
    /*
    public override void _Ready()
    {
        LoadScene();
    }*/
    public Character SelectCharacter(Vector2 mousePos)
    {
        var inBoundCharacters = charactersInScene.Where(c => c.character.InSelectionZone(mousePos)).OrderBy(e=>e.character.order).FirstOrDefault();
        if(inBoundCharacters==null) return null;
        return inBoundCharacters.character;
    }
    public void LoadScene()
    {

        charactersInScene = new List<SceneData>();
        instance = this;
        var file = DataBaseHandler.SelectSavedScene(sceneID.ToString())[DataBaseHandler.saveLocation].AsString();
        DataBaseHandler.ChangeScene(file);
        var sceneDatas= DataBaseHandler.SelectSceneData(sceneID);
        GD.Print(sceneDatas);
        if (sceneDatas == null) return;
        foreach (Dictionary d in sceneDatas)
        {
            charactersInScene.Add(
                new SceneData(
                    d["id"].AsInt32(), 
                    d[DataBaseHandler.characterID].AsInt32(),
                    d[DataBaseHandler.outfitID].AsInt32(),
                    (float)d[DataBaseHandler.posX].AsDouble(),
                    (float)d[DataBaseHandler.posY].AsDouble(),
                    (float)d[DataBaseHandler.scale].AsDouble(),
                    d[DataBaseHandler.mirrored].AsBool()));
        }
        GD.Print($"Scene ID:{sceneID}");
        GD.Print($"Characters in scene: {charactersInScene.Count}");
        foreach (var item in charactersInScene)
        {
            AddingCharacter(item);
            GD.Print("spawning character to scene");
        }
    }
    public void AddingCharacter(SceneData data)
    {
        data.character=(Character)SpawnHandler.Spawn(SpawnableScenes.Character, this);
        GD.Print(data.character.Name);
        data.character.sceneID = data.ID;
        data.character.characterID = data.characterID;
        data.character.outfitID = data.outfitID;
        data.character.PivotOffsetRatio = new Vector2(0, 0);
        data.character.GlobalPosition =new Vector2(data.posX, data.posY);
        data.character.mirrored =data.mirrored;
        data.character.Flip(data.character.mirrored);
        data.character.SetSize(new Vector2(data.scale, data.scale));
        data.character.PivotOffsetRatio = new Vector2(0.5f, 0.5f);
        data.character.isOnlineCharacter = false;
        data.character.SetupCharacter();
        data.character.SetupAnimations(data.outfitID);
    }
    public void SaveScene()
    {
        foreach (var c in charactersInScene)
        {
            c.character.PivotOffsetRatio = new Vector2(0, 0);
            GD.Print(c.character.size.Y);
            GD.Print($"Saving character {c.characterID} with scene id:{c.ID}");
            if(!c.character.isOnlineCharacter)
            DataBaseHandler.UpdateSceneData(
                c.ID, 
                c.character.outfitID,
                c.character.GlobalPosition.X,
                c.character.GlobalPosition.Y,
                c.character.size.Y,
                c.character.mirrored? 1 : 0);
            c.character.PivotOffsetRatio = new Vector2(0.5f, 0.5f);

        }
    }
    public void RemoveCharacterFromListByID(int id)
    {
       var character= charactersInScene.Where(c=>c.IsSameCharacter(id)).FirstOrDefault();
        if (character == null) return;
        charactersInScene.Remove(character);
    }
    public int GetHighestIDForOnline()
    {
       return charactersInScene.Max(c => c.ID)+1000;
    }

    public override void _Process(double delta)
    {
        foreach (var item in charactersInScene)
        {
            if(item.character.type== CharacterType.character_png)
            {
                item.character.Blink((float)delta);
                item.character.Talking((float)delta);
            }
        }

    }
}
