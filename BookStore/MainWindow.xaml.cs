using System.Windows;
using BookStore.Utilities;
using BookStore.View.Admin;
using BookStore.View.Customer;

namespace BookStore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigateBasedOnRole();
        }

        private void NavigateBasedOnRole()
        {
            string role = GlobalSession.Role; // Lấy quyền từ phiên đăng nhập

            if (role == "ADMIN")
            {
                // Nếu là Admin, điều hướng sang giao diện quản trị Admin
                MainFrame.Navigate(new AdminLayoutWindow());
            }
            else if (role == "MANAGER")
            {
                // Nếu là Manager, điều hướng sang màn hình quản lý (hoặc tạo layout riêng cho Manager)
                MainFrame.Navigate(new AdminLayoutWindow()); // Tạm dùng chung layout quản trị
            }
            else if (role == "STAFF")
            {
                // Nếu là Staff, điều hướng sang màn hình nhân viên
                MessageBox.Show("Đang chuyển đến giao diện Nhân viên...", "Thông báo");
                // MainFrame.Navigate(new StaffWindow());
            }
            else
            {
                // Trường hợp là CUSTOMER hoặc Khách vãng lai (Chưa đăng nhập)
                MainFrame.Navigate(new HomeWindow());
            }
        }
    }
}