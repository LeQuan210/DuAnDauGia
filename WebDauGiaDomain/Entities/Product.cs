using System;
using System.ComponentModel.DataAnnotations;

namespace WebDauGiaDomain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal StartPrice { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; } // Để phân loại: 1-Điện tử, 2-Mô hình...
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
