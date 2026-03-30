using Godot;
using Godot.Collections;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;



public static class DataBaseHandler
{
    public enum Table
    {
        /// <summary>
        /// This is where we store our saved scenes
        /// </summary>
        scenes,
        /// <summary>
        /// This is where we store our saved characters
        /// </summary>
        characters,
        /// <summary>
        /// This is where we store our last used scene
        /// </summary>
        last_used_scene,
        /// <summary>
        /// We use this table as a queue for animations
        /// </summary>
        animations_to_play,
        /// <summary>
        /// We store every data in a scene in this table
        /// </summary>
        scene_data,
        /// <summary>
        /// We store the outfits for a character in this table
        /// </summary>
        outfits,
        /// <summary>
        /// We store every animation for each outfit for 1 character in this table
        /// </summary>
        outfit_animations,

    }
    public enum DB
    {
        SavedCharacters,
        Character,
        Scene,
    }
    public static string lastUsedScene = "";
    public static string sceneDataTable = "scene_data";
    public static string characterID = "character_id";
    public static string outfitID = "outfit_id";
    public static string posX = "pos_x";
    public static string posY = "pos_y";
    public static string scale = "scale";
    public static string mirrored = "mirrored";
    public static string savedScenesTable = "scenes";
    public static string savedCharactersTable = "characters";
    public static string saveLocation = "save_location";
    public static string cCharacterType = "character_type";
    public static string outfitsTable = "outfits";
    public static string outfitLocation = "outfit_location";
    public static string outfitAnimationsTable = "outfit_animations";
    public static string oOutfitID = "outfit_id";
    public static string oLocation = "outfit_location";
    public static string oIsSimple = "is_simple";
    public static string oSpriteFile = "sprite_file";
    public static string oAnimLength = "anim_length";
    public static string oAnimCount = "anim_count";
    public static string oAnimType = "anim_type";
    public static string oAnimExtraInfo = "extra_anim_info";
    public static string lastUsedCharacter = "character";//#needs character id at the end
    public static string lastUsedSceneTable = "last_used_scene";

    #region Main database
    public static Dictionary GetRowDataFromArray(Variant data)
    {
        
        return data.AsGodotArray().Count==0?null:data.AsGodotArray()[0].AsGodotDictionary();
    }

    public static Variant RunSavedCharactersSQLCommand(string command)
    {
        return ProgramHandler.DB.Call("RunSavedCharactersSQLCommand", command);
    }
    public static Variant RunSQLCommand(string command,DB db, int characterID = 1 )
    {
        switch (db)
        {
            case DB.SavedCharacters:
            default:
                return RunSavedCharactersSQLCommand(command);
            case DB.Character:
                return RunCharacterInSceneSQLCommand(command, characterID).AsGodotArray();
            case DB.Scene:
                return RunSceneSQLCommand(command);
        }
    }
    public static void InsertIntoCharacters(string name, string type, string location)
    {
        ProgramHandler.DB.Call("InsertIntoCharacters", name, type, location);
    }


    public static int GetNextIDForTable(Table table,int characterID = 1)
    {
        string command = $"SELECT MAX(id) as idm FROM {table.ToString()};";
        GD.Print(command);
        Godot.Collections.Array value;
        switch (table)
        {
            case Table.scenes:
            case Table.characters:
            case Table.animations_to_play:
                value = RunSQLCommand(command,DB.SavedCharacters).AsGodotArray();
                break;
            case Table.outfits:
            case Table.outfit_animations:
                value = RunSQLCommand(command,DB.Character, characterID).AsGodotArray();
                break ;
            case Table.scene_data:
                value = RunSQLCommand(command,DB.Scene).AsGodotArray();
                break;
            default:
            return 1;
        }
        if (value.Count == 0) return 1;
        return ((Dictionary)value[0])["idm"].AsInt32() + 1;

    }
    public static Godot.Collections.Array GetCreatedCharacters()
    {
        return RunSavedCharactersSQLCommand(
            $"SELECT * FROM {savedCharactersTable};").AsGodotArray();
    }

    public static Godot.Collections.Array GetCreatedScenes()
    {
        return RunSavedCharactersSQLCommand(
            $"SELECT * FROM {savedScenesTable};").AsGodotArray();
    }

    public static void UpdateLastUsedCScene(string id)
    {
        ProgramHandler.DB.Call("UpdateLastUsedCScene", id);
    } 
    public static Dictionary SelectSavedScene(string id)
    {
        return GetRowDataFromArray(ProgramHandler.DB.Call("SelectScene", id));
    }

