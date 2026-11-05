using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32; // Cần thiết để mở hộp thoại lưu file (SaveFileDialog)
using BookStore.Controllers;
using BookStore.Utilities;
using BookStore.Models;

namespace BookStore.View.Manager
{
    public partial class VoucherManagementWindow : Window
    {
        private readonly VoucherController _controller;

        public VoucherManagementWindow()
        {
            InitializeComponent();
            _controller = new VoucherController();

            // SỬA TẠI ĐÂY: Chỉ cho phép MANAGER truy cập, từ chối ADMIN và các quyền khác
            if (GlobalSession.Role != "MANAGER")
            {
                MessageBox.Show("Chức năng này chỉ dành riêng cho cấp Quản lý (MANAGER)!", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            dpValidUntil.SelectedDate = DateTime.Now.AddDays(7);
            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            dgVouchers.ItemsSource = _controller.GetVouchers(keyword);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        // BẤM VÀO LƯỚI ĐỂ CHUYỂN QUA CHẾ ĐỘ SỬA
        private void DgVouchers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgVouchers.SelectedItem is TbVoucher selectedVoucher)
            {
                txtVoucherId.Text = selectedVoucher.VoucherId.ToString();
                txtCode.Text = selectedVoucher.Code;
                txtDiscount.Text = selectedVoucher.DiscountValue.ToString("0");
                txtMinOrder.Text = selectedVoucher.MinOrderValue.ToString("0");
                // dpValidUntil.SelectedDate = selectedVoucher.ValidUntil;
            }
        }

        private void BtnClearForm_Click(object sender, RoutedEventArgs e)
        {
            txtVoucherId.Text = "0"; // Reset về chế độ Thêm mới
            txtCode.Text = "";
            txtDiscount.Text = "";
            txtMinOrder.Text = "";
            dgVouchers.SelectedItem = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id = int.Parse(txtVoucherId.Text);
                string code = txtCode.Text.Trim();
                decimal discount = decimal.Parse(txtDiscount.Text);
                decimal minOrder = string.IsNullOrEmpty(txtMinOrder.Text) ? 0 : decimal.Parse(txtMinOrder.Text);
                DateTime validUntil = dpValidUntil.SelectedDate ?? DateTime.Now;

                string result = _controller.SaveVoucher(id, code, discount, minOrder, validUntil);

                if (result == null)
                {
                    MessageBox.Show(id == 0 ? "Thêm mới thành công!" : "Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    BtnClearForm_Click(null, null);
                    LoadData();
                }
                else
                {
                    MessageBox.Show(result, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Vui lòng nhập đúng định dạng số tiền!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int voucherId)
            {
                _controller.ToggleVoucherStatus(voucherId);
                LoadData();
            }
        }

        // TÍNH NĂNG XUẤT CSV 
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
                    var list = _controller.GetVouchers();
                    var csv = new StringBuilder();

                    // Header của file CSV
                    csv.AppendLine("ID,Mã Code,Giảm giá,Đơn tối thiểu,Trạng thái");

                    // Chèn dữ liệu
                    foreach (var v in list)
                    {
                        csv.AppendLine($"{v.VoucherId},{v.Code},{v.DiscountValue:0},{v.MinOrderValue:0},{v.Status}");
                    }

                    // Ghi ra file (Dùng UTF8 để hỗ trợ tiếng Việt trong Excel)
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString(), Encoding.UTF8);

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