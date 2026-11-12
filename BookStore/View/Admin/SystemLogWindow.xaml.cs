using System.Windows;
using System.Windows.Controls;
using BookStore.Controllers;
using BookStore.Utilities;

namespace BookStore.View.Admin
{
    public partial class SystemLogWindow : Window
    {
        private readonly AccountController _controller;

        public SystemLogWindow()
        {
            InitializeComponent();
            _controller = new AccountController();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleAdmin)) return;
            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            dgLogs.ItemsSource = _controller.GetSystemLogs(keyword);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtSearchPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (IsLoaded) LoadData();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            LoadData();
        }
    }
}
