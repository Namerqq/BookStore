using System.Windows;
using System.Windows.Input;
using BookStore.Controllers;
using BookStore.DAO;
using BookStore.Models;
using BookStore.Utilities;
using BookStore.View.Auth;

namespace BookStore.View.Customer
{
    public partial class HomeWindow : Window
    {
        private readonly HomeController _controller;
        private bool _isExpanded = false; // Biến cờ kiểm soát trạng thái Xem tất cả / Thu gọn

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

            // Mặc định load 10 mục
            ProductListControl.ItemsSource = _controller.GetFeaturedProducts();
        }

        // Sự kiện khi bấm vào chữ "Xem tất cả →" / "Thu gọn ←"
        private void TxtViewAllProducts_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isExpanded)
            {
                ProductListControl.ItemsSource = _controller.GetAllProducts();
                txtViewAllProducts.Text = "Thu gọn ←";
                _isExpanded = true;
            }
            else
            {
                ProductListControl.ItemsSource = _controller.GetFeaturedProducts();
                txtViewAllProducts.Text = "Xem tất cả →";
                _isExpanded = false;
            }
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not TbProduct product) return;
            if (!EnsureLoggedIn()) return;

            UiGuard.Run(() => UiGuard.ShowResult(CartDAO.AddToCart(product.ProductId, 1), $"Đã thêm “{product.Name}” vào giỏ hàng."));
        }

        private void BtnSaveVoucher_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not TbVoucher voucher) return;
            if (!EnsureLoggedIn()) return;

            UiGuard.Run(() => UiGuard.ShowResult(SavedVoucherDAO.Save(voucher.VoucherId), $"Đã lưu mã {voucher.Code}."));
        }

        private bool EnsureLoggedIn()
        {
            if (SessionGuard.IsLoggedIn) return true;

            new LoginWindow().Show();
            Close();
            return false;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}