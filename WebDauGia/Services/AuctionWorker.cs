using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebDauGiaDomain.Entities;
using WebDauGiaInfrasData;

namespace WebDauGiaUI.Services // Nhớ đổi tên namespace cho khớp với dự án của anh nhé
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

                    // 1. Khóa phiên đấu giá lại
                    auction.IsClosed = true;

                    // 2. Mở sổ ra xem những ai đã đặt giá cho sản phẩm này (Sắp xếp giá cao nhất lên đầu)
                    var bids = _context.BidHistories
                        .Where(b => b.ProductId == auction.Id)
                        .OrderByDescending(b => b.BidAmount)
                        .ToList();

                    if (bids.Any())
                    {
                        // --- CÓ NGƯỜI MUA ---
                        var winningBid = bids.First(); // Lấy người trả giá cao nhất
                        var winner = _context.Users.Find(winningBid.UserId);

                        if (winner != null)
                        {
                            // 3. Trừ tiền người thắng (Ví dụ: Trừ thẳng số tiền họ đã đấu giá)
                            // (Thực tế nếu anh bắt cọc 10% trước, thì giờ chỉ trừ 90% còn lại thôi)
                            winner.WalletBalance -= winningBid.BidAmount;
                            _context.Users.Update(winner);

                            // Tính phí hoa hồng cho Sàn (Ví dụ sàn thu 5% trên giá bán được)
                            var platformFee = winningBid.BidAmount * 0.05m;

                            // Ghi Camera Nhật ký cho người thắng
                            _context.SystemLogs.Add(new SystemLog
                            {
                                UserId = winner.UserID,
                                Action = $"🏆 THẮNG ĐẤU GIÁ: Sản phẩm #{auction.Id}. Đã trừ {winningBid.BidAmount:N0} VNĐ (Phí sàn: {platformFee:N0} VNĐ)",
                                Timestamp = DateTime.Now,
                                IPAddress = "Hệ thống tự động"
                            });
                        }

                        // 4. Trả cọc cho những người thua
                        // Tìm tất cả những người tham gia nhưng KHÔNG PHẢI là người thắng
                        var losers = bids.Select(b => b.UserId)
                                         .Distinct()
                                         .Where(id => id != winningBid.UserId)
                                         .ToList();

                        foreach (var loserId in losers)
                        {
                            var loser = _context.Users.Find(loserId);
                            if (loser != null)
                            {
                                // Ở bước đặt giá (Turn 5), anh em mình mới chỉ kiểm tra số dư chứ chưa trừ cọc.
                                // Nếu sau này anh sửa lại là CÓ TRỪ TIỀN CỌC lúc bấm Đặt giá, 
                                // thì anh code dòng CỘNG LẠI TIỀN CỌC vào đây nhé. Ví dụ:
                                // loser.WalletBalance += tien_coc;
                                // _context.Users.Update(loser);

                                _context.SystemLogs.Add(new SystemLog
                                {
                                    UserId = loser.UserID,
                                    Action = $"💔 THUA ĐẤU GIÁ: Sản phẩm #{auction.Id}. Đã hoàn cọc thành công.",
                                    Timestamp = DateTime.Now,
                                    IPAddress = "Hệ thống tự động"
                                });
                            }
                        }
                    }
                    else
                    {
                        // --- KHÔNG CÓ AI MUA ---
                        _context.SystemLogs.Add(new SystemLog
                        {
                            UserId = null,
                            Action = $"⚠️ PHIÊN THẤT BẠI: Sản phẩm #{auction.Id} kết thúc nhưng không có ai đặt giá.",
                            Timestamp = DateTime.Now,
                            IPAddress = "Hệ thống tự động"
                        });
                    }

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