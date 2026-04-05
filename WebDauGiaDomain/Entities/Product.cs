using System;
using System.ComponentModel.DataAnnotations;

namespace WebDauGiaDomain.Entities
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        public int SellerID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        // Navigation property
        public User? Seller { get; set; }
    }
}
