using System.Windows;
using BookStore.View.Manager;
using BookStore.View.Admin;
using BookStore.View.Auth;
using BookStore.Utilities;

namespace BookStore.View.Admin
{
    public partial class AdminLayoutWindow : Window
    {
        public AdminLayoutWindow()
        {
            InitializeComponent();
        }

        // 1. Sự kiện mở Quản lý Khuyến mãi / Voucher (Thay thế thông báo cũ)
        private void BtnVouchers_Click(object sender, RoutedEventArgs e)
        {
            VoucherManagementWindow voucherWindow = new VoucherManagementWindow();
            voucherWindow.Owner = this;
            voucherWindow.ShowDialog();
        }

        // 2. Sự kiện mở Quản lý Tài khoản (ADM-01 & ADM-02)
        private void BtnAccounts_Click(object sender, RoutedEventArgs e)
        {
            AdminAccountWindow accountWindow = new AdminAccountWindow();
            accountWindow.Owner = this;
            accountWindow.ShowDialog();
        }

        // 3. Sự kiện Đăng xuất khỏi hệ thống quản trị
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult ask = MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống quản trị?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ask == MessageBoxResult.Yes)
            {
                // Xóa session
                GlobalSession.AccountId = 0;
                GlobalSession.FullName = null;
                GlobalSession.Role = null;

                // Quay lại màn hình Login
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}