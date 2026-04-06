using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebDauGiaAppli.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using WebDauGiaInfrasData;
using WebDauGiaDomain.Entities;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Http;

namespace WebDauGiaUI.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly AuctionDbContext _context;

        public UserController(AuctionDbContext context, IUserRepository userRepository  )     
        {
            _context = context;
                _userRepository = userRepository;
        }

        // Tiêm (Inject) IUserRepository vào thông qua Constructor
            

        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ danh sách user từ cơ sở dữ liệu
            var users = await _userRepository.GetAllUsersAsync();

            // Đưa dữ liệu sang View để hiển thị
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateAuction()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateAuction(string ProductName, int CategoryId, decimal StartPrice, string Description, IFormFile ImageFile)
        {
            string imagePath = "/images/no-image.png"; // Ảnh mặc định nếu anh không tải ảnh

            if (ImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                imagePath = "/uploads/" + fileName;
            }

            // TẠO ĐỐI TƯỢNG VÀ LƯU VÀO DATABASE
            var product = new Product
            {
                Name = ProductName,
                CategoryId = CategoryId,
                StartPrice = StartPrice,
                Description = Description,
                ImageUrl = imagePath,
                CreatedAt = DateTime.Now,
                AuctionEndTime = DateTime.Now.AddHours(4) // Mặc định đấu giá kết thúc sau 4 giờ, anh có thể chỉnh lại sau
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // Lệnh này là quan trọng nhất để dữ liệu xuất hiện trong SSMS

            return RedirectToAction("Index", "Home");
        }
        public IActionResult MyAccount()
        {
            return View();
        }
        public IActionResult Profile()
        {
            // Tạm thời em trả về View trống để anh dựng UI chụp ảnh nộp bài trước.
            // Sau này làm database cho đấu giá xong mình sẽ truyền dữ liệu thật vào đây.
            return View();
        }

        // Trang Quản lý Đấu giá của tôi
        public IActionResult MyAuctions()
        {
            return View();
        }
        public IActionResult ForgotPassword() 
        { 
            return View(); 
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý khi bấm nút Đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password) // THÊM PASSWORD VÀO ĐÂY
        {
            var captchaResponse = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(captchaResponse))
            {
                TempData["Error"] = "Anh vui lòng xác nhận mình không phải là người máy nhé.";
                return View();
            }

            var secretKey = "6Lc136gsAAAAACHmm0POdycuFGmRWQ8E4etF__--";
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}", null);
                var jsonString = await response.Content.ReadAsStringAsync();

                if (!jsonString.Contains("\"success\": true"))
                {
                    TempData["Error"] = "Xác thực reCAPTCHA thất bại. Anh thử lại xem sao.";
                    return View();
                }
            }

            // Tìm user trong Database
            var user = await _userRepository.GetUserByUsernameAsync(username);

            // KIỂM TRA THÊM MẬT KHẨU
            if (user != null && user.PasswordHash == password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.UserID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Tài khoản hoặc mật khẩu không chính xác!";
            return View();
        }
        // Xử lý Đăng xuất
        public async Task<IActionResult> Logout()
        {
            // Xóa Cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Hiển thị form đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Xử lý khi người dùng bấm nút Đăng ký
        [HttpPost]
        public async Task<IActionResult> Register(WebDauGiaDomain.Entities.User user)
        {
            // Kiểm tra xem tên đăng nhập đã có ai dùng chưa
            var existingUser = await _userRepository.GetUserByUsernameAsync(user.Username);
            if (existingUser != null)
            {
                ViewBag.Error = "Tên đăng nhập này đã có người sử dụng rồi anh nhé. Hãy chọn tên khác.";
                return View(user);
            }

            if (ModelState.IsValid)
            {
                // Tự động gán các quyền và chỉ số mặc định cho người mới
                user.Role = "Bidder";
                user.TrustScore = 100;
                user.WalletBalance = 0;

                await _userRepository.AddUserAsync(user);

                // Đăng ký thành công thì em cho chuyển thẳng sang trang Đăng nhập
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // Hàm phụ: Dùng MailKit để gửi thư qua máy chủ Gmail
        private void SendOtpEmail(string toEmail, string otpCode)
        {
            var message = new MimeMessage();
            // Anh thay email của anh vào chỗ này nhé
            message.From.Add(new MailboxAddress("AuctionHub Security", "lequan0971662799@gmail.com"));
            message.To.Add(new MailboxAddress("Người dùng", toEmail));
            message.Subject = "Mã xác thực OTP - AuctionHub";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #0d6efd;'>Khôi phục mật khẩu</h2>
                        <p>Mã xác thực (OTP) của bạn là:</p>
                        <h1 style='color: #dc3545; letter-spacing: 5px;'>{otpCode}</h1>
                        <p>Mã này sẽ hết hạn sau 5 phút. Vui lòng không chia sẻ cho người khác.</p>
                    </div>"
            };

            using (var client = new SmtpClient())
            {
                // Kết nối tới máy chủ Gmail
                client.Connect("smtp.gmail.com", 587, false);

                // Anh thay Email và cái Mật khẩu ứng dụng 16 ký tự (ở Bước 1) vào đây:
                client.Authenticate("lequan0971662799@gmail.com", "pgcm njbt chvt wnhf");

                client.Send(message);
                client.Disconnect(true);
            }
        }

        // Hàm chính: Xử lý khi nhấn nút "Gửi mã" trên giao diện
        [HttpPost]
        public IActionResult SendOtp(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Vui lòng nhập email.");

            // 1. Tạo ngẫu nhiên mã OTP 6 chữ số
            Random rnd = new Random();
            string otp = rnd.Next(100000, 999999).ToString();

            // 2. Lưu OTP và Email vào bộ nhớ ngắn hạn (Session)
            HttpContext.Session.SetString("SavedOTP", otp);
            HttpContext.Session.SetString("ResetEmail", email);

            // 3. Gửi email đi
            SendOtpEmail(email, otp);

            // 4. Trả về thông báo thành công cho giao diện
            return Json(new { success = true, message = "Đã gửi mã OTP thành công! Vui lòng kiểm tra hộp thư." });
        }

        [HttpPost]
        public IActionResult VerifyOtp(string otpCode)
        {
            // 1. Lấy mã OTP gốc đã lưu trong bộ nhớ tạm ra
            string savedOtp = HttpContext.Session.GetString("SavedOTP");

            // 2. So sánh
            if (string.IsNullOrEmpty(savedOtp))
            {
                TempData["Error"] = "Mã OTP đã hết hạn hoặc bạn chưa yêu cầu gửi mã.";
                return RedirectToAction("ForgotPassword");
            }

            if (otpCode == savedOtp)
            {
                // Nếu đúng mã, cho phép đi tiếp sang trang đặt lại mật khẩu
                return RedirectToAction("ResetPassword");
            }
            else
            {
                // Nếu sai mã, đuổi về trang cũ và báo lỗi
                TempData["Error"] = "Mã xác thực không chính xác. Anh kiểm tra lại email nhé!";
                return RedirectToAction("ForgotPassword");
            }
        }

        // 1. Hàm hiển thị trang nhập mật khẩu mới
        [HttpGet]
        public IActionResult ResetPassword()
        {
            // Phải có email trong bộ nhớ tạm (tức là đã qua bước OTP) thì mới cho vào trang này
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            return View(); // Anh nhớ tạo thêm file ResetPassword.cshtml trong thư mục Views/User nhé
        }

        // 2. Hàm xử lý lưu mật khẩu mới xuống Database
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Hai mật khẩu không khớp nhau. Anh nhập lại cẩn thận nhé.";
                return View();
            }

            // Tìm tài khoản trong Database dựa vào Email
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.PasswordHash = newPassword; // Gán mật khẩu mới
                _context.Users.Update(user); // Cập nhật
                await _context.SaveChangesAsync(); // Lưu xuống DB

                // Dọn dẹp trí nhớ của hệ thống
                HttpContext.Session.Remove("ResetEmail");
                HttpContext.Session.Remove("SavedOTP");

                // Đổi thành công thì đuổi về trang Đăng nhập
                TempData["Success"] = "Đổi mật khẩu thành công! Mời anh đăng nhập lại.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = "Không tìm thấy tài khoản của anh trong hệ thống.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            // 1. Kiểm tra xem người dùng đã đăng nhập chưa
            var userIdString = User.FindFirst("UserId")?.Value;
            if (userIdString == null) return Json(new { success = false, message = "Vui lòng đăng nhập anh nhé." });
            var userId = int.Parse(userIdString);

            // 2. Kiểm tra sản phẩm có tồn tại không
            var product = _context.Products.Find(productId);
            if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại." });

            // 3. Tra cứu xem đã yêu thích chưa
            var favorite = _context.FavoriteProducts.FirstOrDefault(fp => fp.UserId == userId && fp.ProductId == productId);

            if (favorite == null)
            {
                // Chưa có thì thêm mới
                var newFavorite = new FavoriteProduct { UserId = userId, ProductId = productId };
                _context.FavoriteProducts.Add(newFavorite);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorited = true, message = "Đã thêm vào yêu thích." });
            }
            else
            {
                // Có rồi thì xóa đi (hủy yêu thích)
                _context.FavoriteProducts.Remove(favorite);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorited = false, message = "Đã xóa khỏi yêu thích." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FavoriteProducts()
        {
            // 1. Kiểm tra xem anh đã đăng nhập chưa
            var userIdString = User.FindFirst("UserId")?.Value;
            if (userIdString == null)
            {
                return RedirectToAction("Login");
            }
            var userId = int.Parse(userIdString);

            // 2. Tìm danh sách ID những sản phẩm anh đã thích
            var favoriteIds = _context.FavoriteProducts
                .Where(fp => fp.UserId == userId)
                .Select(fp => fp.ProductId)
                .ToList();

            // 3. Lấy thông tin chi tiết của những sản phẩm đó
            var favoriteProducts = _context.Products
                .Where(p => favoriteIds.Contains(p.Id))
                .ToList();

            // 4. Trả về View cùng với danh sách sản phẩm
            return View(favoriteProducts);
        }
    }
}
