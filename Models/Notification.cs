namespace MagazaApp.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // Bildirimi hangi kullanıcıya ait olduğunu belirtmek için
        public int UserId { get; set; }
        public User User { get; set; }
    }

}
