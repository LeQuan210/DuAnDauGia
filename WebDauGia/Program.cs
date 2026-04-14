using Microsoft.EntityFrameworkCore;
using WebDauGiaInfrasData;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5); // OTP chỉ có hiệu lực trong 5 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHostedService<WebDauGiaUI.Services.AuctionWorker>(); // Đăng ký dịch vụ chạy ngầm kiểm tra đấu giá
// Đăng ký cơ chế xác thực bằng Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login"; // Đường dẫn khi chưa đăng nhập bị đuổi về đây
        options.AccessDeniedPath = "/User/AccessDenied"; // Đường dẫn khi không đủ quyền Admin
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Cookie sống trong 2 tiếng
    });

// Đăng ký DbContext vào đây anh nhé
// Đăng ký DbContext và chỉ định rõ nơi chứa Migrations
builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WebDauGiaInfrasData")
    ));
// Đăng ký Repository
builder.Services.AddScoped<WebDauGiaAppli.Interfaces.IUserRepository, WebDauGiaInfrasData.Repositories.UserRepository>();
builder.Services.AddScoped<WebDauGiaAppli.Interfaces.IProductRepository, WebDauGiaInfrasData.Repositories.ProductRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Thêm middleware xác thực trước Authorization
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<WebDauGiaUI.Hubs.AuctionHub>("/auctionHub");
app.Run();