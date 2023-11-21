using AuctionService.Data;
using Grpc.Core;

namespace AuctionService;
public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
{
    private readonly AuctionDbContext _context;

    public GrpcAuctionService(AuctionDbContext context)
    {
        _context = context;
    }

    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        var auction = await _context.Auctions.FindAsync(Guid.Parse(request.Id)) ?? throw new RpcException(new Status(StatusCode.NotFound, "Not Found"));
        var response = new GrpcAuctionResponse
        {
            Auction = new GrpcAuctionModel 
            {
                Id = auction.Id.ToString(), AuctionEnd = auction.AuctionEnd.ToString(), 
                ReservePrice = auction.ReservePrice, Seller = auction.Seller 
            }
        };
        return response;
    }
}
