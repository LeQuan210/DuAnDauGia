using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebDauGiaInfrasData;

namespace WebDauGia.Services // Nhớ đổi tên namespace cho khớp với dự án của anh nhé
{
    public class AuctionWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        // Vì BackgroundService chạy ngầm liên tục, nên mình phải dùng IServiceProvider 
        // để gọi Database ra mỗi khi cần, tránh bị lỗi chiếm dụng kết nối.
        public AuctionWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Vòng lặp tuần tra vô tận cho đến khi anh tắt web
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndCloseAuctions();
                }
                catch (Exception ex)
                {
                    // Lưu log lỗi nếu có trục trặc
                    Console.WriteLine($"Lỗi chạy ngầm: {ex.Message}");
                }

                // Đi tuần xong thì nghỉ 10 giây rồi mới đi tiếp cho đỡ nặng máy
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task CheckAndCloseAuctions()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // Gọi kho dữ liệu (DbContext) ra làm việc
                var _context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

                // Lấy ra những sản phẩm ĐÃ HẾT GIỜ nhưng CHƯA ĐÓNG PHIÊN (Giả sử mình thêm cột IsClosed)
                var expiredAuctions = _context.Products
                    .Where(p => p.AuctionEndTime <= DateTime.Now && !p. IsClosed)
                    .ToList();

                foreach (var auction in expiredAuctions)
                {
                    // 1. Khóa phiên đấu giá lại
                    auction.IsClosed = true;

                    // === CHỖ NÀY CHIỀU NAY MÌNH SẼ CODE LOGIC: ===
                    // 2. Tìm người đặt giá cao nhất (Winner)
                    // 3. Trừ tiền người thắng (Tạo hóa đơn, tính hoa hồng cho sàn)
                    // 4. Trả lại tiền cọc (Hoàn cọc) cho những người thua
                    // 5. Ghi vào SystemLogs

                    _context.Products.Update(auction);
                }

                if (expiredAuctions.Any())
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Đã chốt sổ {expiredAuctions.Count} phiên đấu giá.");
                }
            }
        }
    }
}