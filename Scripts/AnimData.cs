using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class AnimData
{
    public string filePath {  get; set; }
    public float animSpeed {  get; set; }
    /// <summary>
    /// The amount of frames in an animation
    /// </summary>
    [JsonConverter(typeof(IntFromDoubleConverter))]
    public int animationCount { get; set; }
    [JsonConverter(typeof(IntFromDoubleConverter))]
    public int activationType {  get; set; }
    public string activation {  get; set; }
    [JsonConverter(typeof(IntFromDoubleConverter))]
    public int animLenght { get; set; }

    public AnimData()
    {
        this.filePath = ProgramManager.VALUENOTSET;
        this.animSpeed = 1;
        this.animationCount = 1;
        this.activation = ProgramManager.VALUENOTSET;
        animLenght = 1;
        activationType = 0;
    }

    public AnimData(string filePath, float animSpeed, int animationCount, string activation, int animLenght)
    {
        this.filePath = filePath;
        this.animSpeed = animSpeed;
        this.animationCount = animationCount;
        this.activation = activation;
        this.animLenght = animLenght;
    }
} 
