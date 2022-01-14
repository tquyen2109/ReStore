using System;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controller
{
    public class BasketController : BaseApiController
    {
        private readonly StoreContext _context;

        public BasketController(StoreContext context)
        {
            _context = context;
        }

        [HttpGet(Name = "GetBasket")]
        public async Task<ActionResult<BasketDto>> GetBasket()
        {
            var basket = await RetrieveBasket();
            if (basket == null) return NotFound();
            return MapBasketToDto(basket);
        }

       


        [HttpPost]
        public async Task<ActionResult<BasketDto>> AddItemToBasket(int productId, int quantity)
        {
            var basket = await RetrieveBasket() ?? CreateBasket();
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();
            basket.AddItem(product,quantity);
            var result = await _context.SaveChangesAsync() > 0;
            return result ? CreatedAtRoute("GetBasket",MapBasketToDto(basket)) : BadRequest(new ProblemDetails{Title = "Problem saving basket"});
        }



        [HttpDelete]
        public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
        {
            var basket = await RetrieveBasket() ?? CreateBasket();
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();
            basket.RemoveItem(productId,quantity);
            var result = await _context.SaveChangesAsync() > 0;
            return result ? StatusCode(201) : BadRequest(new ProblemDetails{Title = "Problem saving basket"});
        }
        
        private async Task<Basket> RetrieveBasket()
        {
            var basket = await _context.Baskets
                .Include(i => i.Items)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(x => x.BuyerId == Request.Cookies["buyerId"]);
            return basket;
        }
        
        private Basket CreateBasket()
        {
            var buyerId = Guid.NewGuid().ToString();
            var cookieOption = new CookieOptions {IsEssential = true, Expires = DateTime.Now.AddDays(30)};
            Response.Cookies.Append("buyerId", buyerId, cookieOption);
            var basket = new Basket
            {
                BuyerId = buyerId
            };
            _context.Baskets.Add(basket);
            return basket;
        }
        
        private static BasketDto MapBasketToDto(Basket basket)
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
    }
}