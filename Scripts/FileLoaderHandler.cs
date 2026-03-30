using Godot;
using System;
using System.IO;

public static class FileLoaderHandler
{

    const string defaultImage = $"res://Sprites/missingImageInPNGTuberApp.png";
    public static byte[] ImageToBytes(Image image)
    {
        return image.SavePngToBuffer();
    }

    public static Image BytesToImage(byte[] bytes) 
    {
        var img= Image.CreateFromData(512,512,false,Image.Format.Rgba16,bytes);
        return img!=null?img:null;
    }

    public static ImageTexture GetCharacterAnim(string location)
    {
        ImageTexture texture = new ImageTexture();
        if (location.EndsWith(".png"))
        {
            location = location.Substr(0, location.Length - 4);
        }
        if (Godot.FileAccess.FileExists($"{location}.png"))
            texture.SetImage(Image.LoadFromFile($"{location}.png"));
        else
        {
            GD.Print($"Failed to load image at location: {location}");
            texture.SetImage(GetDefaultImage());
        }
        return texture;

    }
    static Image GetDefaultImage()
    {
        CompressedTexture2D texture = GD.Load<CompressedTexture2D>(defaultImage);
        Image image = texture.GetImage(); 
        return image;
    }
    public static void ShowFileDialogue()
    {

    }

    public static void CreateSavesFolder()
    {
        if(!Directory.Exists("saves"))Directory.CreateDirectory("saves");
    }
    public static string GetSpriteLocation(string path)
    {
        return path.Substr(0, path.Length - 4);
    }

    public static ImageTexture GetCharacterAnim(Image image)
    {
        ImageTexture texture = new ImageTexture();  
        if(image!=null) texture.SetImage(image);
        else
        {
            GD.Print($"Failed to load image "); 
            texture.SetImage(GetDefaultImage());
        }
        return texture;

    }
    /// <summary>
    /// <br>true file deleted</br>
    /// <br>false file NOT deleted</br>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            { File.Delete(filePath); return true; }
             
            return false; // File is NOT in use
        }
        catch (IOException e)
        {
            GD.Print("File in use"+ e.Message);

            return false; // File IS in use or locked
        }
        catch (UnauthorizedAccessException)
        {
            GD.Print("Un authorized access!");
            return false; // No permission = treat as in use
        }
    }
}
