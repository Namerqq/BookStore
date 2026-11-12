using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BookStore.Helpers;
using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DAO
{
    public class ProductDAO
    {
        private static ProductDAO? _instance;
        public static ProductDAO Instance => _instance ??= new ProductDAO();

        // Kiểm tra SKU trùng
        public bool IsSkuExists(string sku, int currentProductId = 0)
        {
            string normalized = (sku ?? string.Empty).Trim();
            if (normalized.Length == 0) return false;

            using var context = new DbSe2038BookymartFContext();
            return context.TbProducts.Any(p => p.Sku.ToLower() == normalized.ToLower() && p.ProductId != currentProductId);
        }

        // Tìm kiếm và lọc sản phẩm cho khách (GUE-01, GUE-02)
        public List<TbProduct> GetProducts(string? keyword, int? categoryId, string? productType, string? sortBy)
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbProducts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.TbProductImages)
                .Where(p => p.Status == "ACTIVE");

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(kw)
                                         || (p.Author != null && p.Author.ToLower().Contains(kw))
                                         || (p.Brand != null && p.Brand.ToLower().Contains(kw))
                                         || p.Sku.ToLower().Contains(kw));
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

        // Lấy toàn bộ sản phẩm (bao gồm DISCONTINUED) cho Manager (MNG-02)
        public List<TbProduct> GetAllProductsForManager(string? keyword = null)
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbProducts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.TbProductImages)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(kw) || p.Sku.ToLower().Contains(kw));
            }

            return query.OrderByDescending(p => p.ProductId).ToList();
        }

        // Lấy chi tiết 1 sản phẩm (GUE-03)
        public TbProduct? GetProductById(int productId)
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbProducts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.TbProductImages.OrderByDescending(i => i.IsThumbnail).ThenBy(i => i.DisplayOrder))
                .FirstOrDefault(p => p.ProductId == productId);
        }

        private string? ValidateProduct(TbProduct product, int currentProductId)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleManager))
                return "Chỉ Quản lý (MANAGER) mới có quyền thêm/sửa sản phẩm!";

            string sku = (product.Sku ?? string.Empty).Trim();
            string name = (product.Name ?? string.Empty).Trim();

            if (sku.Length == 0 || sku.Length > 50)
                return "Mã SKU là bắt buộc và tối đa 50 ký tự!";
            if (IsSkuExists(sku, currentProductId))
                return $"Mã SKU '{sku}' đã tồn tại trong hệ thống!";
            if (name.Length == 0 || name.Length > 200)
                return "Tên sản phẩm là bắt buộc và tối đa 200 ký tự!";
            if (product.CategoryId <= 0)
                return "Vui lòng chọn danh mục sản phẩm!";
            if (product.ProductType != "BOOK" && product.ProductType != "STATIONERY")
                return "Loại sản phẩm không hợp lệ!";
            if (product.Price <= 0)
                return "Giá bán trước thuế phải lớn hơn 0!";
            if (product.UnitCost < 0)
                return "Giá vốn không được âm!";
            if (product.StockQuantity < 0)
                return "Số lượng tồn kho không được âm!";
            if (product.WeightGrams.HasValue && product.WeightGrams.Value < 0)
                return "Cân nặng (gram) không được âm!";
            if (product.PublicationYear.HasValue && (product.PublicationYear.Value < 1900 || product.PublicationYear.Value > DateTime.Now.Year + 1))
                return "Năm xuất bản không hợp lệ!";

            return null;
        }

        // Thêm sản phẩm mới (MNG-02) - Giá VAT tự động x1.1
        public string? AddProduct(TbProduct product, string? sourceImagePath)
        {
            string? err = ValidateProduct(product, 0);
            if (err != null) return err;

            using var context = new DbSe2038BookymartFContext();
            using var transaction = context.Database.BeginTransaction();

            product.Sku = product.Sku.Trim();
            product.Name = product.Name.Trim();
            product.Price = Math.Round(product.Price * 1.1m, 0, MidpointRounding.AwayFromZero);
            product.Status = "ACTIVE";
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
            }

            OrderLog.Add(context, $"Thêm sản phẩm mới: {product.Name} (SKU: {product.Sku})");
            context.SaveChanges();
            transaction.Commit();
            return null;
        }

        // Cập nhật thông tin sản phẩm (MNG-02)
        public string? UpdateProduct(TbProduct product, string? newImagePath = null)
        {
            string? err = ValidateProduct(product, product.ProductId);
            if (err != null) return err;

            using var context = new DbSe2038BookymartFContext();
            var existing = context.TbProducts.FirstOrDefault(p => p.ProductId == product.ProductId);
            if (existing == null) return "Không tìm thấy sản phẩm cần cập nhật!";

            existing.Sku = product.Sku.Trim();
            existing.Name = product.Name.Trim();
            existing.Price = Math.Round(product.Price * 1.1m, 0, MidpointRounding.AwayFromZero);
            existing.UnitCost = product.UnitCost;
            existing.StockQuantity = product.StockQuantity;
            existing.WeightGrams = product.WeightGrams;
            existing.CategoryId = product.CategoryId;
            existing.ProductType = product.ProductType;
            existing.Isbn = NormalizeOptional(product.Isbn);
            existing.Author = NormalizeOptional(product.Author);
            existing.Publisher = NormalizeOptional(product.Publisher);
            existing.PublicationYear = product.PublicationYear;
            existing.Brand = NormalizeOptional(product.Brand);
            existing.Specifications = NormalizeOptional(product.Specifications);
            existing.Description = NormalizeOptional(product.Description);

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

            OrderLog.Add(context, $"Cập nhật sản phẩm ID {product.ProductId}: {existing.Name}");
            context.SaveChanges();
            return null;
        }

        // Ngừng kinh doanh / Kích hoạt lại sản phẩm
        public string? DiscontinueProduct(int productId)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleManager))
                return "Chỉ Quản lý (MANAGER) mới có quyền thay đổi trạng thái sản phẩm!";

            using var context = new DbSe2038BookymartFContext();
            var prod = context.TbProducts.FirstOrDefault(p => p.ProductId == productId);
            if (prod == null) return "Không tìm thấy sản phẩm!";

            prod.Status = prod.Status == "ACTIVE" ? "DISCONTINUED" : "ACTIVE";
            string actionLabel = prod.Status == "DISCONTINUED" ? "Ngừng kinh doanh" : "Mở bán lại";

            OrderLog.Add(context, $"{actionLabel} sản phẩm ID {productId} ({prod.Name})");
            context.SaveChanges();
            return null;
        }

        private static string? NormalizeOptional(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
