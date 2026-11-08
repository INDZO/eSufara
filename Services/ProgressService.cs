using eSufara.Data;
using eSufara.Model;
using Microsoft.EntityFrameworkCore;

namespace eSufara.Services
{
    public class ProgressService
    {
        private readonly eSufaraDbContext _db;

        public ProgressService(eSufaraDbContext db)
        {
            _db = db;
        }

        /// <summary> Označava da je lekcija završena za korisnika. </summary>
        public async Task MarkLessonCompletedAsync(int userId, int lessonNumber)
        {
            var progress = await GetOrCreateProgressAsync(userId, lessonNumber);
            progress.LessonCompleted = true;
            await _db.SaveChangesAsync();
        }

        /// <summary> Uklanja oznaku da je lekcija završena. </summary>
        public async Task UnmarkLessonCompletedAsync(int userId, int lessonNumber)
        {
            var progress = await _db.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonOrder == lessonNumber);

            if (progress != null)
            {
                progress.LessonCompleted = false;
                await _db.SaveChangesAsync();
            }
        }

        public async Task MarkExerciseCompletedAsync(int userId, int lessonNumber)
        {
            var progress = await GetOrCreateProgressAsync(userId, lessonNumber);
            progress.ExerciseCompleted = true;
            await _db.SaveChangesAsync();
        }

        public async Task UnmarkExerciseCompletedAsync(int userId, int lessonNumber)
        {
            var progress = await _db.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonOrder == lessonNumber);

            if (progress != null)
            {
                progress.ExerciseCompleted = false;
                await _db.SaveChangesAsync();
            }
        }

        public async Task SaveExerciseAttemptAsync(int userId, int lessonNumber, bool passed)
        {
            if (!passed)
                return;

            await MarkExerciseCompletedAsync(userId, lessonNumber);
        }

        private async Task<UserProgress> GetOrCreateProgressAsync(int userId, int lessonNumber)
        {
            var progress = await _db.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonOrder == lessonNumber);

            if (progress != null)
                return progress;

            progress = new UserProgress
            {
                UserId = userId,
                LessonOrder = lessonNumber,
                LessonCompleted = false,
                ExerciseCompleted = false
            };

            await _db.UserProgresses.AddAsync(progress);
            return progress;
        }
    }
}
