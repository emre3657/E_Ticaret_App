using MagazaApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MagazaApp.Models

{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        public string? Description { get; set; }

        // Kategori yolunu saklamak için
        public string CategoryPath { get; set; }

        // Foreign Key (explicitly defined)
        [ForeignKey("ParentCategory")]
        public int? ParentCategoryId { get; set; }
        

        // İlişkili üst kategori
        public virtual Category? ParentCategory { get; set; }

        // Alt kategoriler için koleksiyon (optional)
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

    }
}
