using Microsoft.AspNetCore.Mvc;

namespace MagazaApp.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
