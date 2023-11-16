
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;
public class AuctionControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    public const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_Return3Auctions()
    {
        // act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

        // Asserts
        Assert.Equal(3, response.Count);
    }
    [Fact]
    public async Task GetAuctions_WithValidId_ReturnAuctionDto()
    {
        // arrange

        // act
        var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");

        // Asserts
        Assert.Equal("GT", response.Model);
    }
    [Fact]
    public async Task GetAuctions_WithNumberId_Return400()
    {
        // arrange
        // act
        var response = await _httpClient.GetAsync("api/auctions/1");

        // Asserts
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    }
    [Fact]
    public async Task GetAuctions_WithInvalidId_Return404()
    {
        // arrange
        // act
        var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");

        // Asserts
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task CreateAuction_WithNoAuth_Return401()
    {
        // arrange
        var auction = new CreateAuctionDto{ Make = "Qwerty"};
        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // Asserts
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task CreateAuction_WithAuth_Return201()
    {
        // arrange
        var auction = GetCreateAuctionDto();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // Asserts
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal("bob", createdAuction.Seller);
    }
     [Fact]
    public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
    {
        // arrange
        var auction = new CreateAuctionDto();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions",auction);
        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
    {
        // arrange
        var model = new UpdateAuctionDto {
            Color = "Blue", Make = "GB", Model = "NewModel", Mileage = 1, Year = 2004
        };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}",model);
        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
    {
        // arrange 
        var model = new UpdateAuctionDto {
            Color = "Blue", Make = "GB", Model = "NewModel", Mileage = 1, Year = 2004
        };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("alice"));
        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}",model);
        // assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReInitDb(db);
        return Task.CompletedTask;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    private CreateAuctionDto GetCreateAuctionDto()
    {
        return new CreateAuctionDto
        {
            Color = "Red",
            Make = "Test",
            Model = "Test",
            Mileage = 10,
            Year = 69,
            ImageUrl = "Test",
            ReservePrice = 5000
        };
    }
}
