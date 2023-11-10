using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;
public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }
    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item,string>().Sort(s => s.Descending(i => i.UpdatedAt)).
        Project(d => d.UpdatedAt.ToString()).ExecuteFirstAsync();

        return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"]
         + "api/auctions?date=" + lastUpdated);
    }
}
