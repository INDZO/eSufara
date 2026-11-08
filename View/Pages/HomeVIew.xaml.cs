using eSufara.ViewModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace eSufara.View.Pages
{
    public partial class HomeVIew : Page
    {
        private readonly int _userId;
        private readonly HomeViewModel _viewModel;

        public HomeVIew(int userId)
        {
            InitializeComponent();

            _userId = userId;
            _viewModel = new HomeViewModel();

            Loaded += HomeVIew_Loaded;
        }

        private async void HomeVIew_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                await LoadProgressAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MyProgressBar.Value = 0;
                ProgressPercentText.Text = "Nije dostupno";
            }
        }

        private async Task LoadProgressAsync()
        {
            int lessonsDone = await _viewModel.GetLessonsDoneAsync(_userId);
            int exercisesDone = await _viewModel.GetExercisesDoneAsync(_userId);
            int totalDone = Math.Min(lessonsDone + exercisesDone, HomeViewModel.TotalItems);

            double percent = ((double)totalDone / HomeViewModel.TotalItems) * 100;

            MyProgressBar.Maximum = HomeViewModel.TotalItems;
            MyProgressBar.Value = totalDone;
            ProgressPercentText.Text = $"{percent:F1}%";
        }
    }
}
