using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebDauGia.Models;
using WebDauGiaAppli.Interfaces;
using WebDauGiaDomain.Entities;
using WebDauGiaInfrasData;
using System.Threading.Tasks;


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

        // Anh dán vào ngay dưới hàm Index nhé
        public async Task<IActionResult> RandomizeAuctionEndTime()
        {
            var products = _context.Products.ToList();
            var random = new Random();
            foreach (var product in products)
            {
                // Gán ngẫu nhiên từ 1 tiếng đến 4 tiếng nữa kết thúc
                int randomMinutes = random.Next(1 * 60, 4 * 60);
                product.AuctionEndTime = DateTime.Now.AddMinutes(randomMinutes);
                _context.Products.Update(product);
            }
            await _context.SaveChangesAsync();
            return Content("Đã gán thời gian ngẫu nhiên thành công! Anh hãy xóa hàm này đi nhé.");
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
