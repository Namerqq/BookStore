using System.Windows;
using BookStore.Controllers;
using BookStore.View.Auth; // Thêm namespace này để gọi LoginWindow

namespace BookStore.View.Customer
{
    public partial class HomeWindow : Window
    {
        private readonly HomeController _controller;

        public HomeWindow()
        {
            InitializeComponent();
            _controller = new HomeController();
            LoadDataFromDatabase();
        }

        private void LoadDataFromDatabase()
        {
            VoucherListControl.ItemsSource = _controller.GetActiveVouchers();
            CategoryListControl.ItemsSource = _controller.GetCategories();
            ProductListControl.ItemsSource = _controller.GetFeaturedProducts();
        }

        // Sự kiện khi bấm nút Login trên Header
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close(); // Đóng trang chủ hiện tại lại
        }
    }
}