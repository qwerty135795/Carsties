using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;
public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly IHubContext<NotificationHub> _hub;

    public AuctionFinishedConsumer(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("consumer new auction finished");
        await _hub.Clients.All.SendAsync("AuctionFinished", context.Message);
    }
}
