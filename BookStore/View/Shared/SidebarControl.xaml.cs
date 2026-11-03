using System.Windows;
using System.Windows.Controls;
using BookStore.View.Auth;

namespace BookStore.View.Shared
{
    public partial class SidebarControl : UserControl
    {
        public SidebarControl()
        {
            InitializeComponent();
        }

        private void MenuDashboard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng Tổng quan & Báo cáo đang phát triển.", "Thông báo");
        }

        private void MenuProducts_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng Quản lý Sản phẩm đang phát triển.", "Thông báo");
        }

        private void MenuAccounts_Click(object sender, RoutedEventArgs e)
        {
            // Quản lý tài khoản
        }

        private void MenuVouchers_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng Quản lý Khuyến mãi đang phát triển.", "Thông báo");
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }
}