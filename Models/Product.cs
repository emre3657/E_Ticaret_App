using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagazaApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Brand is required.")]
        public required string Brand { get; set; }

        [Required(ErrorMessage = "Model is required.")]
        public required string Model { get; set; }

        [Required(ErrorMessage = "Features is required.")]
        public required string Features { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative value.")]
        public required int Stock { get; set; }

        // Foreign Key for Category
        [Required(ErrorMessage = "Category is required. Create a category if you haven't already.")]
        public int CategoryId { get; set; }

        // Benzersiz SKU
        public string? SKU { get; set; }

        // Resim yolu veya URL'si (örneğin: '/images/products/urun1.jpg')
        public required string ImagePath { get; set; }

        // İlişkili kategori
        public virtual Category? Category { get; set; }

        [NotMapped] // Bu alan veritabanına kaydedilmez
        public int Quantity { get; set; }

    }
}
