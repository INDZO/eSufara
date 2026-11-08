using eSufara.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace eSufara.View.Pages
{
    public partial class LogInView : Page
    {
        private readonly AuthService _authService;

        public LogInView()
        {
            InitializeComponent();
            _authService = new AuthService(new Data.eSufaraDbContext(), new HashingService());
        }

        private void BackLI_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private async void LogIn_Click(object sender, RoutedEventArgs e)
        {
            LogIn.IsEnabled = false;
            HideErrors();

            var username = UsernameLI.Text.Trim();
            var password = PasswordLI.Password;

            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    GreskaPrazanUnos.Visibility = Visibility.Visible;
                    return;
                }

                int userId = await _authService.LogIn(username, password);

                if (userId == 0 || userId == -1)
                {
                    GreskaNeuspesnaPrijava.Visibility = Visibility.Visible;
                    return;
                }

                var mainWindow = new MainView(userId);
                mainWindow.Show();

                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                GreskaSistem.Visibility = Visibility.Visible;
            }
            finally
            {
                LogIn.IsEnabled = true;
            }
        }

        private void HideErrors()
        {
            GreskaNeuspesnaPrijava.Visibility = Visibility.Collapsed;
            GreskaPrazanUnos.Visibility = Visibility.Collapsed;
            GreskaSistem.Visibility = Visibility.Collapsed;
        }
    }
}
