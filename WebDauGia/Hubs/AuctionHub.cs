using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebDauGiaUI.Hubs
{
    // Kế thừa từ Hub của SignalR
    public class AuctionHub : Hub
    {
        // Hàm này sẽ nhận tín hiệu gửi lên từ trình duyệt của người dùng
        public async Task SendBid(string productId, string userName, int amount)
        {
            // Sau khi nhận, nó sẽ phát thanh ngay lập tức hàm "ReceiveBid" tới TẤT CẢ các máy tính đang kết nối
            await Clients.All.SendAsync("ReceiveBid", productId, userName, amount);
        }
    }
}