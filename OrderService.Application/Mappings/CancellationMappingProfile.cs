using AutoMapper;
using OrderService.Application.DTOs.Cancellation;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.MappingProfiles
{
    public class CancellationMappingProfile : Profile
    {
        public CancellationMappingProfile()
        {
            // ===== DTO to Entity Mappings =====

            // CreateCancellationRequestDTO -> Cancellation
            CreateMap<CreateCancellationRequestDTO, Cancellation>()
                .ForMember(dest => dest.CancellationItems, opt => opt.MapFrom(src => src.CancellationItems));

            // CancellationItemRequestDTO -> CancellationItem
            CreateMap<CancellationItemRequestDTO, CancellationItem>();

            // UpdateCancellationRequestDTO -> Cancellation
            CreateMap<UpdateCancellationRequestDTO, Cancellation>()
                .ForMember(dest => dest.CancellationItems, opt => opt.MapFrom(src => src.CancellationItems)); // map CancellationId -> Id

            // UpdateCancellationItemDTO -> CancellationItem
            CreateMap<UpdateCancellationItemDTO, CancellationItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CancellationItemId));

            // CancellationApprovalRequestDTO -> CancellationApprovalResponseDTO (If needed)
            CreateMap<CancellationApprovalRequestDTO, CancellationApprovalResponseDTO>()
                .ForMember(dest => dest.ProcessedOn, opt => opt.MapFrom(src => DateTime.UtcNow));

            // ===== Entity to DTO Mappings =====

            // Cancellation -> CancellationResponseDTO
            CreateMap<Cancellation, CancellationResponseDTO>()
                .ForMember(dest => dest.CancellationId, opt => opt.MapFrom(src => src.Id)) // Id -> CancellationId
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (CancellationStatusEnum)src.CancellationStatusId))
                .ForMember(dest => dest.RequestedOn, opt => opt.MapFrom(src => src.RequestedAt)) 
                .ForMember(dest => dest.ProcessedOn, opt => opt.MapFrom(src => src.ProcessedAt))
                .ForMember(dest => dest.ReasonText, opt => opt.MapFrom(src => src.Reason != null ? src.Reason.ReasonText : string.Empty))
                .ForMember(dest => dest.CancellationItems, opt => opt.MapFrom(src => src.CancellationItems));

            // CancellationItem -> CancellationItemResponseDTO
            CreateMap<CancellationItem, CancellationItemResponseDTO>()
                .ForMember(dest => dest.CancellationItemId, opt => opt.MapFrom(src => src.Id)); // Id -> CancellationItemId
        }
    }
}
