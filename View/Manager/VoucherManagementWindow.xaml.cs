using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using BookStore.Controllers;
using BookStore.Models;
using BookStore.Utilities;

namespace BookStore.View.Manager
{
    public partial class VoucherManagementWindow : Window
    {
        private readonly VoucherController _controller;

        public VoucherManagementWindow()
        {
            InitializeComponent();
            _controller = new VoucherController();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleManager)) return;
            ResetForm();
            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            dgVouchers.ItemsSource = _controller.GetVouchers(keyword);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtSearchPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (IsLoaded) LoadData();
        }

        private void CbDiscountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            string type = GetSelectedDiscountType();
            bool isPercent = type == "PERCENTAGE";
            lblDiscountValue.Text = isPercent ? "Mức giảm (%) *" : "Mức giảm (VNĐ) *";
            txtMaxDiscountCap.IsEnabled = isPercent;
            if (!isPercent) txtMaxDiscountCap.Text = string.Empty;
        }

        private string GetSelectedDiscountType()
        {
            return (cbDiscountType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "FIXED";
        }

        private void DgVouchers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgVouchers.SelectedItem is TbVoucher selectedVoucher)
            {
                txtVoucherId.Text = selectedVoucher.VoucherId.ToString();
                txtCode.Text = selectedVoucher.Code;

                foreach (ComboBoxItem item in cbDiscountType.Items)
                {
                    if (item.Tag?.ToString() == selectedVoucher.DiscountType)
                    {
                        cbDiscountType.SelectedItem = item;
                        break;
                    }
                }

                txtDiscount.Text = selectedVoucher.DiscountValue.ToString("0.##");
                txtMaxDiscountCap.Text = selectedVoucher.MaxDiscountCap.HasValue
                    ? selectedVoucher.MaxDiscountCap.Value.ToString("0")
                    : string.Empty;
                txtMinOrder.Text = selectedVoucher.MinOrderValue.ToString("0");
                dpStartDate.SelectedDate = selectedVoucher.StartDate;
                dpEndDate.SelectedDate = selectedVoucher.EndDate;
                txtMaxUsageLimit.Text = selectedVoucher.MaxUsageLimit.ToString();
                txtPerCustomerLimit.Text = selectedVoucher.PerCustomerLimit.ToString();
            }
        }

        private void ResetForm()
        {
            txtVoucherId.Text = "0";
            txtCode.Text = string.Empty;
            cbDiscountType.SelectedIndex = 0;
            txtDiscount.Text = string.Empty;
            txtMaxDiscountCap.Text = string.Empty;
            txtMinOrder.Text = "0";
            dpStartDate.SelectedDate = DateTime.Now.Date;
            dpEndDate.SelectedDate = DateTime.Now.Date.AddDays(7);
            txtMaxUsageLimit.Text = "100";
            txtPerCustomerLimit.Text = "1";
            dgVouchers.SelectedItem = null;
        }

        private void BtnClearForm_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtVoucherId.Text, out int id)) id = 0;

            string code = txtCode.Text.Trim();
            string discountType = GetSelectedDiscountType();

            if (!decimal.TryParse(txtDiscount.Text.Trim(), out decimal discount))
            {
                MessageBox.Show("Mức giảm giá phải là số hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal? maxCap = null;
            if (discountType == "PERCENTAGE" && !string.IsNullOrWhiteSpace(txtMaxDiscountCap.Text))
            {
                if (!decimal.TryParse(txtMaxDiscountCap.Text.Trim(), out decimal capVal))
                {
                    MessageBox.Show("Giảm tối đa phải là số hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                maxCap = capVal;
            }

            decimal minOrder = 0;
            if (!string.IsNullOrWhiteSpace(txtMinOrder.Text))
            {
                if (!decimal.TryParse(txtMinOrder.Text.Trim(), out minOrder))
                {
                    MessageBox.Show("Đơn tối thiểu phải là số hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày bắt đầu và ngày kết thúc!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtMaxUsageLimit.Text.Trim(), out int maxUsage)
                || !int.TryParse(txtPerCustomerLimit.Text.Trim(), out int perCustomer))
            {
                MessageBox.Show("Giới hạn lượt dùng phải là số nguyên hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? result = _controller.SaveVoucher(
                id,
                code,
                discountType,
                discount,
                maxCap,
                minOrder,
                dpStartDate.SelectedDate.Value,
                dpEndDate.SelectedDate.Value,
                maxUsage,
                perCustomer);

            if (result == null)
            {
                MessageBox.Show(id == 0 ? "Thêm mới thành công!" : "Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                LoadData();
            }
            else
            {
                MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int voucherId)
            {
                string? err = _controller.ToggleVoucherStatus(voucherId);
                if (err != null)
                {
                    MessageBox.Show(err, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                LoadData();
            }
        }

        private void BtnExportCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"DanhSachVoucher_{DateTime.Now:ddMMyyyy}.csv",
                Title = "Xuất file danh sách Voucher"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _controller.ExportVouchersToCsv(saveFileDialog.FileName, txtSearch.Text.Trim());
                    MessageBox.Show("Xuất file CSV thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
