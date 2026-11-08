using eSufara.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace eSufara.View.Pages
{
    public partial class SignUp : Page
    {
        private byte[]? _slikaBytes;

        public SignUp()
        {
            InitializeComponent();
            _slikaBytes = ProfileImageService.LoadDefaultProfileImageBytes();
        }

        private void BackLI_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void SlikaBTN_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Slike (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (ofd.ShowDialog() != true)
                return;

            if (!ProfileImageService.TryLoadProfileImage(ofd.FileName, out var bitmap, out var imageBytes, out var errorMessage))
            {
                MessageBox.Show(errorMessage, "Profilna slika", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _slikaBytes = imageBytes;

            var previewWindow = new PreviewImageView(bitmap);
            previewWindow.ShowDialog();
        }

        private async void SignUpBTN_Click(object sender, RoutedEventArgs e)
        {
            SignUpBTN.IsEnabled = false;
            HideErrors();

            var username = UsernameSU.Text.Trim();
            var password = PasswordSU.Password;
            var confirmPassword = ConfirmSU.Password;

            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    GreskaPrazanUnos.Visibility = Visibility.Visible;
                    return;
                }

                if (username.Length < 3 || username.Length > 32)
                {
                    GreskaUsernameDuzina.Visibility = Visibility.Visible;
                    return;
                }

                if (password.Length < 8)
                {
                    GreskaMaloKaraktera.Visibility = Visibility.Visible;
                    return;
                }

                if (password != confirmPassword)
                {
                    GreskaNepoklapanje.Visibility = Visibility.Visible;
                    return;
                }

                var finalImage = (_slikaBytes is { Length: > 0 })
                    ? _slikaBytes
                    : ProfileImageService.LoadDefaultProfileImageBytes();

                var authService = new AuthService(new Data.eSufaraDbContext(), new HashingService());
                int registered = await authService.Register(username, password, finalImage);

                if (registered == 0)
                {
                    GreskaUsernameTaken.Visibility = Visibility.Visible;
                    return;
                }

                var mainWindow = new MainView(registered);
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
                SignUpBTN.IsEnabled = true;
            }
        }

        private void HideErrors()
        {
            GreskaPrazanUnos.Visibility = Visibility.Collapsed;
            GreskaUsernameDuzina.Visibility = Visibility.Collapsed;
            GreskaMaloKaraktera.Visibility = Visibility.Collapsed;
            GreskaNepoklapanje.Visibility = Visibility.Collapsed;
            GreskaUsernameTaken.Visibility = Visibility.Collapsed;
            GreskaSistem.Visibility = Visibility.Collapsed;
        }

    }
}
