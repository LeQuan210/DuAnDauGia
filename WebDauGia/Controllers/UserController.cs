using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebDauGiaAppli.Interfaces;
using System.Threading.Tasks;

namespace WebDauGiaUI.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        // Tiêm (Inject) IUserRepository vào thông qua Constructor
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ danh sách user từ cơ sở dữ liệu
            var users = await _userRepository.GetAllUsersAsync();

            // Đưa dữ liệu sang View để hiển thị
            return View(users);
        }

        public IActionResult CreateAuction()
        {
            return View();
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
        public IActionResult ResetPassword() 
        { 
            return View(); 
        }
        // Hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý khi bấm nút Đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string username)
        {
            // Tìm trong Database xem có user này không
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user != null)
            {
                // 1. Tạo "Chứng minh thư" chứa thông tin cơ bản
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.UserID.ToString()) // Lưu lại ID để sau này dùng
                };

                // 2. Đóng dấu chứng minh thư
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. Cấp phát Cookie và cho phép đăng nhập
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                // Đăng nhập xong thì quay về trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Nếu nhập sai thì báo lỗi
            ViewBag.Error = "Tên đăng nhập không tồn tại trong hệ thống!";
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
    }
}
