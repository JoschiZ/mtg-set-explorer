using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MTGSetExplorer.Core.Scryfall.Types;

[JsonConverter(typeof(ColorConverter))]
public enum Color
{
    White,
    Blue,
    Black,
    Red,
    Green
}

internal sealed class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString() ?? "";
        return value switch
        {
            "W" => Color.White,
            "B" => Color.Black,
            "R" => Color.Red,
            "U" => Color.Blue,
            "G" => Color.Green,
            _ => throw new JsonException($"Unknown color value: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToUpper());
    }
}