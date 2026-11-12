using System.Windows;
using System.Windows.Controls;
using BookStore.Utilities;
using BookStore.View.Admin;
using BookStore.View.Auth;
using BookStore.View.Manager;
using BookStore.View.Staff;

namespace BookStore.View.Shared
{
    public partial class SidebarControl : UserControl
    {
        public SidebarControl()
        {
            InitializeComponent();
            ApplyRolePermissions();
        }

        private void ApplyRolePermissions()
        {
            btnDashboard.Visibility = Visibility.Collapsed;
            btnProducts.Visibility = Visibility.Collapsed;
            btnVouchers.Visibility = Visibility.Collapsed;
            btnRefunds.Visibility = Visibility.Collapsed;
            btnOrders.Visibility = Visibility.Collapsed;
            btnAccounts.Visibility = Visibility.Collapsed;
            btnLogs.Visibility = Visibility.Collapsed;

            string role = GlobalSession.Role;
            txtRoleSubtitle.Text = string.IsNullOrEmpty(role)
                ? "Hệ thống quản lý"
                : $"Xin chào, {GlobalSession.FullName} ({role})";

            if (role == SessionGuard.RoleStaff)
            {
                btnOrders.Visibility = Visibility.Visible;
            }
            else if (role == SessionGuard.RoleManager)
            {
                btnDashboard.Visibility = Visibility.Visible;
                btnProducts.Visibility = Visibility.Visible;
                btnVouchers.Visibility = Visibility.Visible;
                btnRefunds.Visibility = Visibility.Visible;
            }
            else if (role == SessionGuard.RoleAdmin)
            {
                btnAccounts.Visibility = Visibility.Visible;
                btnLogs.Visibility = Visibility.Visible;
            }
        }

        private void MenuDashboard_Click(object sender, RoutedEventArgs e)
        {
            new DashboardWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void MenuProducts_Click(object sender, RoutedEventArgs e)
        {
            new ProductManageWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void MenuAccounts_Click(object sender, RoutedEventArgs e)
        {
            new AdminAccountWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void MenuLogs_Click(object sender, RoutedEventArgs e)
        {
            new SystemLogWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void MenuVouchers_Click(object sender, RoutedEventArgs e)
        {
            new VoucherManagementWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void MenuOrders_Click(object sender, RoutedEventArgs e)
        {
            new StaffOrderWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void MenuRefunds_Click(object sender, RoutedEventArgs e)
        {
            new RefundWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult ask = MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ask == MessageBoxResult.Yes)
            {
                GlobalSession.Clear();

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Window? parentWindow = Window.GetWindow(this);
                parentWindow?.Close();
            }
        }
    }
}
