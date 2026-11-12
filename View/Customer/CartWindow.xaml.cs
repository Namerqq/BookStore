using System.Windows;
using System.Windows.Controls;
using BookStore.DAO;
using BookStore.Utilities;

namespace BookStore.View.Customer
{
    /// <summary>CUS-04: giỏ hàng.</summary>
    public partial class CartWindow : Window
    {
        public CartWindow()
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
            UiGuard.Run(() =>
            {
                var lines = CartDAO.GetCartLines();
                dgCart.ItemsSource = lines;

                var selected = lines.Where(l => l.IsSelected).ToList();
                txtEmpty.Visibility = lines.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                chkAll.IsChecked = lines.Count > 0 && selected.Count == lines.Count;
                txtSelectedCount.Text = $"Đã chọn {selected.Count}/{lines.Count} sản phẩm";
                txtSubtotal.Text = MoneyText.Format(selected.Sum(l => l.LineTotal));
                btnCheckout.IsEnabled = selected.Count > 0;
            });
        }

        private static CartLine? LineOf(object sender) => (sender as FrameworkElement)?.DataContext as CartLine;

        private void RunAndReload(Func<string?> action)
        {
            UiGuard.Run(() => UiGuard.ShowResult(action()));
            Reload();
        }

        private void ChkAll_Click(object sender, RoutedEventArgs e)
        {
            bool isSelected = chkAll.IsChecked == true;
            RunAndReload(() => CartDAO.SetAllSelected(isSelected));
        }

        private void ChkItem_Click(object sender, RoutedEventArgs e)
        {
            var line = LineOf(sender);
            if (line == null) return;

            bool isSelected = (sender as CheckBox)?.IsChecked == true;
            RunAndReload(() => CartDAO.SetSelected(line.CartItemId, isSelected));
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            var line = LineOf(sender);
            if (line == null) return;

            if (line.Quantity <= 1)
            {
                if (UiGuard.Confirm($"Xóa “{line.Name}” khỏi giỏ hàng?"))
                {
                    RunAndReload(() => CartDAO.RemoveItem(line.CartItemId));
                }
                return;
            }
            RunAndReload(() => CartDAO.UpdateQuantity(line.CartItemId, line.Quantity - 1));
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            var line = LineOf(sender);
            if (line == null) return;

            RunAndReload(() => CartDAO.UpdateQuantity(line.CartItemId, line.Quantity + 1));
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var line = LineOf(sender);
            if (line == null) return;
            if (!UiGuard.Confirm($"Xóa “{line.Name}” khỏi giỏ hàng?")) return;

            RunAndReload(() => CartDAO.RemoveItem(line.CartItemId));
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            var checkout = new CheckoutWindow { Owner = this };
            checkout.ShowDialog();

            if (checkout.OrderPlaced)
            {
                Close();
                return;
            }
            Reload();
        }
    }
}
