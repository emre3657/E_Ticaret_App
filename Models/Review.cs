namespace MagazaApp.Models
{
    public class Review
    {
        public int ReviewId { get; set; } // Yorumun ID'si
        public int ProductId { get; set; } // Hangi ürüne ait olduğu
        public string UserName { get; set; } // Yorum yapan kullanıcının adı (isteğe bağlı)
        public string Content { get; set; } // Yorum içeriği
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Yorumun oluşturulma tarihi

        public Product Product { get; set; } // İlgili ürün (Navigation property)
    }
}
