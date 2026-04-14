using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDauGiaDomain.Entities
{
    public class BidHistory
    {
        public int Id { get; set; }
        public int ProductId { get; set; } // Mã sản phẩm
        public int UserId { get; set; }    // Mã người đặt giá
        public decimal BidAmount { get; set; } // Số tiền đặt
        public DateTime BidTime { get; set; } // Thời gian đặt
    }
}
