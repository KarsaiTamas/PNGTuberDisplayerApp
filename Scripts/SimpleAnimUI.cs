using Godot;
using System;

public partial class SimpleAnimUI : AnimUI
{
    public override void SetupAnimUI()
    {
        animChangeButton = GetNode<Button>("MCAnimChange/Button");
        animStartFrameImg = GetNode<TextureRect>("MCAnimChange/Button/TextureRect");

    }

    public void SetupSimpleAnim( int outfitID)
    {
        data = new AnimData(outfitID);

    }

}
