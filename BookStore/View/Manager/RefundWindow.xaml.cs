using System.Windows;
using BookStore.DAO;
using BookStore.Utilities;
using BookStore.View.Customer;

namespace BookStore.View.Manager
{
    /// <summary>MNG-04: Manager xác nhận đã hoàn tiền thật cho khách.</summary>
    public partial class RefundWindow : Window
    {
        public RefundWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleManager)) return;
            Reload();
        }

        private void Reload()
        {
            UiGuard.Run(() =>
            {
                var rows = OrderDAO.GetRefundPending();
                dgRefunds.ItemsSource = rows;
                txtEmpty.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not OrderRow row) return;

            new OrderDetailWindow(row.OrderId) { Owner = this }.ShowDialog();
        }

        private void BtnRefund_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not OrderRow row) return;

            bool confirmed = UiGuard.Confirm(
                $"Xác nhận đã chuyển khoản hoàn {MoneyText.Format(row.Total)} cho khách của đơn {row.OrderCode}?\nThao tác này không thể hoàn tác.");
            if (!confirmed) return;

            UiGuard.Run(() => UiGuard.ShowResult(OrderDAO.ConfirmRefund(row.OrderId)));
            Reload();
        }
    }
}
