using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbVoucher
{
    public int VoucherId { get; set; }

    public string Code { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MaxDiscountCap { get; set; }

    public decimal MinOrderValue { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int MaxUsageLimit { get; set; }

    public int PerCustomerLimit { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<TbOrder> TbOrders { get; set; } = new List<TbOrder>();

    public virtual ICollection<TbSavedVoucher> TbSavedVouchers { get; set; } = new List<TbSavedVoucher>();

    public virtual ICollection<TbVoucherUsage> TbVoucherUsages { get; set; } = new List<TbVoucherUsage>();
}
