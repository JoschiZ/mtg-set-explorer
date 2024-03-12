using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MTGSetExplorer.Core.Scryfall;
using MTGSetExplorer.Core.Scryfall.Types;
using Newtonsoft.Json;

namespace MTGSetExplorer.Tests;

public class ScryfallClientTests
{
    [Fact]
    public async Task GetAllScryfallSets()
    {
        var client = new HttpClient();
        var scryfallClient = new ScryfallClient(client);

        var sets = await scryfallClient.GetAllSetsAsync();
        var setArray = sets.ToImmutableArray();
    }

    [Theory]
    [InlineData("mkm")]
    [InlineData("tblb")]
    [InlineData("tmoc")]
    [InlineData("moc")]
    [InlineData("unf")]
    [InlineData("astx")]
    [InlineData("gk1")]
    public async Task CanLoadSetCards(string setCode)
    {
        var client = new HttpClient();
        var scryfallClient = new ScryfallClient(client);

        var set = await scryfallClient.GetSet(setCode);
        Assert.NotNull(set);
        
        var cards = await scryfallClient.GetCardsFromSetAsync(set);
        Assert.NotNull(cards);
        Assert.NotEmpty(cards);
        Assert.True(cards.Count() >= set.CardCount);
    }

    public static IEnumerable<object[]> GetAllLayoutValues()
    {
        return Enum.GetValues<Core.Scryfall.Types.Layout>().Select(layout => (object[])[layout]);
    }
    
    [Theory]
    [MemberData(nameof(GetAllLayoutValues))]
    public async Task CanFetchLayout(Core.Scryfall.Types.Layout layout)
    {
        
        var client = new HttpClient();
        var scryfallClient = new ScryfallClient(client);
        var propertyName = layout.ToString().ToLower();
        var jsonPropertyAttribute = layout.GetAttributeOfType<JsonPropertyNameAttribute>();
        
        if (jsonPropertyAttribute is not null)
        {
            propertyName = jsonPropertyAttribute.Name;
        }
        
        var cards = await scryfallClient.GetCardsAsync($"layout:{propertyName}", true);

        Assert.NotNull(cards);
        Assert.NotEmpty(cards);
    }


    [Theory]
    [InlineData("https://api.scryfall.com/cards/75754468-2850-42e6-ab22-61ff7b9d1214")]
    public async Task CanDeserializeCard(string uri)
    {
        var client = new HttpClient();
        var card = await client.GetFromJsonAsync<ScryfallCardBase>(uri);
        Assert.NotNull(card);
    }
    
}