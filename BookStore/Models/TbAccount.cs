using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbAccount
{
    public int AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool EmailVerified { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTime? LockedUntil { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TbActivityLog> TbActivityLogs { get; set; } = new List<TbActivityLog>();

    public virtual TbAddress? TbAddress { get; set; }

    public virtual TbCart? TbCart { get; set; }

    public virtual ICollection<TbOrder> TbOrders { get; set; } = new List<TbOrder>();

    public virtual ICollection<TbReview> TbReviews { get; set; } = new List<TbReview>();

    public virtual ICollection<TbSavedVoucher> TbSavedVouchers { get; set; } = new List<TbSavedVoucher>();

    public virtual ICollection<TbVoucherUsage> TbVoucherUsages { get; set; } = new List<TbVoucherUsage>();
}
