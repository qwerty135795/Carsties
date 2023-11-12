using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;
public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("----> consuming auction updated");
        var item = _mapper.Map<Item>(context.Message);

        await DB.Update<Item>().MatchID(item.ID)
            .ModifyOnly(x => new {x.Color,x.Make,x.Model,x.Mileage,x.Year},item)
            .ExecuteAsync();

    }
}
