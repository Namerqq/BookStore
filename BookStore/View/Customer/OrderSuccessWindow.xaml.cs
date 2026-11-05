using System.Windows;
using System.Windows.Media.Imaging;
using BookStore.Utilities;

namespace BookStore.View.Customer
{
    /// <summary>Màn hình đặt hàng thành công, có mã VietQR nếu thanh toán online (mục 4.2.4).</summary>
    public partial class OrderSuccessWindow : Window
    {
        private readonly int _orderId;
        private readonly string _orderCode;
        private readonly decimal _total;
        private readonly string _paymentMethod;
        private readonly bool _showViewOrder;

        /// <param name="showViewOrder">false khi mở lại mã QR từ chính màn hình chi tiết đơn.</param>
        public OrderSuccessWindow(int orderId, string orderCode, decimal total, string paymentMethod, bool showViewOrder = true)
        {
            InitializeComponent();
            _orderId = orderId;
            _orderCode = orderCode;
            _total = total;
            _paymentMethod = paymentMethod;
            _showViewOrder = showViewOrder;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool isQr = _paymentMethod == PaymentMethods.OnlineQr;

            txtHeading.Text = _showViewOrder ? "Đặt hàng thành công" : "Thanh toán đơn hàng";
            txtOrderCode.Text = $"Mã đơn: {_orderCode}";
            txtAmount.Text = MoneyText.Format(_total);
            btnViewOrder.Visibility = _showViewOrder ? Visibility.Visible : Visibility.Collapsed;

            if (isQr)
            {
                pnlQr.Visibility = Visibility.Visible;
                txtBankInfo.Text = $"Ngân hàng {PaymentConfig.BankCode}, số tài khoản {PaymentConfig.AccountNumber}\n"
                                 + $"Chủ tài khoản: {PaymentConfig.AccountName}\nNội dung chuyển khoản: {_orderCode}";
                txtDemoWarning.Visibility = PaymentConfig.IsDemoAccount ? Visibility.Visible : Visibility.Collapsed;
                txtNote.Text = "Đơn sẽ được xử lý sau khi nhân viên đối chiếu sao kê và xác nhận đã nhận tiền.";
                LoadQrImage();
            }
            else
            {
                txtNote.Text = "Bạn thanh toán khi nhận hàng. Nhân viên sẽ gọi xác nhận trước khi giao.";
            }
        }

        private void LoadQrImage()
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(PaymentConfig.BuildQrUrl(_total, _orderCode));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.DownloadFailed += (_, _) => ShowQrError();
                imgQr.Source = bitmap;
            }
            catch (Exception)
            {
                ShowQrError();
            }
        }

        private void ShowQrError()
        {
            imgQr.Visibility = Visibility.Collapsed;
            txtQrError.Visibility = Visibility.Visible;
        }

        private void BtnViewOrder_Click(object sender, RoutedEventArgs e)
        {
            new OrderDetailWindow(_orderId) { Owner = this }.ShowDialog();
        }
    }
}
