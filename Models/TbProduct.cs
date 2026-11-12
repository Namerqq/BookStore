using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BookStore.Models;

public partial class TbProduct
{
    public int ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal UnitCost { get; set; }

    public int StockQuantity { get; set; }

    public int? WeightGrams { get; set; }

    public int CategoryId { get; set; }

    public string ProductType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Isbn { get; set; }

    public string? Author { get; set; }

    public string? Publisher { get; set; }

    public int? PublicationYear { get; set; }

    public string? Brand { get; set; }

    public string? Specifications { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TbCategory Category { get; set; } = null!;

    public virtual ICollection<TbCartItem> TbCartItems { get; set; } = new List<TbCartItem>();

    public virtual ICollection<TbOrderDetail> TbOrderDetails { get; set; } = new List<TbOrderDetail>();

    public virtual ICollection<TbProductImage> TbProductImages { get; set; } = new List<TbProductImage>();

    public virtual ICollection<TbReview> TbReviews { get; set; } = new List<TbReview>();

    /// <summary>
    /// URL ảnh đại diện của sản phẩm (ưu tiên ảnh thumbnail và thứ tự hiển thị).
    /// </summary>
    [NotMapped]
    public string? ThumbnailUrl
    {
        get
        {
            var url = TbProductImages?
                .OrderByDescending(img => img.IsThumbnail)
                .ThenBy(img => img.DisplayOrder)
                .Select(img => img.ImageUrl)
                .FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));

            return BookStore.Helpers.ImageHelper.ResolveImagePath(url);
        }
    }
}
