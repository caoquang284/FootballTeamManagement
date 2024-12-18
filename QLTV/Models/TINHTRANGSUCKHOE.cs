using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLDB.Models
{
    public partial class TINHTRANGSUCKHOE
    {
        public int ID { get; set; }
        public string MaTinhTrang { get; set; } = null!;
        public string TenTinhTrang { get; set; } = null!;
        public int? KhaNangRaSan { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<CAUTHU> CAUTHU { get; set; } = new List<CAUTHU>();
    }
}