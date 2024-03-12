using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace MTGSetExplorer.Core.Scryfall.Types;

public abstract class SingleFacedCard: ScryfallCardBase
{
    [JsonPropertyName("mana_cost")]
    public string? ManaCost { get; init; }
    
    [JsonPropertyName("image_uris")]
    public ImageUris? ImageUris { get; init; }
}

public sealed class NormalCard : SingleFacedCard
{
    [JsonPropertyName("oracle_text")]
    public required string OracleText { get; init; }
}

public sealed class SplitCard : SingleFacedCard
{
    [JsonPropertyName("card_faces")]
    public required ImmutableArray<CardFace> CardFaces { get; init; }
}

public sealed class DoubleFacedCard : ScryfallCardBase
{
    [JsonPropertyName("card_faces")]
    public required ImmutableArray<CardFace> CardFaces { get; init; }
}