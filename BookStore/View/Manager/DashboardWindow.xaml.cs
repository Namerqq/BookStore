using System;
using System.Windows;
using Microsoft.Win32;
using BookStore.DAO;
using BookStore.Utilities;

namespace BookStore.View.Manager
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleManager)) return;

            dpFromDate.SelectedDate = DateTime.Now.Date.AddDays(-30);
            dpToDate.SelectedDate = DateTime.Now.Date;
            LoadReport();
        }

        private void LoadReport()
        {
            DateTime from = dpFromDate.SelectedDate ?? DateTime.Now.Date.AddDays(-30);
            DateTime to = dpToDate.SelectedDate ?? DateTime.Now.Date;

            if (to < from)
            {
                MessageBox.Show("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu!", "Lỗi lọc", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var r = ReportDAO.Instance.GetRevenueReport(from, to);
            lblTotalOrders.Text = r.TotalOrders.ToString();
            lblTotalRevenue.Text = $"{r.TotalRevenue:N0} VNĐ";
            lblGrossProfit.Text = $"{r.GrossProfit:N0} VNĐ";

            dgTopSelling.ItemsSource = ReportDAO.Instance.GetTopSellingProducts(from, to, 5);
            dgLowStock.ItemsSource = ReportDAO.Instance.GetLowStockProducts(10);
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadReport();
        }

        private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"Products_{DateTime.Now:ddMMyyyy}.csv"
            };
            if (dialog.ShowDialog() == true)
            {
                ReportDAO.Instance.ExportProductsToCsv(dialog.FileName);
                MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
