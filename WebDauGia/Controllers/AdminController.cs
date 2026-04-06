using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebDauGiaDomain.Entities;
using WebDauGiaInfrasData;

namespace WebDauGiaUI.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ ai có quyền Admin mới được vào đây
    public class AdminController : Controller
    {
        private readonly AuctionDbContext _context;
        public AdminController(AuctionDbContext context) { _context = context; }

        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsLocked = !user.IsLocked; // Đảo trạng thái Khóa/Mở
                _context.Users.Update(user);
                var adminId = User.FindFirst("UserId")?.Value;
                var log = new SystemLog
                {
                    UserId = adminId != null ? int.Parse(adminId) : null,
                    Action = $"{(user.IsLocked ? "Khóa" : "Mở khóa")} tài khoản: {user.Username}",
                    Timestamp = DateTime.Now,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                };
                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Logs()
        {
            // Lấy nhật ký, sắp xếp cái mới nhất lên đầu
            var logs = _context.SystemLogs.OrderByDescending(l => l.Timestamp).ToList();
            return View(logs);
        }
    }
}
