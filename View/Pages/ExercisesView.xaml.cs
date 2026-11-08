using eSufara.Data;
using eSufara.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace eSufara.View.Pages
{
    public partial class ExercisesView : Page
    {
        private readonly int _userId;
        private List<ExerciseRow> _all = new();
        private HashSet<int> _passed = new();

        public ExercisesView(int userId)
        {
            InitializeComponent();
            _userId = userId;

            Loaded += ExercisesView_Loaded;
            IsVisibleChanged += ExercisesView_IsVisibleChanged;
        }

        private async void ExercisesView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAndRenderAsync(showErrors: true);
        }

        private async void ExercisesView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                await LoadAndRenderAsync(showErrors: false);
        }

        private async Task LoadAndRenderAsync(bool showErrors)
        {
            try
            {
                await RefreshFromDbAsync();
                BuildManualList();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                BuildManualList();
                ApplyFilter();

                if (showErrors)
                    MessageBox.Show("Status vežbi trenutno ne može da se učita.", "Vežbe", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task RefreshFromDbAsync()
        {
            using var db = new eSufaraDbContext();
            var list = await db.UserProgresses
                .AsNoTracking()
                .Where(up => up.UserId == _userId && up.ExerciseCompleted == true)
                .Select(up => up.LessonOrder)
                .ToListAsync();

            _passed = new HashSet<int>(list);
        }

        private void BuildManualList()
        {
            _all = CourseCatalog.Exercises
                .Select(item => new ExerciseRow(item.Number, item.Title))
                .ToList();

            foreach (var item in _all)
                item.PassedMark = _passed.Contains(item.Number) ? "✓" : "";
        }

        private void ApplyFilter()
        {
            IEnumerable<ExerciseRow> src = _all;
            if (cbOnlyNotPassed?.IsChecked == true)
                src = src.Where(x => string.IsNullOrEmpty(x.PassedMark));

            lvExercises.ItemsSource = src
                .OrderBy(x => string.IsNullOrEmpty(x.PassedMark) ? 0 : 1)
                .ThenBy(x => x.Number)
                .ToList();
        }

        private void cbOnlyNotPassed_Changed(object sender, RoutedEventArgs e) => ApplyFilter();

        private void lvExercises_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedExercise();
        }

        private void btnOpenExercise_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedExercise();
        }

        private void OpenSelectedExercise()
        {
            if (lvExercises.SelectedItem is not ExerciseRow item)
            {
                MessageBox.Show("Izaberite vežbu koju želite da otvorite.", "Vežbe", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isDone = !string.IsNullOrEmpty(item.PassedMark);
            NavigationService?.Navigate(new ExerciseView(item.Number, _userId, isDone));
        }

        private record ExerciseRow(int Number, string Title)
        {
            public string PassedMark { get; set; } = "";
        }
    }
}
