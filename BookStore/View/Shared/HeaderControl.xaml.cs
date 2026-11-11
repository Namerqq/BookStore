using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookStore.Utilities;
using BookStore.View.Auth;
using BookStore.View.Customer; // Thêm để gọi được HomeWindow và ProfileWindow

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

            // Tài khoản nội bộ không mua hàng; khách vãng lai bấm giỏ hàng sẽ được chuyển sang đăng nhập.
            bool isCustomer = SessionGuard.IsCustomer;
            BtnOrders.Visibility = isCustomer ? Visibility.Visible : Visibility.Collapsed;
            BtnAddresses.Visibility = isCustomer ? Visibility.Visible : Visibility.Collapsed;
            CartBox.Visibility = isCustomer || !SessionGuard.IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionGuard.IsLoggedIn)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                Window.GetWindow(this)?.Close();
                return;
            }

            new CartWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            new OrderHistoryWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void BtnAddresses_Click(object sender, RoutedEventArgs e)
        {
            new AddressBookWindow { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void BtnAuth_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GlobalSession.FullName))
            {
                // Chưa đăng nhập -> Chuyển tới trang Login
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Window parentWindow = Window.GetWindow(this);
                parentWindow?.Close();
            }
            else
            {
                // Đã đăng nhập -> Hiển thị tùy chọn: Xem Hồ sơ cá nhân hoặc Đăng xuất
                MessageBoxResult result = MessageBox.Show(
                    $"Xin chào, {GlobalSession.FullName}!\n\nBạn có muốn ĐĂNG XUẤT khỏi hệ thống không?\n(Nhấn 'Yes' để Đăng xuất, 'No' để mở Hồ sơ cá nhân)",
                    "Tài khoản cá nhân",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // 1. XỬ LÝ ĐĂNG XUẤT
                    GlobalSession.AccountId = 0;
                    GlobalSession.FullName = null;
                    GlobalSession.Role = null;

                    // Load lại trang chủ với tư cách khách vãng lai
                    HomeWindow homeWindow = new HomeWindow();
                    homeWindow.Show();

                    Window parentWindow = Window.GetWindow(this);
                    parentWindow?.Close();
                }
                else if (result == MessageBoxResult.No)
                {
                    // 2. MỞ TRANG HỒ SƠ CÁ NHÂN (PROFILE)
                    ProfileWindow profileWindow = new ProfileWindow();
                    profileWindow.Owner = Window.GetWindow(this);
                    profileWindow.ShowDialog();
                }
                // Nếu bấm Cancel thì không làm gì cả, ở lại trang hiện tại
            }
        }

        // Sự kiện khi click vào chữ Booky.Mart (Logo)
        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            Window currentWindow = Window.GetWindow(this);

            if (currentWindow is HomeWindow)
            {
                return;
            }

            HomeWindow homeWindow = new HomeWindow();
            homeWindow.Show();

            currentWindow?.Close();
        }
    }
}