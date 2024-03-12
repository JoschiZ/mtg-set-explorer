using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MTGSetExplorer.Core.Scryfall.Types;

[JsonConverter(typeof(CardConverter))]
public abstract class ScryfallCardBase
{
    public Guid Id { get; init; }
    
    [JsonPropertyName("scryfall_uri")]
    public required string ScryfallUri { get; init; }
    
    [JsonPropertyName("layout")]
    public required Layout Layout { get; init; }

    [JsonPropertyName("cmc")]
    public decimal ConvertedManaCost { get; init; } = 0;
    
    [JsonPropertyName("color_identity")]
    public ImmutableArray<Color> ColorIdentity { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("type_line")] 
    public string TypeLine { get; init; } = "";
}

public class CardConverter : JsonConverter<ScryfallCardBase>
{
    public override ScryfallCardBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDoc.RootElement;
        var typeDiscriminator = jsonObject.GetProperty("layout").GetString();

        return typeDiscriminator switch
        {
            "split" => JsonSerializer.Deserialize<SplitCard>(jsonObject.GetRawText(), options),
            "flip" => JsonSerializer.Deserialize<DoubleFacedCard>(jsonObject.GetRawText(), options),
            "transform" => JsonSerializer.Deserialize<DoubleFacedCard>(jsonObject.GetRawText(), options),
            "modal_dfc" => JsonSerializer.Deserialize<DoubleFacedCard>(jsonObject.GetRawText(), options),
            "adventure" => JsonSerializer.Deserialize<SplitCard>(jsonObject.GetRawText(), options),
            "battle" => JsonSerializer.Deserialize<DoubleFacedCard>(jsonObject.GetRawText(), options),
            "double_faced_token" => JsonSerializer.Deserialize<DoubleFacedCard>(jsonObject.GetRawText(), options),
            "art_series" => JsonSerializer.Deserialize<DoubleFacedCard>(jsonObject.GetRawText(), options),
            "reversible_card" => JsonSerializer.Deserialize<DoubleFacedCard>(jsonObject.GetRawText(), options),
            _ => JsonSerializer.Deserialize<NormalCard>(jsonObject.GetRawText(), options)
        };

    }

    public override void Write(Utf8JsonWriter writer, ScryfallCardBase value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
} 