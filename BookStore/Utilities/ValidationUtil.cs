using System;
using System.Text.RegularExpressions;

namespace BookStore.Utilities
{
    /// <summary>Hàm kiểm tra hợp lệ dữ liệu dùng chung cho WPF và Controller/DAO (mục 4.4.1 - 4.4.5).</summary>
    public static class ValidationUtil
    {
        private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        private static readonly Regex PhoneRegex = new(@"^(0\d{9}|\+84\d{9})$");
        private static readonly Regex VoucherCodeRegex = new(@"^[A-Z0-9_-]{3,30}$");
        private static readonly Regex HasLetterRegex = new(@"[A-Za-z]");
        private static readonly Regex HasDigitRegex = new(@"\d");

        public static string? ValidateFullName(string? fullName)
        {
            string value = (fullName ?? string.Empty).Trim();
            if (value.Length == 0) return "Họ và tên không được để trống.";
            if (value.Length < 2 || value.Length > 100) return "Họ và tên phải từ 2 đến 100 ký tự.";
            return null;
        }

        public static string? ValidateEmail(string? email)
        {
            string value = (email ?? string.Empty).Trim();
            if (value.Length == 0) return "Email không được để trống.";
            if (value.Length > 100 || !EmailRegex.IsMatch(value)) return "Định dạng email không hợp lệ.";
            return null;
        }

        public static string? ValidatePassword(string? password)
        {
            string value = password ?? string.Empty;
            if (value.Length < 8 || value.Length > 64)
                return "Mật khẩu phải từ 8 đến 64 ký tự.";
            if (!HasLetterRegex.IsMatch(value) || !HasDigitRegex.IsMatch(value))
                return "Mật khẩu phải chứa ít nhất 1 chữ cái và 1 chữ số.";
            return null;
        }

        public static string? ValidateOptionalPhone(string? phone)
        {
            string value = (phone ?? string.Empty).Trim();
            if (value.Length == 0) return null;
            if (!PhoneRegex.IsMatch(value))
                return "Số điện thoại phải gồm 0 + 9 chữ số hoặc +84 + 9 chữ số.";
            return null;
        }

        public static string? ValidateVoucherCode(string? code)
        {
            string value = (code ?? string.Empty).Trim().ToUpperInvariant();
            if (value.Length == 0) return "Mã voucher không được để trống.";
            if (!VoucherCodeRegex.IsMatch(value))
                return "Mã voucher gồm 3-30 ký tự chữ in hoa, số, dấu gạch dưới hoặc gạch ngang.";
            return null;
        }
    }
}
