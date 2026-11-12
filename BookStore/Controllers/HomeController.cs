using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace BookStore.Controllers
{
    public class HomeController
    {
        public List<TbVoucher> GetActiveVouchers()
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbVouchers.ToList();
        }

        public List<TbCategory> GetCategories()
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToList();
        }

        // Sửa TbProduct thành TbProducts
        public List<TbProduct> GetFeaturedProducts() => LoadProducts(limit: 10);

        // Sửa TbProduct thành TbProducts
        public List<TbProduct> GetAllProducts() => LoadProducts();

        public List<TbProduct> GetProductsByCategory(int categoryId) => LoadProducts(categoryId: categoryId);

        private static List<TbProduct> LoadProducts(int? limit = null, int? categoryId = null)
        {
            using var context = new DbSe2038BookymartFContext();
            IQueryable<TbProduct> query = context.TbProducts
                .AsNoTracking()
                .Include(p => p.TbProductImages);

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