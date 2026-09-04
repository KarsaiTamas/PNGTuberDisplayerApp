using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CM = CharacterManager;
using SM = SceneManager;

//using System.IO;
using System.Text.Json;
public static class SaveLoadManager 
{
    public const string SAVEDSCENESLOCATION = "UserScenes/";
    public const string DEFAULTIMAGE = $"res://Sprites/missingImageInPNGTuberApp.png";
    public const string SAVEDSCENESFILE = "SavedScenes";
    public const string SAVEDCHARACTERSLOCATION = "UserCharacters/";
    public const string SAVEDCHARACTERSFILE = "SavedCharacters";
    public static List<SavedCharacters> charactersToLoad;
    public static List<string> scenesToLoad;
    public static void Init()
    {
        charactersToLoad = new List<SavedCharacters>();
        scenesToLoad=new List<string>(); 
    }
    #region File Loading
    public static void LoadSavedScenes()
    {
        var saves= Load<List<string>>(SAVEDSCENESFILE, SAVEDSCENESLOCATION);
        if (saves == null) return;
        scenesToLoad= saves;
        foreach (var sceneNamePath in scenesToLoad)
        {

            var scenePath = sceneNamePath.Split('/');
            var sceneName = scenePath[scenePath.Length - 1];
            UIManager.instance.AddSceneToSceneOptions(sceneName);
        }
    }
    public static void LoadSavedCharacters()
    {
        var saves= Load<List<SavedCharacters>>(SAVEDCHARACTERSFILE, SAVEDCHARACTERSLOCATION);
        if (saves == null) return;
        charactersToLoad= saves;
        foreach (var characterNamePath in charactersToLoad)
        {
            var charPath= characterNamePath.filePath.Split('/');
            var charName=charPath[charPath.Length-1];
            UIManager.instance.AddCharacterToCharacterOptions(charName);
        }
    }
    #endregion

    #region Scene Loading

    public static void SaveScene()
    {
        SM.instance.isEdited = false;
        SM.instance.PutUIDataToSceneData();

        if (!RenamePathInList(scenesToLoad,
            SM.IDLoadedScene,
        SM.instance.data.sceneName,
            SAVEDSCENESFILE,
            SAVEDSCENESLOCATION))
        {
            ConfirmUI.Instance.ShowConfirm("Failed to save Scene.");
            return;
        }

        UIManager.instance.RenameSceneInPopupMenu(
            SM.IDLoadedScene,
        SM.instance.data.sceneName);

        Save(SM.instance.data, SM.instance.data.sceneName, SAVEDSCENESLOCATION);

    }
    public static void LoadScene(int id)
    {
        GD.Print("loading to scene");
        if(SM.instance.isEdited)
        {
            ConfirmUI.Instance.ShowConfirm("You have unsaved changes in your scene! " +
                "Would you like to save before closing this scene?",
                ()=> { SaveScene(); LoadSceneScript(id); },
                ()=> LoadSceneScript(id));
        }
        else LoadSceneScript(id);
    }
    private static void LoadSceneScript(int id)
    {
        SM.instance.RemoveCharacters();
        UIManager.instance.OpenScene(id);
        SM.IDLoadedScene = id - 1;
        SM.instance.data = Load<SceneData>(scenesToLoad[id - 1]);
        UIManager.instance.LoadDataIntoSceneUI();
        SM.instance.isEdited = false;

    }
    public static void OpenFileDialogueForScene()
    {
        UIManager.instance.FileDialogueShowSavedData("scene");

    }


    #endregion

    #region Character Loading

    public static CharacterData LoadCharacterData(int id)
    {
        return Load<CharacterData>(charactersToLoad[id].filePath);

    }
    public static byte[] CharacterDataToBytes(CharacterData characterData)
    {
        return System.Text.Encoding.UTF8.GetBytes(
            System.Text.Json.JsonSerializer.Serialize(characterData));
    }
    public static CharacterData BytesToCharacterData(byte[] bytes)
    {

        string json = System.Text.Encoding.UTF8.GetString(bytes);
        var data = System.Text.Json.JsonSerializer
            .Deserialize<CharacterData>(json);
        return data;
    }

