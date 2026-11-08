using eSufara.Services;
using eSufara.ViewModel;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace eSufara.View
{
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly int _userId;

        public MainView(int userId)
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            _userId = userId;

            Loaded += MainView_Loaded;
            MainContentFrame.Navigate(new View.Pages.HomeVIew(_userId));
        }

        private async void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = await _viewModel.GetUserByIdAsync(_userId);
                if (user != null)
                {
                    Username.Text = user.Username;

                    BitmapImage? bmp = _viewModel.LoadImageFromBytes(user.ProfilePicture);
                    if (bmp != null)
                        ProfilePicture.ImageSource = bmp;
                }
                else
                {
                    Username.Text = "Nepoznat korisnik";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Username.Text = "Korisnik";
                MessageBox.Show("Podaci o korisniku trenutno ne mogu da se učitaju.", "eSufara", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AboutUsBtn_Click(object sender, RoutedEventArgs e)
        {
            AboutUs aboutUs = new AboutUs();
            aboutUs.ShowDialog();
        }

        private void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new View.Pages.HomeVIew(_userId));
        }

        private void ProgressBtn_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new View.Pages.ProgressView(_userId));
        }

        private void LessonsBtn_Click_1(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new View.Pages.LessonsView(_userId));
        }

        private void ExercisesBtn_Click_1(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new View.Pages.ExercisesView(_userId));
        }

        private void Canvas_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AccSettings.Visibility = Visibility.Visible;
            Acc.Background = (Brush)FindResource("BrandBrush");
        }

        private void Canvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AccSettings.Visibility = Visibility.Hidden;
            Acc.Background = Brushes.Transparent;
        }

        private void StackPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AccSettings.Visibility = Visibility.Hidden;
            Acc.Background = Brushes.Transparent;
        }

        private void StackPanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AccSettings.Visibility = Visibility.Visible;
            Acc.Background = (Brush)FindResource("BrandBrush");
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            StartView startView = new StartView();
            startView.Show();

            Window.GetWindow(this)?.Close();
        }

        private async void ChangeImage_Click(object sender, RoutedEventArgs e)
        {
            ChangeImage.IsEnabled = false;

            try
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

                bool isUpdated = await _viewModel.UpdateUserProfileImageAsync(_userId, imageBytes);

                if (isUpdated)
                {
                    var previewWindow = new PreviewImageView(bitmap);
                    previewWindow.ShowDialog();
                    ProfilePicture.ImageSource = bitmap;
                }
                else
                {
                    MessageBox.Show("Profilna slika trenutno ne može da se sačuva.", "Profilna slika", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Profilna slika trenutno ne može da se promeni.", "Profilna slika", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                ChangeImage.IsEnabled = true;
            }
        }

    }
}
