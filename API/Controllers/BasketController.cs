﻿using System;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
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
            var basket = await RetrieveBasket(GetBuyerId());
            if (basket == null) return NotFound();
            return basket.MapBasketToDto();
        }

        
        [HttpPost]
        public async Task<ActionResult<BasketDto>> AddItemToBasket(int productId, int quantity)
        {
            var basket = await RetrieveBasket(GetBuyerId()) ?? CreateBasket();
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return  BadRequest(new ProblemDetails{Title = "Product not found"});
            basket.AddItem(product,quantity);
            var result = await _context.SaveChangesAsync() > 0;
            return result ? CreatedAtRoute("GetBasket",basket.MapBasketToDto()) : BadRequest(new ProblemDetails{Title = "Problem saving basket"});
        }

        
        [HttpDelete]
        public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
        {
            var basket = await RetrieveBasket(GetBuyerId()) ?? CreateBasket();
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();
            basket.RemoveItem(productId,quantity);
            var result = await _context.SaveChangesAsync() > 0;
            return result ? StatusCode(201) : BadRequest(new ProblemDetails{Title = "Problem saving basket"});
        }
        
        private async Task<Basket> RetrieveBasket(string buyerId)
        {
            if (!string.IsNullOrEmpty(buyerId))
                return await _context.Baskets
                    .Include(i => i.Items)
                    .ThenInclude(p => p.Product)
                    .FirstOrDefaultAsync(x => x.BuyerId == buyerId);
            Response.Cookies.Delete("buyerId");
            return null;

        }

        private string GetBuyerId()
        {
            return User.Identity?.Name ?? Request.Cookies["buyerId"];
        }
        
        private Basket CreateBasket()
        {
            var buyerId = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerId))
            {
                buyerId = Guid.NewGuid().ToString();
                var cookieOption = new CookieOptions {IsEssential = true, Expires = DateTime.Now.AddDays(30)};
                Response.Cookies.Append("buyerId", buyerId, cookieOption);
            }
            var basket = new Basket
            {
                BuyerId = buyerId
            };
            _context.Baskets.Add(basket);
            return basket;
        }
    }
}