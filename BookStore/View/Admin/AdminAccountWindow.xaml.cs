using System.Windows;
using System.Windows.Controls;
using BookStore.Controllers;
using BookStore.Models;
using BookStore.Utilities;

namespace BookStore.View.Admin
{
    public partial class AdminAccountWindow : Window
    {
        private readonly AccountController _controller;

        public AdminAccountWindow()
        {
            InitializeComponent();
            _controller = new AccountController();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleAdmin)) return;
            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            string role = "ALL";

            if (cmbRoleFilter.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                role = selectedItem.Tag.ToString() ?? "ALL";
            }

            dgAccounts.ItemsSource = _controller.GetAccounts(keyword, role);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtSearchPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (IsLoaded) LoadData();
        }

        private void CmbRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && dgAccounts != null) LoadData();
        }

        // ADM-01: Sửa thông tin tài khoản (bao gồm tài khoản nội bộ)
        private void BtnEditAccount_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not TbAccount account) return;

            var dialog = new EditAccountDialogWindow(account) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // ADM-01: Khóa/Mở khóa
        private void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int accountId)
            {
                MessageBoxResult ask = MessageBox.Show(
                    $"Bạn có chắc chắn muốn thay đổi trạng thái của tài khoản ID {accountId}?",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    string? result = _controller.ToggleAccountStatus(accountId);
                    if (result == null)
                    {
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // ADM-02: Phân quyền (Thay đổi Role)
        private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int accountId)
            {
                RoleDialogWindow dialog = new RoleDialogWindow { Owner = this };

                if (dialog.ShowDialog() == true)
                {
                    string newRole = dialog.SelectedRole;
                    string? result = _controller.UpdateAccountRole(accountId, newRole);

                    if (result == null)
                    {
                        MessageBox.Show("Cập nhật quyền thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Nút Mở Form Thêm Tài Khoản Mới
        private void BtnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            AddAccountDialogWindow addDialog = new AddAccountDialogWindow { Owner = this };

            if (addDialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // Nút Xóa Tài Khoản
        private void BtnDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int accountId)
            {
                MessageBoxResult ask = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa vĩnh viễn tài khoản này không?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (ask == MessageBoxResult.Yes)
                {
                    string? result = _controller.DeleteAccount(accountId);

                    if (result == null)
                    {
                        MessageBox.Show("Đã xóa tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
