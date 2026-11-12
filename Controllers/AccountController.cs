using System;
using System.Collections.Generic;
using System.Linq;
using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    public class AccountController
    {
        private static readonly HashSet<string> ValidRoles = new(StringComparer.Ordinal)
        {
            SessionGuard.RoleCustomer,
            SessionGuard.RoleStaff,
            SessionGuard.RoleManager,
            SessionGuard.RoleAdmin
        };

        // 1. Hàm xử lý đăng nhập (CUS-01)
        public string? Login(string email, string password)
        {
            string normalizedEmail = (email ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalizedEmail) || string.IsNullOrEmpty(password))
                return "Vui lòng nhập đầy đủ Email và Mật khẩu!";

            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.FirstOrDefault(a => a.Email == normalizedEmail);

            if (account == null) return "Tài khoản không tồn tại!";
            if (account.Status != "ACTIVE") return "Tài khoản đang bị khóa!";
            if (account.LockedUntil.HasValue && account.LockedUntil.Value > DateTime.Now)
                return $"Tài khoản đang bị tạm khóa đến {account.LockedUntil.Value:dd/MM/yyyy HH:mm}!";

            if (!PasswordUtil.VerifyPassword(password, account.PasswordHash))
            {
                account.FailedLoginCount += 1;
                context.SaveChanges();
                return "Sai mật khẩu!";
            }

            account.FailedLoginCount = 0;
            account.LockedUntil = null;

            // Lưu session
            GlobalSession.AccountId = account.AccountId;
            GlobalSession.FullName = account.FullName;
            GlobalSession.Role = account.Role;

            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = account.AccountId,
                Action = $"Đăng nhập hệ thống ({account.Role})",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();

            return null;
        }

        // 2. Hàm đăng ký tài khoản mới (GUE-04)
        public string? Register(string fullName, string email, string password)
        {
            string? err = ValidationUtil.ValidateFullName(fullName)
                       ?? ValidationUtil.ValidateEmail(email)
                       ?? ValidationUtil.ValidatePassword(password);
            if (err != null) return err;

            string trimmedName = fullName.Trim();
            string trimmedEmail = email.Trim();

            using var context = new DbSe2038BookymartFContext();

            bool exists = context.TbAccounts.Any(a => a.Email.ToLower() == trimmedEmail.ToLower());
            if (exists)
                return "Email này đã được sử dụng bởi tài khoản khác!";

            var newAccount = new TbAccount
            {
                FullName = trimmedName,
                Email = trimmedEmail,
                PasswordHash = PasswordUtil.HashPassword(password),
                Role = SessionGuard.RoleCustomer,
                Status = "ACTIVE",
                CreatedAt = DateTime.Now
            };

            context.TbAccounts.Add(newAccount);
            context.SaveChanges();

            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = newAccount.AccountId,
                Action = $"Đăng ký tài khoản mới: {trimmedEmail}",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();

            return null;
        }

        // 3. Lấy thông tin tài khoản hiện tại để hiển thị lên form
        public TbAccount? GetAccountById(int accountId)
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbAccounts.AsNoTracking().FirstOrDefault(a => a.AccountId == accountId);
        }

        // 4. Cập nhật thông tin cơ bản (Họ tên, SĐT) cho Hồ sơ cá nhân (CUS-02)
        public string? UpdateProfile(int accountId, string fullName, string? phone)
        {
            string? err = ValidationUtil.ValidateFullName(fullName)
                       ?? ValidationUtil.ValidateOptionalPhone(phone);
            if (err != null) return err;

            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.FirstOrDefault(a => a.AccountId == accountId);

            if (account == null) return "Không tìm thấy tài khoản!";
            if (account.Role != SessionGuard.RoleCustomer)
                return "Tài khoản nội bộ không được tự sửa thông tin cá nhân. Vui lòng liên hệ Admin!";

            account.FullName = fullName.Trim();
            account.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = accountId,
                Action = "Cập nhật thông tin hồ sơ cá nhân",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();

            GlobalSession.FullName = account.FullName;
            return null;
        }

        // 5. Đổi mật khẩu (CUS-02)
        public string? ChangePassword(int accountId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(oldPassword))
                return "Vui lòng nhập mật khẩu hiện tại!";

            string? err = ValidationUtil.ValidatePassword(newPassword);
            if (err != null) return err;

            if (oldPassword == newPassword)
                return "Mật khẩu mới phải khác mật khẩu hiện tại!";

            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.FirstOrDefault(a => a.AccountId == accountId);

            if (account == null) return "Không tìm thấy tài khoản!";

            if (!PasswordUtil.VerifyPassword(oldPassword, account.PasswordHash))
                return "Mật khẩu hiện tại không chính xác!";

            account.PasswordHash = PasswordUtil.HashPassword(newPassword);
            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = accountId,
                Action = "Đổi mật khẩu tài khoản",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();

            return null;
        }

        // 6. Lấy danh sách tài khoản (ADM-01 & ADM-02)
        public List<TbAccount> GetAccounts(string keyword = "", string role = "")
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbAccounts.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim();
                query = query.Where(a => a.FullName.Contains(kw) || a.Email.Contains(kw)
                                         || (a.Phone != null && a.Phone.Contains(kw)));
            }

            if (!string.IsNullOrEmpty(role) && role != "ALL")
            {
                query = query.Where(a => a.Role == role);
            }

            return query.OrderByDescending(a => a.AccountId).ToList();
        }

        // 7. Khóa / Mở khóa tài khoản (ADM-01)
        public string? ToggleAccountStatus(int accountId)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleAdmin))
                return "Chỉ ADMIN mới có quyền khóa/mở khóa tài khoản!";

            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.Find(accountId);

            if (account == null) return "Không tìm thấy tài khoản!";
            if (account.Role == SessionGuard.RoleAdmin) return "Không thể khóa tài khoản ADMIN!";
            if (account.AccountId == SessionGuard.CurrentAccountId) return "Không thể tự khóa tài khoản đang đăng nhập!";

            account.Status = account.Status == "ACTIVE" ? "LOCKED" : "ACTIVE";
            string stateText = account.Status == "ACTIVE" ? "Mở khóa" : "Khóa";

            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = GlobalSession.AccountId,
                Action = $"{stateText} tài khoản ID {accountId} ({account.Email})",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();

            return null;
        }

        // 8. Thay đổi quyền tài khoản (ADM-02 - Phân quyền)
        public string? UpdateAccountRole(int accountId, string newRole)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleAdmin))
                return "Chỉ ADMIN mới có quyền phân quyền tài khoản!";

            if (!ValidRoles.Contains(newRole))
                return "Quyền được chọn không hợp lệ!";

            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.Find(accountId);

            if (account == null) return "Không tìm thấy tài khoản!";
            if (account.Role == SessionGuard.RoleAdmin && newRole != SessionGuard.RoleAdmin)
                return "Không thể hạ quyền của tài khoản ADMIN!";
            if (account.AccountId == SessionGuard.CurrentAccountId && newRole != SessionGuard.RoleAdmin)
                return "Không thể tự thay đổi quyền của chính mình!";

            string oldRole = account.Role;
            account.Role = newRole;

            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = GlobalSession.AccountId,
                Action = $"Phân quyền tài khoản ID {accountId} ({account.Email}): {oldRole} -> {newRole}",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();

            return null;
        }

        // 9. ADM-03: Lấy danh sách nhật ký hệ thống
        public List<TbActivityLog> GetSystemLogs(string keyword = "")
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbActivityLogs.AsNoTracking().Include(l => l.Account).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim();
                query = query.Where(log => log.Action.Contains(kw)
                                           || (log.Account != null && (log.Account.FullName.Contains(kw) || log.Account.Email.Contains(kw))));
            }

            return query.OrderByDescending(log => log.CreatedAt).ToList();
        }

        // 10. ADM-01: Thêm tài khoản mới (băm mật khẩu bằng PBKDF2)
        public string? AdminAddAccount(string fullName, string email, string password, string role, string? phone = null)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleAdmin))
                return "Chỉ ADMIN mới có quyền thêm tài khoản!";

            if (!ValidRoles.Contains(role))
                return "Quyền được chọn không hợp lệ!";

            string? err = ValidationUtil.ValidateFullName(fullName)
                       ?? ValidationUtil.ValidateEmail(email)
                       ?? ValidationUtil.ValidatePassword(password)
                       ?? ValidationUtil.ValidateOptionalPhone(phone);
            if (err != null) return err;

            string trimmedEmail = email.Trim();

            using var context = new DbSe2038BookymartFContext();

            bool isExist = context.TbAccounts.Any(a => a.Email.ToLower() == trimmedEmail.ToLower());
            if (isExist)
                return "Email này đã tồn tại trong hệ thống! Vui lòng dùng Email khác.";

            var newAccount = new TbAccount
            {
                FullName = fullName.Trim(),
                Email = trimmedEmail,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
                PasswordHash = PasswordUtil.HashPassword(password),
                Role = role,
                Status = "ACTIVE",
                CreatedAt = DateTime.Now
            };

            context.TbAccounts.Add(newAccount);
            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = GlobalSession.AccountId,
                Action = $"Admin tạo tài khoản mới: {trimmedEmail} (Role: {role})",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();

            return null;
        }

        // 11. ADM-01: Admin sửa thông tin tài khoản (kể cả Staff/Manager/Admin)
        public string? AdminUpdateAccount(int accountId, string fullName, string? phone)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleAdmin))
                return "Chỉ ADMIN mới có quyền sửa thông tin tài khoản!";

            string? err = ValidationUtil.ValidateFullName(fullName)
                       ?? ValidationUtil.ValidateOptionalPhone(phone);
            if (err != null) return err;

            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.Find(accountId);
            if (account == null) return "Không tìm thấy tài khoản!";

            account.FullName = fullName.Trim();
            account.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

            if (accountId == GlobalSession.AccountId)
            {
                GlobalSession.FullName = account.FullName;
            }

            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = GlobalSession.AccountId,
                Action = $"Admin sửa thông tin tài khoản ID {accountId} ({account.Email})",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();

            return null;
        }

        // 12. ADM-01: Xóa tài khoản
        public string? DeleteAccount(int accountId)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleAdmin))
                return "Chỉ ADMIN mới có quyền xóa tài khoản!";

            using var context = new DbSe2038BookymartFContext();
            var acc = context.TbAccounts.Find(accountId);
            if (acc == null) return "Không tìm thấy tài khoản cần xóa!";
            if (acc.Role == SessionGuard.RoleAdmin) return "Không thể xóa tài khoản ADMIN!";
            if (acc.AccountId == SessionGuard.CurrentAccountId) return "Không thể tự xóa tài khoản đang đăng nhập!";

            try
            {
                string email = acc.Email;
                context.TbAccounts.Remove(acc);
                context.TbActivityLogs.Add(new TbActivityLog
                {
                    AccountId = GlobalSession.AccountId,
                    Action = $"Admin xóa tài khoản ID {accountId} ({email})",
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
                return null;
            }
            catch (DbUpdateException)
            {
                return "Tài khoản này đã phát sinh dữ liệu (đơn hàng/đánh giá), không thể xóa vĩnh viễn. Vui lòng dùng chức năng Khóa!";
            }
        }
    }
}
