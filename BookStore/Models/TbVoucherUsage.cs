using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbVoucherUsage
{
    public int UsageId { get; set; }

    public int VoucherId { get; set; }

    public int AccountId { get; set; }

    public int OrderId { get; set; }

    public DateTime UsedAt { get; set; }

    public virtual TbAccount Account { get; set; } = null!;

    public virtual TbOrder Order { get; set; } = null!;

    public virtual TbVoucher Voucher { get; set; } = null!;
}
