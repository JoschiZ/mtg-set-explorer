using System.Text.Json.Serialization;

namespace MTGSetExplorer.Core.Scryfall.Types;

internal sealed class CardListResponse : ListResponse<ScryfallCardBase>
{
    [JsonPropertyName("total_cards")]
    public int TotalCardCount { get; set; }
}