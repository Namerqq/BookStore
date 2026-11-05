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
            // 1. Kiểm tra Null hoặc bằng 0
            if (GlobalSession.AccountId == null || GlobalSession.AccountId == 0)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem thông tin!", "Yêu cầu", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }

            // 2. Load dữ liệu từ DB lên Form (ĐÃ SỬA: Thêm .Value)
            var account = _controller.GetAccountById(GlobalSession.AccountId.Value);
            if (account != null)
            {
                txtEmail.Text = account.Email;
                txtFullName.Text = account.FullName;
                txtPhone.Text = account.Phone;
            }

            // 3. ÁP DỤNG RULE: Chỉ Admin được sửa thông tin tài khoản nội bộ
            if (GlobalSession.Role != "CUSTOMER")
            {
                txtInternalWarning.Visibility = Visibility.Visible;
                txtFullName.IsEnabled = false;
                txtPhone.IsEnabled = false;
                BtnUpdateProfile.IsEnabled = false;
                BtnUpdateProfile.Background = System.Windows.Media.Brushes.Gray;
            }
        }

        // Sự kiện Đổi thông tin
        private void BtnUpdateProfile_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();

            // Validate đơn giản
            if (string.IsNullOrEmpty(fullName))
            {
                MessageBox.Show("Họ và tên không được để trống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ĐÃ SỬA: Thêm .Value
            string result = _controller.UpdateProfile(GlobalSession.AccountId.Value, fullName, phone);

            if (result == null)
            {
                MessageBox.Show("Cập nhật thông tin thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Sự kiện Đổi mật khẩu
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPass = txtOldPassword.Password;
            string newPass = txtNewPassword.Password;
            string confirmPass = txtConfirmNewPassword.Password;

            // Validate
            if (string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("Vui lòng điền đủ mật khẩu cũ và mới!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ĐÃ SỬA: Thêm .Value
            string result = _controller.ChangePassword(GlobalSession.AccountId.Value, oldPass, newPass);

            if (result == null)
            {
                MessageBox.Show("Đổi mật khẩu thành công! Hãy dùng mật khẩu mới cho lần đăng nhập sau.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                // Xóa trống các ô sau khi đổi thành công
                txtOldPassword.Password = "";
                txtNewPassword.Password = "";
                txtConfirmNewPassword.Password = "";
            }
            else
            {
                MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}