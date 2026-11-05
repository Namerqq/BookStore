using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbCartItem
{
    public int CartItemId { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public bool IsSelected { get; set; }

    public virtual TbCart Cart { get; set; } = null!;

    public virtual TbProduct Product { get; set; } = null!;
}
