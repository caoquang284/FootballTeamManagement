using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLDB.Models
{
    public partial class TAIKHOAN
    {
        public int ID { get; set; }
        public string MaTaiKhoan { get; set; } = null!;
        public string TenTaiKhoan { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string Hoten { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}