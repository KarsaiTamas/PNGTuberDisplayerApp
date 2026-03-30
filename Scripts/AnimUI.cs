using Godot;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using static Godot.ClassDB;

public class AnimData
{

    public int animID;
    public int outfitID;
    public string animName;
    public string spriteLoc;
    public float animLenght;
    public int animCount;
    public AnimType animType;
    public string extraAnimInfo;

    public AnimData(int animID,int outfitID)
    {
        this.animID = animID;
        this.outfitID=outfitID;
        animName=$"animation{animID}";
        spriteLoc="default";
        animLenght=1f;
        animCount=1;
        animType=AnimType.other;
        extraAnimInfo ="nothing";

    }
    public AnimData(int outfitID)
    {
        this.animID = -1;
        this.outfitID = outfitID;
        animName = $"Simple outfit";
        spriteLoc = "default";
        animLenght = 1f;
        animCount = 1;
        animType = AnimType.other;
        extraAnimInfo = "nothing";

    }

    public AnimData(int animID, int outfitID, string animName, string spriteLoc, float animLenght, int animCount, int animType, string extraAnimInfo)
    {
        this.animID = animID;
        this.outfitID = outfitID;
        this.animName = animName;
        this.animLenght = animLenght;
        this.animCount = animCount;
        this.animType = (AnimType)animType;
        this.extraAnimInfo = extraAnimInfo;
        this.spriteLoc = spriteLoc;
    }

}

public partial class AnimUI : Control
{
    public AnimData data;
    public Button animChangeButton;
    public TextureRect animStartFrameImg;
    public Button removeAnimButton;
    public LineEdit animName;
    public SpinBox animLengthSB;
    public SpinBox frameCountSB;
    public OptionButton animTypeButton;
    public LineEdit ActionNameLE;
    public bool quedForDeletion=false;
    
    public AnimUI(AnimData data)
    {
        this.data = data;
    }
    public AnimUI() { }
    public override void _EnterTree()
    {
        SetupAnimUI();

    }
    public override void _Ready()
    {
        animChangeButton.ButtonUp += ShowFileDialogue;




    }
    public virtual void SetupAnimUI()
    { 
        animName = GetNodeOrNull<LineEdit>("VC/MCAnimName/LineEdit");
        animChangeButton = GetNode<Button>("VC/HBoxContainer/MCAnimChange/Button");
        animStartFrameImg = GetNode<TextureRect>("VC/HBoxContainer/MCAnimChange/Button/TextureRect");
        removeAnimButton = GetNode<Button>("VC/HBoxContainer/Remove Animation/Button");
        animLengthSB = GetNode<SpinBox>("VC/HC/VC/AnimLengthSP");
        frameCountSB = GetNode<SpinBox>("VC/HC/VC2/Anim CountSP");
        animTypeButton = GetNode<OptionButton>("VC/MCAnimAction/OptionButton");
        ActionNameLE = GetNode<LineEdit>("VC/MCActionName/LineEdit");
        removeAnimButton.ButtonUp += ConfirmDeleteAnimation;

    }
    public void LoadDataIntoTheUI()
    {
        animStartFrameImg.Texture=(FileLoaderHandler.GetCharacterAnim(data.spriteLoc));
        animLengthSB.Value=data.animLenght;
        frameCountSB.Value=data.animCount;
        if (animName == null) return;
        animName.Text=data.animName;
        animTypeButton.Selected = (int)data.animType;
        ActionNameLE.Text = data.extraAnimInfo; 
    }
    public void PutUIDataIntoData()
    {
        data.animLenght=(float)animLengthSB.Value;
        data.animCount = (int)frameCountSB.Value;
        if (animName == null) return;
        data.animName = animName.Text;
        data.animType=(AnimType)animTypeButton.Selected;
        data.extraAnimInfo=ActionNameLE.Text;


    }

    public void ShowFileDialogue()
    { 
        ProgramHandler.instance.selectedImage = this;
        ProgramHandler.instance.fileDialog.Show();
    }

    public void GetAnimationLocation(string path)
    {
        data.spriteLoc= FileLoaderHandler.GetSpriteLocation(path);
        animStartFrameImg.Texture= FileLoaderHandler.GetCharacterAnim(data.spriteLoc);
        GD.Print(path);
    }
    void ConfirmDeleteAnimation()
    {
        ConfirmUI.Instance.SetConfirm(
            $"Would you like to delete {animName.Text}?", DeleteAnimation);
    }
    void DeleteAnimation()
    {
        ProgramHandler.instance.characterAnimationsPanel.DeleteAnimation(data.animID);
    }
}
