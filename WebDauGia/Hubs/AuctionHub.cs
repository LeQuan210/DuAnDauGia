using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebDauGiaUI.Hubs // Anh nhớ kiểm tra lại namespace cho khớp với project của anh
{
    public class AuctionHub : Hub
    {
        // Mở cửa cho user vào phòng đấu giá của sản phẩm này
        public async Task JoinProductGroup(string productId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, productId);
        }

        // Mời user ra khỏi phòng khi họ rời trang
        public async Task LeaveProductGroup(string productId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, productId);
        }

        // Hàm phát loa thông báo giá (chắc anh đã có sẵn)
        public async Task SendBid(string productId, string user, decimal amount)
        {
            // Chỉ phát thông báo đến những ai đang ở TRONG NHÓM của sản phẩm này
            await Clients.Group(productId).SendAsync("ReceiveBid", user, amount);
        }
    }
}