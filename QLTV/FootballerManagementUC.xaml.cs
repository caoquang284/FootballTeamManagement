using System;
using System.Collections.Generic;
using System.Linq;
using QLDB.Models;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Input;
using iTextSharp.text.pdf;
using iTextSharp.text;
using MaterialDesignThemes.Wpf;
using OfficeOpenXml;
using System.IO;
using System.Windows.Media;
using Microsoft.Win32;

namespace QLDB
{
    /// <summary>
    /// Interaction logic for FootballerManagementUC.xaml
    /// </summary>
    public partial class FootballerManagementUC : UserControl
    {
        private QLDBContext _context = new QLDBContext();
        public ObservableCollection<CAUTHU> CauThu { get; set; }
        public ObservableCollection<TINHTRANGSUCKHOE> TinhTrangSucKhoe { get; set; }
        public List<string> TenTinhTrangList { get; set; }

        public FootballerManagementUC()
        {
            InitializeComponent();
            _context = new QLDBContext();
            CauThu = new ObservableCollection<CAUTHU>();
            TinhTrangSucKhoe = new ObservableCollection<TINHTRANGSUCKHOE>();

            CauThuDataGrid.ItemsSource = CauThu;
            LoadCauThuData();

            TinhTrangSucKhoeDataGrid.ItemsSource = TinhTrangSucKhoe;
            LoadTinhTrangSucKhoeData();

            TenTinhTrangList = _context.TINHTRANGSUCKHOE.Where(ttsk => !ttsk.IsDeleted).Select(ttsk => ttsk.TenTinhTrang).ToList();
            TinhTrangSucKhoeComboBox.ItemsSource = TenTinhTrangList;
        }

        private void OpenExportMenu_Click(object sender, RoutedEventArgs e)
        {
            // Tham chiếu đến nút
            Button button = sender as Button;

            // Mở ContextMenu
            if (button.ContextMenu != null)
            {
                button.ContextMenu.IsOpen = true;
            }
        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                // Kiểm tra xem tab đầu tiên có được chọn không
                if (((TabControl)sender).SelectedIndex == 0)
                {
                    // Cập nhật lại danh sách TenTinhTrangList
                    TenTinhTrangList = _context.TINHTRANGSUCKHOE
                                                .Where(ttsk => !ttsk.IsDeleted)
                                                .Select(ttsk => ttsk.TenTinhTrang)
                                                .ToList();
                    TinhTrangSucKhoeComboBox.ItemsSource = TenTinhTrangList;
                }
            }
        }

        // Readers 
        private void LoadCauThuData()
        {
            CauThu.Clear();
            var cauthu = _context.CAUTHU.Include(ct => ct.IDTinhTrangSucKhoeNavigation).ToList();
            foreach (var ct in cauthu)
            {
                CauThu.Add(ct);
            }
        }