    #endregion

    #region Character
    /*public static Variant RunCharacterSQLCommand(string command)
    {
        return ProgramHandler.DB.Call("RunCharacterSQLCommand", command);
    }*/
    public static Variant RunCharacterInSceneSQLCommand(string command,int charID)
    {
        Variant def = -1;
        GD.Print(command);
        if (GetCharacterInSaves(charID.ToString()) == null) return def;
        ChangeUsedCharacter(charID);
        var returnData = ProgramHandler.DB.Call("RunCharacterSQLCommand", command);
        CloseUsedCharacter();
        return returnData;
    }
    public static void ChangeUsedCharacter(int id)
    {
        var loc= GetCharacterInSaves(id.ToString());
        if (loc == null) return;
        ProgramHandler.DB.Call("ChangeUsedCharacter", loc[saveLocation].AsString());
    }
    
    public static void CloseUsedCharacter()
    {
        ProgramHandler.DB.Call("CloseUsedCharacter");
    }

    public static void UpdateCharacter(string id,string name, string type)
    {
        ProgramHandler.DB.Call("UpdateCharacter",id, name, type);
    }

    public static void DeleteCharacter(string id)
    {
        string path= $"characters/{ProgramHandler.DB.Call("DeleteCharacter", id).AsString()}.db";
        GD.Print(path);
        if(File.Exists(path)) File.Delete(path);
    }

    public static void DeleteOutfit(int cID,int oID)
    {
        RunSQLCommand(
            $"DELETE FROM {Table.outfit_animations.ToString()} WHERE " +
            $"{outfitID}={oID}", DB.Character, cID);

        RunSQLCommand(
            $"DELETE FROM {Table.outfits.ToString()} WHERE " +
            $"id={oID}", DB.Character, cID);

    }
    public static void DeleteAnimation(int cID, int aID)
    {
        RunSQLCommand(
            $"DELETE FROM {Table.outfit_animations.ToString()} WHERE " +
            $"id={aID}", DB.Character, cID);
    }
    public static Dictionary GetCharacterInSaves(string id)
    {
        return GetRowDataFromArray(ProgramHandler.DB.Call("SelectCharacter", id));
    }

    public static Dictionary GetCharacterInScene(string id)
    {
        return GetRowDataFromArray(ProgramHandler.DB.Call("SelectCharacterInScene", id));
    }

    public static void CreateCharacter(string id, string name, CharacterType type)
    {
        ProgramHandler.DB.Call("CreateCharacter", id, name, type.ToString(), $"character{GetNextIDForTable(Table.characters)}");

    }

    public static Godot.Collections.Array GetCharacterAnimations(int cid,int outfitID)
    {
        var anims = RunCharacterInSceneSQLCommand($"SELECT * FROM {outfitAnimationsTable} WHERE {oOutfitID}={outfitID}", cid);
        GD.Print(anims.VariantType);
        if (anims.VariantType == Variant.Type.Int) return null;
        return anims.AsGodotArray();
        
    }

    public static Dictionary GetOutfit(int charID,int outfitID)
    {
        var returnData = GetRowDataFromArray(
            RunCharacterInSceneSQLCommand(
                $"SELECT * FROM {outfitsTable} WHERE id={outfitID};", charID));

        return returnData==null?null:returnData;
    }

    public static Godot.Collections.Array GetOutfits(int charID)
    {
        var returnData = 
            RunCharacterInSceneSQLCommand(
                $"SELECT * FROM {outfitsTable};", charID).AsGodotArray();

        return returnData == null ? null : returnData;
    }
    public static void InsertAnimationIntoCharacter(
        int id, 
        int oid, 
        int cid,
        string name,
        string spriteFile,
        float animLength,
        int animCount,
        int animType,
        string extraInfo)
    {
        string animFLenght = animLength.ToString().Replace(',', '.');
        RunCharacterInSceneSQLCommand($"INSERT INTO {outfitAnimationsTable} " +
            $"(id, {outfitID}, name," +
            $" {oSpriteFile}, {oAnimLength}," +
            $" {oAnimCount}, {oAnimType}, {oAnimExtraInfo})" +
            $"VALUES({id}," +
            $"{oid}," +
            $"'{name}'," +
            $"'{spriteFile}'," +
            $"{animFLenght},{animCount},{animType},'{extraInfo}')",cid);
    }