    public static void SaveCharacter()
    {
        CM.instance.isEdited = false;
        CM.instance.PutUIDataToCharacterData();
        if (!RenamePathInList(charactersToLoad,
            CM.IDLoaded,
            CM.instance.characterInEdit.data.characterName,
            SAVEDCHARACTERSFILE,
            SAVEDCHARACTERSLOCATION))
        {
            ConfirmUI.Instance.ShowConfirm("Failed to save character.");
            return;
        }
        UIManager.instance.RenameCharacterInPopupMenu(
            CM.IDLoaded,
            CM.instance.characterInEdit.data.characterName);

        Save(CM.instance.characterInEdit.data,
            CM.instance.characterInEdit.data.characterName,
            SAVEDCHARACTERSLOCATION);
        ProgramManager.instance.ReloadCharactersByID(CM.instance.characterInEdit.data.ID);
    }
    public static void LoadCharacterInEditor(long id)
    {

        UIManager.instance.CharacterEditorVisibility(true);
        CM.IDLoaded = (int)id - 1;
        CM.instance.characterInEdit.data = Load<CharacterData>(charactersToLoad[CM.IDLoaded].filePath);
        for (int i = 0; i < CM.instance.characterInEdit.data.animData.Count; i++)
        {
            var anim = CM.instance.characterInEdit.data.animData[i];
            if (anim.filePath.Equals(ProgramManager.VALUENOTSET)) continue;
            CM.instance.UpdateAnim(i, anim.filePath);
        }
        CM.instance.isEdited = false;

        CM.instance.characterInEdit.LoadCharacterData(CM.IDLoaded);
        CM.instance.characterInEdit.UpdateUIWithData();
        CM.instance.PutCharacterDataIntoUI();
    }
    #endregion

    #region Image Loading
    public static byte[] ImageToBytes(Image image)
    {
        int maxDimension = 1024;
        // PNG magic number must be: 89 50 4E 47 0D 0A 1A 0A
        // Work on a duplicate so we don't mutate the original the player is using locally
        var toSend = (Image)image.Duplicate();

        int width = toSend.GetWidth();
        int height = toSend.GetHeight();

        if (width > maxDimension || height > maxDimension)
        {
            // Preserve aspect ratio
            float scale = (float)maxDimension / Mathf.Max(width, height);
            int newWidth = Mathf.RoundToInt(width * scale);
            int newHeight = Mathf.RoundToInt(height * scale);
            toSend.Resize(newWidth, newHeight, Image.Interpolation.Lanczos);
        }
        var pngBytes = toSend.SavePngToBuffer();
        GD.Print($"Encoded PNG size: {pngBytes.Length}, header: {string.Join(" ", pngBytes.Take(8).Select(b => b.ToString("X2")))}");

        return pngBytes; 
    }

    public static Image BytesToImage(byte[] bytes)
    {

        var image = new Image();
        image.LoadPngFromBuffer(bytes);
        //image.Decompress();
        GD.Print($"Encoded PNG size: {bytes.Length}, header: {string.Join(" ", bytes.Take(8).Select(b => b.ToString("X2")))}");

        //var img= Image.CreateFromData(512,512,false,Image.Format.Rgba16,bytes);
        return image != null ? image : null;
    }

    public static ImageTexture GetCharacterAnim(string location)
    {
        ImageTexture texture = new ImageTexture();
        if (Godot.FileAccess.FileExists($"{location}"))
            texture.SetImage(Image.LoadFromFile($"{location}"));
        else
        {
            GD.Print($"Failed to load image at location: {location}");
            texture = GetDefaultImage();
        }
        return texture;

    }

