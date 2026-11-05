using System.Windows;
using System.Windows.Input;
using BookStore.Utilities;
using BookStore.View.Customer; // Namespace cho HomeWindow của khách hàng
using BookStore.View.Admin;    // Bổ sung namespace này để gọi AdminLayoutWindow

namespace BookStore.View.Auth
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            // Lấy mật khẩu dựa trên trạng thái (đang ẩn hay hiện)
            string pass = txtPassword.Visibility == Visibility.Visible ? txtPassword.Password : txtPasswordVisible.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập Email và Mật khẩu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var _controller = new BookStore.Controllers.AccountController();
            string error = _controller.Login(email, pass);

            if (error != null)
            {
                MessageBox.Show(error, "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // ĐĂNG NHẬP THÀNH CÔNG: Điều hướng phân quyền chuẩn xác
                if (GlobalSession.Role == "CUSTOMER")
                {
                    // Khách hàng -> Mở HomeWindow (Trang chủ mua sắm)
                    HomeWindow homeWindow = new HomeWindow();
                    homeWindow.Show();
                }
                else
                {
                    // Admin / Manager / Staff -> Mở AdminLayoutWindow (Giao diện quản trị viên)
                    AdminLayoutWindow adminWindow = new AdminLayoutWindow();
                    adminWindow.Show();
                }

                // Đóng cửa sổ Login hiện tại an toàn bằng .Close()
                this.Close();
            }
        }

        private void LinkRegister_Click(object sender, MouseButtonEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            register.Show();
            this.Close();
        }

        private void LinkForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            // Xử lý mở cửa sổ Quên mật khẩu
        }

        // Sự kiện ẩn/hiện mật khẩu
        private void TogglePassword_Click(object sender, MouseButtonEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible)
            {
                // Hiện mật khẩu dưới dạng text thường
                txtPasswordVisible.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
            }
            else
            {
                // Ẩn mật khẩu (dấu chấm)
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
            }
        }
    }
}