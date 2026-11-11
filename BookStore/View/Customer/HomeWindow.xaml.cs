using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BookStore.Controllers;
using BookStore.DAO;
using BookStore.Models;
using BookStore.Utilities;
using BookStore.View.Auth;

namespace BookStore.View.Customer
{
    public partial class HomeWindow : Window
    {
        private static readonly Brush ActiveBorderBrush = CreateFrozenBrush(0xC2, 0x54, 0x2D);
        private static readonly Brush ActiveBackgroundBrush = CreateFrozenBrush(0xFF, 0xF8, 0xF5);
        private static readonly Brush DefaultBorderBrush = CreateFrozenBrush(0xE0, 0xE0, 0xE0);

        private readonly HomeController _controller;
        private bool _isExpanded = false; // Biến cờ kiểm soát trạng thái Xem tất cả / Thu gọn
        private int? _selectedCategoryId = null;

        public HomeWindow()
        {
            InitializeComponent();
            _controller = new HomeController();
            LoadDataFromDatabase();
        }

        private static Brush CreateFrozenBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        private void LoadDataFromDatabase()
        {
            VoucherListControl.ItemsSource = _controller.GetActiveVouchers();
            CategoryListControl.ItemsSource = _controller.GetCategories();
            ShowDefaultProducts(expanded: false);
        }

        // Sự kiện khi bấm vào ô thể loại ở phần Danh mục
        private void BtnCategory_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not TbCategory category) return;

            if (_selectedCategoryId == category.CategoryId)
            {
                ShowDefaultProducts(expanded: false);
                return;
            }

            _selectedCategoryId = category.CategoryId;
            _isExpanded = false;
            txtProductSubtitle.Text = "DANH MỤC ĐÃ CHỌN";
            txtProductTitle.Text = category.Name;
            txtViewAllProducts.Text = "Xem tất cả →";
            ApplyProducts(_controller.GetProductsByCategory(category.CategoryId));
            UpdateCategoryHighlight();
        }

        // Sự kiện khi bấm vào chữ "Xem tất cả →" / "Thu gọn ←"
        private void TxtViewAllProducts_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCategoryId.HasValue)
            {
                ShowDefaultProducts(expanded: true);
                return;
            }

            ShowDefaultProducts(expanded: !_isExpanded);
        }

        private void ShowDefaultProducts(bool expanded)
        {
            _selectedCategoryId = null;
            _isExpanded = expanded;
            txtProductSubtitle.Text = "BÁN CHẠY";
            txtProductTitle.Text = "Được yêu thích nhất tuần này";
            txtViewAllProducts.Text = expanded ? "Thu gọn ←" : "Xem tất cả →";
            ApplyProducts(expanded ? _controller.GetAllProducts() : _controller.GetFeaturedProducts());
            UpdateCategoryHighlight();
        }

        private void ApplyProducts(List<TbProduct> products)
        {
            ProductListControl.ItemsSource = products;
            txtEmptyProducts.Visibility = products.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateCategoryHighlight()
        {
            for (int i = 0; i < CategoryListControl.Items.Count; i++)
            {
                if (CategoryListControl.Items[i] is not TbCategory cat) continue;
                if (CategoryListControl.ItemContainerGenerator.ContainerFromIndex(i) is not DependencyObject container) continue;
                if (VisualTreeHelper.GetChildrenCount(container) == 0) continue;
                if (VisualTreeHelper.GetChild(container, 0) is not Button btn) continue;

                bool isActive = cat.CategoryId == _selectedCategoryId;
                btn.BorderBrush = isActive ? ActiveBorderBrush : DefaultBorderBrush;
                btn.BorderThickness = new Thickness(isActive ? 2 : 1);
                btn.Background = isActive ? ActiveBackgroundBrush : Brushes.White;
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