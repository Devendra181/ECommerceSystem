using AutoMapper;
using OrderService.Application.DTOs.Cart;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappings
{
    public class CartMappingProfile : Profile
    {
        public CartMappingProfile()
        {
            // Map AddCartItemRequestDTO to CartItem (for create/update operations)
            CreateMap<AddCartItemRequestDTO, CartItem>()
                .ForMember(dest => dest.CartId, opt => opt.Ignore()) // CartId set in service explicitly
                .ForMember(dest => dest.Id, opt => opt.Ignore());    // New entity Id handled by DB

            // Map UpdateCartItemRequestDTO to CartItem (for update operations)
            CreateMap<UpdateCartItemRequestDTO, CartItem>()
                .ForMember(dest => dest.CartId, opt => opt.Ignore()) // CartId set in service explicitly
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Map CartItem entity to CartItemResponseDTO (for returning cart data)
            CreateMap<CartItem, CartItemResponseDTO>()
               .ForMember(dest => dest.CartItemId, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Price))
               .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Price * src.Quantity));

            // Map Cart entity to CartResponseDTO (new)
            CreateMap<Cart, CartResponseDTO>()
                .ForMember(dest => dest.CartId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.CartItems))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
        }
    }
}
