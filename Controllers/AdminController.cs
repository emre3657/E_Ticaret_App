using Microsoft.AspNetCore.Mvc;
using MagazaApp.Models;
using MagazaApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting;
using MagazaApp.ViewModels;
using System.IO;
using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;






namespace MagazaApp.Controllers
{
    
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public AdminController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

   

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

    
        [HttpGet]
        [Route("category")]
        public ActionResult Category()
        {
            var categories = _context.Categories
                .OrderBy(c => c.Name) // Kategorileri isme göre alfabetik sırayla getirir
                .ToList();

            var viewModel = new CategoryListViewModel
            {
                Categories = categories
            };

            return View("Category", viewModel);
        }

        [HttpGet]
        [Route("product")]
        public ActionResult Product()
        {
            var products = _context.Products
                .Include(p => p.Category)  // Kategoriyi dahil et
                .OrderBy(p => p.Name)
                .ToList();

            var viewModel = new ProductListViewModel
            {
                Products = products
            };

            // Kategorileri al ve ViewBag'e ekle
            ViewBag.Categories = GetCategories();

            return View("Product", viewModel);
        }

        [HttpGet]
        [Route("confirmations")]
        public IActionResult Confirmations()
        {
            // Bekleyen siparişleri getir
            var Orders = _context.Orders
                .Include(o => o.User) // User ilişkisini dahil et
                .ToList();

            return View("Confirmations", Orders);
        }

        [HttpGet]
        [Route("order-details")]
        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


        [Route("add-category")]
        [HttpGet]
        public ActionResult AddCategory()
        {
            // Kategorileri al ve ViewBag'e ekle
            ViewBag.Categories = GetCategories();
            return View("CreateCategory");
        }

