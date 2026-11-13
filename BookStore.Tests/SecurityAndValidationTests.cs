using System;
using BookStore.DAO;
using BookStore.Utilities;

namespace BookStore.Tests
{
    [CollectionDefinition("GlobalSessionCollection", DisableParallelization = true)]
    public class GlobalSessionCollectionDefinition { }

    [Collection("GlobalSessionCollection")]
    public class SecurityAndValidationTests
    {
        [Fact]
        public void PasswordUtil_HashAndVerify_SupportsUniqueSaltAndRejectsWrongOrMalformedHashes()
        {
            string hash1 = PasswordUtil.HashPassword("BookStore@123");
            string hash2 = PasswordUtil.HashPassword("BookStore@123");

            Assert.NotEqual(hash1, hash2);
            Assert.True(PasswordUtil.VerifyPassword("BookStore@123", hash1));
            Assert.True(PasswordUtil.VerifyPassword("BookStore@123", hash2));
            Assert.False(PasswordUtil.VerifyPassword("WrongPass@123", hash1));
            Assert.False(PasswordUtil.VerifyPassword("123456", "plain-text-not-base64"));
            Assert.False(PasswordUtil.VerifyPassword("123456", ""));
        }

        [Theory]
        [InlineData("Nguyễn Văn An", true)]
        [InlineData("An", true)]
        [InlineData("", false)]
        [InlineData("A", false)]
        public void ValidationUtil_ValidateFullName_EnforcesLengthRules(string input, bool expectedValid)
        {
            string? error = ValidationUtil.ValidateFullName(input);
            Assert.Equal(expectedValid, error == null);
        }

        [Fact]
        public void ValidationUtil_ValidateFullName_RejectsOver100Chars()
        {
            Assert.NotNull(ValidationUtil.ValidateFullName(new string('A', 101)));
        }

        [Theory]
        [InlineData("customer@bookymart.vn", true)]
        [InlineData("user.name+1@gmail.com", true)]
        [InlineData("", false)]
        [InlineData("invalid-email", false)]
        [InlineData("user@domain", false)]
        public void ValidationUtil_ValidateEmail_MatchesRegex(string email, bool expectedValid)
        {
            Assert.Equal(expectedValid, ValidationUtil.ValidateEmail(email) == null);
        }

        [Theory]
        [InlineData("Pass1234", true)]
        [InlineData("Abcdef12", true)]
        [InlineData("short1", false)]
        [InlineData("OnlyLettersHere", false)]
        [InlineData("12345678", false)]
        public void ValidationUtil_ValidatePassword_RequiresMin8LetterAndDigit(string password, bool expectedValid)
        {
            Assert.Equal(expectedValid, ValidationUtil.ValidatePassword(password) == null);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("0901234567", true)]
        [InlineData("+84901234567", true)]
        [InlineData("09012345", false)]
        [InlineData("84901234567", false)]
        public void ValidationUtil_ValidateOptionalPhone_AllowsBlankOrValidVnPhone(string? phone, bool expectedValid)
        {
            Assert.Equal(expectedValid, ValidationUtil.ValidateOptionalPhone(phone) == null);
        }

        [Theory]
        [InlineData("SALE_20", true)]
        [InlineData("tet-2026", true)]
        [InlineData("AB", false)]
        [InlineData("BAD CODE!", false)]
        [InlineData("", false)]
        public void ValidationUtil_ValidateVoucherCode_NormalizesAndValidates(string code, bool expectedValid)
        {
            Assert.Equal(expectedValid, ValidationUtil.ValidateVoucherCode(code) == null);
        }

        [Theory]
        [InlineData("Trần Bình", "0912345678", "12 Nguyễn Huệ, Quận 1, TP.HCM", true)]
        [InlineData("Trần Bình", "+84912345678", "Ký túc xá khu A", true)]
        [InlineData("", "0912345678", "12 Nguyễn Huệ", false)]
        [InlineData("Trần Bình", "12345", "12 Nguyễn Huệ", false)]
        [InlineData("Trần Bình", "0912345678", "   ", false)]
        public void AddressDAO_Validate_EnforcesReceiverPhoneAndAddress(string name, string phone, string address, bool expectedValid)
        {
            Assert.Equal(expectedValid, AddressDAO.Validate(name, phone, address) == null);
        }

        [Fact]
        public void SessionGuard_TracksRolesAndClearsCleanly()
        {
            try
            {
                GlobalSession.Clear();
                Assert.False(SessionGuard.IsLoggedIn);
                Assert.False(SessionGuard.IsCustomer);
                Assert.Equal(0, SessionGuard.CurrentAccountId);

                GlobalSession.AccountId = 42;
                GlobalSession.FullName = "Quản Lý Kho";
                GlobalSession.Role = SessionGuard.RoleManager;

                Assert.True(SessionGuard.IsLoggedIn);
                Assert.Equal(42, SessionGuard.CurrentAccountId);
                Assert.False(SessionGuard.IsCustomer);
                Assert.True(SessionGuard.HasRole(SessionGuard.RoleManager, SessionGuard.RoleAdmin));
                Assert.False(SessionGuard.HasRole(SessionGuard.RoleStaff));

                GlobalSession.Role = SessionGuard.RoleCustomer;
                Assert.True(SessionGuard.IsCustomer);
            }
            finally
            {
                GlobalSession.Clear();
            }
        }
    }
}
