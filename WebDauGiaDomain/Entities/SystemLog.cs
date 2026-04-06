using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDauGiaDomain.Entities
{
    public class SystemLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; } // Có thể rỗng nếu là khách chưa đăng nhập
        public string Action { get; set; } // Hành động người đó làm
        public DateTime Timestamp { get; set; } // Thời gian làm
        public string IPAddress { get; set; } // Địa chỉ IP
    }
}
