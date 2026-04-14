using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebDauGia.Models;
using WebDauGiaAppli.Interfaces;
using WebDauGiaDomain.Entities;
using WebDauGiaInfrasData;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


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

        [HttpGet]
        // Hàm mở trang Phòng Đấu Giá (Chi tiết sản phẩm)
        public IActionResult Detail(int Id, bool isSeller = false)
        {
            var product = _context.Products.Find(Id);

            // Nếu có người cố tình gõ ID bậy bạ không có thật thì báo lỗi
            if (product == null)
            {
                return NotFound("Không tìm thấy sản phẩm này anh nhé!");
            }

            // Đẩy dữ liệu thật ra ngoài giao diện
            return View(product);
        }

        // 1. Hàm xử lý ô Tìm kiếm theo tên
        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Index"); // Nếu không gõ gì mà bấm tìm thì đuổi về trang chủ
            }

            // Dùng lệnh Where và Contains để tìm các sản phẩm có tên chứa từ khóa
            var products = _context.Products
                .Where(p => p.Name.Contains(query))
                .ToList();

            ViewBag.Keyword = query; // Lưu lại từ khóa để đem ra ngoài hiển thị
            return View(products);
        }

        // 2. Hàm xử lý khi bấm vào Menu Danh mục bên trái
        [HttpGet]
        public IActionResult Explore(int categoryId)
        {
            // Dùng lệnh Where để lọc đúng mã danh mục
            var products = _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToList();

            // Tạm thời gán một cái tên danh mục dựa vào ID để hiển thị cho đẹp
            ViewBag.CategoryName = categoryId switch
            {
                1 => "Đồ điện tử & Công nghệ",
                2 => "Mô hình & Thẻ bài Game",
                3 => "Phương tiện đi lại",
                4 => "Trang sức & Đồng hồ",
                5 => "Kỷ vật thể thao",
                _ => "Sản phẩm"
            };

            return View(products);
        }

        // 1. Phễu lọc: Phiên sắp kết thúc (Dưới 1 tiếng)
        [HttpGet]
        public IActionResult EndingSoon()
        {
            var now = DateTime.Now;
            var oneHourLater = now.AddHours(1);

            // Lọc ra những sản phẩm có thời gian kết thúc lớn hơn hiện tại và nhỏ hơn 1 tiếng nữa
            var products = _context.Products
                .Where(p => p.AuctionEndTime > now && p.AuctionEndTime <= oneHourLater)
                .ToList();

            // Gắn tên danh mục và tái sử dụng lại file giao diện Explore.cshtml
            ViewBag.CategoryName = "Phiên sắp kết thúc (< 1 tiếng)";
            return View("Explore", products);
        }

        // 2. Phễu lọc: Đấu giá từ 0đ
        [HttpGet]
        public IActionResult ZeroPrice()
        {
            // Lọc ra những sản phẩm có giá khởi điểm đúng bằng 0
            var products = _context.Products
                .Where(p => p.StartPrice == 0)
                .ToList();

            // Gắn tên danh mục và tái sử dụng lại file giao diện Explore.cshtml
            ViewBag.CategoryName = "Đấu giá từ 0đ";
            return View("Explore", products);
        }
    }
}
