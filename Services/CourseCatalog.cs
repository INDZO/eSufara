namespace eSufara.Services
{
    public sealed record CourseItem(int Number, string Title);

    public static class CourseCatalog
    {
        public static IReadOnlyList<CourseItem> Lessons { get; } =
        [
            new(0, "Uvod"),
            new(1, "ا"),
            new(2, "د"),
            new(3, "ذ"),
            new(4, "Tenvin i Sukun"),
            new(5, "ر"),
            new(6, "ز"),
            new(7, "ب"),
            new(8, "ت"),
            new(9, "ث"),
            new(10, "Tešdid - pojačanje"),
            new(11, "ن"),
            new(12, "و"),
            new(13, "ي"),
            new(14, "ل"),
            new(15, "Dugi samoglasnici"),
            new(16, "ف"),
            new(17, "ق"),
            new(18, "م"),
            new(19, "ك"),
            new(20, "ه"),
            new(21, "ح"),
            new(22, "خ"),
            new(23, "ج"),
            new(24, "س"),
            new(25, "ش"),
            new(26, "ص"),
            new(27, "ض"),
            new(28, "ط"),
            new(29, "ظ"),
            new(30, "ع"),
            new(31, "غ")
        ];

        public static IReadOnlyList<CourseItem> Exercises { get; } =
            Lessons.Where(item => item.Number > 0).ToList();

        public static int TotalLessons => Lessons.Count;
        public static int TotalExercises => Exercises.Count;
        public static int TotalItems => TotalLessons + TotalExercises;
    }
}
