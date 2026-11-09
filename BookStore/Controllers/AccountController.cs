using BookStore.Models;
using BookStore.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace BookStore.Controllers
{
    public class AccountController
    {
        // 1. Hàm xử lý đăng nhập
        public string Login(string email, string password)
        {
            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.FirstOrDefault(a => a.Email == email);

            if (account == null) return "Tài khoản không tồn tại!";
            if (account.Status != "ACTIVE") return "Tài khoản đang bị khóa!";

            // So sánh trực tiếp chuỗi mật khẩu thô (hoặc dùng hàm Verify nếu dùng mã hóa)
            if (account.PasswordHash != password)
                return "Sai mật khẩu!";

            // Lưu session
            GlobalSession.AccountId = account.AccountId;
            GlobalSession.FullName = account.FullName;
            GlobalSession.Role = account.Role;

            return null; // Đăng nhập thành công
        }

        // 2. Hàm đăng ký tài khoản mới
        public string Register(string fullName, string email, string password)
        {
            using var context = new DbSe2038BookymartFContext();

            var existingAccount = context.TbAccounts.FirstOrDefault(a => a.Email == email);
            if (existingAccount != null)
            {
                return "Email này đã được sử dụng bởi tài khoản khác!";
            }

            var newAccount = new TbAccount
            {
                FullName = fullName,
                Email = email,
                PasswordHash = PasswordUtil.HashPassword(password),
                Role = "CUSTOMER",
                Status = "ACTIVE"
            };

            context.TbAccounts.Add(newAccount);
            context.SaveChanges();

            return null;
        }

        // 3. Lấy thông tin tài khoản hiện tại để hiển thị lên form
        public TbAccount GetAccountById(int accountId)
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbAccounts.FirstOrDefault(a => a.AccountId == accountId);
        }

        // 4. Cập nhật thông tin cơ bản (Họ tên, SĐT) cho Hồ sơ cá nhân
        public string UpdateProfile(int accountId, string fullName, string phone)
        {
            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.FirstOrDefault(a => a.AccountId == accountId);

            if (account == null) return "Không tìm thấy tài khoản!";

            account.FullName = fullName;
            account.Phone = phone;

            context.SaveChanges();

            // Cập nhật lại session ngay lập tức để Header hiển thị tên mới
            GlobalSession.FullName = fullName;

            return null; // Thành công
        }

        // 5. Đổi mật khẩu
        public string ChangePassword(int accountId, string oldPassword, string newPassword)
        {
            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.FirstOrDefault(a => a.AccountId == accountId);

            if (account == null) return "Không tìm thấy tài khoản!";

            // Kiểm tra mật khẩu cũ có đúng không
            if (!PasswordUtil.VerifyPassword(oldPassword, account.PasswordHash))
            {
                return "Mật khẩu hiện tại không chính xác!";
            }

            // Băm và lưu mật khẩu mới
            account.PasswordHash = PasswordUtil.HashPassword(newPassword);
            context.SaveChanges();

            return null; // Thành công
        }

        // 6. Lấy danh sách tài khoản (có tìm kiếm và lọc theo Role) - ADM-01 & ADM-02
        public List<TbAccount> GetAccounts(string keyword = "", string role = "")
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbAccounts.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.FullName.Contains(keyword) || a.Email.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(role) && role != "ALL")
            {
                query = query.Where(a => a.Role == role);
            }

            return query.ToList();
        }

        // 7. Khóa / Mở khóa tài khoản (ADM-01)
        public string ToggleAccountStatus(int accountId)
        {
            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.Find(accountId);

            if (account == null) return "Không tìm thấy tài khoản!";
            if (account.Role == "ADMIN") return "Không thể khóa tài khoản ADMIN tối cao!";

            account.Status = account.Status == "ACTIVE" ? "LOCKED" : "ACTIVE";
            context.SaveChanges();

            return null; // Thành công
        }

        // 8. Thay đổi quyền tài khoản (ADM-02 - Phân quyền)
        public string UpdateAccountRole(int accountId, string newRole)
        {
            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.Find(accountId);

            if (account == null) return "Không tìm thấy tài khoản!";
            if (account.Role == "ADMIN" && newRole != "ADMIN") return "Không thể hạ quyền của ADMIN!";

            account.Role = newRole;
            context.SaveChanges();

            return null; // Thành công
        }

        // 9. ADM-03: Lấy danh sách nhật ký hệ thống
        public List<TbActivityLog> GetSystemLogs(string keyword = "")
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbActivityLogs.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(log => log.Action.Contains(keyword));
            }

            return query.OrderByDescending(log => log.CreatedAt).ToList();
        }
        public string AdminAddAccount(string fullName, string email, string password, string role)
        {
            using var context = new DbSe2038BookymartFContext();

            bool isExist = context.TbAccounts.Any(a => a.Email.ToLower() == email.ToLower());
            if (isExist)
            {
                return "Email này đã tồn tại trong hệ thống! Vui lòng dùng Email khác.";
            }

            var newAccount = new TbAccount
            {
                FullName = fullName,
                Email = email,
                PasswordHash = password,
                Role = role,
                Status = "ACTIVE"
            };

            context.TbAccounts.Add(newAccount);
            context.SaveChanges();

            return null;
        }
        public string DeleteAccount(int accountId)
        {
            using var context = new DbSe2038BookymartFContext();
            var acc = context.TbAccounts.Find(accountId);
            if (acc != null)
            {
                context.TbAccounts.Remove(acc);
                context.SaveChanges();
                return null; // Thành công
            }
            return "Không tìm thấy tài khoản cần xóa!";
        }
    }
}