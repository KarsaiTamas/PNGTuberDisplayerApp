using Godot;
using System;
using System.Collections.Generic;

public class OAnim  
{
    public List<ImageTexture> animationSequence;
    public float frameLength;
    public AnimType animType;
    public string extraAnimInfo;
    public int curFrame;

    public OAnim()
    {
        animationSequence = new List<ImageTexture>();
        curFrame = 0;
    }

    public OAnim(string baseFile, float frameLength, int animCount, AnimType type, string extraInfo)
    {
        animationSequence= new List<ImageTexture>();
        curFrame = 0;
        this.frameLength = frameLength;
        animType = type;
        extraAnimInfo = extraInfo;
        //$"D:\\dokumentumok\\képek_Videók\\darw\\GooseBlink000{i}.png"
        if (animCount == 1)
        {
            animationSequence.Add(FileLoaderHandler.GetCharacterAnim($"{baseFile}"));
            return;

        }
        for (int i = 1; i < animCount+1; i++)
        {
            int num=i.ToString().Length;
            string baseStr = baseFile.Substring(0, baseFile.Length - (num));
            animationSequence.Add(FileLoaderHandler.GetCharacterAnim($"{baseStr}{i}"));
        }
    }
    public OAnim(List<byte[]> byteAnimationSequence, float frameLength)
    {
        animationSequence= new List<ImageTexture>();
        foreach (var animFrame in byteAnimationSequence)
        {
            var img= FileLoaderHandler.BytesToImage(animFrame);
            animationSequence.Add(
                FileLoaderHandler.GetCharacterAnim(img));
        }
        this.frameLength = frameLength;
        animType = AnimType.other;
        extraAnimInfo = "outside";
    }
}