    public static void InsertOutfitIntoCharacter(
        int oid,
        int cid,
        string name,
        bool isSimple,
        string outfitLocation)
    {

        RunCharacterInSceneSQLCommand($"INSERT INTO {outfitsTable} " +
            $"(id, name," +
            $" {oIsSimple}, {oLocation})" + 
            $" VALUES({oid}," +
            $"'{name}'," +
            $"{isSimple}" +
            $",'{outfitLocation}')",cid);
    }

    public static void UpdateAnimationInCharacter(
        int id,
        int oid,
        int cid,
        string name,
        string spriteFile,
        float animLength,
        int animCount,
        int animType,
        string extraInfo)
    {
        string animFLenght = animLength.ToString().Replace(',', '.');
        RunCharacterInSceneSQLCommand($"UPDATE {outfitAnimationsTable} " +
            $"SET name='{name}'," +
            $" {oSpriteFile}='{spriteFile}'," +
            $" {oAnimLength}={animFLenght}," +
            $" {oAnimCount}={animCount}, {oAnimType}={animType}," +
            $" {oAnimExtraInfo}='{extraInfo}'" +
            $"WHERE id={id} AND {outfitID}={oid}",
            cid);
    }

    public static void UpdateOutfitInCharacter(
        int oid,
        int cid,
        string name,
        bool isSimple,
        string outfitLocation)
    {
        RunCharacterInSceneSQLCommand($"UPDATE {outfitsTable} " +
            $"SET name='{name}'," +
            $" {oIsSimple}={isSimple}, {oLocation}='{outfitLocation}' " +
            $"WHERE id={oid}", cid);
    } 
    #endregion

    #region Scene
    public static Variant RunSceneSQLCommand(string command)
    {
        GD.Print(command);
        return ProgramHandler.DB.Call("RunSceneSQLCommand", command);
    }

    public static Dictionary SelectScene(int id)
    {
        return GetRowDataFromArray(RunSavedCharactersSQLCommand( 
            $"SELECT * FROM {Table.scenes} WHERE id={id}"));

    }

    public static void CloseUsedScene()
    {
        ProgramHandler.DB.Call("CloseUsedScene");
    }
    public static void CreateScene(string id, string name)
    {
        ProgramHandler.DB.Call("CreateScene", id, name, $"scenedata{GetNextIDForTable(Table.scenes)}");
    }

    public static void ChangeScene(string sceneName)
    {

        ProgramHandler.DB.Call("ChangeScene", sceneName);
    }

    public static void InsertIntoScenes(string id, string name, string fileName)
    {
        ProgramHandler.DB.Call("InsertIntoScenes", id, name, fileName);
    }

    public static void InsertIntoSceneData(int id, int cID, int oID)
    {
        ProgramHandler.DB.Call("InsertIntoSceneData", id.ToString(), cID.ToString(), oID.ToString());
    }

    public static void UpdateSceneData(int id, int oID,float posX,float posY, float scale,int mirrored)
    {
        GD.Print($"updating scene with id of {id} characterid:{oID} X{posX}Y{posY} S{scale}");
        ProgramHandler.DB.Call("UpdateSceneData", 
            id.ToString(), 
            oID.ToString(),
            Godot.Mathf.Snapped (posX,0.001f),
            Godot.Mathf.Snapped( posY, 0.001f),
            Godot.Mathf.Snapped(scale, 0.001f),
            mirrored);
    }

    public static bool DeleteScene(int id)
    {
        var scene=SelectScene(id);
        string path = $"characters/{scene[saveLocation]}.db";
        CloseUsedScene();
        GD.Print(path);
        if (!FileLoaderHandler.DeleteFile(path)) return false;
        ProgramHandler.DB.Call("DeleteScene", id.ToString());
        return true; 

    }

    public static void DeleteCharacterFromScene(int id)
    {
        RunSQLCommand(
            $"DELETE FROM {Table.scene_data.ToString()} " +
            $"WHERE id={id}", DB.Scene);
    }
    public static Godot.Collections.Array SelectSceneData(int id) 
    {
        return RunSceneSQLCommand($"SELECT * FROM {sceneDataTable};").AsGodotArray();

    }
     
    public static void UpdateScene(string id, string name)
    {
        //change the name
        RunSavedCharactersSQLCommand($"UPDATE {savedScenesTable} " +
            $"SET name='{name}' " +
            $"WHERE id={id}");
        //change data for a scene character
        RunSceneSQLCommand($"");
    }
    #endregion
}
