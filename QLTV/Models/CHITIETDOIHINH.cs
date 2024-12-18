using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLDB.Models
{
    public partial class CHITIETDOIHINH
    {
        public int IDDoiHinh { get; set; }
        public int IDCauThu { get; set; }
        public bool IsDeleted { get; set; }

        public virtual CAUTHU IDCauThuNavigation { get; set; } = null!;
        public virtual DOIHINHTHIDAU IDDoiHinhNavigation { get; set; } = null!;
    }
}