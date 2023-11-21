using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;
[ApiController]
[Route("api/auctions")]
//[Authorize]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(IAuctionRepository auctionRepository, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _auctionRepository = auctionRepository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions(string date)
    {
        return await _auctionRepository.GetAuctionsAsync(date);
    }
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<AuctionDto>> GetAuctions(Guid id)
    {
        var auction = await _auctionRepository.GetAuctionByIdAsync(id);
        if (auction is null) return NotFound();
        return auction;
    }
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AuctionDto>> CreateItem(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = User.Identity.Name;
        _auctionRepository.AddAuction(auction);
        var newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
        if (await _auctionRepository.SaveChangesAsync())
        {
            return CreatedAtAction(nameof(GetAuctions), new { auction.Id }, newAuction);
        }
        return BadRequest();
    }
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _auctionRepository.GetAuctionEntityByIdAsync(id);
        if (auction is null) return NotFound();
        if (auction.Seller != User.Identity.Name) return Forbid();
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction.Item));
        if (await _auctionRepository.SaveChangesAsync()) return Ok();
        return BadRequest();
    }
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _auctionRepository.GetAuctionEntityByIdAsync(id);
        if (auction is null) return NotFound();
        if (auction.Seller != User.Identity.Name) return Forbid();
        _auctionRepository.RemoveAuction(auction);
        await _publishEndpoint.Publish(new AuctionDeleted { Id = id.ToString()});
        if (await _auctionRepository.SaveChangesAsync()) return Ok();
        return BadRequest();
    }
}
