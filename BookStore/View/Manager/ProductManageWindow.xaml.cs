using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using BookStore.DAOs;
using BookStore.Models;

namespace BookStore.View.Manager
{
    public partial class ProductManageWindow : Window
    {
        private string? _selectedImagePath = null;
        private int _currentProductId = 0;

        public ProductManageWindow()
        {
            InitializeComponent();
            LoadCategories();
            LoadDataGrid();
        }

        private void LoadCategories()
        {
            using var context = new DbSe2038BookymartFContext();
            cbCategory.ItemsSource = context.TbCategories.ToList();
        }

        private void LoadDataGrid()
        {
            dgProducts.ItemsSource = ProductDAO.Instance.GetProducts("", null, "ALL", "NEWEST");
        }

        private void TxtSku_LostFocus(object sender, RoutedEventArgs e)
        {
            string sku = txtSku.Text.Trim();
            if (!string.IsNullOrEmpty(sku) && ProductDAO.Instance.IsSkuExists(sku, _currentProductId))
            {
                MessageBox.Show($"SKU '{sku}' đã tồn tại!", "Cảnh báo trùng", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TxtPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtPrice.Text, out decimal price))
            {
                lblVatPrice.Text = $"Giá bán sau VAT (x1.1): {price * 1.1m:N0} VNĐ";
            }
        }

        private void BtnBrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg" };
            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                lblImagePath.Text = _selectedImagePath;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cbCategory.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục!");
                return;
            }

            var p = new TbProduct
            {
                Sku = txtSku.Text.Trim(),
                Name = txtName.Text.Trim(),
                UnitCost = decimal.TryParse(txtUnitCost.Text, out decimal uc) ? uc : 0,
                Price = decimal.TryParse(txtPrice.Text, out decimal pr) ? pr : 0,
                StockQuantity = int.TryParse(txtStock.Text, out int st) ? st : 0,
                CategoryId = (int)cbCategory.SelectedValue,
                ProductType = (cbProductType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "BOOK",
                Status = "ACTIVE"
            };
            ProductDAO.Instance.AddProduct(p, _selectedImagePath);
            MessageBox.Show("Thêm sản phẩm thành công!");
            LoadDataGrid();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProductId == 0) return;
            if (cbCategory.SelectedValue == null) return;

            var p = new TbProduct
            {
                ProductId = _currentProductId,
                Sku = txtSku.Text.Trim(),
                Name = txtName.Text.Trim(),
                UnitCost = decimal.TryParse(txtUnitCost.Text, out decimal uc) ? uc : 0,
                Price = decimal.TryParse(txtPrice.Text, out decimal pr) ? pr : 0,
                StockQuantity = int.TryParse(txtStock.Text, out int st) ? st : 0,
                CategoryId = (int)cbCategory.SelectedValue,
                ProductType = (cbProductType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "BOOK"
            };
            ProductDAO.Instance.UpdateProduct(p, _selectedImagePath);
            MessageBox.Show("Cập nhật sản phẩm thành công!");
            LoadDataGrid();
        }

        private void BtnDiscontinue_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProductId != 0)
            {
                ProductDAO.Instance.DiscontinueProduct(_currentProductId);
                LoadDataGrid();
            }
        }

        private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProducts.SelectedItem is TbProduct p)
            {
                _currentProductId = p.ProductId;
                txtSku.Text = p.Sku;
                txtName.Text = p.Name;
                txtUnitCost.Text = p.UnitCost.ToString();
                txtPrice.Text = (p.Price / 1.1m).ToString("F0");
                txtStock.Text = p.StockQuantity.ToString();
                cbCategory.SelectedValue = p.CategoryId;
                lblVatPrice.Text = $"Giá bán sau VAT (x1.1): {p.Price:N0} VNĐ";
            }
        }
    }
}