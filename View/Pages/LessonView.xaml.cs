using eSufara.Services;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace eSufara.View.Pages
{
    public partial class LessonView : Page
    {
        private readonly int _lessonNumber;
        private readonly string[] _imgExt = [".png", ".jpg", ".jpeg"];
        private readonly int _userId;
        private readonly ProgressService _progressService;
        private readonly bool _isDone;
        private bool _isInitializing = true;

        public LessonView(int lessonNumber, int userId, bool isDone)
        {
            InitializeComponent();
            _lessonNumber = lessonNumber;
            _userId = userId;
            _isDone = isDone;

            _progressService = new ProgressService(new Data.eSufaraDbContext());
            txtTitle.Text = $"Lekcija {_lessonNumber}";
            chkDone.IsChecked = _isDone;
            _isInitializing = false;
            LoadLesson();
        }

        private string LessonDir =>
            Path.Combine(AppContext.BaseDirectory, "Content", "Lekcije", _lessonNumber.ToString());

        private void LoadLesson()
        {
            if (!Directory.Exists(LessonDir))
            {
                MessageBox.Show("Sadržaj za ovu lekciju nije pronađen.", "Lekcija", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_lessonNumber == 0)
            {
                gridLesson0.Visibility = Visibility.Visible;

                string? imgLeftPath = FindFirst($"{_lessonNumber}_img");
                if (imgLeftPath != null)
                    imgLeft0.Source = new BitmapImage(new Uri(imgLeftPath));

                string? introPath = FindFirst("0_2_img");
                if (introPath != null)
                    imgIntroRight0.Source = new BitmapImage(new Uri(introPath));

                row1Panel.Visibility = Visibility.Collapsed;
                row2Panel.Visibility = Visibility.Collapsed;
                return;
            }

            gridLesson0.Visibility = Visibility.Collapsed;

            string? imgLeftPath2 = FindFirst($"{_lessonNumber}_img");
            if (imgLeftPath2 != null)
                imgLeft.Source = new BitmapImage(new Uri(imgLeftPath2));

            string? row1Path = FindFirst("row_1");
            if (row1Path != null)
            {
                imgRow1.Source = new BitmapImage(new Uri(row1Path));
                row1Panel.Visibility = Visibility.Visible;
                BuildButtons(gridRow1Buttons, 1);
            }

            string? row2Path = FindFirst("row_2");
            if (row2Path != null)
            {
                imgRow2.Source = new BitmapImage(new Uri(row2Path));
                row2Panel.Visibility = Visibility.Visible;
                BuildButtons(gridRow2Buttons, 2);
            }
        }

        private string? FindFirst(string stem)
        {
            return _imgExt.Select(ext => Path.Combine(LessonDir, stem + ext))
                          .FirstOrDefault(File.Exists);
        }

        private void BuildButtons(Grid hostGrid, int rowIndex)
        {
            hostGrid.Children.Clear();
            hostGrid.ColumnDefinitions.Clear();

            hostGrid.FlowDirection = FlowDirection.RightToLeft;

            var files = Directory.GetFiles(LessonDir)
                                 .Where(f => new[] { ".wav", ".mp3" }.Contains(Path.GetExtension(f).ToLower()))
                                 .Select(f => new { f, name = Path.GetFileNameWithoutExtension(f).ToLower() })
                                 .Where(x => x.name.EndsWith($"_row_{rowIndex}"))
                                 .OrderBy(x =>
                                 {
                                     var part = x.name.Split(new[] { "_row_" }, StringSplitOptions.None)[0];
                                     return int.TryParse(new string(part.TakeWhile(char.IsDigit).ToArray()), out int k) ? k : int.MinValue;
                                 })
                                 .Select(x => x.f)
                                 .ToList();

            for (int c = 0; c < files.Count; c++)
                hostGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (int i = 0; i < files.Count; i++)
            {
                var btn = new Button
                {
                    Content = (files.Count - i).ToString(),
                    Margin = new Thickness(6),
                    Padding = new Thickness(12, 8, 12, 8),
                    Tag = files[i]
                };
                btn.Click += PlayAudio_Click;
                Grid.SetColumn(btn, i);
                hostGrid.Children.Add(btn);
            }
        }

        private void RowImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Image img)
            {
                if (img.Name == "imgRow1")
                    gridRow1Buttons.Width = img.ActualWidth;
                else if (img.Name == "imgRow2")
                    gridRow2Buttons.Width = img.ActualWidth;
            }
        }

        private System.Windows.Media.MediaPlayer? _player;

        private void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is string path && File.Exists(path))
            {
                _player ??= new System.Windows.Media.MediaPlayer();

                if (_player.Source != null && string.Equals(_player.Source.LocalPath, path, StringComparison.OrdinalIgnoreCase))
                {
                    _player.Stop();
                    _player.Open(null);
                    return;
                }

                _player.Open(new Uri(path));
                _player.Volume = 1.0;
                _player.Play();
            }
        }

        private async void chkDone_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
                return;

            chkDone.IsEnabled = false;
            try
            {
                await _progressService.MarkLessonCompletedAsync(_userId, _lessonNumber);
                MessageBox.Show("Čestitamo! Uspešno ste završili lekciju.", "Lekcija", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Lekcija trenutno ne može da se označi kao naučena.", "Lekcija", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetDoneSilently(false);
            }
            finally
            {
                chkDone.IsEnabled = true;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new View.Pages.LessonsView(_userId));
        }

        private async void chkDone_UncheckedAsync(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
                return;

            var result = MessageBox.Show(
                "Da li želite da uklonite oznaku da je lekcija naučena?",
                "Potvrda",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                SetDoneSilently(true);
                return;
            }

            chkDone.IsEnabled = false;
            try
            {
                await _progressService.UnmarkLessonCompletedAsync(_userId, _lessonNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Oznaka lekcije trenutno ne može da se ukloni.", "Lekcija", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetDoneSilently(true);
            }
            finally
            {
                chkDone.IsEnabled = true;
            }
        }

        private void SetDoneSilently(bool isDone)
        {
            _isInitializing = true;
            chkDone.IsChecked = isDone;
            _isInitializing = false;
        }
    }
}
