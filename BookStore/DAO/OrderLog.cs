using BookStore.Models;
using BookStore.Utilities;

namespace BookStore.DAO
{
    /// <summary>Ghi tbActivityLog trong cùng context/transaction với thao tác nghiệp vụ.</summary>
    internal static class OrderLog
    {
        private const int MaxLength = 255;

        public static void Add(DbSe2038BookymartFContext context, string action)
        {
            int accountId = SessionGuard.CurrentAccountId;
            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = accountId > 0 ? accountId : null,
                Action = action.Length > MaxLength ? action.Substring(0, MaxLength) : action,
                CreatedAt = DateTime.Now
            });
        }
    }
}
