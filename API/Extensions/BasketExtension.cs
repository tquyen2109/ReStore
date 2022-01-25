using System.Linq;
using API.DTOs;
using API.Entities;

namespace API.Extensions
{
    public static class BasketExtension
    {
        public static BasketDto MapBasketToDto(this Basket basket)
        {
            if (basket != null)
            {
                return new BasketDto
                {
                    Id = basket.Id,
                    BuyerId = basket.BuyerId,
                    Items = basket.Items.Select(item => new BasketItemDto
                    {
                        ProductId = item.ProductId,
                        Name = item.Product.Name,
                        Price = item.Product.Price,
                        PictureUrl = item.Product.PictureUrl,
                        Quantity = item.Quantity,
                        Type = item.Product.Type,
                        Brand = item.Product.Brand
                    }).ToList()
                };
            }
            return null;
        }
    }
}