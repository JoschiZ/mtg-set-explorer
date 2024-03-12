using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MTGSetExplorer.Core.Scryfall.Types;

namespace MTGSetExplorer.Core.Scryfall;

internal sealed class ScryfallClient
{
    private readonly HttpClient _client;
    private const string BaseUri = "https://api.scryfall.com/";

    public ScryfallClient(HttpClient client)
    {
        _client = client;
        _client.BaseAddress = new Uri(BaseUri);
    }

    public async Task<IEnumerable<Set>> GetAllSetsAsync()
    {
        var sets = await _client.GetFromJsonAsync<ListResponse<Set>>("sets/");
        if (sets is null)
        {
            return Enumerable.Empty<Set>();
        }
        return await sets.GetAllAsync(_client);
    }

    public async Task<Set?> GetSet(string setCode)
    {
        return await _client.GetFromJsonAsync<Set>($"sets/{setCode}");
    }

    public async Task<ImmutableArray<ScryfallCardBase>> GetCardsFromSetAsync(Set set)
    {
        var initialResponse = await _client.GetFromJsonAsync<CardListResponse>(set.SearchUri);
        if (initialResponse is null)
        {
            return [];
        }
        return await initialResponse.GetAllAsync(_client);
    }

    public async Task<IEnumerable<ScryfallCardBase>> GetCardsAsync(string query, bool firstPageOnly = false)
    {
        CardListResponse? response;
        try
        {
            response = await _client.GetFromJsonAsync<CardListResponse>($"cards/search?q={query}");
        }
        catch (HttpRequestException e) when(e.StatusCode == HttpStatusCode.NotFound)
        {
            return Enumerable.Empty<ScryfallCardBase>();
        }

        
        if (response is null)
        {
            return Enumerable.Empty<ScryfallCardBase>();
        }

        IEnumerable<ScryfallCardBase> allCards;
        if (!firstPageOnly)
        {
            allCards = await response.GetAllAsync(_client);
        }
        else
        {
            allCards = response.Data;
        }


        return allCards;
    }
    
}