using System.Windows;
using BookStore.Controllers;
using BookStore.Utilities;

namespace BookStore.View.Customer
{
    public partial class ProfileWindow : Window
    {
        private readonly AccountController _controller;

        public ProfileWindow()
        {
            InitializeComponent();
            _controller = new AccountController();

            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            if (!SessionGuard.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem thông tin!", "Yêu cầu", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }

            var account = _controller.GetAccountById(SessionGuard.CurrentAccountId);
            if (account != null)
            {
                txtEmail.Text = account.Email;
                txtFullName.Text = account.FullName;
                txtPhone.Text = account.Phone ?? string.Empty;
            }

            // Rule 2.4: Tài khoản nội bộ không được tự sửa thông tin cá nhân, chỉ Admin được sửa
            if (!SessionGuard.IsCustomer)
            {
                txtInternalWarning.Visibility = Visibility.Visible;
                txtFullName.IsEnabled = false;
                txtPhone.IsEnabled = false;
                BtnUpdateProfile.IsEnabled = false;
                BtnUpdateProfile.Background = System.Windows.Media.Brushes.Gray;
            }
        }

        private void BtnUpdateProfile_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionGuard.IsLoggedIn) return;

            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();

            string? validationError = ValidationUtil.ValidateFullName(fullName)
                                   ?? ValidationUtil.ValidateOptionalPhone(phone);
            if (validationError != null)
            {
                MessageBox.Show(validationError, "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? result = _controller.UpdateProfile(SessionGuard.CurrentAccountId, fullName, phone);

            if (result == null)
            {
                MessageBox.Show("Cập nhật thông tin thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionGuard.IsLoggedIn) return;

            string oldPass = txtOldPassword.Password;
            string newPass = txtNewPassword.Password;
            string confirmPass = txtConfirmNewPassword.Password;

            if (string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("Vui lòng điền đủ mật khẩu cũ và mới!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? passError = ValidationUtil.ValidatePassword(newPass);
            if (passError != null)
            {
                MessageBox.Show(passError, "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? result = _controller.ChangePassword(SessionGuard.CurrentAccountId, oldPass, newPass);

            if (result == null)
            {
                MessageBox.Show("Đổi mật khẩu thành công! Hãy dùng mật khẩu mới cho lần đăng nhập sau.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                txtOldPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmNewPassword.Clear();
            }
            else
            {
                MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
