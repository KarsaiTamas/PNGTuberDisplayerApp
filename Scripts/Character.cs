using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public partial class Character : Control
{
    Dictionary<string, OAnim> outfitAnimations;
	public long peerId = 1;
    [Export]
    public int sceneID;
    [Export]
    public int characterID;
    [Export]
    public string cName;
    [Export]
    public int outfitID;
    bool isBaseOutfitloaded;
    public CharacterType type;
    //List<ImageTexture> blink; 
    AudioDetector audioDetector;
    TextureRect mainTexture;
    TextureRect eyesTexture;
    TextureRect mouthTexture;
    TextureRect outfitTexture;
    Button characterInteractButton;
    OptionButton selectedOutfit;
    Button deleteCharacterFromScene;
    public Vector2 position;
    public Vector2 size;
    public bool mirrored;
    public float blinkTimer;
    float blinkFrameTimer;
    float talkFrameTimer;
    public bool isEdited;
    public bool isSimple;
    public bool isOnlineCharacter;
    bool editedCheckForVisibility=false;
    bool uiIsVisible=false;
    public override void _EnterTree()
    {
        //blink=new List<ImageTexture>();
        outfitAnimations=new Dictionary<string, OAnim>();
        audioDetector=new AudioDetector();
        isBaseOutfitloaded = false;
        AddChild(audioDetector);
        /*for (int i = 1; i < 6; i++) 
        {
            ImageTexture tempTexture = new ImageTexture();
            tempTexture.SetImage(Image.LoadFromFile($"D:\\dokumentumok\\képek_Videók\\darw\\GooseBlink000{i}.png"));
            blink.Add(tempTexture);
        }*/
        mainTexture = GetNode<TextureRect>("VC/CInteractButton/MainSprite");
        eyesTexture = GetNode<TextureRect>("VC/CInteractButton/EyesTR");
        mouthTexture = GetNode<TextureRect>("VC/CInteractButton/MouthTR");
        outfitTexture = GetNode<TextureRect>("VC/CInteractButton/SimpleOutfitTR");
        characterInteractButton = GetNode<Button>("VC/CInteractButton");
        selectedOutfit = GetNode<OptionButton>("VC/HC/MCOutfits/OptionButton");
        deleteCharacterFromScene = GetNode<Button>("VC/HC/MCDelete/Button");
        selectedOutfit.ItemSelected += ChangeOutfit;
        //mirrored = true;
        //give animation cycle speed
        //give animation length
        isEdited = false;
        size = new Vector2(512, 512);
        characterInteractButton.CustomMinimumSize = new Vector2(512,512);
        ToggleUIElements();

    }
    public override void _Ready()
    {
        characterInteractButton.ButtonDown +=
        () =>
        {
            ProgramHandler.instance.selectedCharacter = this;
            /*
            blinkPos++;
            if (blinkPos > blink.Count - 1)
            {
                blinkPos = 0;
            }
            texture.TextureNormal = blink[blinkPos];*/
        };
        characterInteractButton.ButtonUp += () =>
        {
            ToggleUIElements();
            ProgramHandler.instance.selectedCharacter = null;
            editedCheckForVisibility = false;
        };
        deleteCharacterFromScene.ButtonUp += DeleteCharacterPopup;
    }

    private void ToggleUIElements()
    { 
        if (editedCheckForVisibility) return; 
        uiIsVisible = !uiIsVisible;
        if (uiIsVisible)
        {
            selectedOutfit.Hide();
            deleteCharacterFromScene.Hide();
        }
        else
        {
            selectedOutfit.Show();
            deleteCharacterFromScene.Show();
        }
    }

    public void CharacterActions(InputEventMouse mouse,Vector2 currentMouseLocation, Vector2 previousMouseLocation)
    {
        if (mouse.ShiftPressed)
        {
            ScaleCharacter(currentMouseLocation, previousMouseLocation);
        }
        else
        {
            MoveCharacterTowardsMouse(currentMouseLocation - previousMouseLocation);
        }
        
    }
    public void ScaleCharacter(Vector2 currentMouseLocation, Vector2 previousMouseLocation)
    {
        float valueChange=5;
        characterInteractButton.CustomMinimumSize =
               currentMouseLocation.Y < previousMouseLocation.Y ?
               new Vector2( 
               Godot.Mathf.Clamp(characterInteractButton.CustomMinimumSize.Y + valueChange, 50, 1024),
               Godot.Mathf.Clamp(characterInteractButton.CustomMinimumSize.Y + valueChange, 50, 1024)) :
               new Vector2( 
               Godot.Mathf.Clamp(characterInteractButton.CustomMinimumSize.Y - valueChange, 50, 1024) ,
               Godot.Mathf.Clamp(characterInteractButton.CustomMinimumSize.Y - valueChange, 50, 1024));

        var wSize = GetViewport().GetVisibleRect().Size;
        GlobalPosition =
            new Vector2(
                Godot.Mathf.Clamp(GlobalPosition.X, -((Size.X / 2) * Scale.X), wSize.X - ((Size.X / 2) * Scale.X)),
                Godot.Mathf.Clamp(GlobalPosition.Y, -((Size.Y / 2) * Scale.Y), wSize.Y - ((Size.Y / 2) * Scale.Y)));

        if(position != GlobalPosition || size!= characterInteractButton.CustomMinimumSize)
        {
            isEdited = true;
            editedCheckForVisibility = true;

        }
        position = GlobalPosition;
         
        size = characterInteractButton.CustomMinimumSize;

    }
    public void MoveCharacterTowardsMouse(Vector2 move)
    {
        var wSize = GetViewport().GetVisibleRect().Size;
        GlobalPosition =
            new Vector2(
                Godot.Mathf.Clamp(GlobalPosition.X + move.X, -((Size.X / 2) * Scale.X), wSize.X - ((Size.X / 2) * Scale.X)),
                Godot.Mathf.Clamp(GlobalPosition.Y + move.Y, -((Size.Y / 2) * Scale.Y), wSize.Y - ((Size.Y / 2) * Scale.Y)));
        if (position != GlobalPosition)
        {
            isEdited = true;
            editedCheckForVisibility = true;

        }
        position = GlobalPosition; 
    }

    public void SetupCharacter()
    {
        var charactersData= DataBaseHandler.GetCharacterInSaves(characterID.ToString());
        var sceneData=DataBaseHandler.GetCharacterInScene(characterID.ToString());
        if (sceneData == null) return;
        outfitID= sceneData[DataBaseHandler.outfitID].AsInt32();
        var cData= DataBaseHandler.GetOutfit(characterID, outfitID);

        cName = charactersData["name"].AsString();
        isSimple = cData == null?false: cData[DataBaseHandler.oIsSimple].AsBool();
        mirrored= sceneData[DataBaseHandler.mirrored].AsBool();
        Flip(mirrored);

        position = GlobalPosition;

        size = characterInteractButton.CustomMinimumSize;
        editedCheckForVisibility = false;
        selectedOutfit.Clear();
        var outfits = DataBaseHandler.GetOutfits(characterID);
        GD.Print("Adding outfits to selected ones");
        foreach (Godot.Collections.Dictionary outfit in outfits)
        {
            selectedOutfit.AddItem(outfit["name"].AsString(), (int)outfit["id"]);
            GD.Print("asdasasdasdasdasdasdasd");
            GD.Print((int)outfit["id"]);
            GD.Print(outfit["name"]);
            GD.Print("asdasasdasdasdasdasdasd");
        }
        GD.Print("selected outfits");
        for (int i = 0; i < selectedOutfit.ItemCount; i++)
        {
            GD.Print("i:"+i);
            GD.Print(selectedOutfit.GetItemId(i));
            GD.Print(selectedOutfit.GetItemText(i));
            if(selectedOutfit.GetItemId(i)== outfitID)
            {
                selectedOutfit.Select(i);
                break;
            }
        }


        // type = charactersData[DataBaseHandler.cCharacterType].AsString();
    }

    public void SetSize(Vector2 scale)
    {
        if (size != scale)
        {
            isEdited = true;
            editedCheckForVisibility = true;

        }
        this.size = scale;
        characterInteractButton.CustomMinimumSize= scale; 

    }

    public void SetupOnlineCharacter()
    {
        isSimple = GetIsSimpleOnline();
        mirrored = false; 
        GetOnlineCharacterName();
    }
    public bool GetIsSimpleOnline()
    {
        GD.Print("Implement data from user at character 206");
        return false;
    }
    public string GetOnlineCharacterName()
    {
        GD.Print("Implement name character 211");
        return "default name";
    }
    public void Flip(bool isFlipped)
    {
        mainTexture.FlipH=isFlipped;
        eyesTexture.FlipH = isFlipped;
        mouthTexture.FlipH =isFlipped;
        outfitTexture.FlipH = isFlipped;
    }

    public void Blink(float delta)
    {
        if (blinkTimer > 0)
        {
            blinkTimer -= delta;
            return;
        }
        if(blinkFrameTimer > 0)
        {
            blinkFrameTimer -= delta;
            return;
        }
        var blinkAnim = outfitAnimations["Blink"];
        blinkAnim.curFrame++;
            blinkFrameTimer = blinkAnim.frameLength;
        if (blinkAnim.curFrame > blinkAnim.animationSequence.Count - 1)
        {
            blinkAnim.curFrame=0;
            blinkTimer = (float)GD.RandRange(5.0, 7.0);
        }
        eyesTexture.Texture = blinkAnim.animationSequence[blinkAnim.curFrame];

    }

    public void Talking(float delta)
    {
        if (talkFrameTimer > 0)
        {
            talkFrameTimer -= delta;
            return;
        }
        var talkAnim = outfitAnimations["Talk"];
        talkFrameTimer= talkAnim.frameLength;
        if (!audioDetector.IsTalking)
        {
            //GD.Print("adsdasdasd");
            mouthTexture.Texture = talkAnim.animationSequence[0];
            return;
        }
        talkAnim.curFrame++;
        if (talkAnim.curFrame > talkAnim.animationSequence.Count - 1)
        {
            talkAnim.curFrame = 0; 
        }
        //GD.Print("nononono");
        mouthTexture.Texture = talkAnim.animationSequence[talkAnim.curFrame];



    }
    public void ChangeOutfit(long newOutfitID)
    {
        GD.Print(newOutfitID);
        int id= selectedOutfit.GetItemId((int)newOutfitID);
        if (newOutfitID == id) return; 
        outfitID = id;
        SetupAnimations(outfitID);
    }
    public void SetupAnimations(int outfitID)
    {
        var outfit= DataBaseHandler.GetOutfit(characterID, outfitID);
        isSimple= outfit==null?false:(bool)outfit[DataBaseHandler.oIsSimple];
        string simpleOutfit = isSimple ? (string)outfit[DataBaseHandler.outfitLocation] :"not loaded";
        
        LoadAnimationsInOutfit(outfitID,isSimple,simpleOutfit);
        
        mainTexture.Texture = outfitAnimations["Idle"].animationSequence[0];
        switch (type)
        {
            case CharacterType.character_png: 
                var talkAnim = outfitAnimations["Talk"];
                audioDetector.SetupAudio(sceneID, peerId);
                audioDetector.SetInputDevice(talkAnim.extraAnimInfo);
                break;
            case CharacterType.prop:
            default:

                break;
        }
        //audioDetector.SetInputDevice(talkAnim.extraAnimInfo);
    }
    public void SetupOnlineAnimations()
    {
        GD.Print("implement online animations at 197 in character");
        var anims = DataBaseHandler.GetCharacterAnimations(characterID, outfitID);
        if (anims == null) return;
        outfitAnimations.Clear();
         
            /*
            outfitAnimations.Add(
                anim["name"].AsString(),
                new OAnim(
                anim[DataBaseHandler.oLocation].AsString(),
                (float)anim[DataBaseHandler.oAnimLength].AsDouble(),
                anim[DataBaseHandler.oAnimCount].AsInt32(),
                (AnimType)anim[DataBaseHandler.oAnimType].AsInt32(),
                anim[DataBaseHandler.oAnimExtraInfo].AsString()
                ));
            */
        

    }
    public void DeleteCharacterPopup()
    {
        ConfirmUI.Instance.SetConfirm($"Delete character: {cName}", DeleteCharacter);
    }
    public void DeleteCharacter()
    {
        if(!isOnlineCharacter) 
            DataBaseHandler.DeleteCharacterFromScene(sceneID);
        SceneHandler.instance.RemoveCharacterFromListByID(sceneID);
        if (isOnlineCharacter)
        {
            //removePlayer
            ProgramHandler.network.RemoveOnlinePlayerFromScene(peerId);
        }
        UnsubingFromAudioHandling();
        QueueFree(); 
    }

    void LoadAnimationsInOutfit(int outfitID, bool isSimple, string simpleOutfit="not loaded")
    {
        
        outfitTexture.Texture = null;
        if (isSimple)
            outfitTexture.Texture = 
                FileLoaderHandler.GetCharacterAnim(simpleOutfit);
        
        if (isBaseOutfitloaded && isSimple) return;        
        
        var anims = DataBaseHandler.GetCharacterAnimations(characterID, isSimple?1: outfitID);
        if (anims == null) return;
         
        GD.Print($"CID{characterID},OID{outfitID}");
        outfitAnimations.Clear();

        GD.Print(anims);
        foreach (Godot.Collections.Dictionary anim in anims)
        {

            outfitAnimations.Add(
                anim["name"].AsString(),
                new OAnim(
                anim[DataBaseHandler.oSpriteFile].AsString(),
                (float)anim[DataBaseHandler.oAnimLength].AsDouble(),
                anim[DataBaseHandler.oAnimCount].AsInt32(),
                (AnimType)anim[DataBaseHandler.oAnimType].AsInt32(),
                anim[DataBaseHandler.oAnimExtraInfo].AsString()
                ));
        }
        isBaseOutfitloaded = isSimple||outfitID==1;
        GD.Print("outfit loaded");
        GD.Print(isBaseOutfitloaded);
        GD.Print(isSimple);
        GD.Print(outfitID);


    }

    public void UnsubingFromAudioHandling()
    {
        audioDetector.RemovingAudio();
    }
}
