using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbSavedVoucher
{
    public int AccountId { get; set; }

    public int VoucherId { get; set; }

    public DateTime SavedAt { get; set; }

    public virtual TbAccount Account { get; set; } = null!;

    public virtual TbVoucher Voucher { get; set; } = null!;
}
