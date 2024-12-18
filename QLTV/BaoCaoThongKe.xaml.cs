using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using QLDB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Win32;

namespace QLDB
{
    public partial class BaoCaoThongKe : INotifyPropertyChanged
    {
        private string _tongSoLuongCauThu;
        private string _soCauThuTre;

        public string TongSoLuongCauThu
        {
            get => _tongSoLuongCauThu;
            set
            {
                _tongSoLuongCauThu = value;
                OnPropertyChanged();
            }
        }

        public string SoCauThuTre
        {
            get => _soCauThuTre;
            set
            {
                _soCauThuTre = value;
                OnPropertyChanged();
            }
        }

        private void LoadThongTinCauThu()
        {
            using (var context = new QLDBContext())
            {
                int tongSoCauThu = context.CAUTHU.Count(c => !c.IsDeleted);
                TongSoLuongCauThu = tongSoCauThu.ToString();

                int soCauThuTre = context.CAUTHU.Count(c => !c.IsDeleted && DateTime.Now.Year - c.NgaySinh.Year < 20);
                SoCauThuTre = soCauThuTre.ToString();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SeriesCollection PieSeriesCollection { get; set; } = new SeriesCollection();
        public SeriesCollection RowViTriSeriesCollection { get; set; }
        public SeriesCollection RowQuocTichSeriesCollection { get; set; }
        public List<string> LabelsViTri { get; set; }
        public List<string> LabelsQuocTich { get; set; }
        public Func<double, string> Formatter { get; set; }

        public BaoCaoThongKe()
        {
            InitializeComponent();
            LoadThongTinCauThu();
            RowViTriSeriesCollection = new SeriesCollection();
            RowQuocTichSeriesCollection = new SeriesCollection();
            LabelsViTri = new List<string>();
            LabelsQuocTich = new List<string>();
            Formatter = value => value.ToString("N0");
            PieSeriesCollection = new SeriesCollection();

            LoadRowViTriThiDauChartData();
            LoadRowQuocTichChartData();
            LoadPieChartData();

            DataContext = this;
        }

        public void LoadRowViTriThiDauChartData()
        {
            using (var context = new QLDBContext())
            {
                var thongKeViTri = context.CAUTHU
                    .Where(c => !c.IsDeleted)
                    .GroupBy(c => c.ViTriThiDau)
                    .Select(g => new
                    {
                        ViTriThiDau = g.Key,
                        SoLuong = g.Count()
                    })
                    .ToList();

                LabelsViTri.Clear();
                LabelsViTri.AddRange(thongKeViTri.Select(x => x.ViTriThiDau));

                RowViTriSeriesCollection.Clear();
                RowViTriSeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Values = new ChartValues<int>(thongKeViTri.Select(x => x.SoLuong)),
                        Fill = null,
                        DataLabels = true,
                    }
                };
            }
        }

        public void LoadRowQuocTichChartData()
        {
            using (var context = new QLDBContext())
            {
                var thongKeQuocTich = context.CAUTHU
                    .Where(cv => !cv.IsDeleted)
                    .GroupBy(cv => cv.QuocTich)
                    .Select(group => new
                    {
                        QuocTich = group.Key,
                        SoLuong = group.Count()
                    })
                    .ToList();

                LabelsQuocTich.Clear();
                LabelsQuocTich.AddRange(thongKeQuocTich.Select(x => x.QuocTich));

                RowQuocTichSeriesCollection.Clear();
                RowQuocTichSeriesCollection.Add(new ColumnSeries
                {
                    Values = new ChartValues<int>(thongKeQuocTich.Select(x => x.SoLuong)),
                    Fill = null,
                    DataLabels = true,
                });
            }
        }

        private void LoadPieChartData()
        {
            using (var context = new QLDBContext())
            {
                var cauThu = context.CAUTHU
                    .Include(cv => cv.IDTinhTrangSucKhoeNavigation)
                    .Where(cv => !cv.IsDeleted)
                    .ToList();

                var healthStatuses = cauThu
                    .GroupBy(cv => cv.IDTinhTrangSucKhoeNavigation.TenTinhTrang)
                    .Select(group => new
                    {
                        TrangThai = group.Key,
                        SoLuong = group.Count()
                    })
                    .ToList();

                PieSeriesCollection = new SeriesCollection();

                foreach (var status in healthStatuses)
                {
                    PieSeriesCollection.Add(new PieSeries
                    {
                        Title = status.TrangThai,
                        Values = new ChartValues<int> { status.SoLuong },
                        DataLabels = true
                    });
                }
            }
        }

        private void btnPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnPDF.Visibility = Visibility.Collapsed;

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    (int)this.ActualWidth,
                    (int)this.ActualHeight,
                    96, 96,
                    PixelFormats.Pbgra32);

                renderBitmap.Render(this);

                string pdfPath = $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
                saveFileDialog.FileName = pdfPath;
                if (saveFileDialog.ShowDialog() == true)
                {
                    pdfPath = saveFileDialog.FileName;

                    using (Document document = new Document(PageSize.A4, 50, 50, 50, 50))
                    {
                        PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(pdfPath, FileMode.Create));
                        document.Open();

                        Paragraph title = new Paragraph("REPORT", new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD));
                        title.Alignment = Element.ALIGN_CENTER;
                        document.Add(title);

                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                        using (var stream = new MemoryStream())
                        {
                            encoder.Save(stream);
                            stream.Position = 0;

                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(stream.ToArray());
                            image.ScaleToFit(document.PageSize.Width - document.LeftMargin - document.RightMargin,
                                document.PageSize.Height - document.TopMargin - document.BottomMargin);
                            image.Alignment = Element.ALIGN_CENTER;
                            document.Add(image);
                        }

                        document.Close();
                        writer.Close();
                    }

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pdfPath) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất báo cáo: {ex.Message}");
            }
            finally
            {
                btnPDF.Visibility = Visibility.Visible;
            }
        }
    }
}