using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MTGSetExplorer.Core.Scryfall.Types;

public class ListResponse<TData>
{
    public ImmutableArray<TData> Data { get; init; }
    
    [MemberNotNullWhen(true, members: [nameof(NextPage)])]
    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }
    
    [JsonPropertyName("next_page")]
    public string? NextPage { get; set; }
    
    public ImmutableArray<string>? Warnings { get; set; }

    public async Task<ListResponse<TData>?> GetNextAsync(HttpClient client)
    {
        if (!HasMore)
        {
            return null;
        }
        
        return await client.GetFromJsonAsync<ListResponse<TData>>(NextPage);
    }

    public async Task<ImmutableArray<TData>> GetAllAsync( HttpClient client)
    {
        if (!HasMore)
        {
            return Data;
        }

        List<IEnumerable<TData>> fullData = [Data];

        var currentSet = this;
        
        while (currentSet is {HasMore:true})
        {
            currentSet = await currentSet.GetNextAsync(client);

            if (currentSet is null)
            {
                break;
            }
            
            fullData.Add(currentSet.Data);
        }
        return [..fullData.SelectMany(x => x)];
    }
}