using Microsoft.AspNetCore.Mvc;
using MagazaApp.Data;
using MagazaApp.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MagazaApp.Models;

public class CategoryMenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CategoryMenuViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        // İlk olarak ana kategorileri alıyoruz (üst kategoriler)
        var categories = _context.Categories
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToList();

        // Her kategori için alt kategorileri dinamik olarak yükleyin
        foreach (var category in categories)
        {
            LoadSubCategories(category);
        }

        var viewModel = new CategoryListViewModel
        {
            Categories = categories
        };

        return View(viewModel);
    }

    private void LoadSubCategories(Category category)
    {
        // Alt kategorileri getir ve sırala
        category.SubCategories = _context.Categories
            .Where(c => c.ParentCategoryId == category.Id)
            .OrderBy(c => c.Name)
            .ToList();

        // Alt kategoriler varsa, her alt kategorinin alt kategorilerini de getir
        foreach (var subCategory in category.SubCategories)
        {
            LoadSubCategories(subCategory); // Rekürsif olarak alt kategorileri yükle
        }
    }


}
