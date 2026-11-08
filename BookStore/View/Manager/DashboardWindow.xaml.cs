using System;
using System.Windows;
using Microsoft.Win32;
using BookStore.DAOs;

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
            var r = ReportDAO.Instance.GetRevenueReport(DateTime.Now.AddDays(-30), DateTime.Now);
            lblTotalOrders.Text = r.TotalOrders.ToString();
            lblTotalRevenue.Text = $"{r.TotalRevenue:N0} VNĐ";
            lblGrossProfit.Text = $"{r.GrossProfit:N0} VNĐ";

            dgTopSelling.ItemsSource = ReportDAO.Instance.GetTopSellingProducts(5);
            dgLowStock.ItemsSource = ReportDAO.Instance.GetLowStockProducts(10);
        }

        private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "Products.csv" };
            if (dialog.ShowDialog() == true)
            {
                ReportDAO.Instance.ExportProductsToCsv(dialog.FileName);
                MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}