using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;
public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly IHubContext<NotificationHub> _hub;

    public BidPlacedConsumer(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("consumer new bid placed");

        await _hub.Clients.All.SendAsync("BidPlaced",context.Message);
    }
}
