using System.Text.Json;
using System.Text.Json.Serialization;

namespace MTGSetExplorer.Core.Scryfall.Types;

[JsonConverter(typeof(LayoutConverter))]
public enum Layout
{
    Normal,
    Split,
    Flip,
    Transform,
    [JsonPropertyName("modal_dfc")]
    ModalDoubleFacedCard,
    Meld,
    Leveler,
    Class,
    Case,
    Saga,
    [JsonPropertyName("adventure")]
    Adventure,
    Mutate,
    Prototype,
    Battle,
    Planar,
    Scheme,
    Vanguard,
    Token,
    [JsonPropertyName("double_faced_token")]
    DoubleFacedToken,
    Emblem,
    Augment,
    Host,
    [JsonPropertyName("art_series")]
    ArtSeries,
    [JsonPropertyName("reversible_card")]
    ReversibleCard
}

public class LayoutConverter : JsonConverter<Layout>
{
    public override Layout Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString() ?? "";
        return value switch
        {
            "adventure" => Layout.Adventure,
            "double_faced_token" => Layout.DoubleFacedToken,
            "art_series" => Layout.ArtSeries,
            "reversible_card" => Layout.ReversibleCard,
            "modal_dfc" => Layout.ModalDoubleFacedCard,
            _ => Enum.GetValues<Layout>().FirstOrDefault(l => l.ToString() == value)
        };
    }

    public override void Write(Utf8JsonWriter writer, Layout value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToUpper());
    }
}