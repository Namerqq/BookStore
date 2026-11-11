using System.Windows;
using System.Windows.Controls;
using BookStore.Controllers;

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
            string pass = txtPassword.Text.Trim();

            ComboBoxItem selectedRoleItem = cmbRoles.SelectedItem as ComboBoxItem;
            string role = selectedRoleItem?.Tag.ToString() ?? "CUSTOMER";

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ các trường bắt buộc!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string result = _controller.AdminAddAccount(name, email, pass, role);

            if (result == null)
            {
                MessageBox.Show("Thêm tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}