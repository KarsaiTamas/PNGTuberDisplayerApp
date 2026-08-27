using Godot;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;

public partial class JsonVariableConverter
{

}

//AI made this script. Tested it, it works. It turns doubles and string to int. when it can.
//I had to use it, cus for some reason sometimes the json files get messed up,
//and the INT variable turned into a DOUBLE in the json file...
//And godot can't handle turning 1.0 into a friking int... sooo here we are
/// <summary>
/// Converts double value to int
/// </summary>
public class IntFromDoubleConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            // Handles both "1" and "1.0"
            
            double d = reader.GetDouble();
            return (int)Math.Round(d);
        }
        if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out int result))
        {
            return result;
        }
        throw new JsonException($"Cannot convert {reader.TokenType} to int.");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}