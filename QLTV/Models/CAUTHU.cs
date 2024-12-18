using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLDB.Models
{
    public partial class CAUTHU
    {
        public int ID { get; set; }
        public string MaCauThu { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public DateTime NgaySinh { get; set; }
        public string QuocTich { get; set; } = null!;
        public string ViTriThiDau { get; set; } = null!;
        public int SoAo { get; set; }
        public int IDTinhTrangSucKhoe { get; set; }
        public bool IsDeleted { get; set; }

        public virtual TINHTRANGSUCKHOE IDTinhTrangSucKhoeNavigation { get; set; } = null!;
        public virtual ICollection<CHITIETDOIHINH> CHITIETDOIHINH { get; set; } = new List<CHITIETDOIHINH>();
    }
}