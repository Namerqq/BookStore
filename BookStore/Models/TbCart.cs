using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbCart
{
    public int CartId { get; set; }

    public int AccountId { get; set; }

    public virtual TbAccount Account { get; set; } = null!;

    public virtual ICollection<TbCartItem> TbCartItems { get; set; } = new List<TbCartItem>();
}
