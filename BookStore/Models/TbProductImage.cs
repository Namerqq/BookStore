using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbProductImage
{
    public int ImageId { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public bool IsThumbnail { get; set; }

    public virtual TbProduct Product { get; set; } = null!;
}
