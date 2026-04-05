using Microsoft.AspNetCore.Mvc;

namespace WebDauGiaUI.Controllers
{
    public class SupportController : Controller
    {
        public IActionResult Regulations() => View(); // Quy chế
        public IActionResult Privacy() => View();    // Bảo mật
        public IActionResult Complaints() => View(); // Khiếu nại
    }
}