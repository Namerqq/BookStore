using System.Windows;
using System.Windows.Controls;
using BookStore.DAO;
using BookStore.Models;
using BookStore.Utilities;

namespace BookStore.View.Customer
{
    /// <summary>CUS-05: đặt hàng. Số tiền hiển thị chỉ để xem; CheckoutDAO tính lại toàn bộ khi lưu.</summary>
    public partial class CheckoutWindow : Window
    {
        private string? _appliedVoucher;

        public bool OrderPlaced { get; private set; }

        public CheckoutWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleCustomer)) return;

            LoadAddresses(null);
            UiGuard.Run(() => cboSaved.ItemsSource = SavedVoucherDAO.GetUsableSaved());
            RefreshQuote();
        }

        private void LoadAddresses(int? keepAddressId)
        {
            UiGuard.Run(() =>
            {
                List<TbAddress> addresses = AddressDAO.GetMyAddresses();
                cboAddress.ItemsSource = addresses;

                var selected = addresses.FirstOrDefault(a => a.AddressId == keepAddressId)
                               ?? addresses.FirstOrDefault(a => a.IsDefault)
                               ?? addresses.FirstOrDefault();
                cboAddress.SelectedItem = selected;
                txtNoAddress.Visibility = addresses.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void RefreshQuote()
        {
            UiGuard.Run(() =>
            {
                CheckoutQuote quote = CheckoutDAO.GetQuote(_appliedVoucher);

                icLines.ItemsSource = quote.Lines;
                txtSubtotal.Text = MoneyText.Format(quote.Subtotal);
                txtShippingLabel.Text = $"Phí vận chuyển ({quote.TotalGrams / 1000m:0.##} kg)";
                txtShipping.Text = MoneyText.Format(quote.ShippingFee);
                txtDiscount.Text = "-" + MoneyText.Format(quote.Discount);
                txtTotal.Text = MoneyText.Format(quote.Total);

                if (quote.VoucherError != null)
                {
                    _appliedVoucher = null;
                    txtVoucherMessage.Text = quote.VoucherError;
                    txtVoucherMessage.Foreground = System.Windows.Media.Brushes.Firebrick;
                    btnRemoveVoucher.Visibility = Visibility.Collapsed;
                }
                else if (quote.AppliedVoucherCode != null)
                {
                    txtVoucherMessage.Text = $"Đã áp dụng mã {quote.AppliedVoucherCode}.";
                    txtVoucherMessage.Foreground = System.Windows.Media.Brushes.DarkOliveGreen;
                    btnRemoveVoucher.Visibility = Visibility.Visible;
                }
                else
                {
                    txtVoucherMessage.Text = string.Empty;
                    btnRemoveVoucher.Visibility = Visibility.Collapsed;
                }

                txtError.Text = quote.Error ?? string.Empty;
                txtError.Visibility = quote.Error == null ? Visibility.Collapsed : Visibility.Visible;
                btnPlaceOrder.IsEnabled = quote.Error == null;
            });
        }

        private void BtnManageAddress_Click(object sender, RoutedEventArgs e)
        {
            int? current = (cboAddress.SelectedItem as TbAddress)?.AddressId;
            new AddressBookWindow { Owner = this }.ShowDialog();
            LoadAddresses(current);
        }

        private void CboSaved_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || cboSaved.SelectedItem is not SavedVoucherOption option) return;

            txtVoucher.Text = option.Code;
            _appliedVoucher = option.Code;
            RefreshQuote();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            string code = txtVoucher.Text.Trim();
            if (code.Length == 0)
            {
                txtVoucherMessage.Text = "Vui lòng nhập mã giảm giá.";
                txtVoucherMessage.Foreground = System.Windows.Media.Brushes.Firebrick;
                return;
            }

            _appliedVoucher = code;
            RefreshQuote();
        }

        private void BtnRemoveVoucher_Click(object sender, RoutedEventArgs e)
        {
            _appliedVoucher = null;
            txtVoucher.Clear();
            cboSaved.SelectedIndex = -1;
            RefreshQuote();
        }

        private void BtnPlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cboAddress.SelectedItem is not TbAddress address)
            {
                UiGuard.ShowResult("Vui lòng chọn địa chỉ giao hàng.");
                return;
            }

            string method = rbQr.IsChecked == true ? PaymentMethods.OnlineQr : PaymentMethods.Cod;

            UiGuard.Run(() =>
            {
                PlaceOrderResult result = CheckoutDAO.PlaceOrder(address.AddressId, method, _appliedVoucher);
                if (result.Error != null)
                {
                    UiGuard.ShowResult(result.Error);
                    RefreshQuote();
                    return;
                }

                OrderPlaced = true;
                new OrderSuccessWindow(result.OrderId, result.OrderCode, result.Total, result.PaymentMethod) { Owner = this }.ShowDialog();
                Close();
            });
        }
    }
}
