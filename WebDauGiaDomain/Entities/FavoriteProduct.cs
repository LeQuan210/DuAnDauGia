using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDauGiaDomain.Entities
{
    public class FavoriteProduct
    {
        public int Id { get; set; }
        public int UserId { get; set; } // ID người dùng
        public int ProductId { get; set; } // ID sản phẩm
    }
}
