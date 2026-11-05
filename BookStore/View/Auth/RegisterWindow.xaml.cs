using System.Windows;
using System.Windows.Input;
using BookStore.Controllers;

namespace BookStore.View.Auth
{
    public partial class RegisterWindow : Window
    {
        private readonly AccountController _controller;

        public RegisterWindow()
        {
            InitializeComponent();
            _controller = new AccountController();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();

            // Lấy mật khẩu dựa trên trạng thái (đang ẩn hay đang hiện)
            string pass = txtPassword.Visibility == Visibility.Visible ? txtPassword.Password : txtPasswordVisible.Text;
            string confirmPass = txtConfirmPassword.Visibility == Visibility.Visible ? txtConfirmPassword.Password : txtConfirmPasswordVisible.Text;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng điền đầy đủ các thông tin bắt buộc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (pass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (chkAgreeTerms.IsChecked != true)
            {
                MessageBox.Show("Bạn cần đồng ý với Điều khoản dịch vụ và Chính sách bảo mật.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string errorMessage = _controller.Register(fullName, email, pass);

            if (errorMessage != null)
            {
                MessageBox.Show(errorMessage, "Lỗi đăng ký", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Đăng ký tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void NavToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        // Sự kiện ẩn/hiện mật khẩu
        private void TogglePassword_Click(object sender, MouseButtonEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
            }
        }

        // Sự kiện ẩn/hiện mật khẩu xác nhận
        private void ToggleConfirmPassword_Click(object sender, MouseButtonEventArgs e)
        {
            if (txtConfirmPassword.Visibility == Visibility.Visible)
            {
                txtConfirmPasswordVisible.Text = txtConfirmPassword.Password;
                txtConfirmPassword.Visibility = Visibility.Collapsed;
                txtConfirmPasswordVisible.Visibility = Visibility.Visible;
            }
            else
            {
                txtConfirmPassword.Password = txtConfirmPasswordVisible.Text;
                txtConfirmPasswordVisible.Visibility = Visibility.Collapsed;
                txtConfirmPassword.Visibility = Visibility.Visible;
            }
        }
    }
}