using System;
using System.Collections.Generic;

namespace BookStore.Models;

public partial class TbAddress
{
    public int AddressId { get; set; }

    public int AccountId { get; set; }

    public string ReceiverName { get; set; } = null!;

    public string ReceiverPhone { get; set; } = null!;

    public string FullAddress { get; set; } = null!;

    public bool IsDefault { get; set; }

    public virtual TbAccount Account { get; set; } = null!;
}
