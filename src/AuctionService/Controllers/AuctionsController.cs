using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;
[ApiController]
[Route("api/auctions")]
//[Authorize]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(o => o.Item.Make).AsQueryable();
        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<AuctionDto>> GetAuctions(Guid id)
    {
        var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a =>
        a.Id == id);
        if (auction is null) return NotFound();
        return _mapper.Map<AuctionDto>(auction);
    }
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateItem(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = User.Identity.Name;
        _context.Auctions.Add(auction);
        var newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
        if (await _context.SaveChangesAsync() > 0)
        {
            return CreatedAtAction(nameof(GetAuctions), new { auction.Id }, newAuction);
        }
        return BadRequest();
    }
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a => a.Id == id);
        if (auction is null) return NotFound();
        if (auction.Seller != User.Identity.Name) return Forbid();
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction.Item));
        if (await _context.SaveChangesAsync() > 0) return Ok();
        return BadRequest();
    }
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a =>
        a.Id == id);
        if (auction is null) return NotFound();
        if (auction.Seller != User.Identity.Name) return Forbid();
         _context.Auctions.Remove(auction);
        await _publishEndpoint.Publish(new AuctionDeleted { Id = id.ToString()});
        if (await _context.SaveChangesAsync() > 0) return Ok();
        return BadRequest();
    }
}
