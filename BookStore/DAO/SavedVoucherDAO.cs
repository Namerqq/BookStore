using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DAO
{
    /// <summary>Mã giảm giá khách đã lưu (tbSavedVoucher).</summary>
    public static class SavedVoucherDAO
    {
        public static string? Save(int voucherId)
        {
            if (!SessionGuard.IsLoggedIn) return "Vui lòng đăng nhập để lưu mã giảm giá.";
            if (!SessionGuard.IsCustomer) return "Tài khoản nội bộ không dùng được mã giảm giá.";

            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();

            if (!context.TbVouchers.Any(v => v.VoucherId == voucherId && v.Status == "ACTIVE"))
                return "Mã giảm giá không còn hiệu lực.";
            if (context.TbSavedVouchers.Any(s => s.AccountId == accountId && s.VoucherId == voucherId))
                return "Bạn đã lưu mã này rồi.";

            context.TbSavedVouchers.Add(new TbSavedVoucher { AccountId = accountId, VoucherId = voucherId });
            context.SaveChanges();
            return null;
        }

        public static List<SavedVoucherOption> GetUsableSaved()
        {
            int accountId = SessionGuard.CurrentAccountId;
            DateTime now = DateTime.Now;
            using var context = new DbSe2038BookymartFContext();

            var vouchers = context.TbSavedVouchers.AsNoTracking()
                .Where(s => s.AccountId == accountId
                            && s.Voucher.Status == "ACTIVE"
                            && s.Voucher.StartDate <= now
                            && s.Voucher.EndDate >= now)
                .Select(s => s.Voucher)
                .ToList();

            return vouchers.Select(v => new SavedVoucherOption
            {
                Code = v.Code,
                Description = Describe(v)
            }).ToList();
        }

        private static string Describe(TbVoucher voucher)
        {
            string discount = voucher.DiscountType == "PERCENTAGE"
                ? $"Giảm {voucher.DiscountValue:0.##}%"
                : $"Giảm {MoneyText.Format(voucher.DiscountValue)}";
            return $"{discount}, đơn từ {MoneyText.Format(voucher.MinOrderValue)}";
        }
    }
}