    public static ImageTexture ImageToTexture(Image image)
    {
        ImageTexture texture = new ImageTexture();
        if (image != null) texture.SetImage(image);
        else
        {
            GD.Print($"Failed to load image ");
            texture=GetDefaultImage();
        }
        return texture;

    }
    public static List<ImageTexture> GetAnimSheet(string location, int frameCount)
    {
        List<ImageTexture> textures = new List<ImageTexture>();
        if (location.EndsWith(".png"))
        {
            location = location.Substr(0, location.Length - 5);
        }
        GD.Print("Frame count"+ frameCount);
        for (int i = 1; i < frameCount+1; i++)
        {
            GD.Print($"{location}{i}.png added");
            textures.Add(GetCharacterAnim($"{location}{i}.png")); 
        }
        return textures;

    }
    public static ImageTexture GetDefaultImage()
    {
        CompressedTexture2D texture = GD.Load<CompressedTexture2D>(DEFAULTIMAGE);
        ImageTexture image = new ImageTexture();
        image.SetImage(texture.GetImage());
        return image;
    }



    public static byte[] AnimsToByte(Character chara)
    {
        //
        Godot.Collections.Dictionary<string,Godot.Collections.Array<byte[]>> anims = new();
        int i = 0;
        foreach (var anim in chara.CharacterAnims)
        {
            anims.Add(i.ToString(),new Godot.Collections.Array<byte[]>());
            foreach (var texture in anim.Value.texture)
            {
                anims[i.ToString()].Add(ImageToBytes(texture.GetImage()));
            }
            i++;
        }
        return SerializeAnims(anims);
    }
    public static Godot.Collections.Dictionary<string,Godot.Collections.Array<byte[]>> ByteArrayToAnim(byte[] animByte)
    { 
        return DeserializeAnims(animByte);
    }

    
    private static byte[] SerializeAnims(
    Godot.Collections.Dictionary<string, Godot.Collections.Array<byte[]>> anims)
    {
        // Convert to a plain C# structure and JSON-serialize it
        var plain = new Dictionary<string, List<string>>();
        foreach (var anim in anims)
        {
            plain[anim.Key] = anim.Value
                .Select(frame => System.Convert.ToBase64String(frame))
                .ToList();
        }
        return System.Text.Encoding.UTF8.GetBytes(
            System.Text.Json.JsonSerializer.Serialize(plain));
    }
    

    private static Godot.Collections.Dictionary<string, Godot.Collections.Array<byte[]>>
    DeserializeAnims(byte[] animBytes)
    {
        string json = System.Text.Encoding.UTF8.GetString(animBytes);
        var plain = System.Text.Json.JsonSerializer
            .Deserialize<Dictionary<string, List<string>>>(json);

        var result = new Godot.Collections.Dictionary<
        string, Godot.Collections.Array<byte[]>>();

        foreach (var anim in plain)
        {
            GD.Print(anim.Key);
            var frames = new Godot.Collections.Array<byte[]>();
            foreach (var b64 in anim.Value)
                frames.Add(Convert.FromBase64String(b64));
            result.Add(anim.Key, frames);
        }
        return result;
    }
    #endregion

