using System.Windows;
using System.Windows.Controls;
using BookStore.View.Manager;
using BookStore.View.Admin;
using BookStore.View.Auth;
using BookStore.Utilities;

namespace BookStore.View.Shared
{
    public partial class SidebarControl : UserControl
    {
        public SidebarControl()
        {
            InitializeComponent();
            ApplyRolePermissions(); // Gọi hàm phân quyền khi load Sidebar
        }

        private void ApplyRolePermissions()
        {
            string role = GlobalSession.Role;

            if (role == "STAFF")
            {
                btnAccounts.Visibility = Visibility.Collapsed;
                btnVouchers.Visibility = Visibility.Collapsed;
            }
            else if (role == "MANAGER")
            {
                // MANAGER được phép quản lý khuyến mãi, ẩn quản lý tài khoản hệ thống
                btnAccounts.Visibility = Visibility.Collapsed;
                btnVouchers.Visibility = Visibility.Visible;
            }
            else if (role == "ADMIN")
            {
                // ADMIN không quản lý khuyến mãi theo yêu cầu của bạn
                btnAccounts.Visibility = Visibility.Visible;
                btnVouchers.Visibility = Visibility.Collapsed;   // Ẩn nút Quản lý Khuyến mãi đối với Admin
            }
        }

        private void MenuDashboard_Click(object sender, RoutedEventArgs e) { }
        private void MenuProducts_Click(object sender, RoutedEventArgs e) { }

        private void MenuAccounts_Click(object sender, RoutedEventArgs e)
        {
            AdminAccountWindow accountWindow = new AdminAccountWindow();
            accountWindow.ShowDialog();
        }

        private void MenuVouchers_Click(object sender, RoutedEventArgs e)
        {
            VoucherManagementWindow voucherWindow = new VoucherManagementWindow();
            voucherWindow.ShowDialog();
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult ask = MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ask == MessageBoxResult.Yes)
            {
                GlobalSession.AccountId = 0;
                GlobalSession.FullName = null;
                GlobalSession.Role = null;

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Window parentWindow = Window.GetWindow(this);
                parentWindow?.Close();
            }
        }
    }
}