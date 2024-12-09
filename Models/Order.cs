
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagazaApp.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public int UserId { get; set; }

        
        public User User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Sipariş içindeki ürünleri temsil eden OrderItems koleksiyonu
        public ICollection<OrderItem> OrderItems { get; set; }
    }

    // Sipariş durumlarını belirten bir enum
    public enum OrderStatus
    {
        Pending,   // Beklemede
        Approved,  // Onaylandı
        Rejected   // Reddedildi
    }
}
