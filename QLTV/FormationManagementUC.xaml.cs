using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using QLDB.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using MaterialDesignThemes.Wpf;
using System.Text;

namespace QLDB
{
    public partial class FormationManagementUC : UserControl
    {
        private QLDBContext _context = new QLDBContext();
        private Button? _selectedPositionButton = null;

        private Dictionary<string, Dictionary<string, Point>> _formationPositions = new Dictionary<string, Dictionary<string, Point>>()
        {
            {
                "4-4-2", new Dictionary<string, Point>()
                {
                    { "GKButton", new Point(143, 378) },
                    { "LBButton", new Point(18, 276) },
                    { "CB1Button", new Point(103, 282) },
                    { "CB2Button", new Point(184, 282) },
                    { "RBButton", new Point(270, 276) },
                    { "LMButton", new Point(18, 139) },
                    { "CM1Button", new Point(103, 163) },
                    { "CM2Button", new Point(184, 163) },
                    { "RMButton", new Point(270, 139) },
                    { "ST1Button", new Point(103, 48) },
                    { "ST2Button", new Point(184, 48) }
                }
            },
            {
                "4-3-3", new Dictionary<string, Point>()
                {
                    { "GKButton", new Point(143, 378) },
                    { "LBButton", new Point(18, 276) },
                    { "CB1Button", new Point(103, 282) },
                    { "CB2Button", new Point(184, 282) },
                    { "RBButton", new Point(270, 276) },
                    { "LMButton", new Point(35, 48) },
                    { "CM1Button", new Point(58, 170) },
                    { "CM2Button", new Point(226, 170) },
                    { "RMButton", new Point(143, 120) },
                    { "ST1Button", new Point(143, 18) },
                    { "ST2Button", new Point(256, 48) }
                }
            },
            {
                "4-2-3-1", new Dictionary<string, Point>()
                {
                    { "GKButton", new Point(140, 385) },
                    { "LBButton", new Point(15, 283) },
                    { "CB1Button", new Point(100, 289) },
                    { "CB2Button", new Point(181, 289) },
                    { "RBButton", new Point(267, 283) },
                    { "LMButton", new Point(15, 90) },
                    { "CM1Button", new Point(73, 194) },
                    { "CM2Button", new Point(208, 194) },
                    { "RMButton", new Point(140, 108) },
                    { "ST1Button", new Point(140, 10) },
                    { "ST2Button", new Point(263, 90) }
                }
            },
            {
                "5-3-2", new Dictionary<string, Point>()
                {
                    { "GKButton", new Point(143, 378) },
                    { "LBButton", new Point(10, 205) },
                    { "CB1Button", new Point(48, 287) },
                    { "CB2Button", new Point(143, 287) },
                    { "RBButton", new Point(270, 205) },
                    { "LMButton", new Point(48, 114) },
                    { "CM1Button", new Point(143, 135) },
                    { "CM2Button", new Point(235, 114) },
                    { "RMButton", new Point(238, 287) },
                    { "ST1Button", new Point(95, 10) },
                    { "ST2Button", new Point(190, 10) }
                }
            }
        };

        public FormationManagementUC()
        {
            InitializeComponent();
            LoadPlayers();
            FormationComboBox.SelectionChanged += FormationComboBox_SelectionChanged;
            PlayersDataGrid.SelectionChanged += PlayersDataGrid_SelectionChanged;
            LoadDoiHinh();
        }

        private void LoadPlayers()
        {
            PlayersDataGrid.ItemsSource = _context.CAUTHU.Where(c => !c.IsDeleted).ToList();
        }

        private void FormationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormationComboBox.SelectedItem != null)
            {
                string selectedFormation = (FormationComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                UpdateButtonContent(selectedFormation);
                UpdateButtonPositions(selectedFormation);
            }
        }

