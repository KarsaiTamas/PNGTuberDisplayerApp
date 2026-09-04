using Godot;
//using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using SLM = SaveLoadManager;
using PM = ProgramManager;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class Character : Control
{
    public CharacterData data;
    public bool uiVisible;
    public bool isChanged;
    public bool isTalking;
    float idleTimer=0;
    float blinkTimer=1;
    float baseAnimFrameDelay=.5f;
    float eyeAnimFrameDelay = .1f;
    float mouthAnimFrameDelay=.5f;
    int baseAnimationIndex = 0;
    int baseTextureCurFrame=0;
    int eyeTextureCurFrame=0;
    int mouthTextureCurFrame=0;
    public bool isEditor=true;
    bool isOnline=false;
    bool alreadyTalked = false;
    public bool isLocal = true; 
    #region UI Elements

    public AudioDetector audioDetector;
    public TextureRect mainTexture;
    public TextureRect eyesTexture;
    public TextureRect mouthTexture;
    public TextureRect outfitTexture;
    public Button characterInteractButton;
    public Button deleteCharacterFromScene;
    public OptionButton selectedOutfit;
    public Dictionary<int, (EBaseAnims animToEffect, List<ImageTexture > texture)> CharacterAnims;
    public List<ImageTexture >  outfitImages;
      
    #endregion
    public override void _EnterTree()
    {
        InitUI();
    }
    private void InitUI()
    {
        data = new CharacterData();
        outfitImages = new List<ImageTexture>();
        mainTexture = GetNode<TextureRect>("VC/CInteractButton/MainSprite");
        eyesTexture = GetNode<TextureRect>("VC/CInteractButton/EyesTR");
        mouthTexture = GetNode<TextureRect>("VC/CInteractButton/MouthTR");
        outfitTexture = GetNode<TextureRect>("VC/CInteractButton/SimpleOutfitTR");
        deleteCharacterFromScene = GetNode<Button>("VC/HC/RemoveB");
        selectedOutfit = GetNode<OptionButton>("VC/HC/OutfitsOB");
        characterInteractButton = GetNode<Button>("VC/CInteractButton");
        isChanged = false;
        UIManager.instance.CharacterOutfitToggle(this,false);
        deleteCharacterFromScene.ButtonDown += RemoveCharacterFromScene;
        CharacterAnims = new Dictionary<int, (EBaseAnims textureToEffect, List<ImageTexture> texture)>();
        data.position = GlobalPosition;
        selectedOutfit.ItemSelected += OnOutfitSelected;
    }


    /// <summary>
    /// After we spawn the character do some extra init
    /// </summary>
    public void SpawnInit(CharacterData data)
    {
        isEditor=false;
        this.data = data;
        LoadCharacterData(SLM.GetCharacterIntID(data.ID));
        UpdateUIWithData();
    }
    public void SpawnOnline()
    {
        isEditor = false;
        isOnline = true;
        isLocal = IsMultiplayerAuthority();
            
        
        GD.Print(IsMultiplayerAuthority());
    }

    public void AddAudioMonitorion()
    {
        if (!AudioManager.instance.audioDetectors.ContainsKey(data.animData[(int)EBaseAnims.Mouth].activation))
        {
            GD.Print("should monitor");
            AudioManager.instance.AddAudioMonitoring(data.animData[(int)EBaseAnims.Mouth].activation, data.animData[(int)EBaseAnims.Mouth].activationType == 0);
        }
        AudioManager.instance.AddCharacter(data.animData[(int)EBaseAnims.Mouth].activation, this);

    }

    public void LoadCharacterData(int ID)
    {
        var lData =SLM.LoadCharacterData(ID);
        RemoveCharacterFromAudio();
        lData.GiveCharacterData(data);
        AddAudioMonitorion();
    }
    public void LoadSceneData(CharacterData data)
    {
        data.GiveSceneData(data);
    }
    #region Button actions

    private void OnOutfitSelected(long index)
    {
        ChangeOutfit((int)index);
    }

    public void RemoveCharacterFromScene()
    {
        SceneManager.instance.isEdited = true;
        ProgramManager.instance.spawnedCharacters.Remove(this);
        SceneManager.instance.charactersOnScene.Remove(this);
        RemoveCharacterFromAudio();
        QueueFree(); 
    }
    public void RemoveOnlineCharacter()
    {
        ProgramManager.instance.spawnedCharacters.Remove(this);
        QueueFree();

    }
    public void RemoveCharacterFromAudio()
    { 
        AudioManager.instance.RemoveCharacter(data.animData[(int)EBaseAnims.Mouth].activation, this);
    }
    #endregion
    #region Movement_Editing
    public bool CharacterMovement(InputEventMouse mouse, Vector2 currentMouseLocation, Vector2 previousMouseLocation)
    {
        if (mouse.ShiftPressed)
        {
            return ScaleCharacter(currentMouseLocation, previousMouseLocation);
        }
        else
        {
            return MoveCharacterTowardsMouse(currentMouseLocation - previousMouseLocation);
        }

    }


    public bool ScaleCharacter(Vector2 currentMouseLocation, Vector2 previousMouseLocation)
    {
        float valueChange = 5;
        characterInteractButton.CustomMinimumSize =
               currentMouseLocation.Y < previousMouseLocation.Y ?
               new Vector2(
               Godot.Mathf.Clamp(characterInteractButton.CustomMinimumSize.Y + valueChange, 50, 1024),
               Godot.Mathf.Clamp(characterInteractButton.CustomMinimumSize.Y + valueChange, 50, 1024)) :
               new Vector2(
               Godot.Mathf.Clamp(characterInteractButton.CustomMinimumSize.Y - valueChange, 50, 1024),
               Godot.Mathf.Clamp(characterInteractButton.CustomMinimumSize.Y - valueChange, 50, 1024));

        var wSize = GetViewport().GetVisibleRect().Size;
        GlobalPosition =
            new Vector2(
                Godot.Mathf.Clamp(GlobalPosition.X, -((Size.X / 2) * Scale.X), wSize.X - ((Size.X / 2) * Scale.X)),
                Godot.Mathf.Clamp(GlobalPosition.Y, -((Size.Y / 2) * Scale.Y), wSize.Y - ((Size.Y / 2) * Scale.Y)));
         
        data.position = GlobalPosition;
        isChanged = data.size!= characterInteractButton.CustomMinimumSize;

        data.size = characterInteractButton.CustomMinimumSize;
        return !isOnline;
    }

    public bool MoveCharacterTowardsMouse(Vector2 move)
    {
        var wSize = GetViewport().GetVisibleRect().Size;
        GlobalPosition =
            new Vector2(
                Godot.Mathf.Clamp(GlobalPosition.X + move.X, -((Size.X / 2) * Scale.X), wSize.X - ((Size.X / 2) * Scale.X)),
                Godot.Mathf.Clamp(GlobalPosition.Y + move.Y, -((Size.Y / 2) * Scale.Y), wSize.Y - ((Size.Y / 2) * Scale.Y)));

        isChanged = data.position!=GlobalPosition;
        data.position = GlobalPosition;
        return !isOnline;
    }
    public bool ChangeCharacterLayer(int amount)
    {
        data.layer = Math.Clamp(data.layer + amount, -100,100);
        this.ZIndex = data.layer;
        isChanged = true;
        return !isOnline;
    }
    public bool MirrorCharacter()
    {
        data.mirrored = !data.mirrored;
        Flip(data.mirrored);
        isChanged = true;
        return !isOnline;
    }

    public void Flip(bool isFlipped)
    {
        mainTexture.FlipH = isFlipped;
        eyesTexture.FlipH = isFlipped;
        mouthTexture.FlipH = isFlipped;
        outfitTexture.FlipH = isFlipped;
    }

    public void Blink(float delta)
    {
        if (blinkTimer > 0)
        {
            blinkTimer-=delta;
            return;
        }
        var eye = data.animData[(int)EBaseAnims.Eyes]; 
        
        PlayAnimation(eyesTexture, (int)EBaseAnims.Eyes,ref eyeTextureCurFrame, eye.animationCount,ref eyeAnimFrameDelay, eye.animSpeed, delta);
        if (eyeTextureCurFrame == 0 && eyeAnimFrameDelay < 0)
        {
            blinkTimer = data.blinkFrequency+PM.instance.rng.RandfRange(-2,2);
        }
    }
    public void BaseAnimation(float delta)
    {
        var baseT = data.animData[baseAnimationIndex];
        PlayAnimation(mainTexture, (int)EBaseAnims.Base,ref baseTextureCurFrame, baseT.animationCount,ref baseAnimFrameDelay, baseT.animSpeed, delta, false);
        
    }
    public void Talk(float delta)
    {
        if (!isTalking) { StopTalking(delta); return; }
        var mouth = data.animData[(int)EBaseAnims.Mouth];
        PlayAnimation(mouthTexture, (int)EBaseAnims.Mouth,ref mouthTextureCurFrame, mouth.animationCount,ref mouthAnimFrameDelay, mouth.animSpeed, delta);
        if (!alreadyTalked)
        {
            OnlineTalk(true);
            alreadyTalked = true;
        }
    }
    public void StopTalking(float delta)
    {
        if (mouthAnimFrameDelay > 0)
        {
            mouthAnimFrameDelay -= delta;
            return;
        }
        if (!alreadyTalked) return;
        isTalking = false;
        OnlineTalk(false);
        alreadyTalked=false;
        mouthTextureCurFrame = 0;
        mouthAnimFrameDelay= 0;
        PlayAnimation(mouthTexture, (int)EBaseAnims.Mouth, ref mouthTextureCurFrame, 0, ref mouthAnimFrameDelay, 0, 0);
    }
    public void OnlineTalk(bool isTalking)
    {
        NetworkManager.instance.SendTalkData(isTalking, Multiplayer.GetUniqueId());
    }

    #endregion
    #region Helper functions
    public bool InSelectionZone(Vector2 pos)
    {
        float xPosMin = data.position.X + characterInteractButton.Position.X;
        float yPosMin = data.position.Y + characterInteractButton.Position.Y;
        float xPosMax = data.position.X + data.size.X + characterInteractButton.Position.X;
        float yPosMax = data.position.Y + data.size.Y + characterInteractButton.Position.Y;
        float x = pos.X;
        float y = pos.Y;
        return (x > xPosMin && y > yPosMin &&
            x < xPosMax && y < yPosMax);
    }

    public void UpdateUIWithData()
    {
        if (!isEditor && !isOnline)
        {
            GlobalPosition = data.position;
            characterInteractButton.CustomMinimumSize= data.size;
        }
        CharacterAnims.Clear();
        GD.Print("Loading animations");
        for (int i = 0; i < data.animData.Count; i++)
        {
            var animData = data.animData[i];
            int animToOverride = i > (int)EBaseAnims.Mouth ? (int)EBaseAnims.Base : i;
            GD.Print(animToOverride);
            GD.Print(animData.filePath);
            CharacterAnims.Add(i,((EBaseAnims)animToOverride, SLM.GetAnimSheet(animData.filePath, animData.animationCount))); 
              
        }
        GD.Print("Loading outfits");
        foreach (var item in data.outfits)
        {
            outfitImages.Add(SLM.GetAnimSheet(item.outfitFilePath, 1)[0]);
        }
        ChangeAnimation(mainTexture, (int)EBaseAnims.Base, 0,false);
        ChangeAnimation(eyesTexture, (int)EBaseAnims.Eyes, 0);
        ChangeAnimation(mouthTexture, (int)EBaseAnims.Mouth, 0);
        AddOutfitsToSelection();
        ChangeOutfit(data.selectedOutfit);

        Flip(data.mirrored);
    }
    public void ChangeAnimation(TextureRect textureUI,int anim,int frame, bool nullable = true)
    {
        try
        {
            textureUI.Texture = CharacterAnims[anim].texture[frame];
            
            textureUI.Visible = !data.animData[anim].filePath.Equals(ProgramManager.VALUENOTSET);
        }
        catch (Exception e)
        { 
            if(nullable)
            {
                textureUI.Visible = false;
                return;
            }
            textureUI.Texture = SaveLoadManager.GetDefaultImage();
        }
    }
    public void PlayAnimation(TextureRect textureUI, int anim,ref int frame,int maxFrame, ref float delay,float setDelay,float delta,bool nullable = true) 
    {
        if (delay > 0)
        {
            delay -= delta;
            return;
        }
        delay=setDelay;
        frame++;
        if (frame >= maxFrame) frame = 0; 
        ChangeAnimation(textureUI, anim, frame, nullable);
    }
    public void AddOutfitsToSelection()
    {
        selectedOutfit.Clear();
        foreach (var item in data.outfits)
        {
            selectedOutfit.AddItem(item.outfitName);
        }
        SelectOutfit();
    }

    public void ChangeOutfit(int outfit)
    {
        //outfitTexture
        if (!isEditor && !isOnline) 
        {
            SceneManager.instance.isEdited = data.selectedOutfit != outfit;    
        }
        try
        {
            outfitTexture.Visible = outfit != 0;
            data.selectedOutfit = outfit;
            
            if (outfit == 0) return;
            outfitTexture.Texture = outfitImages[outfit];
            GD.Print("cool outfit loaded: " + outfit);
        }
        catch (Exception e)
        {
            GD.PrintErr("Missing animation");
            outfitTexture.Texture = SLM.GetDefaultImage();
        }
    }
    public void SelectOutfit()
    {
        if (selectedOutfit.ItemCount >= data.selectedOutfit) data.selectedOutfit = 0;
        selectedOutfit.Select(data.selectedOutfit);

    }
    #endregion
}
