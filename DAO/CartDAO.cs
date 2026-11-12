using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DAO
{
    /// <summary>
    /// CUS-04: giỏ hàng, số lượng giới hạn theo tồn kho thật.
    /// Các hàm thay đổi dữ liệu trả về null nếu thành công, ngược lại trả về câu báo lỗi.
    /// </summary>
    public static class CartDAO
    {
        /// <summary>Giao ước với phần 2: nút "Thêm vào giỏ" gọi thẳng hàm này.</summary>
        public static string? AddToCart(int productId, int quantity)
        {
            if (!SessionGuard.IsLoggedIn) return "Vui lòng đăng nhập để thêm sản phẩm vào giỏ.";
            if (!SessionGuard.IsCustomer) return "Tài khoản nội bộ không được đặt mua hàng.";
            if (quantity <= 0) return "Số lượng phải lớn hơn 0.";

            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();
            using var transaction = context.Database.BeginTransaction();

            var product = context.TbProducts.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            if (product == null) return "Sản phẩm không tồn tại.";
            if (product.Status != "ACTIVE") return $"“{product.Name}” đã ngừng kinh doanh.";

            var cart = context.TbCarts.FirstOrDefault(c => c.AccountId == accountId);
            if (cart == null)
            {
                cart = new TbCart { AccountId = accountId };
                context.TbCarts.Add(cart);
                context.SaveChanges();
            }

            var item = context.TbCartItems.FirstOrDefault(i => i.CartId == cart.CartId && i.ProductId == productId);
            int newQuantity = (item?.Quantity ?? 0) + quantity;
            if (newQuantity > product.StockQuantity)
            {
                return $"“{product.Name}” chỉ còn {product.StockQuantity} sản phẩm trong kho.";
            }

            if (item == null)
            {
                context.TbCartItems.Add(new TbCartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = newQuantity,
                    IsSelected = true
                });
            }
            else
            {
                item.Quantity = newQuantity;
            }

            context.SaveChanges();
            transaction.Commit();
            return null;
        }

        public static List<CartLine> GetCartLines()
        {
            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();
            return context.TbCartItems.AsNoTracking()
                .Where(i => i.Cart.AccountId == accountId)
                .OrderByDescending(i => i.CartItemId)
                .Select(i => new CartLine
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    UnitPrice = i.Product.Price,
                    Quantity = i.Quantity,
                    Stock = i.Product.StockQuantity,
                    WeightGrams = i.Product.WeightGrams ?? 0,
                    IsSelected = i.IsSelected,
                    ProductStatus = i.Product.Status,
                    ThumbnailUrl = i.Product.TbProductImages
                        .OrderByDescending(img => img.IsThumbnail)
                        .ThenBy(img => img.DisplayOrder)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()
                })
                .ToList();
        }

        public static string? UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity <= 0) return "Số lượng phải lớn hơn 0.";

            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();

            var item = context.TbCartItems.Include(i => i.Product)
                .FirstOrDefault(i => i.CartItemId == cartItemId && i.Cart.AccountId == accountId);
            if (item == null) return "Sản phẩm không còn trong giỏ.";
            if (quantity > item.Product.StockQuantity)
            {
                return $"“{item.Product.Name}” chỉ còn {item.Product.StockQuantity} sản phẩm trong kho.";
            }

            item.Quantity = quantity;
            context.SaveChanges();
            return null;
        }

        public static string? RemoveItem(int cartItemId)
        {
            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();

            var item = context.TbCartItems
                .FirstOrDefault(i => i.CartItemId == cartItemId && i.Cart.AccountId == accountId);
            if (item == null) return "Sản phẩm không còn trong giỏ.";

            context.TbCartItems.Remove(item);
            context.SaveChanges();
            return null;
        }

        public static string? SetSelected(int cartItemId, bool isSelected)
        {
            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();

            int affected = context.TbCartItems
                .Where(i => i.CartItemId == cartItemId && i.Cart.AccountId == accountId)
                .ExecuteUpdate(s => s.SetProperty(i => i.IsSelected, isSelected));
            return affected == 0 ? "Sản phẩm không còn trong giỏ." : null;
        }

        public static string? SetAllSelected(bool isSelected)
        {
            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();

            context.TbCartItems
                .Where(i => i.Cart.AccountId == accountId)
                .ExecuteUpdate(s => s.SetProperty(i => i.IsSelected, isSelected));
            return null;
        }
    }
}
