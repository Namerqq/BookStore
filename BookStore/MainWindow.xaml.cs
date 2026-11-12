using System.Windows;
using BookStore.Utilities;
using BookStore.View.Admin;
using BookStore.View.Customer;

namespace BookStore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => NavigateBasedOnRole();
        }

        private void NavigateBasedOnRole()
        {
            Window target = SessionGuard.HasRole(SessionGuard.RoleAdmin, SessionGuard.RoleManager, SessionGuard.RoleStaff)
                ? new AdminLayoutWindow()
                : new HomeWindow();

            target.Show();
            Close();
        }
    }
}
