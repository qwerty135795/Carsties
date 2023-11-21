using AutoMapper;
using BiddingService.DTO;
using BiddingService.Models;
using Contracts;

namespace BiddingService;
public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Bid, BidDto>();
        CreateMap<Bid, BidPlaced>();
    }
}
