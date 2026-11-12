using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BookStore.DAO;
using BookStore.Models;
using BookStore.Utilities;
using BookStore.View.Auth;

namespace BookStore.View.Customer
{
    /// <summary>GUE-03: chi tiết sản phẩm, hiển thị đánh giá và thêm vào giỏ qua CartDAO.AddToCart.</summary>
    public partial class ProductDetailWindow : Window
    {
        private readonly int _productId;
        private int _maxStock;

        public ProductDetailWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProduct();
        }

        private void LoadProduct()
        {
            TbProduct? product = ProductDAO.Instance.GetProductById(_productId);
            if (product == null)
            {
                MessageBox.Show("Không tìm thấy sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            Title = $"{product.Name} — Booky Mart";
            txtTypeBadge.Text = product.ProductType == "BOOK" ? "SÁCH" : "VĂN PHÒNG PHẨM";
            txtCategory.Text = product.Category?.Name ?? string.Empty;
            txtSku.Text = $"SKU: {product.Sku}";
            txtName.Text = product.Name;
            txtPrice.Text = MoneyText.Format(product.Price);
            txtDescription.Text = string.IsNullOrWhiteSpace(product.Description)
                ? "Chưa có mô tả chi tiết cho sản phẩm này."
                : product.Description;

            _maxStock = product.StockQuantity;
            bool available = product.Status == "ACTIVE" && product.StockQuantity > 0;
            if (product.Status != "ACTIVE")
            {
                txtStockInfo.Text = "Ngừng kinh doanh";
                txtStockInfo.Foreground = Brushes.Firebrick;
            }
            else if (product.StockQuantity <= 0)
            {
                txtStockInfo.Text = "Tạm hết hàng";
                txtStockInfo.Foreground = Brushes.Firebrick;
            }
            else
            {
                txtStockInfo.Text = $"Còn {product.StockQuantity} sản phẩm";
                txtStockInfo.Foreground = Brushes.SeaGreen;
            }

            btnAddToCart.IsEnabled = available;

            if (product.ProductType == "BOOK")
            {
                SetSpecLine(txtSpec1, "Tác giả", product.Author);
                SetSpecLine(txtSpec2, "Nhà xuất bản",
                    product.PublicationYear.HasValue && !string.IsNullOrWhiteSpace(product.Publisher)
                        ? $"{product.Publisher} ({product.PublicationYear})"
                        : product.Publisher ?? product.PublicationYear?.ToString());
                SetSpecLine(txtSpec3, "ISBN", product.Isbn);
            }
            else
            {
                SetSpecLine(txtSpec1, "Thương hiệu", product.Brand);
                SetSpecLine(txtSpec2, "Thông số", product.Specifications);
                txtSpec3.Visibility = Visibility.Collapsed;
            }

            txtWeight.Text = product.WeightGrams.HasValue && product.WeightGrams > 0
                ? $"Cân nặng: {product.WeightGrams}g"
                : "Cân nặng: Đang cập nhật";

            LoadImage(product.ThumbnailUrl);
            LoadReviews();
        }

        private static void SetSpecLine(System.Windows.Controls.TextBlock block, string label, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                block.Visibility = Visibility.Collapsed;
                return;
            }
            block.Text = $"{label}: {value}";
            block.Visibility = Visibility.Visible;
        }

        private void LoadImage(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgProduct.Source = bitmap;
                txtNoImage.Visibility = Visibility.Collapsed;
            }
            catch
            {
                txtNoImage.Visibility = Visibility.Visible;
            }
        }

        private void LoadReviews()
        {
            var stats = ReviewDAO.Instance.GetProductRatingStats(_productId);
            txtRatingSummary.Text = stats.TotalReviews > 0
                ? $"★ {stats.AvgRating:0.0}/5 ({stats.TotalReviews} đánh giá)"
                : "Chưa có đánh giá";

            var reviews = ReviewDAO.Instance.GetProductReviews(_productId);
            icReviews.ItemsSource = reviews;
            txtReviewsHeading.Text = $"Đánh giá từ khách hàng ({reviews.Count})";
            txtNoReviews.Visibility = reviews.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private int GetQuantity()
        {
            return int.TryParse(txtQuantity.Text.Trim(), out int q) && q > 0 ? q : 1;
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            int q = GetQuantity();
            if (q > 1) txtQuantity.Text = (q - 1).ToString();
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            int q = GetQuantity();
            if (_maxStock > 0 && q < _maxStock)
                txtQuantity.Text = (q + 1).ToString();
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionGuard.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.", "Yêu cầu đăng nhập",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                new LoginWindow().Show();
                Owner?.Close();
                Close();
                return;
            }

            if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên lớn hơn 0.", "Dữ liệu không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UiGuard.Run(() => UiGuard.ShowResult(
                CartDAO.AddToCart(_productId, quantity),
                "Đã thêm sản phẩm vào giỏ hàng."));
        }
    }
}
