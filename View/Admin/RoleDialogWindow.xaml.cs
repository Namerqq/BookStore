using System.Windows;
using System.Windows.Controls;

namespace BookStore.View.Admin
{
    public partial class RoleDialogWindow : Window
    {
        public string SelectedRole { get; private set; } = string.Empty;

        public RoleDialogWindow()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRoles.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                SelectedRole = selectedItem.Tag.ToString() ?? string.Empty;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một quyền!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
