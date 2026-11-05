using System.Windows;

namespace BookStore.View.Customer
{
    /// <summary>
    /// BẢN TẠM của phần 2 (CUS-09) để phần 3 build được.
    /// Người B thay toàn bộ file này, giữ nguyên constructor nhận orderDetailId.
    /// </summary>
    public partial class ReviewWindow : Window
    {
        public int OrderDetailId { get; }

        public ReviewWindow(int orderDetailId)
        {
            InitializeComponent();
            OrderDetailId = orderDetailId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
