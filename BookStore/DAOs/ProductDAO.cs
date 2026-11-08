using BookStore.Helpers;
using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BookStore.DAOs
{
    public class ProductDAO
    {
        private static ProductDAO? _instance;
        public static ProductDAO Instance => _instance ??= new ProductDAO();

        // Kiểm tra SKU trùng
        public bool IsSkuExists(string sku, int currentProductId = 0)
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbProducts.Any(p => p.Sku == sku && p.ProductId != currentProductId);
        }

        // Tìm kiếm và lọc sản phẩm (GUE-01, GUE-02)
        public List<TbProduct> GetProducts(string? keyword, int? categoryId, string? productType, string? sortBy)
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbProducts
                .Include(p => p.Category)
                .Include(p => p.TbProductImages)
                .Where(p => p.Status == "ACTIVE");

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(kw) ||
                                         (p.Author != null && p.Author.ToLower().Contains(kw)) ||
                                         p.Sku.ToLower().Contains(kw));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(productType) && productType != "ALL")
            {
                query = query.Where(p => p.ProductType == productType);
            }

            query = sortBy switch
            {
                "PRICE_ASC" => query.OrderBy(p => p.Price),
                "PRICE_DESC" => query.OrderByDescending(p => p.Price),
                "NEWEST" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            return query.ToList();
        }

        // Thêm sản phẩm mới (MNG-02) - Giá VAT tự động x1.1
        public void AddProduct(TbProduct product, string? sourceImagePath)
        {
            using var context = new DbSe2038BookymartFContext();

            product.Price = Math.Round(product.Price * 1.1m, 2);
            product.CreatedAt = DateTime.Now;

            context.TbProducts.Add(product);
            context.SaveChanges();

            if (!string.IsNullOrEmpty(sourceImagePath) && File.Exists(sourceImagePath))
            {
                string localImagePath = ImageHelper.SaveProductImage(sourceImagePath, product.ProductId);
                context.TbProductImages.Add(new TbProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl = localImagePath,
                    DisplayOrder = 0,
                    IsThumbnail = true
                });
                context.SaveChanges();
            }

            // Ghi log hệ thống
            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = GlobalSession.AccountId,
                Action = $"Thêm sản phẩm mới: {product.Name} (SKU: {product.Sku})",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }

        // Cập nhật thông tin sản phẩm
        public void UpdateProduct(TbProduct product, string? newImagePath = null)
        {
            using var context = new DbSe2038BookymartFContext();
            var existing = context.TbProducts.FirstOrDefault(p => p.ProductId == product.ProductId);
            if (existing == null) return;

            existing.Sku = product.Sku;
            existing.Name = product.Name;
            existing.Price = Math.Round(product.Price * 1.1m, 2);
            existing.UnitCost = product.UnitCost;
            existing.StockQuantity = product.StockQuantity;
            existing.CategoryId = product.CategoryId;
            existing.ProductType = product.ProductType;

            if (!string.IsNullOrEmpty(newImagePath) && File.Exists(newImagePath))
            {
                string localPath = ImageHelper.SaveProductImage(newImagePath, existing.ProductId);
                var existingImg = context.TbProductImages.FirstOrDefault(i => i.ProductId == existing.ProductId && i.IsThumbnail);
                if (existingImg != null)
                {
                    existingImg.ImageUrl = localPath;
                }
                else
                {
                    context.TbProductImages.Add(new TbProductImage
                    {
                        ProductId = existing.ProductId,
                        ImageUrl = localPath,
                        DisplayOrder = 0,
                        IsThumbnail = true
                    });
                }
            }

            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = GlobalSession.AccountId,
                Action = $"Cập nhật sản phẩm ID {product.ProductId}: {product.Name}",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();
        }

        // Ngừng kinh doanh sản phẩm
        public void DiscontinueProduct(int productId)
        {
            using var context = new DbSe2038BookymartFContext();
            var prod = context.TbProducts.FirstOrDefault(p => p.ProductId == productId);
            if (prod != null)
            {
                prod.Status = "DISCONTINUED";
                context.TbActivityLogs.Add(new TbActivityLog
                {
                    AccountId = GlobalSession.AccountId,
                    Action = $"Ngừng kinh doanh sản phẩm ID {productId}",
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
            }
        }
    }
}