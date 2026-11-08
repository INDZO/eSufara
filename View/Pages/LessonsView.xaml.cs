using eSufara.Data;
using eSufara.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace eSufara.View.Pages
{
    public partial class LessonsView : Page
    {
        private readonly int _userId;
        private List<LessonRow> _all = new();
        private HashSet<int> _passed = new();

        public LessonsView(int userId)
        {
            InitializeComponent();
            _userId = userId;

            Loaded += LessonsView_Loaded;
            IsVisibleChanged += LessonsView_IsVisibleChanged;
        }

        private async void LessonsView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAndRenderAsync(showErrors: true);
        }

        private async void LessonsView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
                    MessageBox.Show("Status lekcija trenutno ne može da se učita.", "Lekcije", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task RefreshFromDbAsync()
        {
            using var db = new eSufaraDbContext();
            var list = await db.UserProgresses
                .AsNoTracking()
                .Where(up => up.UserId == _userId && up.LessonCompleted == true)
                .Select(up => up.LessonOrder)
                .ToListAsync();

            _passed = new HashSet<int>(list);
        }

        private void BuildManualList()
        {
            _all = CourseCatalog.Lessons
                .Select(item => new LessonRow(item.Number, item.Title))
                .ToList();

            foreach (var item in _all)
                item.PassedMark = _passed.Contains(item.Number) ? "✓" : "";
        }

        private void ApplyFilter()
        {
            IEnumerable<LessonRow> src = _all;
            if (cbOnlyNotPassed?.IsChecked == true)
                src = src.Where(x => string.IsNullOrEmpty(x.PassedMark));

            lvLessons.ItemsSource = src
                .OrderBy(x => string.IsNullOrEmpty(x.PassedMark) ? 0 : 1)
                .ThenBy(x => x.Number)
                .ToList();
        }

        private void cbOnlyNotPassed_Changed(object sender, RoutedEventArgs e) => ApplyFilter();

        private void lvLessons_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedLesson();
        }

        private void btnOpenLesson_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedLesson();
        }

        private void OpenSelectedLesson()
        {
            if (lvLessons.SelectedItem is not LessonRow item)
            {
                MessageBox.Show("Izaberite lekciju koju želite da otvorite.", "Lekcije", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isDone = !string.IsNullOrEmpty(item.PassedMark);
            NavigationService?.Navigate(new LessonView(item.Number, _userId, isDone));
        }

        private record LessonRow(int Number, string Title)
        {
            public string PassedMark { get; set; } = "";
        }
    }
}
