namespace MagazaApp.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Brand { get; set; }  // Marka
        public string Model { get; set; }  // Model
        public string ImagePath { get; set; }  // Ürün resmi yolu
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
