using eSufara.Data;
using eSufara.Services;
using Microsoft.EntityFrameworkCore;

namespace eSufara.ViewModel
{
    internal class ProgressViewModel
    {
        private readonly int _id;

        public int LessonsDone { get; private set; }
        public int ExercisesDone { get; private set; }
        public int TotalLessons => CourseCatalog.TotalLessons;
        public int TotalExercises => CourseCatalog.TotalExercises;

        public ProgressViewModel(int id)
        {
            _id = id;
        }

        public async Task LoadProgressAsync()
        {
            using var context = new eSufaraDbContext();
            LessonsDone = await context.UserProgresses.CountAsync(x => x.UserId == _id && x.LessonCompleted == true);
            ExercisesDone = await context.UserProgresses.CountAsync(x => x.UserId == _id && x.ExerciseCompleted == true);
        }
    }
}
