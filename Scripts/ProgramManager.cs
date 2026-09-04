using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ProgramManager : Node
{
    public static ProgramManager instance;
    public Character selectedCharacter;
    public string keyToPress=" ";
    public List<Character> spawnedCharacters;
    public Vector2 previousMouseLocation = Vector2.Zero;
    public Vector2 currentMouseLocation = Vector2.Zero;
    public const string VALUENOTSET = "Not Set";
    public RandomNumberGenerator rng = new RandomNumberGenerator();
    public override void _EnterTree()
    {
        Init();
    }
    void Init()
    {
        instance = this;
        spawnedCharacters=new List<Character>();
        SpawnManager.Init();
        SaveLoadManager.Init();
        GetTree().AutoAcceptQuit=false;
    }
    public override void _Notification(int what)
    {
        if (what == (int)NotificationWMCloseRequest)
        {
            if (!CharacterManager.instance.isEdited && !SceneManager.instance.isEdited)
            {
                GetTree().Quit();
                return;
            }
            UIManager.instance.SaveBeforeQuitPopup();
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        foreach (var character in spawnedCharacters)
        {
            character.Blink((float)delta);
            character.BaseAnimation((float)delta);
            character.Talk((float)delta); 
            
        }
    }
    bool isHosting=false;
    bool isJoined=false;
    public override void _Input(InputEvent e)
    {/*
        if (e is InputEventKey key1)
        {
            if (key1.Keycode.Equals(Key.H) && key1.IsReleased())
            {
                if (isHosting) return;
                isHosting = true;
                isJoined = true;
                NetworkManager.instance.HostLocalServer();
            }
            if (key1.Keycode.Equals(Key.J) && key1.IsReleased())
            {
                if (isJoined) return;
                isHosting = true;
                isJoined = true;
                NetworkManager.instance.JoinLocalServer();
            }
        }*/
        if (spawnedCharacters.Count == 0) return;
        if (CharacterManager.instance.Visible) return;
        
            if (e is InputEventMouseButton mouseB)
        {
            if (mouseB.Pressed)
            {
                GD.Print("click");

                SelectCharacter(mouseB.Position);
                currentMouseLocation = mouseB.Position;
                previousMouseLocation = mouseB.Position;
            }
            if (mouseB.IsReleased())
            {
                if (selectedCharacter == null) return;
                if(!selectedCharacter .isChanged)
                UIManager.instance.CharacterOutfitToggle(selectedCharacter,!selectedCharacter.uiVisible);
                DeselectCharacter();
                return;
            }
        }
        if (selectedCharacter == null) return;

        if (e is InputEventMouse mouse)
        {
            currentMouseLocation = mouse.Position;
            GD.Print("asdasd");
            SceneManager.instance.isEdited=selectedCharacter.CharacterMovement(mouse,currentMouseLocation,previousMouseLocation);
            previousMouseLocation = mouse.Position;
            //GD.Print("mouse clicked");
        }

        if (e is InputEventKey key)
        {
            if (key.AsText().ToUpper().Equals(keyToPress)) GD.Print(key.AsText());

            if (key.Keycode.Equals(Key.F) && key.IsReleased()) SceneManager.instance.isEdited= selectedCharacter.MirrorCharacter();
            
            if((key.Keycode.Equals(Key.Up) || key.Keycode.Equals(Key.W)) && key.IsReleased()) selectedCharacter.ChangeCharacterLayer(1);
            
            if ((key.Keycode.Equals(Key.Down) || key.Keycode.Equals(Key.S)) && key.IsReleased()) selectedCharacter.ChangeCharacterLayer(-1);
            
        }
    }
    void SelectCharacter(Vector2 pos)
    {
        selectedCharacter = spawnedCharacters.Where(c => c.InSelectionZone(pos) && !c.isEditor).OrderBy(e=>e.data.layer).LastOrDefault();
    }
    void DeselectCharacter()
    {
        selectedCharacter = null;
    }
    public void ReloadCharactersByID(string ID)
    {
        foreach (var character in GetSpawnedCharactersByID(ID))
        {
            character.LoadCharacterData(SaveLoadManager.GetCharacterIntID(ID));
        }
    }
    public List<Character> GetSpawnedCharactersByID(string ID)
    {
        return spawnedCharacters.Where(c => c.data.ID.Equals(ID)).ToList();
    }
}
