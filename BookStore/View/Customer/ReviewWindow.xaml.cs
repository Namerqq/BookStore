using BookStore.DAOs;
using BookStore.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BookStore.View.Customer
{
    public partial class ReviewWindow : Window
    {
        private readonly int _orderDetailId;

        public ReviewWindow(int orderDetailId)
        {
            InitializeComponent();
            _orderDetailId = orderDetailId;

            // Xử lý int? sang int an toàn với null-coalescing (?? 0)
            int currentAccountId = GlobalSession.AccountId ?? 0;

            if (currentAccountId == 0)
            {
                MessageBox.Show("Vui lòng đăng nhập trước khi đánh giá sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            if (!ReviewDAO.Instance.CanUserReview(currentAccountId, _orderDetailId, out string errorMsg))
            {
                MessageBox.Show(errorMsg, "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
            }
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            int currentAccountId = GlobalSession.AccountId ?? 0;
            if (currentAccountId == 0)
            {
                MessageBox.Show("Phiên đăng nhập không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string ratingStr = (cbRating.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "5";
            byte rating = byte.Parse(ratingStr);
            string comment = txtComment.Text.Trim();

            try
            {
                ReviewDAO.Instance.AddReview(currentAccountId, _orderDetailId, rating, comment);
                MessageBox.Show("Gửi đánh giá thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi đánh giá: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}