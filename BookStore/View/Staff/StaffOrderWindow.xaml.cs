using System.Windows;
using System.Windows.Controls;
using BookStore.DAO;
using BookStore.Utilities;
using BookStore.View.Shared;

namespace BookStore.View.Staff
{
    /// <summary>
    /// STF-01 đến STF-05. Nút thao tác hiện theo OrderStatus.ALLOWED_TRANSITIONS;
    /// OrderDAO vẫn kiểm tra lại độc lập khi thực hiện.
    /// </summary>
    public partial class StaffOrderWindow : Window
    {
        private int _currentId;

        public StaffOrderWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleStaff)) return;
            ReloadList();
        }

        private void ReloadList()
        {
            string? filter = (cboFilter.SelectedItem as ComboBoxItem)?.Tag as string;
            string keyword = txtSearch.Text;

            UiGuard.Run(() =>
            {
                var rows = OrderDAO.GetOrdersForStaff(filter, keyword);
                dgOrders.ItemsSource = rows;
                dgOrders.SelectedItem = rows.FirstOrDefault(r => r.OrderId == _currentId);
            });
        }

        private void ShowDetail(int orderId)
        {
            UiGuard.Run(() =>
            {
                OrderDetailData? data = OrderDAO.GetOrderDetail(orderId);
                if (data == null)
                {
                    _currentId = 0;
                    pnlDetail.Visibility = Visibility.Collapsed;
                    txtPlaceholder.Visibility = Visibility.Visible;
                    return;
                }

                _currentId = orderId;
                Render(data);
                pnlDetail.Visibility = Visibility.Visible;
                txtPlaceholder.Visibility = Visibility.Collapsed;
            });
        }

        private void Render(OrderDetailData data)
        {
            OrderRow o = data.Order;

            txtCode.Text = o.OrderCode;
            txtStatusLine.Text = o.StatusText;
            txtReceiver.Text = $"{o.ReceiverName} - {o.ReceiverPhone}\n{o.ShippingAddress}";
            txtPayment.Text = $"{o.PaymentMethodText}\n{o.PaymentStatusText}, tổng {MoneyText.Format(o.Total)}";
            txtPhone.Text = o.PhoneAttemptCount > 0
                ? $"{o.PhoneConfirmText} ({o.PhoneAttemptCount}/{PhoneState.MaxFailedAttempts} lần gọi không được)"
                : o.PhoneConfirmText;
            icLines.ItemsSource = data.Lines;

            var notes = new List<string>();
            if (!string.IsNullOrEmpty(o.CancelReason)) notes.Add($"Lý do hủy: {o.CancelReason}");
            if (o.ReturnStatus != null) notes.Add($"Hoàn hàng: {o.ReturnStatusText}. Lý do: {o.ReturnReason}");
            txtNotice.Text = string.Join("\n", notes);
            bdNotice.Visibility = notes.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            bool phoneDone = o.PhoneConfirmStatus == PhoneState.Confirmed;
            bool isProcessing = o.Status == OrderStatus.Processing;
            bool returnWaiting = o.ReturnStatus == ReturnState.Requested;

            ShowIf(btnConfirmPayment, o.Status == OrderStatus.PendingPayment
                                      && OrderStatus.CanTransition(o.Status, OrderStatus.Pending));
            ShowIf(btnConfirmOrder, o.Status == OrderStatus.Pending
                                    && OrderStatus.CanTransition(o.Status, OrderStatus.Processing));
            ShowIf(btnResume, o.Status == OrderStatus.OnHold
                              && OrderStatus.CanTransition(o.Status, OrderStatus.Processing));
            ShowIf(btnPhoneOk, isProcessing && !phoneDone);
            ShowIf(btnPhoneFail, isProcessing && !phoneDone);
            ShowIf(btnShip, OrderStatus.CanTransition(o.Status, OrderStatus.Shipped));
            btnShip.IsEnabled = phoneDone;
            ShowIf(btnDelivered, OrderStatus.CanTransition(o.Status, OrderStatus.Delivered));
            ShowIf(btnFailed, OrderStatus.CanTransition(o.Status, OrderStatus.OnHold));
            ShowIf(btnCancel, OrderStatus.CanTransition(o.Status, OrderStatus.Cancelled));
            ShowIf(btnReturnOk, returnWaiting);
            ShowIf(btnReturnReject, returnWaiting);

            txtActionHint.Text = isProcessing && !phoneDone
                ? "Cần gọi xác nhận khách trước khi chuyển giao hàng."
                : string.Empty;
        }

        private static void ShowIf(UIElement element, bool condition)
        {
            element.Visibility = condition ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RunAction(Func<int, string?> action, string? confirm = null, string? success = null)
        {
            int id = _currentId;
            if (id == 0) return;
            if (confirm != null && !UiGuard.Confirm(confirm)) return;

            UiGuard.Run(() =>
            {
                UiGuard.ShowResult(action(id), success);
                ReloadList();
                ShowDetail(id);
            });
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded) ReloadList();
        }

        private void CboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ReloadList();
        }

        private void DgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Đơn có thể rời khỏi bộ lọc sau khi xử lý: giữ nguyên khung chi tiết đang mở.
            if (dgOrders.SelectedItem is OrderRow row && row.OrderId != _currentId)
            {
                ShowDetail(row.OrderId);
            }
        }

        private void BtnConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            RunAction(OrderDAO.ConfirmPayment, "Xác nhận đã đối chiếu sao kê và nhận đủ tiền cho đơn này?");
        }

        private void BtnConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            RunAction(OrderDAO.ConfirmOrder);
        }

        private void BtnResume_Click(object sender, RoutedEventArgs e)
        {
            RunAction(OrderDAO.ResumeOrder, "Xử lý lại đơn này? Bạn sẽ cần gọi xác nhận khách lại từ đầu.");
        }

        private void BtnPhoneOk_Click(object sender, RoutedEventArgs e)
        {
            RunAction(OrderDAO.ConfirmPhone);
        }

        private void BtnPhoneFail_Click(object sender, RoutedEventArgs e)
        {
            RunAction(OrderDAO.MarkPhoneNotReached,
                $"Ghi nhận không liên hệ được khách? Đủ {PhoneState.MaxFailedAttempts} lần liên tiếp, đơn sẽ tự động bị hủy.");
        }

        private void BtnShip_Click(object sender, RoutedEventArgs e)
        {
            RunAction(OrderDAO.ShipOrder, "Chuyển đơn này sang đang giao?");
        }

        private void BtnDelivered_Click(object sender, RoutedEventArgs e)
        {
            RunAction(OrderDAO.MarkDelivered, "Xác nhận đơn đã giao thành công?");
        }

        private void BtnFailed_Click(object sender, RoutedEventArgs e)
        {
            string? reason = ReasonDialog.Ask(this, "Giao thất bại", "Lý do tạm giữ đơn:");
            if (reason == null) return;

            RunAction(id => OrderDAO.MarkDeliveryFailed(id, reason));
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            string? reason = ReasonDialog.Ask(this, "Hủy đơn hàng", "Lý do hủy đơn (thao tác này không thể hoàn tác):");
            if (reason == null) return;

            RunAction(id => OrderDAO.CancelByStaff(id, reason));
        }

        private void BtnReturnOk_Click(object sender, RoutedEventArgs e)
        {
            RunAction(OrderDAO.ConfirmReturnReceived,
                "Xác nhận đã nhận lại hàng? Tồn kho sẽ được cộng lại và đơn chuyển sang chờ Manager hoàn tiền.");
        }

        private void BtnReturnReject_Click(object sender, RoutedEventArgs e)
        {
            string? reason = ReasonDialog.Ask(this, "Từ chối hoàn hàng", "Lý do từ chối (thao tác này không thể hoàn tác):");
            if (reason == null) return;

            RunAction(id => OrderDAO.RejectReturn(id, reason));
        }
    }
}
