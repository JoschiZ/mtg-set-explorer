using System.Text.Json.Serialization;

namespace MTGSetExplorer.Core.Scryfall.Types;

public class Set
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }

    [JsonPropertyName("released_at")]
    public DateOnly? ReleasedAt { get; init; }
    
    [JsonPropertyName("card_count")]
    public int CardCount { get; init; }
    
    [JsonPropertyName("digital")]
    public bool IsDigitalOnly { get; init; }
    
    [JsonPropertyName("scryfall_uri")]
    public required string ScryfallUri { get; init; }
    
    [JsonPropertyName("icon_svg_uri")]
    public required string IconUri { get; init; }
    
    [JsonPropertyName("search_uri")]
    public required string SearchUri { get; init; }
}