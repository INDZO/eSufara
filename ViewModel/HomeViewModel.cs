using eSufara.Data;
using eSufara.Services;
using Microsoft.EntityFrameworkCore;

namespace eSufara.ViewModel
{
    internal class HomeViewModel
    {
        public async Task<int> GetLessonsDoneAsync(int userId)
        {
            using var context = new eSufaraDbContext();
            return await context.UserProgresses
                .CountAsync(up => up.UserId == userId && up.LessonCompleted == true);
        }

        public async Task<int> GetExercisesDoneAsync(int userId)
        {
            using var context = new eSufaraDbContext();
            return await context.UserProgresses
                .CountAsync(up => up.UserId == userId && up.ExerciseCompleted == true);
        }

        public static int TotalItems => CourseCatalog.TotalItems;
    }
}
