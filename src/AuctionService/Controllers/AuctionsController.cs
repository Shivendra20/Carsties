using AuctionService.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[ApiController] //data validation
[Route("api/Auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDBContext context;
    public AuctionsController(AuctionDBContext context, IMapper mapper)
    {
        this.context = context;

    }
    

}