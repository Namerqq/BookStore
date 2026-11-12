using System.Windows;

namespace BookStore.View.Shared
{
    public partial class ReasonDialog : Window
    {
        public string Reason => txtReason.Text.Trim();

        public ReasonDialog(string title, string prompt, int maxLength)
        {
            InitializeComponent();
            Title = title;
            txtPrompt.Text = prompt;
            txtReason.MaxLength = maxLength;
        }

        /// <summary>Trả về lý do đã nhập, hoặc null nếu người dùng đóng hộp thoại.</summary>
        public static string? Ask(Window owner, string title, string prompt, int maxLength = 255)
        {
            var dialog = new ReasonDialog(title, prompt, maxLength) { Owner = owner };
            return dialog.ShowDialog() == true ? dialog.Reason : null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtReason.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Vui lòng nhập lý do.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }
    }
}
