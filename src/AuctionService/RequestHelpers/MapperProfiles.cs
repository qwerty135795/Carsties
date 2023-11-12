using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers;
public class MapperProfiles : Profile
{
    public MapperProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(a => a.Item);
        CreateMap<Item,AuctionDto>();
        CreateMap<CreateAuctionDto,Auction>().ForMember(x => x.Item, dest => dest.MapFrom(s => s));
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<AuctionDto, AuctionCreated>();
        CreateMap<Item, AuctionUpdated>().ForMember(x => x.Id, dest => dest.MapFrom(src => src.AuctionId));
    }
}
