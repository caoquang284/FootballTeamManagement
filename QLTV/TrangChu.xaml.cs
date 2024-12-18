using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QLDB.Models;

namespace QLDB
{
    /// <summary>
    /// Interaction logic for TrangChu.xaml
    /// </summary>
    public partial class TrangChu : UserControl
    {
        public int TongCauThu { get; set; }
        public int TongDoiHinh { get; set; }

        public TrangChu()
        {
            InitializeComponent();
            LoadDuLieu();
            this.DataContext = this;
        }

        private void LoadDuLieu()
        {
            using (var context = new QLDBContext())
            {
                TongCauThu = context.CAUTHU.Count();
                TongDoiHinh = context.DOIHINHTHIDAU.Where(dh => !dh.IsDeleted).Count();
            }
        }
    }
}