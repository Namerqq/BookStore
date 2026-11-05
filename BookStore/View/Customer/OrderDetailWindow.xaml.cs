using System.Windows;
using BookStore.DAO;
using BookStore.Utilities;
using BookStore.View.Shared;

namespace BookStore.View.Customer
{
    /// <summary>CUS-06/07/08: chi tiết đơn, hủy đơn, yêu cầu hoàn hàng. Staff và Manager mở ở chế độ chỉ xem.</summary>
    public partial class OrderDetailWindow : Window
    {
        private readonly int _orderId;
        private OrderRow? _order;

        public OrderDetailWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void Reload()
        {
            UiGuard.Run(() =>
            {
                OrderDetailData? data = OrderDAO.GetOrderDetail(_orderId);
                if (data == null)
                {
                    UiGuard.ShowResult("Không tìm thấy đơn hàng.");
                    Close();
                    return;
                }

                _order = data.Order;
                Render(data);
            });
        }

        private void Render(OrderDetailData data)
        {
            OrderRow o = data.Order;

            txtCode.Text = o.OrderCode;
            txtCreatedAt.Text = $"Đặt lúc {o.CreatedAt:dd/MM/yyyy HH:mm}";
            txtStatus.Text = o.StatusText;
            txtReceiver.Text = $"{o.ReceiverName} - {o.ReceiverPhone}";
            txtAddress.Text = o.ShippingAddress;
            txtPayment.Text = o.PaymentMethodText;
            txtPaymentStatus.Text = o.PaymentStatusText;

            dgLines.ItemsSource = data.Lines;

            txtSubtotal.Text = MoneyText.Format(o.Subtotal);
            txtShipping.Text = MoneyText.Format(o.ShippingFee);
            txtDiscountLabel.Text = o.VoucherCode == null ? "Giảm giá" : $"Giảm giá ({o.VoucherCode})";
            txtDiscount.Text = "-" + MoneyText.Format(o.DiscountAmount);
            txtTotal.Text = MoneyText.Format(o.Total);

            string notice = BuildNotice(o);
            txtNotice.Text = notice;
            bdNotice.Visibility = notice.Length == 0 ? Visibility.Collapsed : Visibility.Visible;

            bool isCustomer = SessionGuard.IsCustomer;
            ShowIf(btnCancel, isCustomer && o.Status == OrderStatus.Pending
                              && OrderStatus.CanTransition(o.Status, OrderStatus.Cancelled));
            ShowIf(btnReturn, isCustomer && o.Status == OrderStatus.Delivered && o.ReturnStatus == null);
            ShowIf(btnShowQr, isCustomer && o.Status == OrderStatus.PendingPayment);
        }

        private static string BuildNotice(OrderRow o)
        {
            var parts = new List<string>();

            if (o.Status == OrderStatus.PendingPayment)
                parts.Add("Đơn đang chờ nhân viên xác nhận đã nhận tiền. Nếu cần hủy, vui lòng liên hệ cửa hàng.");
            if (o.Status == OrderStatus.OnHold)
                parts.Add("Đơn đang tạm giữ do chưa giao được. Cửa hàng sẽ liên hệ lại với bạn.");
            if (!string.IsNullOrEmpty(o.CancelReason))
                parts.Add($"Lý do hủy: {o.CancelReason}");
            if (o.ReturnStatus != null)
                parts.Add($"Hoàn hàng: {o.ReturnStatusText}. Lý do: {o.ReturnReason}");
            if (o.PaymentStatus == PaymentState.RefundPending)
                parts.Add("Cửa hàng đang xử lý hoàn tiền cho bạn.");
            if (o.PaymentStatus == PaymentState.Refunded)
                parts.Add("Cửa hàng đã hoàn tiền cho đơn này.");

            return string.Join("\n", parts);
        }

        private static void ShowIf(UIElement element, bool condition)
        {
            element.Visibility = condition ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            string? reason = ReasonDialog.Ask(this, "Hủy đơn hàng", "Lý do hủy đơn (thao tác này không thể hoàn tác):");
            if (reason == null) return;

            UiGuard.Run(() => UiGuard.ShowResult(OrderDAO.CancelByCustomer(_orderId, reason), "Đã hủy đơn hàng."));
            Reload();
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            string? reason = ReasonDialog.Ask(this, "Yêu cầu hoàn hàng", "Lý do hoàn hàng (áp dụng cho toàn bộ đơn, mỗi đơn gửi được một lần):", 500);
            if (reason == null) return;

            UiGuard.Run(() => UiGuard.ShowResult(OrderDAO.RequestReturn(_orderId, reason), "Đã gửi yêu cầu hoàn hàng."));
            Reload();
        }

        private void BtnShowQr_Click(object sender, RoutedEventArgs e)
        {
            if (_order == null) return;

            new OrderSuccessWindow(_order.OrderId, _order.OrderCode, _order.Total, _order.PaymentMethod, false) { Owner = this }.ShowDialog();
        }

        private void BtnReview_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not OrderLineRow line) return;

            new ReviewWindow(line.OrderDetailId) { Owner = this }.ShowDialog();
            Reload();
        }
    }
}
