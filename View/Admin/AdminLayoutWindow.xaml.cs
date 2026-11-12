using System.Windows;
using BookStore.Utilities;

namespace BookStore.View.Admin
{
    public partial class AdminLayoutWindow : Window
    {
        public AdminLayoutWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleStaff, SessionGuard.RoleManager, SessionGuard.RoleAdmin))
                return;

            txtWelcomeTitle.Text = $"Xin chào, {GlobalSession.FullName} ({GlobalSession.Role})";
            txtRoleSummary.Text = GlobalSession.Role switch
            {
                SessionGuard.RoleStaff => "Bạn đang đăng nhập với vai trò Nhân viên (STAFF). Chọn mục 'Xử lý Đơn hàng' bên trái để duyệt đơn, xác nhận VietQR, gọi xác nhận khách và tiếp nhận hoàn hàng.",
                SessionGuard.RoleManager => "Bạn đang đăng nhập với vai trò Quản lý (MANAGER). Chọn các mục bên trái để xem Dashboard doanh thu, quản lý sản phẩm & tồn kho, voucher và xác nhận hoàn tiền.",
                SessionGuard.RoleAdmin => "Bạn đang đăng nhập với vai trò Quản trị viên (ADMIN). Chọn các mục bên trái để quản lý tài khoản, phân quyền và theo dõi nhật ký hệ thống.",
                _ => string.Empty
            };
        }
    }
}
