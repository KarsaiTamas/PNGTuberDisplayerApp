using Godot;
using System;

public partial class AnimUI : Node
{
    public LineEdit animNameLE;
    public TextureRect animLookTR;
    public Button animChangeB;
    public SpinBox frameTimeSB;
    public SpinBox frameCountSB;
    public OptionButton keyActionOB;
    public LineEdit keyActionLE;
    public void Init(bool isSpawned)
    {
        if (HasNode("VC/KeyActionLE")) keyActionLE= GetNode<LineEdit>("VC/KeyActionLE");
        if (HasNode("VC/FrameTimeHC/SpinBox")) frameTimeSB = GetNode<SpinBox>("VC/FrameTimeHC/SpinBox");
        if (HasNode("VC/FrameCountHC/SpinBox")) frameCountSB = GetNode<SpinBox>("VC/FrameCountHC/SpinBox");
        if (HasNode("VC/KeyActionOB")) keyActionOB = GetNode<OptionButton>("VC/KeyActionOB");
        if (HasNode("VC/AnimNameLE")) animNameLE = GetNode<LineEdit>("VC/AnimNameLE");

        if (isSpawned)
        { 
            animChangeB=GetNode<Button>("VC/AnimChangeB");
            animLookTR=GetNode<TextureRect>("VC/AnimChangeB/TextureRect");
            frameTimeSB=GetNode<SpinBox>("VC/HC/VC/AnimLengthSP");
            frameCountSB=GetNode<SpinBox>("VC/HC/VC2/AnimCountSP"); 
            return;
        }
        animChangeB =GetNode<Button>("VC/AnimChangeB");
        animLookTR=GetNode<TextureRect>("VC/AnimChangeB/TextureRect");
        
       

    } 
}
