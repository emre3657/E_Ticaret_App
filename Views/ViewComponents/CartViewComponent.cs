using MagazaApp.Extensions;
using MagazaApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace MagazaApp.Views.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IViewComponentResult Invoke()
        {
            var cart = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }
    }

}
