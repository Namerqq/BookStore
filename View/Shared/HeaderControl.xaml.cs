using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookStore.Utilities;
using BookStore.View.Auth;
using BookStore.View.Customer;

namespace BookStore.View.Shared
{
    public partial class HeaderControl : UserControl
    {
        public event Action<string>? SearchRequested;
        public event Action<string>? ProductTypeRequested;
        public event Action? PromotionsRequested;

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

        public void SetSearchText(string keyword)
        {
            txtSearch.Text = keyword;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtSearchPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (SearchRequested != null)
            {
                SearchRequested.Invoke(txtSearch.Text.Trim());
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            string kw = txtSearch.Text.Trim();
            if (SearchRequested != null)
            {
                SearchRequested.Invoke(kw);
                return;
            }

            Window? currentWindow = Window.GetWindow(this);
            if (currentWindow is not HomeWindow)
            {
                new HomeWindow(kw, null).Show();
                currentWindow?.Close();
            }
        }

        private void BtnBooks_Click(object sender, RoutedEventArgs e)
        {
            if (ProductTypeRequested != null)
            {
                ProductTypeRequested.Invoke("BOOK");
                return;
            }

            Window? currentWindow = Window.GetWindow(this);
            if (currentWindow is not HomeWindow)
            {
                new HomeWindow(null, "BOOK").Show();
                currentWindow?.Close();
            }
        }

        private void BtnStationery_Click(object sender, RoutedEventArgs e)
        {
            if (ProductTypeRequested != null)
            {
                ProductTypeRequested.Invoke("STATIONERY");
                return;
            }

            Window? currentWindow = Window.GetWindow(this);
            if (currentWindow is not HomeWindow)
            {
                new HomeWindow(null, "STATIONERY").Show();
                currentWindow?.Close();
            }
        }

        private void BtnPromotions_Click(object sender, RoutedEventArgs e)
        {
            if (PromotionsRequested != null)
            {
                PromotionsRequested.Invoke();
                return;
            }

            Window? currentWindow = Window.GetWindow(this);
            if (currentWindow is not HomeWindow)
            {
                new HomeWindow().Show();
                currentWindow?.Close();
            }
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
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Window? parentWindow = Window.GetWindow(this);
                parentWindow?.Close();
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Xin chào, {GlobalSession.FullName}!\n\nBạn có muốn ĐĂNG XUẤT khỏi hệ thống không?\n(Nhấn 'Yes' để Đăng xuất, 'No' để mở Hồ sơ cá nhân)",
                    "Tài khoản cá nhân",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    GlobalSession.Clear();

                    HomeWindow homeWindow = new HomeWindow();
                    homeWindow.Show();

                    Window? parentWindow = Window.GetWindow(this);
                    parentWindow?.Close();
                }
                else if (result == MessageBoxResult.No)
                {
                    ProfileWindow profileWindow = new ProfileWindow();
                    profileWindow.Owner = Window.GetWindow(this);
                    profileWindow.ShowDialog();
                    CheckUserSession();
                }
            }
        }

        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            Window? currentWindow = Window.GetWindow(this);

            if (currentWindow is HomeWindow home)
            {
                home.ResetToHome();
                return;
            }

            HomeWindow homeWindow = new HomeWindow();
            homeWindow.Show();

            currentWindow?.Close();
        }
    }
}
