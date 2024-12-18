using System;
using System.Windows;
using System.Windows.Controls;
using QLDB.Models;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace QLDB
{
    public partial class AUInterface : Window
    {
        private readonly QLDBContext _context;

        public AUInterface()
        {
            InitializeComponent();
            _context = new QLDBContext();
            MainFrame.Source = new Uri("TrangChu.xaml", UriKind.Relative);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TrangChu_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Source = new Uri("TrangChu.xaml", UriKind.Relative);
            CloseSidebar(); // Đóng sidebar sau khi load UserControl
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Source = new Uri("BaoCaoThongKe.xaml", UriKind.Relative);
            CloseSidebar(); // Đóng sidebar sau khi load UserControl
        }

        private void FootballerManagement_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Source = new Uri("FootballerManagementUC.xaml", UriKind.Relative);
            CloseSidebar(); // Đóng sidebar sau khi load UserControl
        }

        private void FormationManagement_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Source = new Uri("FormationManagementUC.xaml", UriKind.Relative);
            CloseSidebar(); // Đóng sidebar sau khi load UserControl
        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            bool isCollapsed = SidebarOverlay.Visibility == Visibility.Collapsed;

            SidebarOverlay.Visibility = isCollapsed ? Visibility.Visible : Visibility.Collapsed;

            var animation = new DoubleAnimation
            {
                From = isCollapsed ? -250 : 0,
                To = isCollapsed ? 0 : -250,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = isCollapsed ? EasingMode.EaseOut : EasingMode.EaseIn }
            };

            if (!isCollapsed)
            {
                animation.Completed += (s, _) => SidebarOverlay.Visibility = Visibility.Collapsed;
            }

            Sidebar.RenderTransform = new TranslateTransform(-250, 0);
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void CloseSidebar()
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = -250,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            animation.Completed += (s, _) => SidebarOverlay.Visibility = Visibility.Collapsed;
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void CloseSidebar_Click(object sender, MouseButtonEventArgs e)
        {
            // Ẩn sidebar 
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = -250,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            animation.Completed += (s, _) => SidebarOverlay.Visibility = Visibility.Collapsed;
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
    }
}