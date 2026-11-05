using System.Windows;
using System.Windows.Controls;
using BookStore.Controllers;
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

            // Bảo mật: Đảm bảo chỉ ADMIN mới được vào màn hình này
            if (GlobalSession.Role != "ADMIN")
            {
                MessageBox.Show("Chỉ ADMIN mới có quyền truy cập chức năng này!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }

            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            string role = "ALL";

            if (cmbRoleFilter.SelectedItem is ComboBoxItem selectedItem)
            {
                role = selectedItem.Tag.ToString();
            }

            dgAccounts.ItemsSource = _controller.GetAccounts(keyword, role);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(); // Tự động load lại khi gõ tìm kiếm
        }

        private void CmbRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAccounts != null) LoadData(); // Tự động load lại khi đổi bộ lọc
        }

        // ADM-01: Khóa/Mở khóa
        private void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int accountId)
            {
                MessageBoxResult ask = MessageBox.Show($"Bạn có chắc chắn muốn thay đổi trạng thái của tài khoản ID {accountId}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    string result = _controller.ToggleAccountStatus(accountId);
                    if (result == null)
                    {
                        LoadData(); // Cập nhật lại grid
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
                // Gọi Dialog chọn quyền
                RoleDialogWindow dialog = new RoleDialogWindow();
                dialog.Owner = this; // Đặt cửa sổ cha là cửa sổ hiện tại

                if (dialog.ShowDialog() == true)
                {
                    string newRole = dialog.SelectedRole;

                    // Gọi hàm cập nhật xuống Database
                    string result = _controller.UpdateAccountRole(accountId, newRole);

                    if (result == null)
                    {
                        MessageBox.Show("Cập nhật quyền thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData(); // Load lại bảng DataGrid
                    }
                    else
                    {
                        MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void BtnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm tài khoản nội bộ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}