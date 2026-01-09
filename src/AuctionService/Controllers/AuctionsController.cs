using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AuctionService.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/Auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDBContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public AuctionsController(AuctionDBContext context, IMapper mapper, ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuctionsDto>>> GetAllAuctions()
    {
        const string cacheKey = "auctions:all";

        var cachedAuctions = await _cacheService.GetAsync<List<AuctionsDto>>(cacheKey);
        if (cachedAuctions != null)
        {
            return cachedAuctions;
        }

        var auctions = await _context
            .Auctions.Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
        var result = _mapper.Map<List<AuctionsDto>>(auctions);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
        return result;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionsDto>> CreateAuction(
        [FromBody] CreateAuctionDto createAuctionDto
    )
    {
        var username =
            User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
            ?? User.FindFirst("unique_name")?.Value;

        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized("User not authenticated");
        }

        var userRole =
            User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            ?? User.FindFirst("role")?.Value;

        if (userRole != "Auctioneer" && userRole != "Both")
        {
            return Forbid("Only auctioneers can create auctions");
        }

        var auction = _mapper.Map<Auction>(createAuctionDto);
        auction.Id = Guid.NewGuid();
        auction.Seller = username;
        auction.CreatedAt = DateTime.UtcNow;
        auction.EndedAt = createAuctionDto.EndDate;
        auction.Status = Status.Live;

        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest("Could not save auction");
        }

        await _cacheService.RemoveAsync("auctions:all");

        var dto = _mapper.Map<AuctionsDto>(auction);
        return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, dto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionsDto>> GetAuctionById(Guid id)
    {
        var cacheKey = $"auction:{id}";

        var cachedAuction = await _cacheService.GetAsync<AuctionsDto>(cacheKey);
        if (cachedAuction != null)
        {
            return cachedAuction;
        }
        var auction = await _context
            .Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null)
            return NotFound();

        var dto = _mapper.Map<AuctionsDto>(auction);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));

        return dto;
    }

    [HttpPost("update/{id}")]
    public async Task<ActionResult<bool>> UpdateAuction(
        Guid id,
        [FromBody] UpdateAuctionDto updateAuctionDto
    )
    {
        var auction = await _context
            .Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null)
            return NotFound();

        _mapper.Map(updateAuctionDto, auction);
        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not update DB");

        await _cacheService.RemoveAsync($"auction:{id}");
        await _cacheService.RemoveAsync("auctions:all");

        return Ok(true);
    }

    [HttpPost("delete/{id}")]
    public async Task<ActionResult<bool>> DeleteAuction(Guid id)
    {
        var auction = await _context
            .Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null)
            return NotFound();

        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not update DB");

        await _cacheService.RemoveAsync($"auction:{id}");
        await _cacheService.RemoveAsync("auctions:all");

        return Ok(true);
    }
}
