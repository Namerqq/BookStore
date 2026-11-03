using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbCategory
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<TbProduct> TbProducts { get; set; } = new List<TbProduct>();
}
