using System;
using System.ComponentModel.DataAnnotations;

namespace WebDauGiaDomain.Entities
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Bidder"; // Bidder, Seller, Admin
        public int TrustScore { get; set; } = 100;
        public decimal WalletBalance { get; set; } = 0;
        public bool IsLocked { get; set; } = false;
    }
}