        private void ThemCauThu_Click(object sender, RoutedEventArgs e)
        {
            // Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(TenCauThuTextBox.Text))
            {
                icTenCauThuError.ToolTip = "Họ tên cầu thủ không được để trống";
                icTenCauThuError.Visibility = Visibility.Visible;
            }
            if (TinhTrangSucKhoeComboBox.SelectedItem == null)
            {
                icTinhTrangSucKhoeError.ToolTip = "Phải chọn tình trạng sức khỏe";
                icTinhTrangSucKhoeError.Visibility = Visibility.Visible;
            }
            if (string.IsNullOrWhiteSpace(QuocTichTextBox.Text))
            {
                icQuocTichError.ToolTip = "Quốc tịch không được để trống";
                icQuocTichError.Visibility = Visibility.Visible;
            }
            if (string.IsNullOrWhiteSpace(ViTriThiDauComboBox.Text))
            {
                icViTriThiDauError.ToolTip = "Vị trí thi đấu không được để trống";
                icViTriThiDauError.Visibility = Visibility.Visible;
            }
            if (string.IsNullOrWhiteSpace(SoAoTextBox.Text))
            {
                icSoAoError.ToolTip = "Số áo không được để trống";
                icSoAoError.Visibility = Visibility.Visible;
            }

            // Kiểm tra trùng lặp số áo
            if (int.TryParse(SoAoTextBox.Text, out int soAo) && _context.CAUTHU.Any(ct => ct.SoAo == soAo))
            {
                icSoAoError.ToolTip = "Số áo đã tồn tại!";
                icSoAoError.Visibility = Visibility.Visible;
            }

            // Kiểm tra còn lỗi không
            if (HasError())
            {
                MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Lấy tên tình trạng sức khỏe từ ComboBox
                string tenTinhTrang = TinhTrangSucKhoeComboBox.SelectedItem as string;

                // Tìm ID tương ứng trong database
                var tinhTrangSucKhoe = _context.TINHTRANGSUCKHOE.FirstOrDefault(ttsk => ttsk.TenTinhTrang == tenTinhTrang);

                if (tinhTrangSucKhoe == null)
                {
                    MessageBox.Show("Tình trạng sức khỏe không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Tạo cầu thủ mới
                var newCauThu = new CAUTHU
                {
                    HoTen = TenCauThuTextBox.Text,
                    IDTinhTrangSucKhoe = tinhTrangSucKhoe.ID,
                    NgaySinh = dpNgaySinh.SelectedDate ?? DateTime.Now,
                    QuocTich = QuocTichTextBox.Text,
                    ViTriThiDau = ViTriThiDauComboBox.Text,
                    SoAo = int.Parse(SoAoTextBox.Text)
                };

                _context.CAUTHU.Add(newCauThu);
                _context.SaveChanges();

                LoadCauThuData();
                ClearInputs();
                RefreshCauThuTab();
                CauThuDataGrid.UnselectAll();
                TTCTSearchTextBox.Text = "";
                TTCTSearchCriteriaComboBox.SelectedItem = null;

                MessageBox.Show("Thêm cầu thủ thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Lỗi khi thêm cầu thủ vào cơ sở dữ liệu: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CapNhatCauThu_Click(object sender, RoutedEventArgs e)
        {
            if (CauThuDataGrid.SelectedItem is CAUTHU selectedCauThu)
            {
                try
                {
                    // Validate dữ liệu đầu vào
                    if (string.IsNullOrWhiteSpace(TenCauThuTextBox.Text))
                    {
                        icTenCauThuError.ToolTip = "Họ tên cầu thủ không được để trống";
                        icTenCauThuError.Visibility = Visibility.Visible;
                    }
                    if (TinhTrangSucKhoeComboBox.SelectedItem == null)
                    {
                        icTinhTrangSucKhoeError.ToolTip = "Phải chọn tình trạng sức khỏe";
                        icTinhTrangSucKhoeError.Visibility = Visibility.Visible;
                    }
                    if (string.IsNullOrWhiteSpace(QuocTichTextBox.Text))
                    {
                        icQuocTichError.ToolTip = "Quốc tịch không được để trống";
                        icQuocTichError.Visibility = Visibility.Visible;
                    }
                    if (string.IsNullOrWhiteSpace(ViTriThiDauComboBox.Text))
                    {
                        icViTriThiDauError.ToolTip = "Vị trí thi đấu";
                        icViTriThiDauError.Visibility = Visibility.Visible;
                    }
                    if (string.IsNullOrWhiteSpace(SoAoTextBox.Text))
                    {
                        icSoAoError.ToolTip = "Số áo không được để trống";
                        icSoAoError.Visibility = Visibility.Visible;
                    }

                    // Kiểm tra trùng lặp số áo với các cầu thủ khác
                    if (int.TryParse(SoAoTextBox.Text, out int soAo) && _context.CAUTHU.Any(ct => ct.SoAo == soAo && ct.ID != selectedCauThu.ID))
                    {
                        icSoAoError.ToolTip = "Số áo đã tồn tại!";
                        icSoAoError.Visibility = Visibility.Visible;
                    }

                    // Kiểm tra còn lỗi không
                    if (HasError())
                    {
                        MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi cập nhật!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Lấy tên tình trạng sức khỏe từ ComboBox
                    string tenTinhTrang = TinhTrangSucKhoeComboBox.SelectedItem as string;

                    // Tìm ID tương ứng trong database
                    var tinhTrangSucKhoe = _context.TINHTRANGSUCKHOE.FirstOrDefault(ttsk => ttsk.TenTinhTrang == tenTinhTrang);

                    if (tinhTrangSucKhoe == null)
                    {
                        MessageBox.Show("Tình trạng sức khỏe không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    selectedCauThu.HoTen = TenCauThuTextBox.Text;
                    selectedCauThu.IDTinhTrangSucKhoe = tinhTrangSucKhoe.ID;
                    selectedCauThu.NgaySinh = dpNgaySinh.SelectedDate ?? selectedCauThu.NgaySinh;
                    selectedCauThu.QuocTich = QuocTichTextBox.Text;
                    selectedCauThu.ViTriThiDau = ViTriThiDauComboBox.Text;
                    selectedCauThu.SoAo = int.Parse(SoAoTextBox.Text);

                    _context.SaveChanges();
                    ClearInputs();
                    RefreshCauThuTab();
                    CauThuDataGrid.UnselectAll();
                    TTCTSearchTextBox.Text = "";
                    TTCTSearchCriteriaComboBox.SelectedItem = null;

                    // Cập nhật UI
                    var index = CauThu.IndexOf(selectedCauThu);
                    CauThu[index] = selectedCauThu;
                    CauThuDataGrid.Items.Refresh();

                    MessageBox.Show("Cập nhật thông tin cầu thủ thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (DbUpdateException ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật cầu thủ trong cơ sở dữ liệu: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một cầu thủ để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        //Validate cho Update  
        private void TenCauThuTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TenCauThuTextBox.Text))
            {
                icTenCauThuError.ToolTip = "Họ tên cầu thủ không được để trống";
                icTenCauThuError.Visibility = Visibility.Visible;
            }
            else
            {
                icTenCauThuError.Visibility = Visibility.Collapsed;
            }
        }

        private void TinhTrangSucKhoeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TinhTrangSucKhoeComboBox.SelectedItem != null)
            {
                icTinhTrangSucKhoeError.Visibility = Visibility.Collapsed;
            }
        }

        private void dpNgaySinh_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = (dpNgaySinh.Template.FindName("PART_TextBox", dpNgaySinh) as TextBox);
            if (textBox != null)
            {
                textBox.TextChanged += NgaySinh_TextChanged;
            }
        }

        private void NgaySinh_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy" };
            if (!DateTime.TryParseExact(textBox.Text, formats, null, DateTimeStyles.AllowWhiteSpaces, out DateTime ngaySinh))
            {
                icNgaySinhError.ToolTip = "Ngày sinh không hợp lệ (định dạng đúng: dd/MM/yyyy)";
                icNgaySinhError.Visibility = Visibility.Visible;
                return;
            }

            icNgaySinhError.Visibility = Visibility.Collapsed;
        }

        private void QuocTichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuocTichTextBox.Text))
            {
                icQuocTichError.ToolTip = "Quốc tịch không được để trống";
                icQuocTichError.Visibility = Visibility.Visible;
            }
            else
            {
                icQuocTichError.Visibility = Visibility.Collapsed;
            }
        }

        private void ViTriThiDauComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViTriThiDauComboBox.SelectedItem == null)
            {
                icViTriThiDauError.ToolTip = "Vị trí thi đấu không được để trống";
                icViTriThiDauError.Visibility = Visibility.Visible;
            }
            else
            {
                icViTriThiDauError.Visibility = Visibility.Collapsed;
            }
        }

