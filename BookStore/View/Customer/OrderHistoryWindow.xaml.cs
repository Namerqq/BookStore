using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookStore.DAO;
using BookStore.Utilities;

namespace BookStore.View.Customer
{
    /// <summary>CUS-06: lịch sử đơn hàng.</summary>
    public partial class OrderHistoryWindow : Window
    {
        public OrderHistoryWindow()
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
            string? status = (cboStatus.SelectedItem as ComboBoxItem)?.Tag as string;

            UiGuard.Run(() =>
            {
                var orders = OrderDAO.GetMyOrders(status);
                dgOrders.ItemsSource = orders;
                txtEmpty.Visibility = orders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void OpenDetail(OrderRow? row)
        {
            if (row == null) return;

            new OrderDetailWindow(row.OrderId) { Owner = this }.ShowDialog();
            Reload();
        }

        private void CboStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) Reload();
        }

        private void DgOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenDetail(dgOrders.SelectedItem as OrderRow);
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            OpenDetail((sender as FrameworkElement)?.DataContext as OrderRow);
        }
    }
}
