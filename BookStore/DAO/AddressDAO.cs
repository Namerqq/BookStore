using System.Text.RegularExpressions;
using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DAO
{
    /// <summary>CUS-03: sổ địa chỉ. Rule validate theo mục 4.4.3.</summary>
    public static class AddressDAO
    {
        private static readonly Regex PhoneRegex = new(@"^(0\d{9}|\+84\d{9})$");

        public static List<TbAddress> GetMyAddresses()
        {
            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();
            return context.TbAddresses.AsNoTracking()
                .Where(a => a.AccountId == accountId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.AddressId)
                .ToList();
        }

        public static string? Validate(string receiverName, string receiverPhone, string fullAddress)
        {
            if (string.IsNullOrWhiteSpace(receiverName)) return "Tên người nhận không được để trống.";
            if (receiverName.Trim().Length > 100) return "Tên người nhận tối đa 100 ký tự.";
            if (string.IsNullOrWhiteSpace(receiverPhone)) return "SĐT người nhận là bắt buộc.";
            if (!PhoneRegex.IsMatch(receiverPhone.Trim()))
                return "SĐT phải gồm 0 + 9 số hoặc +84 + 9 số.";
            if (string.IsNullOrWhiteSpace(fullAddress)) return "Địa chỉ cụ thể không được để trống.";
            if (fullAddress.Trim().Length > 255) return "Địa chỉ cụ thể tối đa 255 ký tự.";
            return null;
        }

        public static string? Add(string receiverName, string receiverPhone, string fullAddress, bool setDefault)
        {
            if (!SessionGuard.IsCustomer) return "Chỉ khách hàng mới dùng được sổ địa chỉ.";
            string? error = Validate(receiverName, receiverPhone, fullAddress);
            if (error != null) return error;

            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();
            using var transaction = context.Database.BeginTransaction();

            // Địa chỉ đầu tiên của tài khoản tự động là mặc định.
            bool isFirst = !context.TbAddresses.Any(a => a.AccountId == accountId);
            bool makeDefault = isFirst || setDefault;
            if (makeDefault) ClearDefault(context, accountId);

            context.TbAddresses.Add(new TbAddress
            {
                AccountId = accountId,
                ReceiverName = receiverName.Trim(),
                ReceiverPhone = receiverPhone.Trim(),
                FullAddress = fullAddress.Trim(),
                IsDefault = makeDefault
            });
            context.SaveChanges();
            transaction.Commit();
            return null;
        }

        public static string? Update(int addressId, string receiverName, string receiverPhone, string fullAddress, bool setDefault)
        {
            if (!SessionGuard.IsCustomer) return "Chỉ khách hàng mới dùng được sổ địa chỉ.";
            string? error = Validate(receiverName, receiverPhone, fullAddress);
            if (error != null) return error;

            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();
            using var transaction = context.Database.BeginTransaction();

            var address = context.TbAddresses.FirstOrDefault(a => a.AddressId == addressId && a.AccountId == accountId);
            if (address == null) return "Không tìm thấy địa chỉ.";

            address.ReceiverName = receiverName.Trim();
            address.ReceiverPhone = receiverPhone.Trim();
            address.FullAddress = fullAddress.Trim();
            context.SaveChanges();

            if (setDefault && !address.IsDefault)
            {
                // Bỏ mặc định cũ trước để không vi phạm UQ_Address_OneDefault.
                ClearDefault(context, accountId);
                context.TbAddresses.Where(a => a.AddressId == addressId)
                    .ExecuteUpdate(s => s.SetProperty(a => a.IsDefault, true));
            }

            transaction.Commit();
            return null;
        }

        public static string? SetDefault(int addressId)
        {
            if (!SessionGuard.IsCustomer) return "Chỉ khách hàng mới dùng được sổ địa chỉ.";

            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();
            using var transaction = context.Database.BeginTransaction();

            if (!context.TbAddresses.Any(a => a.AddressId == addressId && a.AccountId == accountId))
                return "Không tìm thấy địa chỉ.";

            ClearDefault(context, accountId);
            context.TbAddresses.Where(a => a.AddressId == addressId)
                .ExecuteUpdate(s => s.SetProperty(a => a.IsDefault, true));

            transaction.Commit();
            return null;
        }

        public static string? Delete(int addressId)
        {
            if (!SessionGuard.IsCustomer) return "Chỉ khách hàng mới dùng được sổ địa chỉ.";

            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();
            using var transaction = context.Database.BeginTransaction();

            var address = context.TbAddresses.FirstOrDefault(a => a.AddressId == addressId && a.AccountId == accountId);
            if (address == null) return "Không tìm thấy địa chỉ.";

            bool wasDefault = address.IsDefault;
            context.TbAddresses.Remove(address);
            context.SaveChanges();

            // Xóa địa chỉ mặc định thì chuyển mặc định sang địa chỉ mới nhất còn lại.
            if (wasDefault)
            {
                int? nextId = context.TbAddresses
                    .Where(a => a.AccountId == accountId)
                    .OrderByDescending(a => a.AddressId)
                    .Select(a => (int?)a.AddressId)
                    .FirstOrDefault();

                if (nextId != null)
                {
                    context.TbAddresses.Where(a => a.AddressId == nextId)
                        .ExecuteUpdate(s => s.SetProperty(a => a.IsDefault, true));
                }
            }

            transaction.Commit();
            return null;
        }

        private static void ClearDefault(DbSe2038BookymartFContext context, int accountId)
        {
            context.TbAddresses.Where(a => a.AccountId == accountId && a.IsDefault)
                .ExecuteUpdate(s => s.SetProperty(a => a.IsDefault, false));
        }
    }
}
