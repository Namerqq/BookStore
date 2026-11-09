using BookStore.Models;
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
            return context.TbCategories.ToList();
        }

        // Sửa TbProduct thành TbProducts
        public List<TbProduct> GetFeaturedProducts()
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbProducts.Take(10).ToList();
        }

        // Sửa TbProduct thành TbProducts
        public List<TbProduct> GetAllProducts()
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbProducts.ToList();
        }
    }
}