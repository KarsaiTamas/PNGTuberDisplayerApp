using Godot;
using NAudio.CoreAudioApi;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    public static AudioManager instance;
    public float TalkThreshold { get; set; } = 0.01f;
    //may put an amplyfier here 
    public Dictionary<string, AudioDetector> audioDetectors; 
    public bool IsTalking { get; private set; }
    public float CurrentLevel { get; private set; }
    private const float delay = 0.1f;
    private AudioEffectCapture effect;
    private AudioStreamPlayer micInput; 
    private int _micBusIndex;
    private float delayTimer;
    string applicationName;
    //public bool isLocal = false;
    //public bool characterAdded = false;
    public override void _EnterTree()
    {
        instance=this;
    }
    public override void _Ready()
    {
        audioDetectors=new Dictionary<string, AudioDetector>();
        // Create a dedicated mic bus

        // Create mic input player on that bus
    }

    public override void _Process(double delta)
    { 
        if (delayTimer > 0f)
        {
            delayTimer -= (float)delta;
            return;
        }
        foreach (var item in audioDetectors)
        {
            if (item.Value.isCustom)
            {
                item.Value.MonitorCustomAudio();
            }
        } 
        delayTimer = delay;
         
        IsTalking = false;

    }
     

    public void AddAudioMonitoring(string deviceName,bool isCustom)
    {
        GD.Print("Setting up audio");
        GD.Print(deviceName);
        if (deviceName.Equals(ProgramManager.VALUENOTSET)) return;
        AudioDetector aud = new AudioDetector();
        audioDetectors.Add(deviceName,aud); 
        audioDetectors[deviceName].SetDeviceToUse(deviceName, isCustom);
        if (isCustom) return;
        audioDetectors[deviceName].StartMonitoring();

    }
    public void SetupAudio(string device,long peerID, bool isCustom)
    {
        SetMultiplayerAuthority((int)peerID);
        if (!IsMultiplayerAuthority()) return; 
        if (isCustom) return;  
         

    }
     
    public override void _ExitTree()
    {
        if (micInput != null && micInput.Playing)
            micInput.Stop();
    } 
    public void RemovingAudio(string device)
    {
        if (!audioDetectors.ContainsKey(device)) return;
        audioDetectors[device].StopMonitoring();
        audioDetectors[device].Dispose();
        audioDetectors.Remove(device);
    }
    public void AddCharacter(string device,Character character)
    {
        if (!audioDetectors.ContainsKey(device)) return;
        if (audioDetectors[device].characterList.Contains(character)) return;
        audioDetectors[device].characterList.Add(character);
    }
    public void RemoveCharacter(string device, Character character)
    {
        if (!audioDetectors.ContainsKey(device)) return;
        audioDetectors[device].characterList.Remove(character);
        if (audioDetectors[device].characterList.Count == 0) RemovingAudio(device);
    }

    /// <summary>
    /// Lists all active audio input (capture) devices, e.g. microphones.
    /// </summary>
    public static IReadOnlyList<string> GetInputDevices()
    {
        var list = new List<string>();
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        for (int i = 0; i < devices.Count; i++)
            list.Add(devices[i].FriendlyName);
        return list;
    }

    /// <summary>
    /// Lists all active audio output (render) devices, e.g. speakers, headphones.
    /// </summary>
    public static IReadOnlyList<string> GetOutputDevices()
    {
        var list = new List<string>();
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        for (int i = 0; i < devices.Count; i++)
            list.Add(devices[i].FriendlyName);
        return list;
    }
}