    #region Utility Scripts
    public static void Save<T>(T dataToSave, string saveName,string directory) where T : class
    {
        try
        {
            if(!DirAccess.DirExistsAbsolute(directory)) DirAccess.MakeDirRecursiveAbsolute(directory);
            string write = JsonSerializer.Serialize(dataToSave);
            //if a directory is missing than creat the missing path
            using var file= FileAccess.Open(directory + saveName+".json", FileAccess.ModeFlags.Write);
            file.StoreString(write);
            GD.Print("File created with name of: "+saveName);
        }
        catch (Exception e)
        {
            GD.PrintErr("Failed to save: ");
            GD.PrintErr(e);
        }
    }
    public static T Load<T>(string saveName, string directory) where T : class
    {
        try
        {

            if(!DirAccess.DirExistsAbsolute(directory)) DirAccess.MakeDirRecursiveAbsolute(directory);
            if (!FileAccess.FileExists(directory + saveName+".json"))
            {
                GD.PrintErr($"File {saveName} doesn't exists.");
                return null;
            }
            //if a directory is missing than creat the missing path
            using var file = FileAccess.Open(directory + saveName+".json", FileAccess.ModeFlags.Read);
            GD.Print("Opened file: " + saveName);
            string json =file.GetAsText();
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception e)
        {
            GD.PrintErr("Failed to Load: ");
            GD.PrintErr(e);
            return null;
        }
    }
    /// <summary>
    /// Loads file from full file path+name no directory checking
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file"></param>
    /// <returns></returns>
    public static T Load<T>(string fileName) where T : class
    {
        try
        {

            if (!FileAccess.FileExists(fileName + ".json"))
            {
                GD.PrintErr($"File {fileName} doesn't exists.");
                return null;
            }
            //if a directory is missing than creat the missing path
            using var file = FileAccess.Open(fileName + ".json", FileAccess.ModeFlags.Read);
            GD.Print("File to open: " + fileName);
            string json = file.GetAsText();
            GD.Print(json);
            var options = new JsonSerializerOptions();
            options.Converters.Add(new IntFromDoubleConverter());
            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch (Exception e)
        {
            GD.PrintErr("Failed to Load: ");
            GD.PrintErr(e);
            return null;
        }
    }
    public static bool DeleteFile(string fileWithPath)
    {
        try
        {

            if (!FileAccess.FileExists(fileWithPath + ".json"))
            {
                ConfirmUI.Instance.ShowConfirm("File doesn't exists.");
                GD.PrintErr($"File {fileWithPath} doesn't exists.");
                return false;
            }
            
            Error err = DirAccess.RemoveAbsolute(fileWithPath + ".json");
            if (err == Error.Ok)
            {
                GD.Print($"File {fileWithPath} deleted.");
                return true;
            }            
                ConfirmUI.Instance.ShowConfirm("Failed to delete the file.");
            GD.PrintErr($"Failed to delete the file: {err}");
            return false;
        }
        catch (Exception e)
        {
            ConfirmUI.Instance.ShowConfirm("Failed to delete the file.");
            GD.PrintErr("Failed to delete file: ");
            GD.PrintErr(e);
            return false;
        }
    }
    public static bool RenameFile(string oldFileWithFullPath,string newName)
    {
        var path= oldFileWithFullPath.Split('/');
        string oldFileName = path[path.Length-1];
        var dir = DirAccess.Open(oldFileWithFullPath.Substring(0, oldFileWithFullPath.Length - oldFileName.Length));
        if (dir != null)
        {
            Error err = dir.Rename($"{oldFileName}.json", $"{newName}.json");
            if (err != Error.Ok)
            {
                GD.PrintErr("Rename failed: " + err);
                return false;
            }
            return true;
        }
        else
        {
            GD.PrintErr("Failed to open directory: " + DirAccess.GetOpenError());
            return false;
        }
    }
    public static bool RenamePathInList(List<SavedCharacters>paths, int index,string newName,string fileName,string location)
    {
        var path = paths[index].filePath.Split('/');
        string oldFileName = path[path.Length - 1];
        string newFileName= paths[index].filePath.Substring(0, paths[index].filePath.Length-oldFileName.Length)+newName;
 
        if(!RenameFile(paths[index].filePath, newName))return false;
        paths[index].filePath = newFileName;
        Save(paths, fileName, location);
        return true;

    }

    public static bool RenamePathInList(List<string> paths, int index, string newName, string fileName, string location)
    {
        var path = paths[index].Split('/');
        string oldFileName = path[path.Length - 1];
        string newFileName = paths[index].Substring(0, paths[index].Length - oldFileName.Length) + newName;
 
        if (!RenameFile(paths[index], newName)) return false;
        paths[index] = newFileName;
        Save(paths, fileName, location);
        return true;

    }

    public static string DateTimeForSave()
    {
        return DateTime.Now.ToString().Replace(':','_').Replace(" ","").Replace('/','_');
    }

    public static int GetCharacterIntID(string ID)
    { 
        for (int i = 0; i < charactersToLoad.Count; i++)
        {
            if (charactersToLoad[i].ID.Equals(ID))return i;
        } 
        return 0;
    }
    #endregion
}
