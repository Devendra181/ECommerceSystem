using AutoMapper;
using OrderService.Application.DTOs.Returns;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.MappingProfiles
{
    public class ReturnMappingProfile : Profile
    {
        public ReturnMappingProfile()
        {
            // DTO to Entity Mappings

            // CreateReturnRequestDTO to Return entity
            CreateMap<CreateReturnRequestDTO, Return>()
                .ForMember(dest => dest.ReturnItems, opt => opt.MapFrom(src => src.ReturnItems));

            // ReturnItemRequestDTO to ReturnItem entity
            CreateMap<ReturnItemRequestDTO, ReturnItem>();

            // UpdateReturnRequestDTO to Return entity
            CreateMap<UpdateReturnRequestDTO, Return>()
                .ForMember(dest => dest.ReturnItems, opt => opt.MapFrom(src => src.ReturnItems))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ReturnId));

            // UpdateReturnItemDTO to ReturnItem entity
            CreateMap<UpdateReturnItemDTO, ReturnItem>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ReturnItemId));

            // ReturnApprovalRequestDTO to ReturnApprovalResponseDTO
            CreateMap<ReturnApprovalRequestDTO, ReturnApprovalResponseDTO>()
                .ForMember(dest => dest.ProcessedOn, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Entity to DTO Mappings

            // Return entity to ReturnResponseDTO
            CreateMap<Return, ReturnResponseDTO>()
                .ForMember(dest => dest.ReturnId, opt => opt.MapFrom(src => src.Id)) // Id -> ReturnId
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (CancellationStatusEnum)src.ReturnStatusId))
                .ForMember(dest => dest.RequestedOn, opt => opt.MapFrom(src => src.RequestedAt))
                .ForMember(dest => dest.ProcessedOn, opt => opt.MapFrom(src => src.ProcessedAt))
                .ForMember(dest => dest.ReasonText, opt => opt.MapFrom(src => src.Reason != null ? src.Reason.ReasonText : string.Empty))
                .ForMember(dest => dest.ReturnItems, opt => opt.MapFrom(src => src.ReturnItems));

            // ReturnItem entity to ReturnItemResponseDTO
            CreateMap<ReturnItem, ReturnItemResponseDTO>()
                .ForMember(dest => dest.ReturnItemId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
