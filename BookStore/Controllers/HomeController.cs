using BookStore.Models;
using System.Collections.Generic;
using System.Linq;

namespace BookStore.Controllers
{
    public class HomeController
    {
        private readonly DbSe2038BookymartFContext _context;

        public HomeController()
        {
            _context = new DbSe2038BookymartFContext();
        }

        // Lấy danh sách voucher từ bảng tbVoucher
        public List<TbVoucher> GetActiveVouchers()
        {
            return _context.TbVouchers.Where(v => v.Status == "ACTIVE").ToList();
        }

        // Lấy danh sách danh mục từ bảng tbCategory
        public List<TbCategory> GetCategories()
        {
            return _context.TbCategories.ToList();
        }

        // Lấy danh sách sản phẩm từ bảng tbProduct
        public List<TbProduct> GetFeaturedProducts()
        {
            return _context.TbProducts.Take(8).ToList(); // Lấy tối đa 8 sản phẩm lên trang chủ
        }
    }
}