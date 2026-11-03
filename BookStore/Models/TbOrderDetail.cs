using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbOrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductNameSnapshot { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal CostPriceSnapshot { get; set; }

    public virtual TbOrder Order { get; set; } = null!;

    public virtual TbProduct Product { get; set; } = null!;

    public virtual TbReview? TbReview { get; set; }
}
