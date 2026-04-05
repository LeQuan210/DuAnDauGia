using System;
using System.ComponentModel.DataAnnotations;

namespace WebDauGiaDomain.Entities
{
    public class AuctionSession
    {
        [Key]
        public int SessionID { get; set; }
        public int ProductID { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public decimal StartingPrice { get; set; }
        public decimal StepPrice { get; set; }
        public decimal CurrentHighestPrice { get; set; }
        
        public string Status { get; set; } = "Pending"; // Pending, Active, Closed
        
        // Navigation property
        public Product? Product { get; set; }
    }
}
