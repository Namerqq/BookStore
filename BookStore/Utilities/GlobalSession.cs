namespace BookStore.Utilities
{
    public static class GlobalSession
    {
        public static int? AccountId { get; set; }
        public static string FullName { get; set; } = string.Empty;
        public static string Role { get; set; } = string.Empty;

        public static void Clear()
        {
            AccountId = null;
            FullName = string.Empty;
            Role = string.Empty;
        }
    }
}