using Microsoft.AspNetCore.Mvc;
using MagazaApp.Models;
using System.Linq;
using MagazaApp.Data;

public class NotificationsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NotificationsViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke(int userId)
    {
        var notifications = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .ToList();

        return View(notifications);
    }
}
