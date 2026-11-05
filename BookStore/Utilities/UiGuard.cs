using System.Windows;

namespace BookStore.Utilities
{
    /// <summary>Hỗ trợ dùng chung cho code-behind: báo lỗi CSDL và chặn mở cửa sổ sai quyền.</summary>
    public static class UiGuard
    {
        public static bool Run(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                string detail = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Không thực hiện được thao tác.\n\n{detail}", "Lỗi hệ thống",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>Hiện kết quả từ DAO. Trả về true nếu không có lỗi.</summary>
        public static bool ShowResult(string? error, string? successMessage = null)
        {
            if (error != null)
            {
                MessageBox.Show(error, "Không thể thực hiện", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (successMessage != null)
            {
                MessageBox.Show(successMessage, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return true;
        }

        public static bool Confirm(string message, string title = "Xác nhận")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
                   == MessageBoxResult.Yes;
        }

        /// <summary>Gọi trong sự kiện Loaded: sai quyền thì báo và đóng cửa sổ.</summary>
        public static bool RequireRole(Window window, params string[] roles)
        {
            if (SessionGuard.HasRole(roles)) return true;

            MessageBox.Show("Tài khoản của bạn không có quyền mở chức năng này.", "Không có quyền",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            window.Close();
            return false;
        }
    }
}