        private void SoAoTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập số  
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SoAoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SoAoTextBox.Text))
            {
                icSoAoError.ToolTip = "Số áo không được để trống";
                icSoAoError.Visibility = Visibility.Visible;
            }
            else if (!int.TryParse(SoAoTextBox.Text, out int soAo) || soAo <= 0)
            {
                icSoAoError.ToolTip = "Số áo phải là số nguyên dương";
                icSoAoError.Visibility = Visibility.Visible;
            }
            else
            {
                icSoAoError.Visibility = Visibility.Collapsed;
            }
        }

        private void XoaCauThu_Click(object sender, RoutedEventArgs e)
        {
            if (CauThuDataGrid.SelectedItem is CAUTHU selectedCauThu)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa cầu thủ này?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Xóa cầu thủ khỏi CSDL  
                        var cauThuToDelete = _context.CAUTHU.Find(selectedCauThu.ID);
                        if (cauThuToDelete != null)
                        {
                            _context.CAUTHU.Remove(cauThuToDelete);
                            _context.SaveChanges();

                            CauThu.Remove(selectedCauThu);
                            ClearInputs();
                            RefreshCauThuTab();
                            CauThuDataGrid.UnselectAll();
                            TTCTSearchTextBox.Text = "";
                            TTCTSearchCriteriaComboBox.SelectedItem = null;

                            MessageBox.Show("Xóa cầu thủ thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy cầu thủ để xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa cầu thủ: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một cầu thủ để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private CAUTHU GetCauThuFromInputs()
        {
            try
            {
                // Lấy tên tình trạng sức khỏe từ ComboBox  
                string tenTinhTrang = TinhTrangSucKhoeComboBox.Text;

                // Tìm ID tương ứng trong database  
                var tinhTrangSucKhoe = _context.TINHTRANGSUCKHOE.FirstOrDefault(ttsk => ttsk.TenTinhTrang == tenTinhTrang);

                if (tinhTrangSucKhoe == null)
                {
                    MessageBox.Show("Tình trạng sức khỏe không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                // Kiểm tra số áo  
                if (!int.TryParse(SoAoTextBox.Text, out int soAo))
                {
                    MessageBox.Show("Số áo không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                return new CAUTHU
                {
                    HoTen = TenCauThuTextBox.Text,
                    IDTinhTrangSucKhoe = tinhTrangSucKhoe.ID,
                    NgaySinh = dpNgaySinh.SelectedDate ?? DateTime.Now,
                    QuocTich = QuocTichTextBox.Text,
                    ViTriThiDau = ViTriThiDauComboBox.Text,
                    SoAo = soAo
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private string NormalizeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return new string(
                text.Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            ).Normalize(NormalizationForm.FormC).ToLower();
        }

        private void TTCTSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = NormalizeString(TTCTSearchTextBox.Text.Trim());
            string searchCriteria = (TTCTSearchCriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var query = _context.CAUTHU
                .Include(ct => ct.IDTinhTrangSucKhoeNavigation)
                .AsEnumerable() // Chuyển về IEnumerable để lọc trên máy khách  
                .ToList();

            // Kiểm tra xem có tiêu chí tìm kiếm được chọn hay không  
            if (string.IsNullOrEmpty(searchCriteria))
            {
                // Nếu không có tiêu chí, tìm kiếm trên tất cả các trường  
                query = query.Where(ct =>
                    NormalizeString(ct.MaCauThu).Contains(searchTerm) ||
                    NormalizeString(ct.HoTen).Contains(searchTerm) ||
                    NormalizeString(ct.IDTinhTrangSucKhoeNavigation.TenTinhTrang).Contains(searchTerm) ||
                    NormalizeString(ct.QuocTich).Contains(searchTerm) ||
                    NormalizeString(ct.SoAo.ToString()).Contains(searchTerm)
                ).ToList();
            }
            else
            {
                // Nếu có tiêu chí, lọc theo tiêu chí được chọn  
                switch (searchCriteria)
                {
                    case "Mã Cầu Thủ":
                        query = query.Where(ct => NormalizeString(ct.MaCauThu).Contains(searchTerm)).ToList();
                        break;
                    case "Tên Cầu Thủ":
                        query = query.Where(ct => NormalizeString(ct.HoTen).Contains(searchTerm)).ToList();
                        break;
                    case "Tình Trạng Sức Khỏe":
                        query = query.Where(ct => NormalizeString(ct.IDTinhTrangSucKhoeNavigation.TenTinhTrang).Contains(searchTerm)).ToList();
                        break;
                    case "Quốc Tịch":
                        query = query.Where(ct => NormalizeString(ct.QuocTich).Contains(searchTerm)).ToList();
                        break;
                    case "Số Áo":
                        query = query.Where(ct => NormalizeString(ct.SoAo.ToString()).Contains(searchTerm)).ToList();
                        break;
                }
            }

            // Cập nhật ItemsSource cho DataGrid  
            CauThu.Clear();
            foreach (var cauThu in query)
            {
                CauThu.Add(cauThu);
            }
        }

        private void ClearInputs()
        {
            TenCauThuTextBox.Text = string.Empty;
            TinhTrangSucKhoeComboBox.SelectedItem = null;
            dpNgaySinh.SelectedDate = null;
            QuocTichTextBox.Text = string.Empty;
            ViTriThiDauComboBox.Text = string.Empty;
            SoAoTextBox.Text = string.Empty;

            // Ẩn tất cả các icon lỗi  
            icTenCauThuError.Visibility = Visibility.Collapsed;
            icTinhTrangSucKhoeError.Visibility = Visibility.Collapsed;
            icNgaySinhError.Visibility = Visibility.Collapsed;
            icQuocTichError.Visibility = Visibility.Collapsed;
            icViTriThiDauError.Visibility = Visibility.Collapsed;
            icSoAoError.Visibility = Visibility.Collapsed;
        }

        private void CauThuDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CauThuDataGrid.SelectedItem is CAUTHU selectedCauThu)
            {
                TenCauThuTextBox.Text = selectedCauThu.HoTen;
                TinhTrangSucKhoeComboBox.SelectedItem = selectedCauThu.IDTinhTrangSucKhoeNavigation.TenTinhTrang;
                dpNgaySinh.SelectedDate = selectedCauThu.NgaySinh;
                QuocTichTextBox.Text = selectedCauThu.QuocTich;
                ViTriThiDauComboBox.Text = selectedCauThu.ViTriThiDau;
                SoAoTextBox.Text = selectedCauThu.SoAo.ToString();
            }
        }
        // Reader Types  
        private void LoadTinhTrangSucKhoeData()
        {
            var tinhTrang = _context.TINHTRANGSUCKHOE.Where(ttsk => !ttsk.IsDeleted).ToList();
            TinhTrangSucKhoeDataGrid.ItemsSource = tinhTrang;
            TenTinhTrangList = _context.TINHTRANGSUCKHOE.Where(ttsk => !ttsk.IsDeleted)
                                                        .Select(ttsk => ttsk.TenTinhTrang)
                                                        .ToList();
            TinhTrangSucKhoeComboBox.ItemsSource = TenTinhTrangList;
        }

        private void ThemTinhTrang_Click(object sender, RoutedEventArgs e)
        {
            // Validate dữ liệu đầu vào  
            if (string.IsNullOrWhiteSpace(TenTinhTrangTextBox.Text))
            {
                icTenTinhTrangError.ToolTip = "Tình trạng sức khỏe không được để trống";
                icTenTinhTrangError.Visibility = Visibility.Visible;
            }
            if (string.IsNullOrWhiteSpace(KhaNangRaSanTextBox.Text))
            {
                icKhaNangRaSanError.ToolTip = "Khả năng ra sân không được để trống";
                icKhaNangRaSanError.Visibility = Visibility.Visible;
            }

            // Kiểm tra trùng lặp tên tình trạng sức khỏe  
            string tenTinhTrang = TenTinhTrangTextBox.Text;
            if (_context.TINHTRANGSUCKHOE.Any(ttsk => ttsk.TenTinhTrang == tenTinhTrang && !ttsk.IsDeleted))
            {
                icTenTinhTrangError.ToolTip = "Tình trạng sức khỏe đã tồn tại!";
                icTenTinhTrangError.Visibility = Visibility.Visible;
            }

            // Kiểm tra còn lỗi không  
            if (HasError())
            {
                MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Tạo tình trạng sức khỏe mới  
                var newTinhTrang = new TINHTRANGSUCKHOE
                {
                    TenTinhTrang = TenTinhTrangTextBox.Text,
                    KhaNangRaSan = int.TryParse(KhaNangRaSanTextBox.Text, out int khaNang) ? khaNang : null
                };

                _context.TINHTRANGSUCKHOE.Add(newTinhTrang);
                _context.SaveChanges();

                LoadTinhTrangSucKhoeData();
                ClearReaderTypeInputs();
                RefreshTinhTrangSucKhoeTab();
                TinhTrangSucKhoeDataGrid.UnselectAll();
                MessageBox.Show("Thêm tình trạng sức khỏe thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm tình trạng sức khỏe: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CapNhatTinhTrang_Click(object sender, RoutedEventArgs e)
        {
            if (TinhTrangSucKhoeDataGrid.SelectedItem is TINHTRANGSUCKHOE selectedTinhTrang)
            {
                try
                {
                    // Validate dữ liệu đầu vào  
                    if (string.IsNullOrWhiteSpace(TenTinhTrangTextBox.Text))
                    {
                        icTenTinhTrangError.ToolTip = "Tình trạng sức khỏe không được để trống";
                        icTenTinhTrangError.Visibility = Visibility.Visible;
                    }
                    if (string.IsNullOrWhiteSpace(KhaNangRaSanTextBox.Text))
                    {
                        icKhaNangRaSanError.ToolTip = "Khả năng ra sân không được để trống";
                        icKhaNangRaSanError.Visibility = Visibility.Visible;
                    }

                    // Kiểm tra trùng lặp tên tình trạng sức khỏe với các loại khác  
                    string tenTinhTrang = TenTinhTrangTextBox.Text;
                    if (_context.TINHTRANGSUCKHOE.Any(ttsk => ttsk.TenTinhTrang == tenTinhTrang && ttsk.ID != selectedTinhTrang.ID && !ttsk.IsDeleted))
                    {
                        icTenTinhTrangError.ToolTip = "Tình trạng sức khỏe đã tồn tại!";
                        icTenTinhTrangError.Visibility = Visibility.Visible;
                    }

                    // Kiểm tra còn lỗi không  
                    if (HasError())
                    {
                        MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi cập nhật!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    selectedTinhTrang.TenTinhTrang = TenTinhTrangTextBox.Text;
                    selectedTinhTrang.KhaNangRaSan = int.TryParse(KhaNangRaSanTextBox.Text, out int khaNang) ? khaNang : null;

                    _context.SaveChanges();

                    LoadTinhTrangSucKhoeData();
                    ClearReaderTypeInputs();
                    RefreshTinhTrangSucKhoeTab();
                    TinhTrangSucKhoeDataGrid.UnselectAll();
                    LoadCauThuData();
                    CauThuDataGrid.Items.Refresh();

                    MessageBox.Show("Cập nhật tình trạng sức khỏe thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật tình trạng sức khỏe: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một tình trạng sức khỏe để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Xử lý sự kiện cho TenTinhTrangTextBox  
        private void TenTinhTrangTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TenTinhTrangTextBox.Text))
            {
                icTenTinhTrangError.ToolTip = "Tình trạng sức khỏe không được để trống";
                icTenTinhTrangError.Visibility = Visibility.Visible;
            }
            else
            {
                icTenTinhTrangError.Visibility = Visibility.Collapsed;
            }
        }

        // Xử lý sự kiện cho KhaNangRaSanTextBox  
        private void KhaNangRaSanTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(KhaNangRaSanTextBox.Text))
            {
                icKhaNangRaSanError.ToolTip = "Khả năng ra sân không được để trống";
                icKhaNangRaSanError.Visibility = Visibility.Visible;
            }
            else if (!int.TryParse(KhaNangRaSanTextBox.Text, out int khaNang) || khaNang < 0 || khaNang > 100)
            {
                icKhaNangRaSanError.ToolTip = "Khả năng ra sân phải là số nguyên từ 0 đến 100";
                icKhaNangRaSanError.Visibility = Visibility.Visible;
            }
            else
            {
                icKhaNangRaSanError.Visibility = Visibility.Collapsed;
            }
        }

        // Chỉ cho phép nhập số vào KhaNangRaSanTextBox  
        private void KhaNangRaSanTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void XoaTinhTrang_Click(object sender, RoutedEventArgs e)
        {
            if (TinhTrangSucKhoeDataGrid.SelectedItem is TINHTRANGSUCKHOE selectedTinhTrang)
            {
                // Kiểm tra xem tình trạng sức khỏe có đang được sử dụng bởi cầu thủ nào không  
                var existingCauThu = _context.CAUTHU.Any(ct => ct.IDTinhTrangSucKhoe == selectedTinhTrang.ID);
                if (existingCauThu)
                {
                    MessageBox.Show("Không thể xóa tình trạng sức khỏe này vì đang được sử dụng bởi ít nhất một cầu thủ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa tình trạng sức khỏe này?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Soft delete tình trạng sức khỏe  
                        var tinhTrangToDelete = _context.TINHTRANGSUCKHOE.Find(selectedTinhTrang.ID);
                        if (tinhTrangToDelete != null)
                        {
                            tinhTrangToDelete.IsDeleted = true;
                            _context.SaveChanges();

                            LoadTinhTrangSucKhoeData();
                            ClearReaderTypeInputs();
                            RefreshTinhTrangSucKhoeTab();
                            TinhTrangSucKhoeDataGrid.UnselectAll();
                            MessageBox.Show("Xóa tình trạng sức khỏe thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa tình trạng sức khỏe: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một tình trạng sức khỏe để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TinhTrangSucKhoeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TinhTrangSucKhoeDataGrid.SelectedItem is TINHTRANGSUCKHOE selectedTinhTrang)
            {
                TenTinhTrangTextBox.Text = selectedTinhTrang.TenTinhTrang;
                KhaNangRaSanTextBox.Text = selectedTinhTrang.KhaNangRaSan.ToString();
            }
        }
        private void ClearReaderTypeInputs()
        {
            TenTinhTrangTextBox.Text = string.Empty;
            KhaNangRaSanTextBox.Text = string.Empty;

            icTenTinhTrangError.Visibility = Visibility.Collapsed;
            icKhaNangRaSanError.Visibility = Visibility.Collapsed;
        }

        private void TTSKSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = NormalizeString(TTSKSearchTextBox.Text.Trim());
            string searchCriteria = (TTSKSearchCriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var query = _context.TINHTRANGSUCKHOE
                                .Where(ttsk => !ttsk.IsDeleted) // Thêm điều kiện lọc  
                                .AsEnumerable()
                                .ToList();

            if (string.IsNullOrEmpty(searchCriteria))
            {
                query = query.Where(ttsk =>
                    NormalizeString(ttsk.TenTinhTrang).Contains(searchTerm) ||
                    (ttsk.KhaNangRaSan != null && NormalizeString(ttsk.KhaNangRaSan.ToString()).Contains(searchTerm)) // Kiểm tra null  
                ).ToList();
            }
            else
            {
                switch (searchCriteria)
                {
                    case "Tên Tình Trạng":
                        query = query.Where(ttsk => NormalizeString(ttsk.TenTinhTrang).Contains(searchTerm)).ToList();
                        break;
                    case "Khả Năng Ra Sân":
                        query = query.Where(ttsk => ttsk.KhaNangRaSan != null && NormalizeString(ttsk.KhaNangRaSan.ToString()).Contains(searchTerm)).ToList();
                        break;
                }
            }

            TinhTrangSucKhoe.Clear();
            foreach (var tinhTrang in query)
            {
                TinhTrangSucKhoe.Add(tinhTrang);
            }
        }

        private void LamMoiCauThu_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
            RefreshCauThuTab();
            CauThuDataGrid.UnselectAll();
            TTCTSearchTextBox.Text = "";
            TTCTSearchCriteriaComboBox.SelectedItem = null;
            LoadCauThuData();
            MessageBox.Show("Làm mới dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LamMoiTinhTrang_Click(object sender, RoutedEventArgs e)
        {
            ClearReaderTypeInputs();
            RefreshTinhTrangSucKhoeTab();
            TinhTrangSucKhoeDataGrid.UnselectAll();
            TTSKSearchTextBox.Text = "";
            TTSKSearchCriteriaComboBox.SelectedItem = null;
            LoadTinhTrangSucKhoeData();
            MessageBox.Show("Làm mới dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshCauThuTab()
        {
            CauThu.Clear();
            LoadCauThuData();
            CauThuDataGrid.Items.Refresh();

            // Reset các trường nhập liệu  
            TenCauThuTextBox.Text = string.Empty;
            TinhTrangSucKhoeComboBox.SelectedItem = null;
            dpNgaySinh.SelectedDate = null;
            QuocTichTextBox.Text = string.Empty;
            ViTriThiDauComboBox.Text = string.Empty;
            SoAoTextBox.Text = string.Empty;
        }

        private void RefreshTinhTrangSucKhoeTab()
        {
            TinhTrangSucKhoe.Clear();
            LoadTinhTrangSucKhoeData();
            TinhTrangSucKhoeDataGrid.Items.Refresh();

            // Reset các trường nhập liệu  
            TenTinhTrangTextBox.Text = string.Empty;
            KhaNangRaSanTextBox.Text = string.Empty;
        }
        // Import and Export
        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files|*.xlsx";

                if (openFileDialog.ShowDialog() == true)
                {
                    var filePath = openFileDialog.FileName;

                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        int soDongThanhCong = 0;
                        int soDongBiLoi = 0;
                        List<string> danhSachLoi = new List<string>();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            List<string> loiTrongDong = new List<string>(); // Danh sách lỗi của mỗi dòng

                            try
                            {
                                // Lấy dữ liệu từ các cột tương ứng
                                string hoTen = worksheet.Cells[row, 1].Value?.ToString();
                                string tenTinhTrang = worksheet.Cells[row, 2].Value?.ToString();

                                DateTime ngaySinh;
                                if (!DateTime.TryParseExact(worksheet.Cells[row, 3].Value?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ngaySinh))
                                {
                                    loiTrongDong.Add($"Ngày sinh không hợp lệ hoặc không đúng định dạng 'dd/MM/yyyy'.");
                                }

                                string quocTich = worksheet.Cells[row, 4].Value?.ToString();
                                string viTriThiDau = worksheet.Cells[row, 5].Value?.ToString();
                                int soAo;
                                if (!int.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out soAo))
                                {
                                    loiTrongDong.Add($"Số áo không hợp lệ.");
                                }

                                var soAoExists = _context.CAUTHU.Any(ct => ct.SoAo == soAo);
                                if (soAoExists)
                                {
                                    loiTrongDong.Add($"Số áo đã tồn tại: {soAo}.");
                                }

                                // Kiểm tra sự tồn tại của Tình trạng sức khỏe
                                var tinhTrangSucKhoe = _context.TINHTRANGSUCKHOE.FirstOrDefault(ttsk => ttsk.TenTinhTrang == tenTinhTrang);
                                if (tinhTrangSucKhoe == null)
                                {
                                    loiTrongDong.Add($"Tình trạng sức khỏe không tồn tại trong cơ sở dữ liệu: {tenTinhTrang}.");
                                }

                                var cauThuExists = _context.CAUTHU.Any(ct => ct.HoTen == hoTen && ct.NgaySinh == ngaySinh && ct.QuocTich == quocTich);
                                if (cauThuExists)
                                {
                                    loiTrongDong.Add($"Cầu thủ đã tồn tại: {hoTen} ({ngaySinh:dd/MM/yyyy}) - {quocTich}.");
                                }

                                // Nếu có lỗi trong dòng, ghi nhận và chuyển sang dòng tiếp theo
                                if (loiTrongDong.Count > 0)
                                {
                                    soDongBiLoi++;
                                    danhSachLoi.Add($"Dòng {row}: {string.Join(", ", loiTrongDong)}");
                                    continue; // Chuyển sang dòng tiếp theo
                                }

                                // Tạo cầu thủ mới nếu không có lỗi nào
                                var newCauThu = new CAUTHU
                                {
                                    HoTen = hoTen,
                                    IDTinhTrangSucKhoe = tinhTrangSucKhoe.ID,
                                    NgaySinh = ngaySinh,
                                    QuocTich = quocTich,
                                    ViTriThiDau = viTriThiDau,
                                    SoAo = soAo
                                };

                                _context.CAUTHU.Add(newCauThu);
                                soDongThanhCong++;
                            }
                            catch (Exception ex)
                            {
                                soDongBiLoi++;
                                danhSachLoi.Add($"Dòng {row}: {ex.Message}");
                            }
                        }

                        _context.SaveChanges();

                        string ketQua = $"Nhập dữ liệu từ file Excel hoàn tất!\n" +
                                        $"Số dòng thêm thành công: {soDongThanhCong}\n" +
                                        $"Số dòng bị lỗi: {soDongBiLoi}";

                        if (soDongBiLoi > 0)
                        {
                            string fileLog = "LogLoiImportCauThu.txt";
                            File.WriteAllLines(fileLog, danhSachLoi);
                            ketQua += $"\nChi tiết lỗi được ghi tại: {fileLog}";
                            System.Diagnostics.Process.Start("notepad.exe", fileLog);
                        }

                        MessageBox.Show(ketQua, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadCauThuData(); // Cập nhật lại DataGrid
                    }
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "Không có chi tiết bổ sung.";
                MessageBox.Show($"Đã xảy ra lỗi khi nhập dữ liệu từ file Excel: {ex.Message}\nChi tiết: {innerMessage}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel Files|*.xlsx";

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;

                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Danh sách cầu thủ");

                        // Tạo tiêu đề cột 
                        worksheet.Cells[1, 1].Value = "Mã cầu thủ";
                        worksheet.Cells[1, 2].Value = "Họ tên";
                        worksheet.Cells[1, 3].Value = "Tình trạng sức khỏe";
                        worksheet.Cells[1, 4].Value = "Ngày sinh";
                        worksheet.Cells[1, 5].Value = "Quốc tịch";
                        worksheet.Cells[1, 6].Value = "Vị trí thi đấu";
                        worksheet.Cells[1, 7].Value = "Số áo";

                        // Điền dữ liệu 
                        int row = 2;
                        foreach (var cauThu in CauThu)
                        {
                            worksheet.Cells[row, 1].Value = cauThu.MaCauThu;
                            worksheet.Cells[row, 2].Value = cauThu.HoTen;
                            worksheet.Cells[row, 3].Value = cauThu.IDTinhTrangSucKhoeNavigation?.TenTinhTrang;
                            worksheet.Cells[row, 4].Value = cauThu.NgaySinh.ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 5].Value = cauThu.QuocTich;
                            worksheet.Cells[row, 6].Value = cauThu.ViTriThiDau;
                            worksheet.Cells[row, 7].Value = cauThu.SoAo;
                            row++;
                        }

                        // Lưu 
                        var fileInfo = new FileInfo(filePath);
                        package.SaveAs(fileInfo);
                        MessageBox.Show("Xuất dữ liệu ra Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    FileName = "DanhSachCauThu.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        Document document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                        PdfWriter writer = PdfWriter.GetInstance(document, fs);
                        document.Open();

                        // Font 
                        BaseFont baseFont = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        Font titleFont = new Font(baseFont, 16, Font.BOLD);
                        Font headerFont = new Font(baseFont, 10, Font.BOLD);
                        Font dataFont = new Font(baseFont, 10, Font.NORMAL);

                        // Tiêu đề
                        iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("DANH SÁCH CẦU THỦ", titleFont)
                        {
                            Alignment = Element.ALIGN_CENTER
                        };
                        document.Add(title);
                        document.Add(new iTextSharp.text.Paragraph(" ")); // Khoảng trắng

                        // Tạo bảng với 7 cột
                        PdfPTable table = new PdfPTable(7)
                        {
                            WidthPercentage = 100,
                            SpacingBefore = 10f,
                            SpacingAfter = 10f
                        };
                        table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 1.5f }); // Điều chỉnh độ rộng cột

                        // Tiêu đề cột
                        string[] headers = {
                    "Mã cầu thủ",
                    "Họ tên",
                    "Tình trạng sức khỏe",
                    "Ngày sinh",
                    "Quốc tịch",
                    "Vị trí thi đấu",
                    "Số áo"
                };

                        foreach (string header in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                Padding = 5f
                            };
                            table.AddCell(cell);
                        }

                        // Dữ liệu
                        var cauThu = _context.CAUTHU
                            .Include(ct => ct.IDTinhTrangSucKhoeNavigation)
                            .ToList();

                        foreach (var ct in cauThu)
                        {
                            table.AddCell(new Phrase(ct.MaCauThu.ToString(), dataFont));
                            table.AddCell(new Phrase(ct.HoTen ?? "", dataFont));
                            table.AddCell(new Phrase(ct.IDTinhTrangSucKhoeNavigation?.TenTinhTrang ?? "", dataFont));
                            table.AddCell(new Phrase(ct.NgaySinh.ToString("dd/MM/yyyy"), dataFont));
                            table.AddCell(new Phrase(ct.QuocTich ?? "", dataFont));
                            table.AddCell(new Phrase(ct.ViTriThiDau ?? "", dataFont));
                            table.AddCell(new Phrase(ct.SoAo.ToString(), dataFont));
                        }

                        document.Add(table);
                        document.Close();

                        MessageBox.Show("Xuất PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm kiểm tra còn lỗi không
        public bool HasError()
        {
            // Tìm tất cả các PackIcon
            foreach (var icon in FindVisualChildren<PackIcon>(this))
            {
                if (icon.Style == FindResource("ErrorIcon") && icon.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        // Hàm tìm kiếm các control con
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T t)
                    {
                        yield return t;
                    }
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}