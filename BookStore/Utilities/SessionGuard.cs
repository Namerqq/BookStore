namespace BookStore.Utilities
{
    /// <summary>Đọc phiên đăng nhập. Lưu ý: khi đăng xuất AccountId có thể bằng 0 chứ không phải null.</summary>
    public static class SessionGuard
    {
        public const string RoleCustomer = "CUSTOMER";
        public const string RoleStaff = "STAFF";
        public const string RoleManager = "MANAGER";

        public static int CurrentAccountId => GlobalSession.AccountId ?? 0;

        public static bool IsLoggedIn => CurrentAccountId > 0;

        public static bool IsCustomer => HasRole(RoleCustomer);

        public static bool HasRole(params string[] roles)
        {
            return IsLoggedIn && roles.Contains(GlobalSession.Role);
        }
    }
}
