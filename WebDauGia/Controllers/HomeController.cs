using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebDauGia.Models;
using WebDauGiaAppli.Interfaces;
using WebDauGiaDomain.Entities;
using WebDauGiaInfrasData;


namespace WebDauGiaUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuctionDbContext _context;


        public HomeController(ILogger<HomeController> logger, AuctionDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy toàn bộ sản phẩm từ Database ra
            var products = _context.Products.OrderByDescending(p => p.CreatedAt).ToList();
            return View(products); // Gửi danh sách này sang giao diện
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Hàm mở trang Phòng Đấu Giá (Chi tiết sản phẩm)
        public IActionResult Detail(bool isSeller = false)
        {
            ViewBag.IsSeller = isSeller; // Truyền thông tin người bán cho View
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
