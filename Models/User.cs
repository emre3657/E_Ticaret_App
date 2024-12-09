using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagazaApp.Models
{
    public enum UserType
    {
        Admin,
        Customer

    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Kullanıcı türünü belirtir, varsayılan olarak müşteri olarak ayarlanır
        public UserType UserType { get; set; } = UserType.Customer;

        // Müşteri bilgileri

        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string? PhoneNumber { get; set; }

        // Ek Bilgiler
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Sipariş ilişkisi: Bir kullanıcı birden fazla siparişe sahip olabilir
        // public ICollection<Order>? Orders { get; set; }
    }
}
