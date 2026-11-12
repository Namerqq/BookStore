using System.Windows;
using System.Windows.Controls;
using BookStore.Controllers;
using BookStore.Utilities;

namespace BookStore.View.Admin
{
    public partial class AddAccountDialogWindow : Window
    {
        private readonly AccountController _controller;

        public AddAccountDialogWindow()
        {
            InitializeComponent();
            _controller = new AccountController();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string name = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string pass = txtPassword.Text;

            var selectedRoleItem = cmbRoles.SelectedItem as ComboBoxItem;
            string role = selectedRoleItem?.Tag?.ToString() ?? SessionGuard.RoleCustomer;

            string? err = ValidationUtil.ValidateFullName(name)
                       ?? ValidationUtil.ValidateEmail(email)
                       ?? ValidationUtil.ValidateOptionalPhone(phone)
                       ?? ValidationUtil.ValidatePassword(pass);
            if (err != null)
            {
                MessageBox.Show(err, "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? result = _controller.AdminAddAccount(name, email, pass, role, phone);

            if (result == null)
            {
                MessageBox.Show("Thêm tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            else
            {
                MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
