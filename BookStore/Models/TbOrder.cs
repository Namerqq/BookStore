using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbOrder
{
    public int OrderId { get; set; }

    public string OrderCode { get; set; } = null!;

    public int AccountId { get; set; }

    public string Status { get; set; } = null!;

    public string ReceiverName { get; set; } = null!;

    public string ReceiverPhone { get; set; } = null!;

    public string ShippingAddress { get; set; } = null!;

    public decimal DiscountAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public int? VoucherId { get; set; }

    public string? CancelReason { get; set; }

    public string? ReturnStatus { get; set; }

    public DateTime? ReturnRequestedAt { get; set; }

    public string? ReturnReason { get; set; }

    public string PhoneConfirmStatus { get; set; } = null!;

    public int PhoneAttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TbAccount Account { get; set; } = null!;

    public virtual ICollection<TbOrderDetail> TbOrderDetails { get; set; } = new List<TbOrderDetail>();

    public virtual TbVoucherUsage? TbVoucherUsage { get; set; }

    public virtual TbVoucher? Voucher { get; set; }
}
