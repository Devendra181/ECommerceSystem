using AutoMapper;
using OrderService.Application.DTOs.Refunds;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.MappingProfiles
{
    public class RefundMappingProfile : Profile
    {
        public RefundMappingProfile()
        {
            // Refund entity to RefundResponseDTO
            CreateMap<Refund, RefundResponseDTO>()
                .ForMember(dest => dest.RefundStatus, opt => opt.MapFrom(src => (RefundStatusEnum)src.RefundStatusId))
                .ForMember(dest => dest.RefundId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.RefundDate));
        }
    }
}
