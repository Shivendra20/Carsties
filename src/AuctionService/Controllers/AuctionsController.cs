using AuctionService.Data;
using AuctionService.Dto;
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

    // GET: api/auctions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuctionsDto>>> GetAllAuctions()
    {
        const string cacheKey = "auctions:all";
        
        // Try to get from cache first
        var cachedAuctions = await _cacheService.GetAsync<List<AuctionsDto>>(cacheKey);
        if (cachedAuctions != null)
        {
            return cachedAuctions;
        }

        // If not in cache, get from database
        var auctions = await _context.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();
        var result = _mapper.Map<List<AuctionsDto>>(auctions);

        // Store in cache for 30 minutes
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }

    // POST: api/auctions
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionsDto>> CreateAuction([FromBody] CreateAuctionDto createAuctionDto)
    {
        // Get the username from the JWT token
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value 
                      ?? User.FindFirst("unique_name")?.Value;
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized("User not authenticated");
        }

        // Check if user has Auctioneer or Both role
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value 
                      ?? User.FindFirst("role")?.Value;
        
        if (userRole != "Auctioneer" && userRole != "Both")
        {
            return Forbid("Only auctioneers can create auctions");
        }

        // Create the auction
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

        // Invalidate cache
        await _cacheService.RemoveAsync("auctions:all");

        var dto = _mapper.Map<AuctionsDto>(auction);
        return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, dto);
    }

    // GET: api/auctions/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionsDto>> GetAuctionById(Guid id)
    {
        var cacheKey = $"auction:{id}";
        
        // Try to get from cache first
        var cachedAuction = await _cacheService.GetAsync<AuctionsDto>(cacheKey);
        if (cachedAuction != null)
        {
            return cachedAuction;
        }

        // If not in cache, get from database
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null) return NotFound();
        
        var dto = _mapper.Map<AuctionsDto>(auction);
        
        // Store in cache for 30 minutes
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));
        
        return dto;
    }

    // POST: api/auctions/update/{id}
    [HttpPost("update/{id}")]
    public async Task<ActionResult<bool>> UpdateAuction(Guid id, [FromBody] UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null) return NotFound();
        
        _mapper.Map(updateAuctionDto, auction);
        var result = await _context.SaveChangesAsync() > 0;
        
        if (!result) return BadRequest("Could not update DB");
        
        // Invalidate cache after update
        await _cacheService.RemoveAsync($"auction:{id}");
        await _cacheService.RemoveAsync("auctions:all");
        
        return Ok(true);
    }

    // POST: api/auctions/delete/{id}
    [HttpPost("delete/{id}")]
    public async Task<ActionResult<bool>> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null) return NotFound();
        
        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        
        if (!result) return BadRequest("Could not update DB");
        
        // Invalidate cache after delete
        await _cacheService.RemoveAsync($"auction:{id}");
        await _cacheService.RemoveAsync("auctions:all");
        
        return Ok(true);
    }

}