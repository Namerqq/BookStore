using System.Windows;
using System.Windows.Controls;
using BookStore.Utilities;
using BookStore.View.Auth;

namespace BookStore.View.Shared
{
    public partial class HeaderControl : UserControl
    {
        public HeaderControl()
        {
            InitializeComponent();
            CheckUserSession();
        }

        private void CheckUserSession()
        {
            if (!string.IsNullOrEmpty(GlobalSession.FullName))
            {
                BtnAuth.Content = $"👤 {GlobalSession.FullName}";
            }
            else
            {
                BtnAuth.Content = "👤 Login";
            }
        }

        private void BtnAuth_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GlobalSession.FullName))
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Window parentWindow = Window.GetWindow(this);
                parentWindow?.Close();
            }
            else
            {
                MessageBox.Show($"Đang đăng nhập với quyền: {GlobalSession.Role}", "Thông tin tài khoản");
            }
        }
    }
}