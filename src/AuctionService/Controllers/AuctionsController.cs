using AuctionService.Data;
using AuctionService.Dto;
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
    public AuctionsController(AuctionDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // get all auctions from this call 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuctionsDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();
        var result = _mapper.Map<List<AuctionsDto>>(auctions);

        return _mapper.Map<List<AuctionsDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionsDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null) return NotFound();
        var dto = _mapper.Map<AuctionsDto>(auction);
        return dto;
    }

}