        private void UpdateButtonContent(string formation)
        {
            Dictionary<string, Dictionary<string, string>> buttonContent = new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "4-4-2", new Dictionary<string, string>()
                    {
                        { "GKButton", "GK" },
                        { "LBButton", "LB" },
                        { "CB1Button", "CB1" },
                        { "CB2Button", "CB2" },
                        { "RBButton", "RB" },
                        { "LMButton", "LM" },
                        { "CM1Button", "CM1" },
                        { "CM2Button", "CM2" },
                        { "RMButton", "RM" },
                        { "ST1Button", "ST1" },
                        { "ST2Button", "ST2" }
                    }
                },
                {
                    "4-3-3", new Dictionary<string, string>()
                    {
                        { "GKButton", "GK" },
                        { "LBButton", "LB" },
                        { "CB1Button", "LCB" },
                        { "CB2Button", "RCB" },
                        { "RBButton", "RB" },
                        { "LMButton", "LW" },
                        { "CM1Button", "LCM" },
                        { "CM2Button", "RCM" },
                        { "RMButton", "CAM" },
                        { "ST1Button", "ST" },
                        { "ST2Button", "RW" }
                    }
                },
                {
                    "4-2-3-1", new Dictionary<string, string>()
                    {
                        { "GKButton", "GK" },
                        { "LBButton", "LB" },
                        { "CB1Button", "LCB" },
                        { "CB2Button", "RCB" },
                        { "RBButton", "RB" },
                        { "LMButton", "LAM" },
                        { "CM1Button", "LDM" },
                        { "CM2Button", "RDM" },
                        { "RMButton", "CAM" },
                        { "ST1Button", "ST" },
                        { "ST2Button", "RAM" }
                    }
                },
                {
                    "5-3-2", new Dictionary<string, string>()
                    {
                        { "GKButton", "GK" },
                        { "LBButton", "LWB" },
                        { "CB1Button", "CB1" },
                        { "CB2Button", "CB2" },
                        { "RBButton", "RWB" },
                        { "LMButton", "CM1" },
                        { "CM1Button", "CM2" },
                        { "CM2Button", "CM3" },
                        { "RMButton", "CB3" },
                        { "ST1Button", "CF1" },
                        { "ST2Button", "CF2" }
                    }
                }
            };

            Dictionary<string, string> content = buttonContent[formation];

            GKButton.Content = content["GKButton"];
            LBButton.Content = content["LBButton"];
            CB1Button.Content = content["CB1Button"];
            CB2Button.Content = content["CB2Button"];
            RBButton.Content = content["RBButton"];
            LMButton.Content = content["LMButton"];
            CM1Button.Content = content["CM1Button"];
            CM2Button.Content = content["CM2Button"];
            RMButton.Content = content["RMButton"];
            ST1Button.Content = content["ST1Button"];
            ST2Button.Content = content["ST2Button"];
        }

        private void UpdateButtonPositions(string formation)
        {
            if (_formationPositions.ContainsKey(formation))
            {
                Dictionary<string, Point> positions = _formationPositions[formation];

                Canvas.SetLeft(GKButton, positions["GKButton"].X);
                Canvas.SetTop(GKButton, positions["GKButton"].Y);
                Canvas.SetLeft(LBButton, positions["LBButton"].X);
                Canvas.SetTop(LBButton, positions["LBButton"].Y);
                Canvas.SetLeft(CB1Button, positions["CB1Button"].X);
                Canvas.SetTop(CB1Button, positions["CB1Button"].Y);
                Canvas.SetLeft(CB2Button, positions["CB2Button"].X);
                Canvas.SetTop(CB2Button, positions["CB2Button"].Y);
                Canvas.SetLeft(RBButton, positions["RBButton"].X);
                Canvas.SetTop(RBButton, positions["RBButton"].Y);
                Canvas.SetLeft(LMButton, positions["LMButton"].X);
                Canvas.SetTop(LMButton, positions["LMButton"].Y);
                Canvas.SetLeft(CM1Button, positions["CM1Button"].X);
                Canvas.SetTop(CM1Button, positions["CM1Button"].Y);
                Canvas.SetLeft(CM2Button, positions["CM2Button"].X);
                Canvas.SetTop(CM2Button, positions["CM2Button"].Y);
                Canvas.SetLeft(RMButton, positions["RMButton"].X);
                Canvas.SetTop(RMButton, positions["RMButton"].Y);
                Canvas.SetLeft(ST1Button, positions["ST1Button"].X);
                Canvas.SetTop(ST1Button, positions["ST1Button"].Y);
                Canvas.SetLeft(ST2Button, positions["ST2Button"].X);
                Canvas.SetTop(ST2Button, positions["ST2Button"].Y);
            }
        }

        private void PositionButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedPositionButton = sender as Button;

            List<string> positionsToHighlight = GetPositionsForButton(_selectedPositionButton.Name);

            HighlightPlayersByPosition(positionsToHighlight);
        }

        private List<string> GetPositionsForButton(string buttonName)
        {
            Dictionary<string, List<string>> buttonPositions = new Dictionary<string, List<string>>()
            {
                { "GKButton", new List<string>() { "Thủ môn" } },
                { "LBButton", new List<string>() { "Hậu vệ" } },
                { "CB1Button", new List<string>() { "Hậu vệ" } },
                { "CB2Button", new List<string>() { "Hậu vệ" } },
                { "RBButton", new List<string>() { "Hậu vệ" } },
                { "LMButton", new List<string>() { "Tiền vệ" } },
                { "CM1Button", new List<string>() { "Tiền vệ" } },
                { "CM2Button", new List<string>() { "Tiền vệ" } },
                { "RMButton", new List<string>() { "Tiền vệ" } },
                { "ST1Button", new List<string>() { "Tiền đạo" } },
                { "ST2Button", new List<string>() { "Tiền đạo" } }
            };

            if (buttonPositions.ContainsKey(buttonName))
            {
                return buttonPositions[buttonName];
            }
            else
            {
                return new List<string>();
            }
        }

        private void HighlightPlayersByPosition(List<string> positions)
        {
            for (int i = 0; i < PlayersDataGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow)PlayersDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (row != null)
                {
                    row.Foreground = Brushes.Black;
                }
            }

            for (int i = 0; i < PlayersDataGrid.Items.Count; i++)
            {
                CAUTHU player = (CAUTHU)PlayersDataGrid.Items[i];
                if (positions.Contains(player.ViTriThiDau))
                {
                    DataGridRow row = (DataGridRow)PlayersDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row != null)
                    {
                        row.Foreground = Brushes.Red;
                    }
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DoiHinhThiDauDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PlayersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedPositionButton != null && PlayersDataGrid.SelectedItem is CAUTHU player)
            {
                bool isPlayerAlreadySelected = false;
                foreach (var button in FormationCanvas.Children.OfType<Button>())
                {
                    if (button != _selectedPositionButton && button.Content.ToString() == player.HoTen)
                    {
                        isPlayerAlreadySelected = true;
                        break;
                    }
                }

                if (isPlayerAlreadySelected)
                {
                    MessageBox.Show("Cầu thủ này đã được chọn trong đội hình!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    _selectedPositionButton.Content = player.HoTen;
                    _selectedPositionButton = null;
                }
            }
        }

        private void TenDoiHinhTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tenDoiHinh = TenDoiHinhTextBox.Text.Trim();

            if (_context.DOIHINHTHIDAU.Any(dh => dh.TenDoiHinh == tenDoiHinh && !dh.IsDeleted))
            {
                icTenDoiHinhError.Visibility = Visibility.Visible;
                icTenDoiHinhError.ToolTip = "Tên đội hình đã tồn tại!";
            }
            else
            {
                icTenDoiHinhError.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (HasError())
                {
                    MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi cập nhật!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (FormationCanvas.Children.OfType<Button>().Any(btn => btn.Content.ToString() == btn.Name.Replace("Button", "")))
                {
                    MessageBox.Show("Vui lòng chọn đủ cầu thủ cho đội hình.");
                    return;
                }

                var doiHinh = new DOIHINHTHIDAU
                {
                    TenDoiHinh = TenDoiHinhTextBox.Text,
                    SoDoThiDau = (FormationComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    ChienThuatThiDau = (ChienThuatComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                };

                _context.DOIHINHTHIDAU.Add(doiHinh);
                _context.SaveChanges();

                foreach (var button in FormationCanvas.Children.OfType<Button>())
                {
                    if (button.Content.ToString() != button.Name.Replace("Button", ""))
                    {
                        var playerName = button.Content.ToString();
                        var player = _context.CAUTHU.FirstOrDefault(c => c.HoTen == playerName);
                        if (player != null)
                        {
                            var chiTiet = new CHITIETDOIHINH
                            {
                                IDDoiHinh = doiHinh.ID,
                                IDCauThu = player.ID
                            };
                            _context.CHITIETDOIHINH.Add(chiTiet);
                        }
                    }
                }

                _context.SaveChanges();
                MessageBox.Show("Lưu đội hình thành công!");
                LoadDoiHinh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Xóa tất cả lựa chọn cầu thủ trên sân
            foreach (var button in FormationCanvas.Children.OfType<Button>())
            {
                button.Content = button.Name.Replace("Button", "");
            }

            // Reset ComboBoxes về trạng thái ban đầu
            FormationComboBox.SelectedIndex = -1;
            ChienThuatComboBox.SelectedIndex = -1;

            // Xóa nội dung TextBox
            TenDoiHinhTextBox.Text = "";

            // Load lại danh sách cầu thủ từ database
            LoadPlayers();

            MessageBox.Show("Làm mới dữ liệu thành công!");
        }

        private void LoadDoiHinh()
        {
            var doiHinh = _context.DOIHINHTHIDAU.Where(dh => !dh.IsDeleted).ToList();
            DoiHinhThiDauDataGrid.ItemsSource = doiHinh;
        }

        private void LamMoiDoiHinh_Click(object sender, RoutedEventArgs e)
        {
            DSDHSearchTextBox.Text = "";
            DSDHSearchCriteriaComboBox.SelectedItem = null;
            LoadDoiHinh();
            DoiHinhThiDauDataGrid.UnselectAll();
            MessageBox.Show("Làm mới danh sách đội hình thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void XoaDoiHinh_Click(object sender, RoutedEventArgs e)
        {
            if (DoiHinhThiDauDataGrid.SelectedItem is DOIHINHTHIDAU selectedDoiHinh)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa đội hình này?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var doiHinhToDelete = _context.DOIHINHTHIDAU.Find(selectedDoiHinh.ID);
                        if (doiHinhToDelete != null)
                        {
                            doiHinhToDelete.IsDeleted = true;
                            _context.SaveChanges();

                            LoadDoiHinh();

                            MessageBox.Show("Xóa đội hình thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy đội hình để xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa đội hình: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một đội hình để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DSDHSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = NormalizeString(DSDHSearchTextBox.Text.Trim());
            string searchCriteria = DSDHSearchCriteriaComboBox.SelectedItem?.ToString();

            var query = _context.DOIHINHTHIDAU
                .Where(dh => !dh.IsDeleted)
                .AsEnumerable()
                .ToList();

            if (!string.IsNullOrEmpty(searchCriteria))
            {
                switch (searchCriteria)
                {
                    case "Tên Đội Hình":
                        query = query.Where(dh => NormalizeString(dh.TenDoiHinh).Contains(searchTerm)).ToList();
                        break;
                    case "Chiến Thuật":
                        query = query.Where(dh => NormalizeString(dh.ChienThuatThiDau).Contains(searchTerm)).ToList();
                        break;
                }
            }
            else
            {
                query = query.Where(dh =>
                    NormalizeString(dh.TenDoiHinh).Contains(searchTerm) ||
                    NormalizeString(dh.ChienThuatThiDau).Contains(searchTerm)
                ).ToList();
            }

            DoiHinhThiDauDataGrid.ItemsSource = query;
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

        public bool HasError()
        {
            foreach (var icon in FindVisualChildren<PackIcon>(this))
            {
                if (icon.Style == FindResource("ErrorIcon") && icon.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }

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