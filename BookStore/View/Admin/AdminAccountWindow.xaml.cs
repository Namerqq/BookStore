using System.Windows;
using System.Windows.Controls;
using BookStore.Controllers;

namespace BookStore.View.Admin
{
    public partial class AdminAccountWindow : Window
    {
        private readonly AccountController _controller;

        public AdminAccountWindow()
        {
            InitializeComponent();
            _controller = new AccountController();

            // Gọi hàm lấy dữ liệu đổ lên DataGrid ngay khi mở cửa sổ
            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            string role = "";

            if (cboRoleFilter.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                role = selectedItem.Tag.ToString();
            }

            // Lấy List<TbAccount> từ Controller và gán vào DataGrid
            dgAccounts.ItemsSource = _controller.GetAccounts(keyword, role);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sẽ làm Window thêm tài khoản ở bước tiếp theo.", "Thông báo");
            // new View.Admin.AddAccountWindow().ShowDialog();
            // LoadData();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int accountId)
            {
                MessageBox.Show($"Sẽ làm Window sửa cho tài khoản ID: {accountId}", "Thông báo");
                // new View.Admin.EditAccountWindow(accountId).ShowDialog();
                // LoadData();
            }
        }

        private void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int accountId)
            {
                var confirm = MessageBox.Show("Bạn có chắc muốn đổi trạng thái (Khóa/Mở) tài khoản này?",
                                              "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm == MessageBoxResult.Yes)
                {
                    bool success = _controller.ToggleAccountStatus(accountId);
                    if (success)
                    {
                        LoadData(); // Load lại bảng sau khi update
                    }
                    else
                    {
                        MessageBox.Show("Không thể thay đổi trạng thái tài khoản ADMIN.", "Lỗi phân quyền",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}