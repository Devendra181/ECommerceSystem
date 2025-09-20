using AutoMapper;
using OrderService.Application.DTOs.Order;
using OrderService.Contracts.Enums;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Mappings
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            //Entity To DTO Mapping
            CreateMap<Order, OrderResponseDTO>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => (OrderStatusEnum)src.OrderStatusId))
                .ForMember(dest => dest.ShippingAddressId, opt => opt.MapFrom(src => src.ShippingAddressId))
                .ForMember(dest => dest.BillingAddressId, opt => opt.MapFrom(src => src.BillingAddressId))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => Enum.Parse(typeof(PaymentMethodEnum), src.PaymentMethod)))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id));

            CreateMap<OrderItem, OrderItemResponseDTO>()
                .ForMember(dest => dest.ItemStatusId, opt => opt.MapFrom(src => (OrderStatusEnum)src.ItemStatusId))
                .ForMember(dest => dest.OrderItemId, opt => opt.MapFrom(src => src.Id));

            CreateMap<OrderStatusHistory, OrderStatusHistoryResponseDTO>()
                .ForMember(dest => dest.OldStatus, opt => opt.MapFrom(src => (OrderStatusEnum)src.OldStatusId))
                .ForMember(dest => dest.NewStatus, opt => opt.MapFrom(src => (OrderStatusEnum)src.NewStatusId))
                .ForMember(dest => dest.ChangedOn, opt => opt.MapFrom(src => src.ChangedAt)); 

            // Request DTO -> Domain Entity mappings 
            CreateMap<CreateOrderRequestDTO, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderStatusId, opt => opt.Ignore());

            CreateMap<OrderItemRequestDTO, OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.ItemStatusId, opt => opt.Ignore());

            CreateMap<ChangeOrderStatusRequestDTO, OrderStatusHistory>()
                .ForMember(dest => dest.OldStatusId, opt => opt.Ignore())
                .ForMember(dest => dest.NewStatusId, opt => opt.MapFrom(src => (int)src.NewStatus))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ChangedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
