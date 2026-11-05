using System.Windows;
using BookStore.Controllers;
using BookStore.DAO;
using BookStore.Models;
using BookStore.Utilities;
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

        // Nút "+" trên thẻ sản phẩm: thêm 1 sản phẩm vào giỏ (CartDAO của phần 3)
        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not TbProduct product) return;
            if (!EnsureLoggedIn()) return;

            UiGuard.Run(() => UiGuard.ShowResult(CartDAO.AddToCart(product.ProductId, 1), $"Đã thêm “{product.Name}” vào giỏ hàng."));
        }

        // Nút "Lưu mã" trên thẻ voucher (tbSavedVoucher của phần 3)
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

        // Sự kiện khi bấm nút Login trên Header
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close(); // Đóng trang chủ hiện tại lại
        }
    }
}