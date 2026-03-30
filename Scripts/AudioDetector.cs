using Godot; 

public partial class AudioDetector : Node
{
	public float TalkThreshold { get; set; } = 0.01f;
    //may put an amplyfier here 
    public DetectAudio audioDetector;
	public bool IsTalking { get; private set; }
	public float CurrentLevel { get; private set; } 
    private const float delay = 0.1f;
    private AudioEffectCapture effect;
	private AudioStreamPlayer micInput;
	private AudioStreamGenerator playBack;
	private int _micBusIndex;
	private float delayTimer; 

	public override void _Ready()
	{
        // Create a dedicated mic bus

        // Create mic input player on that bus
        
    }

	public override void _Process(double delta)
	{
        //ProcessTalking(delta);

        if (delayTimer > 0f)
        {
            delayTimer -= (float)delta;
            return;
        }  
        //GD.Print(IsTalking);
        Rpc("SetIsTalking", IsTalking);
        IsTalking = false;
        delayTimer = delay;

    }

	void ProcessTalking(double delta)
	{

        if (effect == null) return;
        if (delayTimer > 0f)
        {
            delayTimer -= (float)delta;
            return;
        }
        GD.Print("processing");
        int frames = effect.GetFramesAvailable();
        if (frames <= 0) return;

        var buffer = effect.GetBuffer(frames);

        float max = 0;

        for (int i = 0; i < buffer.Length; i++)
        {
            float s = buffer[i].X;
            if (s > TalkThreshold) { max = s; break; }
        }
        delayTimer = delay;
        CurrentLevel = max;
        GD.Print(CurrentLevel.ToString());
        if(IsTalking != max > TalkThreshold)
        {
            IsTalking = max > TalkThreshold;
            GD.Print(IsTalking ? "Talking" : "not talking");
            Rpc("SetIsTalking", IsTalking);
        }
    } 
    
    public void SetInputDevice(string deviceName)
    {
        /* For some reason this doesn't work right now so no changing yet
            // Stop current mic player
            if (micInput != null && micInput.Playing)
                micInput.Stop();

            // Clear any buffered audio from the previous device
            effect.ClearBuffer();
            // Set the input device on the AudioServer
            AudioServer.SetInputDeviceActive(false);
            AudioServer.InputDevice = deviceName;
            AudioServer.SetInputDeviceActive(true);
            GD.Print(deviceName);
            // Restart the mic player to use the new device 
            micInput.Play(); 
        */
        audioDetector.StopMonitoring();
        audioDetector.SetDeviceToUse(deviceName);
        audioDetector.StartMonitoring();

    }
    public void SetupAudio(int sceneID,long peerID)
	{
		SetMultiplayerAuthority((int)peerID); 
        if (!IsMultiplayerAuthority()) return;
        audioDetector = new DetectAudio();
        audioDetector.UseDefaultInputDevice();
        audioDetector.StartMonitoring();
        ChangeTreshold(.1f);
        audioDetector.OnVolumeThresholdExceeded += VolumeExceededTreshHold;

        /*
        GD.Print("audio");
        AudioServer.AddBus();
        _micBusIndex = AudioServer.BusCount - 1;
        AudioServer.SetBusName(_micBusIndex, "MicCapture" + sceneID);
        AudioServer.SetBusMute(_micBusIndex, true); // Mute playback so we don't hear ourselves
        AudioServer.SetBusSend(_micBusIndex, "Master");

        micInput = new AudioStreamPlayer();
        micInput.Stream = new AudioStreamMicrophone();
        micInput.Bus = "MicCapture"+ sceneID;
        AddChild(micInput);
        micInput.Play();
        // Add capture effect to the bus
        effect = new AudioEffectCapture(); 
        AudioServer.AddBusEffect(_micBusIndex, effect);
        */
        //playBack=

    }
    /*
	 currentCharacterTalkButton.ItemSelected += (index) =>
{
    string selectedDevice = currentCharacterTalkButton.GetItemText((int)index);
    audioDetector.SetInputDevice(selectedDevice);
};
	 */

    [Rpc(mode: MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    void SetIsTalking(bool isTalking)
    {
        IsTalking = isTalking;
    }
    public void VolumeExceededTreshHold(float volume)
    {

        IsTalking = true;
        //GD.Print($"current volume {volume}");
    }
    public override void _ExitTree()
	{
		if (micInput != null && micInput.Playing)
			micInput.Stop();
	}
    public void ChangeTreshold(float threshold)
    {
        audioDetector.Threshold =  threshold;
        TalkThreshold = threshold;
    }
    public void RemovingAudio()
    {
        audioDetector.OnVolumeThresholdExceeded -= VolumeExceededTreshHold;
        audioDetector.Dispose();
    }
}
