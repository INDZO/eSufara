using eSufara.ViewModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace eSufara.View.Pages
{
    public partial class ProgressView : Page
    {
        private readonly ProgressViewModel _viewModel;

        public ProgressView(int id)
        {
            InitializeComponent();

            _viewModel = new ProgressViewModel(id);
            Loaded += ProgressView_Loaded;
        }

        private async void ProgressView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                await _viewModel.LoadProgressAsync();
                RenderProgress();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ProgressPercentLessons.Text = "Nije dostupno";
                ProgressPercentExercises.Text = "Nije dostupno";
            }
        }

        private void RenderProgress()
        {
            LessonsProgress.Maximum = _viewModel.TotalLessons;
            ExercisesProgress.Maximum = _viewModel.TotalExercises;

            LessonsProgress.Value = _viewModel.LessonsDone;
            ExercisesProgress.Value = _viewModel.ExercisesDone;

            double lessonsPercent = ((double)_viewModel.LessonsDone / _viewModel.TotalLessons) * 100;
            double exercisesPercent = ((double)_viewModel.ExercisesDone / _viewModel.TotalExercises) * 100;

            ProgressPercentLessons.Text = $"{Math.Round(lessonsPercent)}%";
            ProgressPercentExercises.Text = $"{Math.Round(exercisesPercent)}%";
        }
    }
}
