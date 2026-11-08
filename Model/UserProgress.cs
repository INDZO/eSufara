using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eSufara.Model
{
    public partial class UserProgress
    {
        [Key, Column(Order = 0)]  // Definiši prvi deo kompozitnog ključa
        public int UserId { get; set; }

        [Key, Column(Order = 1)]  // Definiši drugi deo kompozitnog ključa
        public int LessonOrder { get; set; }

        public bool? LessonCompleted { get; set; }

        public bool? ExerciseCompleted { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
