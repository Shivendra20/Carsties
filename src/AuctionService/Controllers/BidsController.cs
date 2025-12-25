using System.Security.Claims;
using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly AuctionDBContext _context;
    private readonly IMapper _mapper;

    public BidsController(AuctionDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("auction/{auctionId}")]
    public async Task<ActionResult<IEnumerable<BidDto>>> GetBidsByAuction(Guid auctionId)
    {
        var bids = await _context
            .Bids.Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidTime)
            .ToListAsync();

        return Ok(_mapper.Map<List<BidDto>>(bids));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDto>> PlaceBid([FromBody] CreateBidDto createBidDto)
    {
        var username =
            User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("unique_name")?.Value;

        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized("User not authenticated");
        }

        var auction = await _context
            .Auctions.Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == createBidDto.AuctionId);

        if (auction == null)
        {
            return NotFound("Auction not found");
        }

        if (auction.EndedAt < DateTime.UtcNow)
        {
            return BadRequest("Auction has ended");
        }

        var highestBid = await _context
            .Bids.Where(b => b.AuctionId == createBidDto.AuctionId)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefaultAsync();

        var minimumBid = highestBid?.Amount ?? auction.ReservePrice;

        if (createBidDto.Amount <= minimumBid)
        {
            return BadRequest($"Bid must be higher than current highest bid of {minimumBid}");
        }

        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            AuctionId = createBidDto.AuctionId,
            Bidder = username,
            Amount = createBidDto.Amount,
            BidTime = DateTime.UtcNow,
        };

        _context.Bids.Add(bid);

        auction.CurrentHighBit = createBidDto.Amount;
        auction.CurrentPrice = createBidDto.Amount;

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest("Could not save bid");
        }

        return CreatedAtAction(
            nameof(GetBidsByAuction),
            new { auctionId = bid.AuctionId },
            _mapper.Map<BidDto>(bid)
        );
    }

    [HttpGet("highest/{auctionId}")]
    public async Task<ActionResult<BidDto>> GetHighestBid(Guid auctionId)
    {
        var highestBid = await _context
            .Bids.Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefaultAsync();

        if (highestBid == null)
        {
            return NotFound("No bids found for this auction");
        }

        return Ok(_mapper.Map<BidDto>(highestBid));
    }
}
