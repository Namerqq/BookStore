using System.Windows;
using System.Windows.Controls;
using BookStore.Controllers;
using BookStore.Utilities;

namespace BookStore.View.Admin
{
    public partial class SystemLogWindow : Window
    {
        private readonly AccountController _controller;

        public SystemLogWindow()
        {
            InitializeComponent();
            _controller = new AccountController();

            // Phân quyền: Chỉ ADMIN được xem nhật ký hệ thống
            if (GlobalSession.Role != "ADMIN")
            {
                MessageBox.Show("Chỉ ADMIN mới có quyền xem Nhật ký hệ thống!", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            dgLogs.ItemsSource = _controller.GetSystemLogs(keyword);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(); // Tìm kiếm theo thời gian thực khi gõ phím
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            LoadData();
        }
    }
}