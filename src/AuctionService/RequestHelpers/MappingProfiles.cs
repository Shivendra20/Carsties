using AuctionService.Dto;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers;


public class MappingProfiles : Profile
{

    public MappingProfiles()
    {
        CreateMap<Auction, AuctionsDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionsDto>();
        CreateMap<CreateAuctionDto, Auction>()
           .ForMember(dest => dest.Item, opt => opt.MapFrom(src => src));

        CreateMap<CreateAuctionDto, Item>();
        
        CreateMap<UpdateAuctionDto, Auction>()
           .ForMember(dest => dest.Item, opt => opt.MapFrom(src => src));
        CreateMap<UpdateAuctionDto, Item>();
    }

}