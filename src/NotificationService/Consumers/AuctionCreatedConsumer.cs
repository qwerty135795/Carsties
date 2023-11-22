using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;
public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IHubContext<NotificationHub> _hub;

    public AuctionCreatedConsumer(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("consumer new auction created");

        await _hub.Clients.All.SendAsync("AuctionCreated", context.Message);
    }
}
