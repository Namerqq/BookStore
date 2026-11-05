using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DAO
{
    /// <summary>
    /// CUS-05: đặt hàng. Giá, phí ship và giảm giá luôn tính lại bằng C# từ dữ liệu trong DB,
    /// không dùng số đang hiển thị trên giao diện.
    /// </summary>
    public static class CheckoutDAO
    {
        /// <summary>Tính tạm để hiển thị trên màn hình Checkout, chưa ghi gì xuống DB.</summary>
        public static CheckoutQuote GetQuote(string? voucherCode)
        {
            var quote = new CheckoutQuote
            {
                Lines = CartDAO.GetCartLines().Where(l => l.IsSelected).ToList()
            };

            if (quote.Lines.Count == 0)
            {
                quote.Error = "Bạn chưa chọn sản phẩm nào để đặt hàng.";
                return quote;
            }

            var unavailable = quote.Lines.FirstOrDefault(l => !l.IsAvailable);
            if (unavailable != null)
            {
                quote.Error = $"“{unavailable.Name}”: {unavailable.Warning}. Vui lòng cập nhật lại giỏ hàng.";
            }

            quote.Subtotal = quote.Lines.Sum(l => l.LineTotal);
            quote.TotalGrams = quote.Lines.Sum(l => l.WeightGrams * l.Quantity);
            quote.ShippingFee = ShippingCalculator.Calculate(quote.TotalGrams);

            if (!string.IsNullOrWhiteSpace(voucherCode))
            {
                string code = voucherCode.Trim().ToUpper();
                using var context = new DbSe2038BookymartFContext();
                var voucher = context.TbVouchers.AsNoTracking().FirstOrDefault(v => v.Code == code);
                if (voucher == null)
                {
                    quote.VoucherError = "Mã giảm giá không tồn tại.";
                }
                else
                {
                    quote.VoucherError = EvaluateVoucher(context, voucher, SessionGuard.CurrentAccountId,
                        quote.Subtotal, out decimal discount);
                    if (quote.VoucherError == null)
                    {
                        quote.Discount = discount;
                        quote.AppliedVoucherCode = voucher.Code;
                    }
                }
            }

            return quote;
        }

        /// <summary>Đặt hàng trong một transaction duy nhất: lỗi ở bất kỳ bước nào đều rollback toàn bộ.</summary>
        public static PlaceOrderResult PlaceOrder(int addressId, string paymentMethod, string? voucherCode)
        {
            if (!SessionGuard.IsCustomer) return Fail("Chỉ tài khoản khách hàng mới được đặt hàng.");
            if (paymentMethod != PaymentMethods.Cod && paymentMethod != PaymentMethods.OnlineQr)
                return Fail("Phương thức thanh toán không hợp lệ.");

            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();
            using var transaction = context.Database.BeginTransaction();

            var address = context.TbAddresses.AsNoTracking()
                .FirstOrDefault(a => a.AddressId == addressId && a.AccountId == accountId);
            if (address == null) return Fail("Vui lòng chọn địa chỉ giao hàng hợp lệ.");

            // Sắp theo ProductId để nhiều đơn cùng lúc luôn khóa sản phẩm theo một thứ tự.
            var items = context.TbCartItems.Include(i => i.Product)
                .Where(i => i.IsSelected && i.Cart.AccountId == accountId)
                .OrderBy(i => i.ProductId)
                .ToList();
            if (items.Count == 0) return Fail("Bạn chưa chọn sản phẩm nào để đặt hàng.");

            var order = new TbOrder
            {
                OrderCode = $"ORD-{DateTime.Now:yyMMddHHmmss}-{Random.Shared.Next(100, 1000)}",
                AccountId = accountId,
                Status = paymentMethod == PaymentMethods.Cod ? OrderStatus.Pending : OrderStatus.PendingPayment,
                ReceiverName = address.ReceiverName,
                ReceiverPhone = address.ReceiverPhone,
                ShippingAddress = address.FullAddress,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentState.Unpaid,
                PhoneConfirmStatus = PhoneState.Pending,
                CreatedAt = DateTime.Now
            };

            decimal subtotal = 0m;
            int totalGrams = 0;

            foreach (var item in items)
            {
                var product = item.Product;

                // Trừ kho có điều kiện: nếu không đủ hàng thì không có dòng nào được cập nhật.
                int quantity = item.Quantity;
                int productId = item.ProductId;
                int affected = context.TbProducts
                    .Where(p => p.ProductId == productId && p.Status == "ACTIVE" && p.StockQuantity >= quantity)
                    .ExecuteUpdate(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity - quantity));

                if (affected == 0)
                {
                    return Fail($"Không đủ hàng cho “{product.Name}” (cần {quantity}). Đơn hàng chưa được tạo.");
                }

                subtotal += product.Price * quantity;
                totalGrams += (product.WeightGrams ?? 0) * quantity;

                order.TbOrderDetails.Add(new TbOrderDetail
                {
                    ProductId = productId,
                    ProductNameSnapshot = product.Name,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    CostPriceSnapshot = product.UnitCost
                });
            }

            order.ShippingFee = ShippingCalculator.Calculate(totalGrams);

            TbVoucher? voucher = null;
            if (!string.IsNullOrWhiteSpace(voucherCode))
            {
                string code = voucherCode.Trim().ToUpper();

                // Khóa dòng voucher để hai đơn cùng lúc không vượt giới hạn lượt dùng.
                voucher = context.TbVouchers
                    .FromSqlInterpolated($"SELECT * FROM tbVoucher WITH (UPDLOCK, ROWLOCK) WHERE code = {code}")
                    .FirstOrDefault();
                if (voucher == null) return Fail("Mã giảm giá không tồn tại.");

                string? voucherError = EvaluateVoucher(context, voucher, accountId, subtotal, out decimal discount);
                if (voucherError != null) return Fail(voucherError);

                order.VoucherId = voucher.VoucherId;
                order.DiscountAmount = discount;
            }

            context.TbOrders.Add(order);

            if (voucher != null)
            {
                context.TbVoucherUsages.Add(new TbVoucherUsage
                {
                    VoucherId = voucher.VoucherId,
                    AccountId = accountId,
                    Order = order,
                    UsedAt = DateTime.Now
                });
            }

            context.TbCartItems.RemoveRange(items);
            OrderLog.Add(context, $"Khách đặt đơn {order.OrderCode}");

            context.SaveChanges();
            transaction.Commit();

            return new PlaceOrderResult
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                Total = subtotal - order.DiscountAmount + order.ShippingFee,
                PaymentMethod = paymentMethod
            };
        }

        /// <summary>Kiểm tra hạn dùng, lượt dùng, giá trị đơn và tính số tiền giảm. Trả về null nếu hợp lệ.</summary>
        private static string? EvaluateVoucher(DbSe2038BookymartFContext context, TbVoucher voucher,
            int accountId, decimal subtotal, out decimal discount)
        {
            discount = 0m;
            DateTime now = DateTime.Now;

            if (voucher.Status != "ACTIVE") return "Mã giảm giá không còn hiệu lực.";
            if (now < voucher.StartDate) return "Mã giảm giá chưa đến ngày áp dụng.";
            if (now > voucher.EndDate) return "Mã giảm giá đã hết hạn.";
            if (subtotal < voucher.MinOrderValue)
                return $"Đơn hàng cần tối thiểu {MoneyText.Format(voucher.MinOrderValue)} để dùng mã này.";

            int voucherId = voucher.VoucherId;
            int totalUsed = context.TbVoucherUsages.Count(u => u.VoucherId == voucherId);
            if (totalUsed >= voucher.MaxUsageLimit) return "Mã giảm giá đã hết lượt dùng.";

            int usedByCustomer = context.TbVoucherUsages.Count(u => u.VoucherId == voucherId && u.AccountId == accountId);
            if (usedByCustomer >= voucher.PerCustomerLimit) return "Bạn đã dùng hết lượt của mã giảm giá này.";

            if (voucher.DiscountType == "PERCENTAGE")
            {
                discount = subtotal * voucher.DiscountValue / 100m;
                if (voucher.MaxDiscountCap.HasValue && discount > voucher.MaxDiscountCap.Value)
                {
                    discount = voucher.MaxDiscountCap.Value;
                }
            }
            else
            {
                discount = voucher.DiscountValue;
            }

            discount = Math.Min(Math.Round(discount, 0, MidpointRounding.AwayFromZero), subtotal);
            return null;
        }

        private static PlaceOrderResult Fail(string message) => new() { Error = message };
    }
}
