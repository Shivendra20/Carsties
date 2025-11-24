using AuctionService.Data;
using AuctionService.Dto;
using AuctionService.Dtos;
using AuctionService.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController] //data validation
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

    // get all auctions from this call 
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