using System.Text.Json.Serialization;

namespace MTGSetExplorer.Core.Scryfall.Types;

public sealed class CardFace
{
    public required string Name { get; init; }
    
    [JsonPropertyName("mana_cost")]
    public required string ManaCost { get; init; }
    
    [JsonPropertyName("image_uris")]
    public ImageUris? ImageUris { get; init; }
    
    [JsonPropertyName("oracle_text")]
    public required string OracleText { get; init; }
    
    [JsonPropertyName("type_line")]
    public required string TypeLine { get; init; }
}