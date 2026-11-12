using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using BookStore.DAO;
using BookStore.Models;
using BookStore.Utilities;

namespace BookStore.View.Manager
{
    public partial class ProductManageWindow : Window
    {
        private string? _selectedImagePath = null;
        private int _currentProductId = 0;

        public ProductManageWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleManager)) return;
            LoadCategories();
            LoadDataGrid();
        }

        private void LoadCategories()
        {
            using var context = new DbSe2038BookymartFContext();
            var categories = context.TbCategories.OrderBy(c => c.Name).ToList();
            cbCategory.ItemsSource = categories;
            if (categories.Count > 0 && cbCategory.SelectedIndex < 0)
                cbCategory.SelectedIndex = 0;
        }

        private void LoadDataGrid()
        {
            dgProducts.ItemsSource = ProductDAO.Instance.GetAllProductsForManager(txtManagerSearch.Text);
        }

        private void TxtManagerSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded) LoadDataGrid();
        }

        private void CbProductType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            string type = (cbProductType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "BOOK";
            bool isBook = type == "BOOK";
            pnlBookFields.Visibility = isBook ? Visibility.Visible : Visibility.Collapsed;
            pnlStationeryFields.Visibility = isBook ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CheckDuplicateSkuUi()
        {
            string sku = txtSku.Text.Trim();
            if (!string.IsNullOrEmpty(sku) && ProductDAO.Instance.IsSkuExists(sku, _currentProductId))
            {
                lblSkuWarning.Text = $"⚠ SKU '{sku}' đã tồn tại trong hệ thống!";
                lblSkuWarning.Visibility = Visibility.Visible;
            }
            else
            {
                lblSkuWarning.Visibility = Visibility.Collapsed;
            }
        }

        private void TxtSku_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckDuplicateSkuUi();
        }

        private void TxtSku_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded) CheckDuplicateSkuUi();
        }

        private void UpdateVatPreview()
        {
            if (decimal.TryParse(txtPrice.Text.Trim(), out decimal price) && price > 0)
            {
                decimal vatPrice = Math.Round(price * 1.1m, 0, MidpointRounding.AwayFromZero);
                lblVatPrice.Text = $"Giá bán sau VAT (x1.1): {vatPrice:N0} VNĐ";
            }
            else
            {
                lblVatPrice.Text = "Giá bán sau VAT (x1.1): 0 VNĐ";
            }
        }

        private void TxtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded) UpdateVatPreview();
        }

        private void TxtPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateVatPreview();
        }

        private void BtnBrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp"
            };
            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                lblImagePath.Text = _selectedImagePath;
            }
        }

        private TbProduct? BuildValidatedProduct()
        {
            string sku = txtSku.Text.Trim();
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(sku) || sku.Length > 50)
            {
                MessageBox.Show("Mã SKU là bắt buộc và tối đa 50 ký tự!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (ProductDAO.Instance.IsSkuExists(sku, _currentProductId))
            {
                MessageBox.Show($"Mã SKU '{sku}' đã tồn tại! Vui lòng nhập SKU khác.", "Trùng SKU", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (string.IsNullOrEmpty(name) || name.Length > 200)
            {
                MessageBox.Show("Tên sản phẩm là bắt buộc và tối đa 200 ký tự!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (cbCategory.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (!decimal.TryParse(txtUnitCost.Text.Trim(), out decimal unitCost) || unitCost < 0)
            {
                MessageBox.Show("Giá vốn phải là số >= 0!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal preVatPrice) || preVatPrice <= 0)
            {
                MessageBox.Show("Giá bán trước VAT phải là số > 0!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (!int.TryParse(txtStock.Text.Trim(), out int stock) || stock < 0)
            {
                MessageBox.Show("Số lượng tồn kho phải là số nguyên >= 0!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            int? weightGrams = null;
            if (!string.IsNullOrWhiteSpace(txtWeight.Text))
            {
                if (!int.TryParse(txtWeight.Text.Trim(), out int w) || w < 0)
                {
                    MessageBox.Show("Cân nặng (gram) phải là số nguyên >= 0!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
                weightGrams = w;
            }

            int? pubYear = null;
            string productType = (cbProductType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "BOOK";
            if (productType == "BOOK" && !string.IsNullOrWhiteSpace(txtPublicationYear.Text))
            {
                if (!int.TryParse(txtPublicationYear.Text.Trim(), out int y) || y < 1900 || y > DateTime.Now.Year + 1)
                {
                    MessageBox.Show("Năm xuất bản không hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
                pubYear = y;
            }

            return new TbProduct
            {
                ProductId = _currentProductId,
                Sku = sku,
                Name = name,
                UnitCost = unitCost,
                Price = preVatPrice,
                StockQuantity = stock,
                WeightGrams = weightGrams,
                CategoryId = (int)cbCategory.SelectedValue,
                ProductType = productType,
                Isbn = productType == "BOOK" ? txtIsbn.Text.Trim() : null,
                Author = productType == "BOOK" ? txtAuthor.Text.Trim() : null,
                Publisher = productType == "BOOK" ? txtPublisher.Text.Trim() : null,
                PublicationYear = pubYear,
                Brand = productType == "STATIONERY" ? txtBrand.Text.Trim() : null,
                Specifications = productType == "STATIONERY" ? txtSpecifications.Text.Trim() : null,
                Description = txtDescription.Text.Trim(),
                Status = "ACTIVE"
            };
        }

        private void ClearForm()
        {
            _currentProductId = 0;
            _selectedImagePath = null;
            txtSku.Clear();
            lblSkuWarning.Visibility = Visibility.Collapsed;
            txtName.Clear();
            txtUnitCost.Clear();
            txtPrice.Clear();
            lblVatPrice.Text = "Giá bán sau VAT (x1.1): 0 VNĐ";
            txtStock.Clear();
            txtWeight.Text = "300";
            txtAuthor.Clear();
            txtIsbn.Clear();
            txtPublisher.Clear();
            txtPublicationYear.Clear();
            txtBrand.Clear();
            txtSpecifications.Clear();
            txtDescription.Clear();
            lblImagePath.Text = "Chưa chọn file";
            cbProductType.SelectedIndex = 0;
            if (cbCategory.Items.Count > 0) cbCategory.SelectedIndex = 0;
            dgProducts.SelectedItem = null;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            _currentProductId = 0;
            var product = BuildValidatedProduct();
            if (product == null) return;

            string? err = ProductDAO.Instance.AddProduct(product, _selectedImagePath);
            if (err != null)
            {
                MessageBox.Show(err, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Thêm sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm();
            LoadDataGrid();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProductId == 0)
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm trong danh sách để cập nhật!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var product = BuildValidatedProduct();
            if (product == null) return;

            string? err = ProductDAO.Instance.UpdateProduct(product, _selectedImagePath);
            if (err != null)
            {
                MessageBox.Show(err, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Cập nhật sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm();
            LoadDataGrid();
        }

        private void BtnDiscontinue_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProductId == 0)
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm để thay đổi trạng thái!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!UiGuard.Confirm("Bạn có chắc chắn muốn thay đổi trạng thái kinh doanh của sản phẩm này?"))
                return;

            string? err = ProductDAO.Instance.DiscontinueProduct(_currentProductId);
            if (err != null)
            {
                MessageBox.Show(err, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ClearForm();
            LoadDataGrid();
        }

        private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProducts.SelectedItem is TbProduct p)
            {
                _currentProductId = p.ProductId;
                _selectedImagePath = null;
                txtSku.Text = p.Sku;
                txtName.Text = p.Name;
                txtUnitCost.Text = p.UnitCost.ToString("0");
                txtPrice.Text = Math.Round(p.Price / 1.1m, 2).ToString("0.##");
                txtStock.Text = p.StockQuantity.ToString();
                txtWeight.Text = (p.WeightGrams ?? 0).ToString();
                cbCategory.SelectedValue = p.CategoryId;

                foreach (ComboBoxItem item in cbProductType.Items)
                {
                    if (item.Tag?.ToString() == p.ProductType)
                    {
                        cbProductType.SelectedItem = item;
                        break;
                    }
                }

                txtAuthor.Text = p.Author ?? string.Empty;
                txtIsbn.Text = p.Isbn ?? string.Empty;
                txtPublisher.Text = p.Publisher ?? string.Empty;
                txtPublicationYear.Text = p.PublicationYear?.ToString() ?? string.Empty;
                txtBrand.Text = p.Brand ?? string.Empty;
                txtSpecifications.Text = p.Specifications ?? string.Empty;
                txtDescription.Text = p.Description ?? string.Empty;
                lblImagePath.Text = p.ThumbnailUrl ?? "Chưa có ảnh";
                lblVatPrice.Text = $"Giá bán sau VAT (x1.1): {p.Price:N0} VNĐ";
                lblSkuWarning.Visibility = Visibility.Collapsed;
            }
        }
    }
}
