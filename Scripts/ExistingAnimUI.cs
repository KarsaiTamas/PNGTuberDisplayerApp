using Godot;
using System;

public partial class ExistingAnimUI : AnimUI
{
    public override void SetupAnimUI()
    {
        animChangeButton = GetNode<Button>("VC/MCAnimChange/Button");
        animStartFrameImg = GetNode<TextureRect>("VC/MCAnimChange/Button/TextureRect");

        animLengthSB = GetNode<SpinBox>("VC/HC/VC/AnimLengthSP");
        frameCountSB = GetNode<SpinBox>("VC/HC/VC2/Anim CountSP");

    }
}
