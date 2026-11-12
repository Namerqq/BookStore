using System.Globalization;

namespace BookStore.Utilities
{
    /// <summary>Trạng thái đơn hàng và state machine ALLOWED_TRANSITIONS (mục 4.2.5).</summary>
    public static class OrderStatus
    {
        public const string PendingPayment = "PENDING_PAYMENT";
        public const string Pending = "PENDING";
        public const string Processing = "PROCESSING";
        public const string Shipped = "SHIPPED";
        public const string Delivered = "DELIVERED";
        public const string OnHold = "ON_HOLD";
        public const string Cancelled = "CANCELLED";

        public static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> ALLOWED_TRANSITIONS =
            new Dictionary<string, IReadOnlySet<string>>
            {
                [PendingPayment] = new HashSet<string> { Pending, Cancelled },
                [Pending] = new HashSet<string> { Processing, Cancelled },
                [Processing] = new HashSet<string> { Shipped, OnHold, Cancelled },
                [Shipped] = new HashSet<string> { Delivered, OnHold },
                [OnHold] = new HashSet<string> { Processing, Cancelled },
                [Delivered] = new HashSet<string>(),
                [Cancelled] = new HashSet<string>()
            };

        public static bool CanTransition(string current, string target)
        {
            return ALLOWED_TRANSITIONS.TryGetValue(current, out var next) && next.Contains(target);
        }

        public static string ToText(string? status) => status switch
        {
            PendingPayment => "Chờ thanh toán",
            Pending => "Chờ xác nhận",
            Processing => "Đang xử lý",
            Shipped => "Đang giao",
            Delivered => "Đã giao",
            OnHold => "Tạm giữ",
            Cancelled => "Đã hủy",
            _ => status ?? ""
        };
    }

    public static class PaymentState
    {
        public const string Unpaid = "UNPAID";
        public const string Paid = "PAID";
        public const string RefundPending = "REFUND_PENDING";
        public const string Refunded = "REFUNDED";

        public static string ToText(string? value) => value switch
        {
            Unpaid => "Chưa thanh toán",
            Paid => "Đã thanh toán",
            RefundPending => "Chờ hoàn tiền",
            Refunded => "Đã hoàn tiền",
            _ => value ?? ""
        };
    }

    public static class PaymentMethods
    {
        public const string Cod = "COD";
        public const string OnlineQr = "ONLINE_QR";

        public static string ToText(string? value) => value switch
        {
            Cod => "Thanh toán khi nhận hàng",
            OnlineQr => "Chuyển khoản VietQR",
            _ => value ?? ""
        };
    }

    public static class ReturnState
    {
        public const string Requested = "REQUESTED";
        public const string Completed = "COMPLETED";
        public const string Rejected = "REJECTED";

        public static string ToText(string? value) => value switch
        {
            Requested => "Đang chờ tiếp nhận",
            Completed => "Đã nhận lại hàng",
            Rejected => "Đã từ chối",
            _ => "Không có"
        };
    }

    public static class PhoneState
    {
        public const string Pending = "PENDING";
        public const string Confirmed = "CONFIRMED";
        public const string NotConfirmed = "NOT_CONFIRMED";

        /// <summary>Số lần liên tiếp không liên hệ được thì hệ thống tự hủy đơn.</summary>
        public const int MaxFailedAttempts = 3;

        public static string ToText(string? value) => value switch
        {
            Pending => "Chưa gọi",
            Confirmed => "Đã xác nhận",
            NotConfirmed => "Không liên hệ được",
            _ => value ?? ""
        };
    }

    public static class MoneyText
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        public static string Format(decimal value) => value.ToString("N0", Vi) + "đ";
    }
}
