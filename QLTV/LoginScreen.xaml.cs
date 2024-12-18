using QLDB.Models;
using System;
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
using System.Windows.Shapes;

namespace QLDB
{
    /// <summary>
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        private readonly QLDBContext _context;
        public LoginScreen()
        {
            InitializeComponent();
            _context = new QLDBContext();
        }
        
        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = pbPassword.Password;
            // Xác thực người dùng
            var user = ValidateUser(username, password);

            if (user != null)
            {
                    AUInterface mw = new AUInterface();
                    mw.Show();
                    this.Close();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        private TAIKHOAN ValidateUser(string username, string password)
        {
            // Tìm tài khoản người dùng từ cơ sở dữ liệu
            TAIKHOAN user = null;

            user = _context.TAIKHOAN.FirstOrDefault(u => u.TenTaiKhoan == username && u.MatKhau == password);
            return user;
        }
    }
}
