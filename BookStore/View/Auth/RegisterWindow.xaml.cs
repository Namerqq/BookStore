using System.Windows;
using System.Windows.Input;
using BookStore.Controllers; // Nhớ thêm using này

namespace BookStore.View.Auth
{
    public partial class RegisterWindow : Window
    {
        private readonly AccountController _controller;

        public RegisterWindow()
        {
            InitializeComponent();
            _controller = new AccountController(); // Khởi tạo controller
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string pass = txtPassword.Password;
            string confirmPass = txtConfirmPassword.Password;

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

            // Gọi Controller để đẩy dữ liệu xuống Database
            string errorMessage = _controller.Register(fullName, email, pass);

            if (errorMessage != null)
            {
                MessageBox.Show(errorMessage, "Lỗi đăng ký", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Đăng ký tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Chuyển hướng về màn hình Đăng nhập
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
    }
}