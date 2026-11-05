using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbReview
{
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    public int AccountId { get; set; }

    public int OrderDetailId { get; set; }

    public byte Rating { get; set; }

    public string? Comment { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual TbAccount Account { get; set; } = null!;

    public virtual TbOrderDetail OrderDetail { get; set; } = null!;

    public virtual TbProduct Product { get; set; } = null!;
}
