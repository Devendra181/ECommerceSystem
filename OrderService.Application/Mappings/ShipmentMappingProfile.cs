using AutoMapper;
using OrderService.Application.DTOs.Shipments;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.MappingProfiles
{
    public class ShipmentMappingProfile : Profile
    {
        public ShipmentMappingProfile()
        {
            // Map CreateShipmentRequestDTO to Shipment entity
            CreateMap<CreateShipmentRequestDTO, Shipment>()
                .ForMember(dest => dest.ShipmentItems, opt => opt.MapFrom(src => src.ShipmentIItems));

            // Map ShipmentItemRequestDTO entity to ShipmentItem
            CreateMap<ShipmentItemRequestDTO, ShipmentItem>();

            // Map Shipment entity to ShipmentResponseDTO
            CreateMap<Shipment, ShipmentResponseDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (ShipmentStatusEnum)src.ShipmentStatusId))
                .ForMember(dest => dest.ShipmentItems, opt => opt.MapFrom(src => src.ShipmentItems))
                .ForMember(dest => dest.ShipmentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedAt));

            // Map ShipmentItem entity to ShipmentItemResponseDTO
            CreateMap<ShipmentItem, ShipmentItemResponseDTO>()
                .ForMember(dest => dest.ShipmentItemId, opt => opt.MapFrom(src => src.Id));

            // Map ShipmentStatusHistory entity to ShipmentStatusHistoryDTO
            CreateMap<ShipmentStatusHistory, ShipmentStatusHistoryDTO>()
                .ForMember(dest => dest.OldStatus, opt => opt.MapFrom(src => (ShipmentStatusEnum)src.OldStatusId))
                .ForMember(dest => dest.NewStatus, opt => opt.MapFrom(src => (ShipmentStatusEnum)src.NewStatusId))
                .ForMember(dest => dest.ChangedOn, opt => opt.MapFrom(src => src.ChangedAt));

            // Map Shipment entity to ShipmentTrackingResponseDTO
            CreateMap<Shipment, ShipmentTrackingResponseDTO>()
                .ForMember(dest => dest.CurrentStatus, opt => opt.MapFrom(src => (ShipmentStatusEnum)src.ShipmentStatusId))
                .ForMember(dest => dest.ShipmentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}