        [Route("add-product")]
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Kategorileri al ve ViewBag'e ekle
            ViewBag.Categories = GetCategories();
            return View("CreateProduct");
        }


        public List<Category> GetCategories()
        {
            // Veritabanından kategorileri al
            var categories = _context.Categories.OrderBy(c => c.Name).ToList(); // Tüm kategorileri alır
            return categories;
        }



        [HttpPost]
        [Route("create-product")]
        public async Task<ActionResult> CreateProduct(Product product, IFormFile imageFile)
        {
            // SKU otomatik olarak oluşturuluyor (brand + model + features)
            product.SKU = $"{product.Brand}-{product.Model}-{product.Features}".Trim().Replace(" ", "-").ToUpper();

            // Aynı SKU'ya sahip ürün var mı kontrol ediyoruz
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.SKU.Equals(product.SKU));

            if (existingProduct != null)
            {
                TempData["ErrorMessageSKU"] = "A product with the same SKU already exists!";
                TempData["SKU"] = "The SKU: " + product.SKU;
                ViewBag.Categories = GetCategories();
                return View("CreateProduct");
            }

            // Kategori bilgilerini veritabanından alıyoruz
            var category = await _context.Categories.FindAsync(product.CategoryId);

            // CategoryPath kullanarak dosya yolunu oluşturuyoruz
            string categoryFolderPath = category.CategoryPath;

            // Resim yükleme işlemi
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, categoryFolderPath, fileName);

                // Resmi belirtilen klasöre kaydetme
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Ürün resim yolunu ayarlıyoruz (kategori klasörü altına)
                product.ImagePath = $"/{categoryFolderPath.Replace(_webHostEnvironment.WebRootPath, "")}/{fileName}".Replace("\\", "/");
            }

            // Ürünü veritabanına ekleme
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product has been created successfully!";
            TempData["SKU"] = product.SKU;

            ViewBag.Categories = GetCategories();
            return View("CreateProduct");
        }

        [HttpDelete]
        [Route("delete-product/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            // Silme işlemi öncesinde bir log ekleyin
            Console.WriteLine("Ürün bulundu, siliniyor: " + id);

            // Resmi sil
            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImagePath.TrimStart('/'));
                Console.WriteLine("imagepath: " + imagePath);
                if (System.IO.File.Exists(imagePath))
                {
                    Console.WriteLine("imagı sildim");
                    System.IO.File.Delete(imagePath);
                }
            }
          
            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Route("get-product/{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Return product data as JSON
            return Json(new
            {
                id = product.Id,
                name = product.Name,
                brand = product.Brand,
                model = product.Model,
                features = product.Features,
                description = product.Description,
                price = product.Price,
                stock = product.Stock,
                imagePath = product.ImagePath,
                categoryId = product.CategoryId
            });
        }

        [HttpPost]
        [Route("edit-product/{id}")]
        public async Task<ActionResult> EditProduct(int id, Product updatedProduct, IFormFile editImageFile)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return Json(new { success = false, errorMessage = "Product not found!" });
            }

            // SKU otomatik olarak oluşturuluyor (brand + model + features)
            product.SKU = $"{updatedProduct.Brand}-{updatedProduct.Model}-{updatedProduct.Features}".Trim().Replace(" ", "-").ToUpper();

            // Aynı SKU'ya sahip başka bir ürün olup olmadığını kontrol et (mevcut düzenlenen ürün hariç)
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.SKU.Equals(product.SKU) && p.Id != product.Id);

            if (existingProduct != null)
            {
                return Json(new { success = false, errorMessage = $"A product with the same SKU already exists! SKU: {product.SKU}" });
            }

            // Diğer güncellemeleri yap
            product.Name = updatedProduct.Name;
            product.Brand = updatedProduct.Brand;
            product.Model = updatedProduct.Model;
            product.Features = updatedProduct.Features;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;
            product.CategoryId = updatedProduct.CategoryId;

            // Eğer yeni bir resim yüklendiyse
            if (editImageFile != null && editImageFile.Length > 0)
            {
                // Eski resmi silme işlemi
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

                // Kategori bilgilerini veritabanından alıyoruz
                var category = await _context.Categories.FindAsync(product.CategoryId);
                string categoryFolderPath = category.CategoryPath;

                var fileName = Path.GetFileName(editImageFile.FileName);
                var newImagePath = Path.Combine(_webHostEnvironment.WebRootPath, categoryFolderPath, fileName);

                // Resmi belirtilen klasöre kaydetme
                using (var stream = new FileStream(newImagePath, FileMode.Create))
                {
                    await editImageFile.CopyToAsync(stream);
                }

                // Ürün resim yolunu ayarlıyoruz (kategori klasörü altına)
                product.ImagePath = $"/{categoryFolderPath.Replace(_webHostEnvironment.WebRootPath, "")}/{fileName}".Replace("\\", "/");
            }
            // Eğer yeni resim yoksa, eski resim taşınacak
            else
            {
                // Eski resim yolunu al
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImagePath.TrimStart('/'));

                // Yeni kategori yolunu al
                var category = await _context.Categories.FindAsync(product.CategoryId);
                string newCategoryFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, category.CategoryPath);

                var fileName = Path.GetFileName(oldImagePath); // Eski resmin dosya adını al
                var newImagePath = Path.Combine(newCategoryFolderPath, fileName); // Yeni yol

                // Eğer eski resim varsa ve yeni kategori klasörüne taşınması gerekiyorsa
                if (oldImagePath != newImagePath)
                {
         
                    // Eski resmi yeni kategori klasörüne taşı
                    System.IO.File.Move(oldImagePath, newImagePath);

                    // Ürün resim yolunu yeni kategoriye göre güncelle
                    product.ImagePath = $"/{category.CategoryPath}/{fileName}".Replace("\\", "/");
                }
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }



        [HttpPost]
        [Route("create-category")]
        public ActionResult CreateCategory(string name, Category category)
        {
            // Kategori isminin zaten var olup olmadığını kontrol et
            bool categoryExists = _context.Categories
                .Any(c => c.Name.Trim().Replace(" ", "").ToLower() == name.Trim().Replace(" ", "").ToLower());

            if (categoryExists)
            {
                TempData["ErrorMessage"] = "A category with this name already exists.";
                ViewBag.Categories = GetCategories();
                return View("CreateCategory");
            }

            // parentCategory == None
            if (category.ParentCategoryId == null)
            {
                // Üst kategori yoksa, doğrudan ana kategori oluşturulacak
                category.CategoryPath = Path.Combine("images", "categories", category.Name.Trim().ToLower().Replace(" ", "-"));
                category.ParentCategory = null;

            }

            // parentCategory != None
            // Eğer bir ParentCategoryId seçildiyse, bu kategoriyi üst kategoriye bağla
            else 
            {
                category.ParentCategory = _context.Categories.Find(category.ParentCategoryId);
                category.ParentCategory.SubCategories.Add(category);
                Console.WriteLine("subcategory: " + category.ParentCategory.SubCategories);
                string parentCategoryPath = category.ParentCategory.CategoryPath;
                category.CategoryPath = Path.Combine(parentCategoryPath, category.Name.Trim().ToLower().Replace(" ", "-"));
            }
            

            // Yeni kategoriyi veritabanına ekliyoruz
            _context.Categories.Add(category);
            _context.SaveChanges(); // Veritabanına kaydet

            // Kategori için klasör oluşturma işlemi
            string categoryPath = Path.Combine(_webHostEnvironment.WebRootPath, category.CategoryPath);
            if (!Directory.Exists(categoryPath))
            {
                Directory.CreateDirectory(categoryPath);
            }

            // Başarılı mesajını TempData'ya ekleyin
            TempData["SuccessMessage"] = $"The category \"{category.Name}\" has been created successfully!";
            ViewBag.Categories = GetCategories();
            return View("CreateCategory");
        }

        [HttpDelete]
        [Route("delete-category/{id}")]
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories
                .Include(c=> c.SubCategories)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Make sure there are no subcategories or associated products before deleting
            if (category.SubCategories.Any())
            {
                return BadRequest(new { message = "Cannot delete a category that has subcategories." });
            }

            // Optionally check for products in this category, if applicable
             if (_context.Products.Any(p => p.CategoryId == id)) {
                 return BadRequest(new { message = "Cannot delete category with associated products." });
             }

            // Delete the category directory
            if (!string.IsNullOrEmpty(category.CategoryPath))
            {
                var categoryDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.CategoryPath.TrimStart('/'));

                if (Directory.Exists(categoryDirectoryPath))
                {
                    try
                    {
                        // Delete directory and its contents recursively
                        Directory.Delete(categoryDirectoryPath); // if second parameter is true, it will be recursively deletion.
                        Console.WriteLine("The category directory have been deleted!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting category directory: {ex.Message}");
                        return StatusCode(500, new { message = "Error deleting category directory." });
                    }
                }
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Route("get-category/{id}")]
        public IActionResult GetCategoryById(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            // tüm kategorileri getir.
            var allCategories = GetCategories();

            // Kendisini çıkar
            allCategories.Remove(category);

            // Alt kategorileri bul
            var subCategories = category.SubCategories;
            
            // Alt kategorileri çıkar
            allCategories = allCategories.Where(c => !subCategories.Contains(c)).OrderBy(c => c.Name).ToList(); // Alt kategorileri listeden çıkar



            return Json(new
            {
                id = category.Id,
                name = category.Name,
                description = category.Description,
                parentCategoryId = category.ParentCategoryId,
                allCategories = allCategories.Select(c => new { c.Id, c.Name }),

            });

        }
       
        [HttpPost]
        [Route("edit-category/{id}")]
        public async Task<IActionResult> EditCategory(int id, Category updatedCategory)
        {
            try {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return Json(new { success = false, errorMessage = "Category not found!" });
                }


                // Kategori isminin zaten var olup olmadığını kontrol et
                bool categoryExists = _context.Categories
                    .Any(c => c.Name.Trim().Replace(" ", "").ToLower() == updatedCategory.Name.Trim().Replace(" ", "").ToLower() && c.Id != updatedCategory.Id);

                if (categoryExists)
                {
                    return Json(new { success = false, errorMessage = "A category with the same name already exists!" });
                }

                // Kategori güncellemesi
                category.Name = updatedCategory.Name;
                category.Description = updatedCategory.Description;
                // category.ParentCategoryId = updatedCategory.ParentCategoryId;

                // kategori değiştirilmemiş ise:
                if (category.ParentCategoryId == updatedCategory.ParentCategoryId)
                {

                    _context.Categories.Update(category);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });

                }

                // kategori değiştirilmiş ise:
                else
                {

                    // güncellenmiş üst kategori None ise / new parentCategoryId == Null
                    if (updatedCategory.ParentCategoryId == null)
                    {
                        // var olan kategorinin üst kategorisi var ise üst kategorinin alt kategori listesinden bu kategoriyi çıkar!!!
                        if (category.ParentCategoryId != null)
                        {
                            category.ParentCategory = _context.Categories.Find(category.ParentCategoryId);
                            category.ParentCategory.SubCategories.Remove(category);

                        }

                        // üst kategori ve üst kategor id'si null yapma
                        category.ParentCategoryId = null;
                        category.ParentCategory = null;

                        // veri tabanı işlemleri
                        string oldCategoryPath = Path.Combine(_webHostEnvironment.WebRootPath, category.CategoryPath).TrimStart('/');
                        category.CategoryPath = Path.Combine("images", "categories", category.Name.Trim().ToLower().Replace(" ", "-"));
                        // Üst kategori yoksa, doğrudan ana kategori oluşturulacak
                        string newCategoryPath = Path.Combine(_webHostEnvironment.WebRootPath, category.CategoryPath);

                        // Sunucuda kategori klasörünü taşı
                        System.IO.Directory.Move(oldCategoryPath, newCategoryPath);

                        // Ürünlerin resim yollarını güncelleme
                        var products = _context.Products.Where(p => p.CategoryId == category.Id).ToList();
                        foreach (var product in products)
                        {
                            // image pathini bul 
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImagePath.TrimStart('/'));
                            // pathi ile dosya adınını bul
                            string fileName = Path.GetFileName(oldImagePath);
                            // veri tabanına yeni image pathi kaydet
                            product.ImagePath = "/" + Path.Combine(category.CategoryPath, fileName).Replace("\\", "/");
                            // resmin sunucudaki yeni yeri
                            string newImagePath = Path.Combine(newCategoryPath, fileName);

                            // Eski dosya mevcutsa yeni konuma taşı
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Move(oldImagePath, newImagePath);
                            }

                        }

                        _context.Categories.Update(category);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true });


                    }

                    // güncellenmiş üst kategori None değil ise / new parentCategoryId != Null
                    else
                    {
                        // üst kategorisi var ise üst kategorinin alt kategori listesinden bu kategoriyi çıkar!!!
                        if (category.ParentCategoryId != null)
                        {
                            category.ParentCategory = _context.Categories.Find(category.ParentCategoryId);
                            category.ParentCategory.SubCategories.Remove(category);
                        }

                        // üst kategori id'si değiştirme
                        category.ParentCategoryId = updatedCategory.ParentCategoryId;

                        // üst kategoriyi değiştirme
                        category.ParentCategory = _context.Categories.Find(updatedCategory.ParentCategoryId);
                        category.ParentCategory.SubCategories.Add(category);


                        string oldCategoryPath = Path.Combine(_webHostEnvironment.WebRootPath, category.CategoryPath).TrimStart('/');

                        // veri tabanı işlemleri
                        category.ParentCategory = _context.Categories.Find(updatedCategory.ParentCategoryId);
                        category.CategoryPath = Path.Combine(category.ParentCategory.CategoryPath, category.Name.Trim().ToLower().Replace(" ", "-"));
                        string newCategoryPath = Path.Combine(_webHostEnvironment.WebRootPath, category.CategoryPath);
 
                        // Sunucuda kategori klasörünü taşı
                        System.IO.Directory.Move(oldCategoryPath, newCategoryPath);

                        // Ürünlerin resim yollarını güncelleme
                        var products = _context.Products.Where(p => p.CategoryId == category.Id).ToList();
                        foreach (var product in products)
                        {
                            // image pathini bul 
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImagePath.TrimStart('/'));
                            // pathi ile dosya adınını bul
                            string fileName = Path.GetFileName(oldImagePath);
                            // veri tabanına yeni image pathi kaydet
                            product.ImagePath = "/" + Path.Combine(category.CategoryPath, fileName).Replace("\\", "/");
                            // resmin sunucudaki yeni yeri
                            string newImagePath = Path.Combine(newCategoryPath, fileName);

                            // Eski dosya mevcutsa yeni konuma taşı
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Move(oldImagePath, newImagePath);
                            }
                        }

                        _context.Categories.Update(category);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true });
                        

                    }

                }

            }
            
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = $"An error occurred: {ex.Message}" });
            }

        }


        [HttpPost]
        [Route("confirm-order")]
        public async Task<IActionResult> ConfirmOrder(int orderId, bool isApproved)
        {
            var order = _context.Orders.Include(o => o.OrderItems)
                                       .ThenInclude(oi => oi.Product) // Her OrderItem ile ilişkili ürünü de dahil et
                                       .FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                if (isApproved)
                {
                    // Ürün stoklarını güncelle
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Quantity <= item.Product.Stock)
                        {
                            item.Product.Stock -= item.Quantity;
                        }
                        else
                        {
                            TempData["ErrorMessage"] = $"Hata: Stok Yetersizliği<br />SKU: {item.Product.SKU}<br />Mevcut stok: {item.Product.Stock}";
                            return RedirectToAction("Confirmations");
                        }
                        
                    }
                    // Sipariş durumu güncelle
                    order.Status = OrderStatus.Approved;
                    TempData["ConfirmationMessage"] = $"{order.OrderId} nolu sipariş onaylandı.";

                    // Bildirim oluşturma
                    CreateNotification(order.UserId, $"{order.OrderId} nolu siparişiniz onaylandı.");
                }
                else
                {
                    // Reddedildi olarak güncelle
                    order.Status = OrderStatus.Rejected;
                    TempData["ConfirmationMessage"] = $"{order.OrderId} nolu sipariş reddedildi.";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Confirmations");
        }

        // Bildirim oluşturma metodu
        [HttpPost]
        [Route("create-notification")]
        public IActionResult CreateNotification(int userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Bildirim oluşturuldu" });
        }



        [HttpPost]
        [Route("cancel-order")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = _context.Orders.Include(o => o.OrderItems)
                                       .ThenInclude(oi => oi.Product)
                                       .FirstOrDefault(o => o.OrderId == orderId);

            if (order != null && order.Status == OrderStatus.Approved)
            {
                // Stokları eski haline getir
                foreach (var item in order.OrderItems)
                {
                    item.Product.Stock += item.Quantity;
                }

                // Sipariş durumunu 'Pending' olarak güncelle
                order.Status = OrderStatus.Pending;
                TempData["ConfirmationMessage"] = $"{order.OrderId} nolu sipariş iptal edildi ve bekleme durumuna alındı.";

                await _context.SaveChangesAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "Sipariş iptal edilemedi.";
            }

            return RedirectToAction("Confirmations");
        }



        [HttpPost]
        [Route("delete-order")]
        public IActionResult DeleteOrder(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Her OrderItem ile ilişkili ürünü dahil et
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
              
                // Order'a bağlı OrderItem'leri de sil
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                _context.SaveChanges();

                TempData["ConfirmationMessage"] = $"{order.OrderId} nolu sipariş başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Sipariş bulunamadı.";
            }

            return RedirectToAction("Confirmations");
        }


        [HttpGet]
        [Route("check-stock")]
        public IActionResult CheckStock(int productId, int quantity, int orderItemId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            var order = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.OrderItems.Any(oi => oi.OrderItemId == orderItemId));

            var orderItem = order?.OrderItems.FirstOrDefault(oi => oi.OrderItemId == orderItemId);



            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Miktar 0 ya da daha düşük olamaz!" });
            }
            
            if (order.Status == OrderStatus.Approved)
            {
                if (quantity > product.Stock + orderItem.Quantity)
                {
                    return Json(new { success = false, message = $"Yeterli stok yok. Mevcut stok: {product.Stock + orderItem.Quantity}" });
                }
            }

            else // order.Status == OrderStatus.Pending
            {
                if (quantity > product.Stock)
                {
                    return Json(new { success = false, message = $"Yeterli stok yok. Mevcut stok: {product.Stock}" });
                }
            }

           

            return Json(new { success = true });
        }

        [HttpPost]
        [Route("update-order-item")]
        public IActionResult UpdateOrderItem(int orderItemId, int quantity)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.OrderItems.Any(oi => oi.OrderItemId == orderItemId));

            var orderItem = order?.OrderItems.FirstOrDefault(oi => oi.OrderItemId == orderItemId);


            if (orderItem == null || quantity <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz ürün veya miktar.";
                return RedirectToAction("OrderDetails", new { id = orderItem?.OrderId });
            }

            // Siparişin mevcut durumu

            if (order?.Status == OrderStatus.Approved)
            {
                // Stok miktarını eski hale getir (eski miktarı iade et)
                orderItem.Product.Stock += orderItem.Quantity;

                // Yeni miktarı düş 
                orderItem.Product.Stock -= quantity; 
             
            }

            // Ürün miktarını güncelle ve toplam tutarı yeniden hesapla
            orderItem.Quantity = quantity;

            // İtemin güncel fiyatı
            orderItem.UnitPrice = orderItem.Product.Price;

            // Siparişin toplam tutarını yeniden hesapla
            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

            _context.SaveChanges();

            TempData["ConfirmationMessage"] = $"Ürün miktarı başarıyla güncellendi.";
            return RedirectToAction("OrderDetails", new { id = order.OrderId });
        }


        [HttpPost]
        [Route("delete-order-item")]
        public IActionResult DeleteOrderItem(int orderItemId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.OrderItems.Any(oi => oi.OrderItemId == orderItemId));

            var orderItem = order?.OrderItems.FirstOrDefault(oi => oi.OrderItemId == orderItemId);

            if (orderItem == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction("OrderDetails", new { id = orderItem?.Order.OrderId });
            }

            if (order.Status == OrderStatus.Approved)
            {
                orderItem.Product.Stock += orderItem.Quantity;
            }

            // OrderItem'ı siparişten sil
            _context.OrderItems.Remove(orderItem);

            // Siparişte başka ürün kalmadıysa, siparişi de sil
            if (order.OrderItems.Count == 1)
            {
                _context.Orders.Remove(order);
                TempData["ConfirmationMessage"] = "Sipariş içindeki son ürün silindi. Sipariş de silindi.";
                _context.SaveChanges();
                return RedirectToAction("Confirmations");
            }
            else
            {
                // Siparişin toplam tutarını güncelle
                order.TotalAmount = order.OrderItems
                    .Where(oi => oi.OrderItemId != orderItemId)
                    .Sum(oi => oi.Quantity * oi.UnitPrice);

                TempData["ConfirmationMessage"] = "Ürün siparişten başarıyla silindi.";
                _context.SaveChanges();
                return RedirectToAction("OrderDetails", new { id = order.OrderId });
            }

            
        }




    }

}

