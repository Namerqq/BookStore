using System;
using System.Collections.Generic;
using System.Linq;
using BookStore.DAO;
using BookStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    public class HomeController
    {
        public List<TbVoucher> GetActiveVouchers()
        {
            DateTime now = DateTime.Now;
            using var context = new DbSe2038BookymartFContext();
            return context.TbVouchers
                .AsNoTracking()
                .Where(v => v.Status == "ACTIVE" && v.StartDate <= now && v.EndDate >= now)
                .OrderByDescending(v => v.VoucherId)
                .ToList();
        }

        public List<TbCategory> GetCategories()
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToList();
        }

        public List<TbProduct> GetFeaturedProducts() => LoadProducts(limit: 10);

        public List<TbProduct> GetAllProducts() => LoadProducts();

        public List<TbProduct> GetProductsByCategory(int categoryId) => LoadProducts(categoryId: categoryId);

        public List<TbProduct> SearchProducts(string? keyword, int? categoryId, string? productType, string? sortBy)
        {
            return ProductDAO.Instance.GetProducts(keyword, categoryId, productType, sortBy);
        }

        private static List<TbProduct> LoadProducts(int? limit = null, int? categoryId = null)
        {
            using var context = new DbSe2038BookymartFContext();
            IQueryable<TbProduct> query = context.TbProducts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.TbProductImages)
                .Where(p => p.Status == "ACTIVE");

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            query = query.OrderBy(p => p.ProductId);

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return query.ToList();
        }
    }
}
