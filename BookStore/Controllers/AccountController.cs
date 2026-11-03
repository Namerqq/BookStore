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

            // Sửa chỗ này: So sánh trực tiếp password nhập vào với password_hash trong DB
            if (account.PasswordHash != password)
                return "Sai mật khẩu!";

            // Lưu session
            GlobalSession.AccountId = account.AccountId;
            GlobalSession.FullName = account.FullName;
            GlobalSession.Role = account.Role;

            return null; // Đăng nhập thành công
        }

        // 2. Lấy danh sách tài khoản
        public List<TbAccount> GetAccounts(string keyword = "", string role = "")
        {
            using var context = new DbSe2038BookymartFContext();
            var query = context.TbAccounts.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.FullName.Contains(keyword) || a.Email.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(a => a.Role == role);
            }

            return query.ToList();
        }

        // 3. Khóa / Mở khóa tài khoản
        public bool ToggleAccountStatus(int accountId)
        {
            using var context = new DbSe2038BookymartFContext();
            var account = context.TbAccounts.Find(accountId);

            if (account != null)
            {
                if (account.Role == "ADMIN") return false;
                account.Status = account.Status == "ACTIVE" ? "LOCKED" : "ACTIVE";
                context.SaveChanges();
                return true;
            }
            return false;
        }
        // Thêm hàm này vào trong class AccountController.cs
        public string Register(string fullName, string email, string password)
        {
            using var context = new DbSe2038BookymartFContext();

            // Kiểm tra xem email đã tồn tại trong database chưa
            var existingAccount = context.TbAccounts.FirstOrDefault(a => a.Email == email);
            if (existingAccount != null)
            {
                return "Email này đã được sử dụng bởi tài khoản khác!";
            }

            // Tạo đối tượng tài khoản mới với vai trò mặc định là CUSTOMER
            var newAccount = new TbAccount
            {
                FullName = fullName,
                Email = email,
                PasswordHash = password, // Lưu ý: Nếu bạn dùng mật khẩu thô theo cách đã sửa ở phần Login, giữ nguyên biến password.
                Role = "CUSTOMER",
                Status = "ACTIVE"
            };

            context.TbAccounts.Add(newAccount);
            context.SaveChanges(); // Đẩy xuống database thực sự

            return null; // Trả về null nghĩa là đăng ký thành công
        }


    }


}