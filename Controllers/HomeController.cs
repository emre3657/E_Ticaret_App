using MagazaApp.Data;
using MagazaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using MagazaApp.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace MagazaApp.Controllers
{

    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;

        }

        public IActionResult Index()
        {


            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {

            // Email'in benzersiz olup olmadığını kontrol et
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten kayıtlı.");
                return View(user);
            }

            user.CreatedAt = DateTime.Now;
            user.UserType = UserType.Customer;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            // Başarılı kayıt mesajını TempData'ya ekleyin
            TempData["SuccessMessage"] = "Kayıt işlemi başarılı. Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login", "Home");


        }


        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Kullanıcı bilgilerini Claims ile saklayın
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcının ID'sini claim olarak ekleyin
                    new Claim(ClaimTypes.Role, user.UserType == UserType.Admin ? "Admin" : "User") // Admin veya User olarak rol atayın
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Home sayfasına yönlendir.
                return RedirectToAction("Index", "Home");

            }

            TempData["ErrorMessage"] = "Hatalı email ya da şifre.";
            return View();
        }




        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Tüm çerezleri sil
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // Kimlik doğrulama çerezini sonlandır
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Home");
        }


        public IActionResult AccessDenied()
        {
            return View(); // Erişim engellendi sayfası
        }

        [Authorize]
        public IActionResult Orders()
        {
            var userEmail = User.Identity.Name; // Giriş yapan kullanıcının email adresini al
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user != null)
            {
                var orders = _context.Orders.Where(o => o.UserId == user.Id).ToList(); // Kullanıcıya ait siparişler
                return View(orders); // Siparişleri görüntülemek için Orders görünümüne yönlendir
            }

            return RedirectToAction("Login", "Home"); // Kullanıcı bulunamazsa giriş sayfasına yönlendir
        }

        [Authorize]
        public IActionResult OrderDetails(int id)
        {
            // Kullanıcının siparişini al (Kullanıcı doğrulamasını ekleyin)
            var userEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return RedirectToAction("Login", "Home");

            // İlgili siparişi ve sipariş öğelerini al
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Ürün bilgilerini almak için
                .FirstOrDefault(o => o.OrderId == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }





        [HttpGet]
        public async Task<IActionResult> ListProducts(int id)
        {
            // Alt kategori ID'lerini toplayacak liste
            var categoryIds = new List<int>();

            // Recursive bir metodla tüm alt kategorileri ekle
            async Task CollectAllCategoryIdsAsync(int categoryId)
            {
                var category = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == categoryId);

                if (category == null) return;

                categoryIds.Add(category.Id);

                // Bu kategoriye ait alt kategorileri getir
                var subCategories = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.ParentCategoryId == category.Id)
                    .ToListAsync();

                foreach (var subCategory in subCategories)
                {
                    await CollectAllCategoryIdsAsync(subCategory.Id);
                }
            }

            // Seçilen kategori ve tüm alt kategorilerini topla
            await CollectAllCategoryIdsAsync(id);

            // Tüm kategorilerdeki ürünleri getir
            var products = await _context.Products
                .Where(p => categoryIds.Contains(p.CategoryId))
                .Include(p => p.Category) // Ürünle ilişkili kategori bilgilerini de dahil et
                .ToListAsync();

            return View("ListProducts", products); // ListProducts görünümüne yönlendirme
        }

        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var product = _context.Products.Find(id);
            if (product != null)
            {
                var cartItem = cart.FirstOrDefault(p => p.ProductId == id);
                if (cartItem != null)
                {
                    return Json(new { success = false, message = "Bu ürün zaten sepette mevcut. Miktarı sepette ayarlayabilirsiniz." });
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        Brand = product.Brand, // Marka
                        Model = product.Model, // Model
                        ImagePath = product.ImagePath, // Resim yolu
                        Price = product.Price,
                        Quantity = 1
                    });
                }
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new { success = true, totalQuantity = cart.Sum(c => c.Quantity) });
        }

        [HttpGet]
        public IActionResult UpdateCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return PartialView("_CartPartial", cart);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int change)
        {
            // Sepet bilgilerini Session'dan al
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);

            if (cartItem != null)
            {
                // Ürün bilgilerini veritabanından al (stok kontrolü için)
                var product = _context.Products.Find(id);

                if (product != null)
                {
                    // Artırma işlemi yapılacaksa stok kontrolü yap
                    if (change > 0 && cartItem.Quantity + change > product.Stock)
                    {
                        // Stok yetersiz ise hata döndür
                        return Json(new { success = false, message = $"Stok yetersiz. Mevcut stok: {product.Stock} adet." });
                    }

                    // Stok yeterliyse miktarı güncelle
                    cartItem.Quantity += change;

                    // Miktar 0 veya daha az ise ürünü sepetten kaldır
                    if (cartItem.Quantity <= 0)
                    {
                        cart.Remove(cartItem);
                    }
                }
            }

            // Güncellenmiş sepeti Session'a kaydet
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true });
        }


        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);

            if (cartItem != null)
            {
                cart.Remove(cartItem); // Ürünü sepetten çıkar
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true });
        }

        public IActionResult CartItemCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            int totalQuantity = cart.Sum(item => item.Quantity);

            return Content(totalQuantity.ToString()); // Toplam miktarı döndür
        }

        [HttpPost]
        public IActionResult UpdateQuantityDirectly(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);

            if (cartItem != null)
            {
                var product = _context.Products.Find(id);

                if (product != null)
                {
                    if (quantity > product.Stock)
                    {
                        // Miktarı maksimum stoğa ayarla ve hata mesajı gönder
                        cartItem.Quantity = product.Stock;
                        HttpContext.Session.SetObjectAsJson("Cart", cart); // Güncellenmiş sepeti kaydet

                        // Stok yetersizse mesaj ve mevcut stok bilgisi ile döndür
                        return Json(new { success = false, message = $"Stok yetersiz. Mevcut stok: {product.Stock} adet.", maxStock = product.Stock });
                    }

                    cartItem.Quantity = quantity;
                    if (cartItem.Quantity <= 0)
                    {
                        cart.Remove(cartItem);
                    }
                }
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true });
        }

        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        [HttpGet]
        public IActionResult UpdateCheckout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            if (cart == null || !cart.Any())
            {
                // Sepet boşsa "_EmptyCartPartial" görünümünü döndür
                return PartialView("_EmptyCartPartial");
            }

            // Sepet doluysa "_CheckoutPartial" görünümünü döndür
            return PartialView("_CheckoutPartial", cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult PlaceOrder()
        {
            // Kullanıcıyı doğrulama
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Home"); // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            }

            int userId = int.Parse(userIdClaim.Value); // Kullanıcı ID'sini al

            // Sepet bilgilerini Session'dan al
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                // Sepet boşsa "_EmptyCartPartial" görünümünü döndür
                return PartialView("_EmptyCartPartial");
            }

            // Toplam tutarı hesapla
            decimal totalAmount = cart.Sum(item => item.Price * item.Quantity);

            // Order kaydını oluştur
            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending, // Başlangıçta beklemede olarak ayarlayın
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };

            // OrderItem kayıtlarını ekle
            foreach (var item in cart)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };

                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order); // Order ve ilişkili OrderItem kayıtlarını veritabanına ekle
            _context.SaveChanges(); // Veritabanına kaydet

            // Sipariş tamamlandıktan sonra sepeti temizle
            HttpContext.Session.Remove("Cart");

            TempData["SuccessMessage"] = "Sipariş başarıyla oluşturuldu!";
            return RedirectToAction("Orders"); // Siparişler sayfasına yönlendirin
        }

        [HttpPost]
        public IActionResult AddReview(int productId, string userName, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Yorum boş olamaz.";
                return RedirectToAction("ListProducts");
            }

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı!";
                return RedirectToAction("ListProducts");
            }


            var review = new Review
            {
                ProductId = productId,
                UserName = userName ?? "Misafir",
                Content = content
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Yorum başarıyla eklendi.";
            return RedirectToAction("ListProducts");
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["query"] = "query";
                // Boş aramalarda boş liste döner
                return View("ListProducts", new List<Product>()); 
            }

            var results = _context.Products
                .Include(p => p.Category) // Category'yi ekleyin
                .Where(p => p.Name.Contains(query) || 
                       p.Brand.Contains(query) ||
                       p.Model.Contains(query) || 
                       p.Category.Name.Contains(query))
                .ToList();

            return View("ListProducts", results); // Sonuçları döndür
        }

     
        
        [HttpGet]
        public IActionResult GetNotifications()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToList();

            return Json(notifications);
        }
        
        [HttpPost]
        public IActionResult MarkAsRead(int notificationId)
        {
            var notification = _context.Notifications.Find(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }

            // Geri dönüş olarak yeni okunmamış bildirim sayısını gönder
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var unreadCount = _context.Notifications.Count(n => n.UserId == userId && !n.IsRead);

            return Json(new { count = unreadCount });
        }


        // Okunmamış bildirim sayısını döndüren metot
        [HttpGet]
        public IActionResult GetUnreadNotificationCount()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var unreadCount = _context.Notifications
                .Count(n => n.UserId == userId && !n.IsRead);

            return Json(new { count = unreadCount });
        }




    }


}


