using System.Windows;
using BookStore.DAO;
using BookStore.Models;
using BookStore.Utilities;

namespace BookStore.View.Customer
{
    /// <summary>CUS-03: sổ địa chỉ, dùng chung cho trang tài khoản và màn hình Checkout.</summary>
    public partial class AddressBookWindow : Window
    {
        private int _editingId;

        public AddressBookWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleCustomer)) return;
            Reload();
        }

        private void Reload()
        {
            UiGuard.Run(() => dgAddresses.ItemsSource = AddressDAO.GetMyAddresses());
        }

        private void ClearForm()
        {
            _editingId = 0;
            txtFormTitle.Text = "Thêm địa chỉ mới";
            txtName.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            chkDefault.IsChecked = false;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            bool setDefault = chkDefault.IsChecked == true;

            UiGuard.Run(() =>
            {
                string? error = _editingId == 0
                    ? AddressDAO.Add(txtName.Text, txtPhone.Text, txtAddress.Text, setDefault)
                    : AddressDAO.Update(_editingId, txtName.Text, txtPhone.Text, txtAddress.Text, setDefault);

                if (UiGuard.ShowResult(error))
                {
                    ClearForm();
                    Reload();
                }
            });
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not TbAddress address) return;

            _editingId = address.AddressId;
            txtFormTitle.Text = "Sửa địa chỉ";
            txtName.Text = address.ReceiverName;
            txtPhone.Text = address.ReceiverPhone;
            txtAddress.Text = address.FullAddress;
            chkDefault.IsChecked = address.IsDefault;
        }

        private void BtnSetDefault_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not TbAddress address) return;

            UiGuard.Run(() =>
            {
                UiGuard.ShowResult(AddressDAO.SetDefault(address.AddressId));
                Reload();
            });
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not TbAddress address) return;
            if (!UiGuard.Confirm($"Xóa địa chỉ của {address.ReceiverName}?")) return;

            UiGuard.Run(() =>
            {
                UiGuard.ShowResult(AddressDAO.Delete(address.AddressId));
                if (_editingId == address.AddressId) ClearForm();
                Reload();
            });
        }
    }
}
