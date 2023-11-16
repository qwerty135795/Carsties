using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;
public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEnd;
    private readonly IMapper _mapper;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    public AuctionControllerTests()
    {
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEnd = new Mock<IPublishEndpoint>();
        _fixture = new Fixture();
        var mockMapper = new MapperConfiguration(cnf =>
        {
            cnf.AddMaps(typeof(MapperProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);
        _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEnd.Object)
        {
            ControllerContext = new ControllerContext 
            {
                HttpContext = new DefaultHttpContext
                {
                    User = Helper.GetIdentity()
                }
            }
        };
    }
    [Fact]
    public async Task GetAuctions_WithNoParams_Return10Auctions()
    {
        // arrange
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepo.Setup(r => r.GetAuctionsAsync(null)).ReturnsAsync(auctions);
        // act
        var result = await _controller.GetAuctions(null);

        // Asserts
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }
    [Fact]
    public async Task GetAuctions_WithValidGuid_ReturnAuction()
    {
        // arrange
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepo.Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        // act
        var result = await _controller.GetAuctions(auction.Id);

        // Asserts
        Assert.Equal(auction.Make, result.Value.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }
    [Fact]
    public async Task GetAuctions_WithInvalidGuid_Return404()
    {
        // arrange
        _auctionRepo.Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);
        // act
        var result = await _controller.GetAuctions(Guid.NewGuid());

        // Asserts

        Assert.IsType<NotFoundResult>(result.Result);
    }
    [Fact]
    public async Task CreateItem_WithModel_ReturnCreatedResult()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(r => r.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        // act
        var result = await _controller.CreateItem(auction);
        var createdResult = result.Result as CreatedAtActionResult;
        // Asserts

        Assert.NotNull(createdResult);
        Assert.IsType<AuctionDto>(createdResult.Value);
        Assert.Equal("GetAuctions",createdResult.ActionName);
    }
    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(r => r.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);
        // act
        var result = await _controller.CreateItem(auction);
        // Asserts
        Assert.IsType<BadRequestResult>(result.Result);
        Assert.Null(result.Value);

    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
        auction.Seller = "test";
        auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
        var updateAuction = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(a => a.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        // act
        var result = await _controller.UpdateAuction(auction.Id,updateAuction);
        // Asserts
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
        var updateAuction = _fixture.Create<UpdateAuctionDto>();
        auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
        _auctionRepo.Setup(a => a.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        // act
        var result = await _controller.UpdateAuction(auction.Id,updateAuction);
        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        var updateAuction = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value:null);
        // act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), updateAuction);
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
        auction.Seller = "test";
        auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        // act
        var result = await _controller.DeleteAuction(auction.Id);
        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value:null);
        // act
        var result = await _controller.DeleteAuction(Guid.NewGuid());
        // Asserts
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
         var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
         auction.Seller = "NoTest";
        auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value:auction);
        // act
        var result = await _controller.DeleteAuction(auction.Id);
        // Assert
        Assert.IsType<ForbidResult>(result);
    }
}
