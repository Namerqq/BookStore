using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    public class VoucherController
    {
        // 1. Lấy danh sách Voucher
        public List<TbVoucher> GetVouchers(string keyword = "")
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbVouchers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToUpperInvariant();
                query = query.Where(v => v.Code.Contains(kw));
            }

            return query.OrderByDescending(v => v.VoucherId).ToList();
        }

        // 2. Thêm hoặc Sửa Voucher (MNG-03)
        public string? SaveVoucher(
            int voucherId,
            string code,
            string discountType,
            decimal discountValue,
            decimal? maxDiscountCap,
            decimal minOrder,
            DateTime startDate,
            DateTime endDate,
            int maxUsageLimit,
            int perCustomerLimit)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleManager))
                return "Chỉ Quản lý (MANAGER) mới có quyền quản lý voucher!";

            string normalizedCode = (code ?? string.Empty).Trim().ToUpperInvariant();
            string? codeErr = ValidationUtil.ValidateVoucherCode(normalizedCode);
            if (codeErr != null) return codeErr;

            if (discountType != "FIXED" && discountType != "PERCENTAGE")
                return "Loại giảm giá phải là FIXED hoặc PERCENTAGE!";
            if (discountValue <= 0)
                return "Mức giảm giá phải lớn hơn 0!";
            if (discountType == "PERCENTAGE" && discountValue > 100)
                return "Giảm theo phần trăm không được vượt quá 100%!";
            if (maxDiscountCap.HasValue && maxDiscountCap.Value <= 0)
                return "Giảm tối đa phải lớn hơn 0!";
            if (minOrder < 0)
                return "Giá trị đơn tối thiểu không được âm!";
            if (endDate < startDate)
                return "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu!";
            if (voucherId == 0 && endDate.Date < DateTime.Now.Date)
                return "Ngày kết thúc không được nằm trong quá khứ!";
            if (maxUsageLimit < 1)
                return "Tổng lượt dùng tối đa phải >= 1!";
            if (perCustomerLimit < 1 || perCustomerLimit > maxUsageLimit)
                return "Lượt dùng mỗi khách phải >= 1 và <= tổng lượt dùng tối đa!";

            using var context = new DbSe2038BookymartFContext();

            if (voucherId == 0)
            {
                if (context.TbVouchers.Any(v => v.Code == normalizedCode))
                    return "Mã Voucher này đã tồn tại!";

                var newVoucher = new TbVoucher
                {
                    Code = normalizedCode,
                    DiscountType = discountType,
                    DiscountValue = discountValue,
                    MaxDiscountCap = discountType == "PERCENTAGE" ? maxDiscountCap : null,
                    MinOrderValue = minOrder,
                    StartDate = startDate.Date,
                    EndDate = endDate.Date.AddDays(1).AddSeconds(-1),
                    MaxUsageLimit = maxUsageLimit,
                    PerCustomerLimit = perCustomerLimit,
                    Status = "ACTIVE"
                };
                context.TbVouchers.Add(newVoucher);
                context.TbActivityLogs.Add(new TbActivityLog
                {
                    AccountId = GlobalSession.AccountId,
                    Action = $"Tạo voucher mới: {normalizedCode}",
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                var voucher = context.TbVouchers.Find(voucherId);
                if (voucher == null) return "Không tìm thấy Voucher!";

                if (context.TbVouchers.Any(v => v.Code == normalizedCode && v.VoucherId != voucherId))
                    return "Mã Voucher này đã bị trùng!";

                voucher.Code = normalizedCode;
                voucher.DiscountType = discountType;
                voucher.DiscountValue = discountValue;
                voucher.MaxDiscountCap = discountType == "PERCENTAGE" ? maxDiscountCap : null;
                voucher.MinOrderValue = minOrder;
                voucher.StartDate = startDate.Date;
                voucher.EndDate = endDate.Date.AddDays(1).AddSeconds(-1);
                voucher.MaxUsageLimit = maxUsageLimit;
                voucher.PerCustomerLimit = perCustomerLimit;

                context.TbActivityLogs.Add(new TbActivityLog
                {
                    AccountId = GlobalSession.AccountId,
                    Action = $"Cập nhật voucher ID {voucherId}: {normalizedCode}",
                    CreatedAt = DateTime.Now
                });
            }

            context.SaveChanges();
            return null;
        }

        // 3. Bật / Tắt Voucher
        public string? ToggleVoucherStatus(int voucherId)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleManager))
                return "Chỉ Quản lý (MANAGER) mới có quyền thay đổi trạng thái voucher!";

            using var context = new DbSe2038BookymartFContext();
            var voucher = context.TbVouchers.Find(voucherId);

            if (voucher == null) return "Không tìm thấy Voucher!";

            voucher.Status = voucher.Status == "ACTIVE" ? "INACTIVE" : "ACTIVE";
            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = GlobalSession.AccountId,
                Action = $"Đổi trạng thái voucher {voucher.Code} -> {voucher.Status}",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
            return null;
        }

        // 4. Xuất CSV danh sách Voucher (MNG-03 / MNG-05)
        public void ExportVouchersToCsv(string filePath, string keyword = "")
        {
            var list = GetVouchers(keyword);
            var csv = new StringBuilder();
            csv.AppendLine("ID,MaCode,LoaiGiam,MucGiam,GiamToiDa,DonToiThieu,NgayBatDau,NgayKetThuc,TongLuotDung,LuotMoiKhach,TrangThai");

            foreach (var v in list)
            {
                string maxCap = v.MaxDiscountCap.HasValue ? v.MaxDiscountCap.Value.ToString("0") : "";
                csv.AppendLine(
                    $"{v.VoucherId},\"{v.Code.Replace("\"", "\"\"")}\",{v.DiscountType},{v.DiscountValue:0},{maxCap},{v.MinOrderValue:0}," +
                    $"{v.StartDate:yyyy-MM-dd},{v.EndDate:yyyy-MM-dd},{v.MaxUsageLimit},{v.PerCustomerLimit},{v.Status}");
            }

            File.WriteAllText(filePath, csv.ToString(), new UTF8Encoding(true));

            using var context = new DbSe2038BookymartFContext();
            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = GlobalSession.AccountId,
                Action = $"Xuất CSV danh sách voucher ra tệp: {filePath}",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }
    }
}
