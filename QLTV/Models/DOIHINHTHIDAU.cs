using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLDB.Models
{
    public partial class DOIHINHTHIDAU
    {
        public int ID { get; set; }
        public string MaDoiHinh { get; set; } = null!;
        public string TenDoiHinh { get; set; } = null!;
        public string? SoDoThiDau { get; set; }
        public string? ChienThuatThiDau { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<CHITIETDOIHINH> CHITIETDOIHINH { get; set; } = new List<CHITIETDOIHINH>();
    }
}