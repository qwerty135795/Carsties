using AutoMapper;
using Contracts;
using SearchService.Models;

namespace SearchService.Helpers;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<AuctionCreated,Item>();
        CreateMap<AuctionUpdated, Item>();
    }
}
