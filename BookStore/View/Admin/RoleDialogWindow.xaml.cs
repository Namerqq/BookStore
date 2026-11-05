using System.Windows;
using System.Windows.Controls;

namespace BookStore.View.Admin
{
    public partial class RoleDialogWindow : Window
    {
        public string SelectedRole { get; private set; }

        public RoleDialogWindow()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRoles.SelectedItem is ComboBoxItem selectedItem)
            {
                SelectedRole = selectedItem.Tag.ToString();
                this.DialogResult = true; // Trả về true báo hiệu đã lưu
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một quyền!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Trả về false báo hiệu hủy
        }
    }
}