using System.Text.Json.Serialization;

namespace MTGSetExplorer.Core.Scryfall.Types;

public sealed class ImageUris
{
    [JsonPropertyName("large")]
    public string? Large { get; init; }
    
    [JsonPropertyName("normal")]
    public string? Normal { get; init; }
    
    [JsonPropertyName("small")]
    public string? Small { get; init; }
    
    public string? Png { get; init; }
    
    [JsonPropertyName("border_crop")]
    public string? BorderCrop { get; init; }
}