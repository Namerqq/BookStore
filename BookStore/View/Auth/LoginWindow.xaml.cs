using System.Windows;
using System.Windows.Input;
using BookStore.Controllers;
using BookStore.Utilities;

namespace BookStore.View.Auth
{
    public partial class LoginWindow : Window
    {
        private readonly AccountController _controller;

        public LoginWindow()
        {
            InitializeComponent();
            _controller = new AccountController();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ email và mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Gọi hàm Login từ AccountController
            string errorMessage = _controller.Login(email, password);

            if (errorMessage != null)
            {
                MessageBox.Show(errorMessage, "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show($"Đăng nhập thành công! Chào {GlobalSession.FullName}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Điều hướng dựa vào Role
                if (GlobalSession.Role == "ADMIN")
                {
                    View.Admin.AdminAccountWindow adminWindow = new View.Admin.AdminAccountWindow();
                    adminWindow.Show();
                }
                else
                {
                    MessageBox.Show("Chuyển hướng cho các vai trò khác sẽ được cập nhật sau.", "Thông báo");
                }

                this.Close(); // Đóng cửa sổ đăng nhập
            }
        }

        private void NavToRegister_Click(object sender, MouseButtonEventArgs e)
        {
            // Mở cửa sổ đăng ký và đóng cửa sổ đăng nhập hiện tại
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}