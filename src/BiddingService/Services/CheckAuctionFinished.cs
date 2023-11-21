

using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;
public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _services;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start checking auction finished");

        stoppingToken.Register(() => _logger.LogInformation("stop checking"));

        while(!stoppingToken.IsCancellationRequested)
        {
            await CheckFinished(stoppingToken);

            await Task.Delay(5000,stoppingToken);
        }
    }

    private async Task CheckFinished(CancellationToken stoppingToken)
    {
        var auctionEnd = await DB.Find<Auction>()
            .Match(x => x.AuctionEnd <= DateTime.UtcNow)
            .Match(x => !x.Finished).ExecuteAsync(stoppingToken);
        if (auctionEnd.Count == 0) return;
        _logger.LogInformation($"{auctionEnd.Count} finished auction");

        using var scope = _services.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        foreach (var item in auctionEnd)
        {
            item.Finished = true;
            await item.SaveAsync(null, stoppingToken);

            var highBid = await DB.Find<Bid>()
            .Match(a => a.AuctionId == item.ID)
            .Match( b => b.BidStatus == BidStatus.Accepted)
            .Sort(a => a.Descending(b => b.Amount)).ExecuteFirstAsync();
            var auctionFinished = new AuctionFinished
            { 
                Seller = item.Seller, AuctionId = item.ID, 
                ItemSold = highBid != null,
                Winner = highBid?.Bidder, Amount = highBid?.Amount
            };
            await endpoint.Publish(auctionFinished);
        }
    }
}
