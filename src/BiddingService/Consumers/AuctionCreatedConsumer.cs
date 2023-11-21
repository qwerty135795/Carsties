using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Consumers;
public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("consuming new auction from action service");
        var auction = new Auction 
        { ID = context.Message.Id.ToString(), ReservePrice = context.Message.ReservePrice,
         AuctionEnd = context.Message.AuctionEnd, Seller = context.Message.Seller};
        await auction.SaveAsync();
    }
}
