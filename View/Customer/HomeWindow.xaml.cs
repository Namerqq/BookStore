using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _isExpanded = false;
        private int? _selectedCategoryId = null;
        private string _searchKeyword = string.Empty;
        private bool _initialized = false;

        public HomeWindow() : this(null, null)
        {
        }

        public HomeWindow(string? initialKeyword, string? initialType)
        {
            InitializeComponent();
            _controller = new HomeController();

            HeaderBar.SearchRequested += OnHeaderSearchRequested;
            HeaderBar.ProductTypeRequested += OnHeaderProductTypeRequested;
            HeaderBar.PromotionsRequested += () => VoucherSection.BringIntoView();

            if (!string.IsNullOrWhiteSpace(initialKeyword))
            {
                _searchKeyword = initialKeyword.Trim();
                HeaderBar.SetSearchText(_searchKeyword);
            }

            if (!string.IsNullOrWhiteSpace(initialType))
            {
                SelectTypeCombo(initialType);
            }

            _initialized = true;
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
            RefreshProductList();
        }

        public void ResetToHome()
        {
            _selectedCategoryId = null;
            _isExpanded = false;
            _searchKeyword = string.Empty;
            HeaderBar.SetSearchText(string.Empty);
            cboTypeFilter.SelectedIndex = 0;
            cboSort.SelectedIndex = 0;
            RefreshProductList();
            UpdateCategoryHighlight();
        }

        private void OnHeaderSearchRequested(string keyword)
        {
            _searchKeyword = keyword;
            RefreshProductList();
        }

        private void OnHeaderProductTypeRequested(string productType)
        {
            _selectedCategoryId = null;
            SelectTypeCombo(productType);
            UpdateCategoryHighlight();
            RefreshProductList();
            ProductSection.BringIntoView();
        }

        private void SelectTypeCombo(string productType)
        {
            foreach (ComboBoxItem item in cboTypeFilter.Items)
            {
                if (item.Tag?.ToString() == productType)
                {
                    cboTypeFilter.SelectedItem = item;
                    break;
                }
            }
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_initialized) RefreshProductList();
        }

        private void BtnCategory_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not TbCategory category) return;

            if (_selectedCategoryId == category.CategoryId)
            {
                _selectedCategoryId = null;
            }
            else
            {
                _selectedCategoryId = category.CategoryId;
            }

            RefreshProductList();
            UpdateCategoryHighlight();
        }

        private void TxtViewAllProducts_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCategoryId.HasValue)
            {
                _selectedCategoryId = null;
                _isExpanded = true;
                UpdateCategoryHighlight();
                RefreshProductList();
                return;
            }

            _isExpanded = !_isExpanded;
            RefreshProductList();
        }

        private void BtnShopNow_Click(object sender, RoutedEventArgs e)
        {
            ProductSection.BringIntoView();
        }

        private void RefreshProductList()
        {
            string typeFilter = (cboTypeFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "ALL";
            string sortBy = (cboSort.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "DEFAULT";

            bool hasFilter = !string.IsNullOrWhiteSpace(_searchKeyword)
                             || _selectedCategoryId.HasValue
                             || typeFilter != "ALL"
                             || sortBy != "DEFAULT";

            List<TbProduct> products;
            if (hasFilter)
            {
                products = _controller.SearchProducts(_searchKeyword, _selectedCategoryId, typeFilter, sortBy);
                txtProductSubtitle.Text = !string.IsNullOrWhiteSpace(_searchKeyword) ? "KẾT QUẢ TÌM KIẾM" : "BỘ LỌC SẢN PHẨM";
                txtProductTitle.Text = !string.IsNullOrWhiteSpace(_searchKeyword)
                    ? $"Từ khóa: “{_searchKeyword}” ({products.Count})"
                    : $"Tìm thấy {products.Count} sản phẩm";
                txtViewAllProducts.Text = "Xem tất cả →";
            }
            else if (_isExpanded)
            {
                products = _controller.GetAllProducts();
                txtProductSubtitle.Text = "TẤT CẢ SẢN PHẨM";
                txtProductTitle.Text = $"Toàn bộ cửa hàng ({products.Count})";
                txtViewAllProducts.Text = "Thu gọn ←";
            }
            else
            {
                products = _controller.GetFeaturedProducts();
                txtProductSubtitle.Text = "BÁN CHẠY";
                txtProductTitle.Text = "Được yêu thích nhất tuần này";
                txtViewAllProducts.Text = "Xem tất cả →";
            }

            ApplyProducts(products);
        }

        private void ApplyProducts(List<TbProduct> products)
        {
            var ratingStats = ReviewDAO.Instance.GetProductRatingStatsBatch(products.Select(p => p.ProductId));

            var cards = products.Select(p =>
            {
                ratingStats.TryGetValue(p.ProductId, out var stats);
                var (avg, total) = stats;
                return new HomeProductCard
                {
                    Product = p,
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ProductTypeLabel = p.ProductType == "BOOK" ? "SÁCH" : "VĂN PHÒNG PHẨM",
                    Subtitle = p.ProductType == "BOOK" ? (p.Author ?? "Đang cập nhật tác giả") : (p.Brand ?? "Văn phòng phẩm"),
                    ThumbnailUrl = p.ThumbnailUrl,
                    PriceText = MoneyText.Format(p.Price),
                    RatingDisplay = total > 0 ? $"★ {avg:0.0} ({total} đánh giá)" : "Chưa có đánh giá"
                };
            }).ToList();

            ProductListControl.ItemsSource = cards;
            txtEmptyProducts.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
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

        private void ProductCard_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not HomeProductCard card) return;

            new ProductDetailWindow(card.ProductId) { Owner = this }.ShowDialog();
            RefreshProductList();
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if ((sender as FrameworkElement)?.DataContext is not HomeProductCard card) return;
            if (!EnsureLoggedIn()) return;

            UiGuard.Run(() => UiGuard.ShowResult(CartDAO.AddToCart(card.ProductId, 1), $"Đã thêm “{card.Name}” vào giỏ hàng."));
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

        private sealed class HomeProductCard
        {
            public TbProduct Product { get; init; } = null!;
            public int ProductId { get; init; }
            public string Name { get; init; } = string.Empty;
            public string ProductTypeLabel { get; init; } = string.Empty;
            public string Subtitle { get; init; } = string.Empty;
            public string? ThumbnailUrl { get; init; }
            public string PriceText { get; init; } = string.Empty;
            public string RatingDisplay { get; init; } = string.Empty;
        }
    }
}
