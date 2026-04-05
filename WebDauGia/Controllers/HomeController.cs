using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebDauGia.Models;

namespace WebDauGia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Hàm mở trang Phòng Đấu Giá (Chi tiết sản phẩm)
        public IActionResult Detail()
        {
            return View();
        }

        public IActionResult Explore(string category)
        {
            // Thêm dòng này để View biết đang ở danh mục nào
            ViewBag.Category = category;

            ViewBag.CategoryTitle = category switch
            {
                "tech" => "Đồ công nghệ",
                "antique" => "Đồ cổ & Sưu tầm",
                "ending" => "Phiên sắp kết thúc",
                "zero" => "Đấu giá từ 0đ",
                _ => "Khám phá Sản phẩm"
            };
            return View();
        }

        public IActionResult Search(string query) 
        { 
            ViewBag.Query = query; 
            return View(); 
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
