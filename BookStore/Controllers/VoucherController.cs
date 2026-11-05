using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookStore.Controllers
{
    public class VoucherController
    {
        // 1. Lấy danh sách Voucher
        public List<TbVoucher> GetVouchers(string keyword = "")
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbVouchers.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(v => v.Code.Contains(keyword));
            }
            // Ưu tiên hiển thị voucher mới tạo lên trước
            return query.OrderByDescending(v => v.VoucherId).ToList();
        }

        // 2. Thêm hoặc Sửa Voucher
        public string SaveVoucher(int voucherId, string code, decimal discountValue, decimal minOrder, DateTime validUntil)
        {
            using var context = new DbSe2038BookymartFContext();

            // Validate chung
            if (string.IsNullOrEmpty(code)) return "Mã Voucher không được để trống!";
            if (discountValue <= 0) return "Mức giảm giá phải lớn hơn 0!";
            if (validUntil < DateTime.Now.Date && voucherId == 0) return "Hạn sử dụng không được trong quá khứ!";

            if (voucherId == 0) // THÊM MỚI
            {
                if (context.TbVouchers.Any(v => v.Code == code)) return "Mã Voucher này đã tồn tại!";

                var newVoucher = new TbVoucher
                {
                    Code = code.ToUpper(),
                    DiscountValue = discountValue,
                    MinOrderValue = minOrder,
                    // Giả sử bảng của bạn có cột hạn dùng và trạng thái. Tùy chỉnh tên biến nếu cần.
                    // ValidUntil = validUntil, 
                    Status = "ACTIVE"
                };
                context.TbVouchers.Add(newVoucher);
            }
            else // CẬP NHẬT
            {
                var voucher = context.TbVouchers.Find(voucherId);
                if (voucher == null) return "Không tìm thấy Voucher!";

                // Kiểm tra xem mã sửa có bị trùng với mã khác không
                if (context.TbVouchers.Any(v => v.Code == code && v.VoucherId != voucherId))
                    return "Mã Voucher này đã bị trùng!";

                voucher.Code = code.ToUpper();
                voucher.DiscountValue = discountValue;
                voucher.MinOrderValue = minOrder;
                // voucher.ValidUntil = validUntil;
            }

            context.SaveChanges();
            return null; // Thành công
        }

        // 3. Bật / Tắt Voucher
        public string ToggleVoucherStatus(int voucherId)
        {
            using var context = new DbSe2038BookymartFContext();
            var voucher = context.TbVouchers.Find(voucherId);

            if (voucher == null) return "Không tìm thấy Voucher!";

            voucher.Status = voucher.Status == "ACTIVE" ? "INACTIVE" : "ACTIVE";
            context.SaveChanges();
            return null;
        }
    }
}