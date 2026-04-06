using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDauGiaDomain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal StartPrice { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; } // Để phân loại: 1-Điện tử, 2-Mô hình...
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime AuctionEndTime { get; set; }
    }
}
