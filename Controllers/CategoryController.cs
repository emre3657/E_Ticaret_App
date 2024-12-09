using Microsoft.AspNetCore.Mvc;

namespace MagazaApp.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
