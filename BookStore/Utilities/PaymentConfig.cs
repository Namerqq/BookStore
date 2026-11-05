namespace BookStore.Utilities
{
    /// <summary>
    /// Tài khoản nhận tiền VietQR của cửa hàng. Khi có tài khoản thật của nhóm,
    /// chỉ cần đổi 3 giá trị dưới đây.
    /// </summary>
    public static class PaymentConfig
    {
        public const string BankCode = "MB";
        public const string AccountNumber = "0000000000";
        public const string AccountName = "BOOKY MART";

        public static bool IsDemoAccount => AccountNumber == "0000000000";

        public static string BuildQrUrl(decimal amount, string orderCode)
        {
            long roundedAmount = (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero);
            return $"https://img.vietqr.io/image/{BankCode}-{AccountNumber}-compact2.png"
                 + $"?amount={roundedAmount}"
                 + $"&addInfo={Uri.EscapeDataString(orderCode)}"
                 + $"&accountName={Uri.EscapeDataString(AccountName)}";
        }
    }
}
