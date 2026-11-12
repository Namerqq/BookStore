using System.Windows;
using BookStore.Controllers;
using BookStore.Models;
using BookStore.Utilities;

namespace BookStore.View.Admin
{
    public partial class EditAccountDialogWindow : Window
    {
        private readonly AccountController _controller = new();
        private readonly int _accountId;

        public EditAccountDialogWindow(TbAccount account)
        {
            InitializeComponent();
            _accountId = account.AccountId;
            txtEmail.Text = account.Email;
            txtFullName.Text = account.FullName;
            txtPhone.Text = account.Phone ?? string.Empty;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();

            string? err = ValidationUtil.ValidateFullName(fullName)
                       ?? ValidationUtil.ValidateOptionalPhone(phone);
            if (err != null)
            {
                MessageBox.Show(err, "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? result = _controller.AdminUpdateAccount(_accountId, fullName, phone);
            if (result == null)
            {
                MessageBox.Show("Cập nhật thông tin tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
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
