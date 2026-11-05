using BookStore.Utilities;

namespace BookStore.DAO
{
    public class CartLine
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public int WeightGrams { get; set; }
        public bool IsSelected { get; set; }
        public string ProductStatus { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
        public bool IsAvailable => ProductStatus == "ACTIVE" && Stock >= Quantity;

        public string Warning
        {
            get
            {
                if (ProductStatus != "ACTIVE") return "Ngừng kinh doanh";
                if (Stock <= 0) return "Hết hàng";
                if (Stock < Quantity) return $"Chỉ còn {Stock} sản phẩm";
                return string.Empty;
            }
        }
    }

    public class SavedVoucherOption
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Display => $"{Code} ({Description})";
    }

    public class CheckoutQuote
    {
        public List<CartLine> Lines { get; set; } = new();
        public decimal Subtotal { get; set; }
        public int TotalGrams { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public string? AppliedVoucherCode { get; set; }

        /// <summary>Voucher không dùng được, nhưng vẫn đặt được đơn nếu bỏ voucher.</summary>
        public string? VoucherError { get; set; }

        /// <summary>Lỗi chặn đặt hàng, ví dụ giỏ trống hoặc thiếu hàng.</summary>
        public string? Error { get; set; }

        public decimal Total => Subtotal - Discount + ShippingFee;
    }

    public class PlaceOrderResult
    {
        public string? Error { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class OrderRow
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int AccountId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public string? VoucherCode { get; set; }
        public string? CancelReason { get; set; }
        public string? ReturnStatus { get; set; }
        public string? ReturnReason { get; set; }
        public DateTime? ReturnRequestedAt { get; set; }
        public string PhoneConfirmStatus { get; set; } = string.Empty;
        public int PhoneAttemptCount { get; set; }

        public decimal Total => Subtotal - DiscountAmount + ShippingFee;
        public string StatusText => OrderStatus.ToText(Status);
        public string PaymentStatusText => PaymentState.ToText(PaymentStatus);
        public string PaymentMethodText => PaymentMethods.ToText(PaymentMethod);
        public string ReturnStatusText => ReturnState.ToText(ReturnStatus);
        public string PhoneConfirmText => PhoneState.ToText(PhoneConfirmStatus);

        public string RefundReason => Status == OrderStatus.Cancelled
            ? $"Hủy đơn: {CancelReason}"
            : $"Hoàn hàng: {ReturnReason}";
    }

    public class OrderLineRow
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool HasReview { get; set; }
        public bool CanReview { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class OrderDetailData
    {
        public OrderRow Order { get; set; } = new();
        public List<OrderLineRow> Lines { get; set; } = new();
    }
}
