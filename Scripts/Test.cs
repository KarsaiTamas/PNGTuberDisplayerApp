using Godot;
using System;

public partial class Test : Node
{
    public AudioDetector audio;
    float timer;
    public override void _EnterTree()
    {
        audio = new AudioDetector();
        AddChild(audio);
        float twitchBotSound= DetectAudio.GetApplicationVolume("TunaszTwitchbot");
    }

    public override void _Process(double delta)
    {
        if(timer>0)
        {
            timer -= (float)delta;
            return;     
        }
        float twitchBotSound= DetectAudio.GetApplicationVolume("TunaszTwitchbot");
        GD.Print(twitchBotSound);
        timer = .1f;
    }
}
