using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BookStore.DAO;
using BookStore.Utilities;

namespace BookStore.View.Customer
{
    /// <summary>CUS-09: Đánh giá sản phẩm theo orderDetailId (chỉ khách hàng sở hữu đơn DELIVERED và mỗi dòng 1 lần).</summary>
    public partial class ReviewWindow : Window
    {
        public int OrderDetailId { get; }
        private byte _selectedRating = 5;

        public ReviewWindow(int orderDetailId)
        {
            InitializeComponent();
            OrderDetailId = orderDetailId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UiGuard.RequireRole(this, SessionGuard.RoleCustomer)) return;

            if (!ReviewDAO.Instance.CanUserReview(SessionGuard.CurrentAccountId, OrderDetailId, out string error))
            {
                MessageBox.Show(error, "Không thể đánh giá", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            txtProductName.Text = ReviewDAO.Instance.GetOrderDetailProductName(OrderDetailId) ?? $"Dòng đơn hàng #{OrderDetailId}";
            RenderStars();
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && byte.TryParse(btn.Tag?.ToString(), out byte rating))
            {
                _selectedRating = rating;
                RenderStars();
            }
        }

        private void RenderStars()
        {
            Button[] stars = { btnStar1, btnStar2, btnStar3, btnStar4, btnStar5 };
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].Foreground = (i + 1) <= _selectedRating ? Brushes.DarkOrange : Brushes.LightGray;
            }

            txtRatingLabel.Text = _selectedRating switch
            {
                1 => "1/5 - Rất tệ",
                2 => "2/5 - Không hài lòng",
                3 => "3/5 - Bình thường",
                4 => "4/5 - Hài lòng",
                _ => "5/5 - Tuyệt vời"
            };
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string comment = txtComment.Text.Trim();
            if (comment.Length > 1000)
            {
                MessageBox.Show("Nội dung đánh giá tối đa 1000 ký tự.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UiGuard.Run(() =>
            {
                string? err = ReviewDAO.Instance.AddReview(
                    SessionGuard.CurrentAccountId,
                    OrderDetailId,
                    _selectedRating,
                    comment);

                if (UiGuard.ShowResult(err, "Cảm ơn bạn đã đánh giá sản phẩm!"))
                {
                    DialogResult = true;
                }
            });
        }
    }
}
