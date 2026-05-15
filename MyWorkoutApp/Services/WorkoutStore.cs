using System.Collections.ObjectModel;
using MyWorkoutApp.Models;

namespace MyWorkoutApp.Services;

public static class WorkoutStore
{
    public static UserProfile Profile { get; set; } = new UserProfile();

    // תבניות קבועות - מעודכנות עם הנתיבים המדויקים לתמונות
    public static ObservableCollection<WorkoutTemplate> Templates { get; } = new()
    {
        new WorkoutTemplate
        {
            Name = "אימון חזה ותלת ראשי",
            Exercises = new List<Exercise>
            {
                new() { Name = "לחיצת חזה (מוט)", MuscleGroup = "חזה", ImagePath = "bench_press_barbell.webp" },
                new() { Name = "לחיצת חזה בשיפוע חיובי (משקולות)", MuscleGroup = "חזה", ImagePath = "incline_bench_dumbbell.webp" },
                new() { Name = "פרפר חופשי (משקולות)", MuscleGroup = "חזה", ImagePath = "dumbbell_fly.webp" },
                new() { Name = "פשיטת מרפקים בפולי עליון (חבל)", MuscleGroup = "תלת ראשי", ImagePath = "tricep_pushdown_rope.webp" },
                new() { Name = "מקבילים", MuscleGroup = "תלת ראשי", ImagePath = "tricep_dips.webp" },
            }
        },
        new WorkoutTemplate
        {
            Name = "אימון גב ודו ראשי",
            Exercises = new List<Exercise>
            {
                new() { Name = "משיכות פולי עליון", MuscleGroup = "גב", ImagePath = "lat_pulldown.webp" },
                new() { Name = "חתירה בישיבה (כבלים)", MuscleGroup = "גב", ImagePath = "seated_cable_row.webp" },
                new() { Name = "פולאובר (משקולת)", MuscleGroup = "חזה", ImagePath = "dumbbell_pullover.webp" },
                new() { Name = "כפיפת מרפקים (מוט)", MuscleGroup = "יד קדמית", ImagePath = "barbell_curl.webp" },
                new() { Name = "כפיפת מרפקים פטישים (Hammer)", MuscleGroup = "יד קדמית", ImagePath = "hammer_curl.webp" },
            }
        }
    };

    // היסטוריית sessions
    public static ObservableCollection<WorkoutSession> History { get; } = new();

    // רשימת תרגילים זמינים לבחירה
    public static List<Exercise> AvailableExercises { get; } = new()
    {
        // ── חזה (Chest) ──
        new() { Name = "לחיצת חזה (מוט)", MuscleGroup = "חזה", ImagePath = "bench_press_barbell.webp" },
        new() { Name = "לחיצת חזה (משקולות)", MuscleGroup = "חזה", ImagePath = "bench_press_dumbbell.webp" },
        new() { Name = "לחיצת חזה בשיפוע חיובי (מוט)", MuscleGroup = "חזה", ImagePath = "incline_bench_barbell.webp" },
        new() { Name = "לחיצת חזה בשיפוע חיובי (משקולות)", MuscleGroup = "חזה", ImagePath = "incline_bench_dumbbell.webp" },
        new() { Name = "לחיצת חזה בשיפוע שלילי", MuscleGroup = "חזה", ImagePath = "decline_bench_press.webp" },
        new() { Name = "פרפר (מכונה / Pec Deck)", MuscleGroup = "חזה", ImagePath = "pec_deck_fly.webp" },
        new() { Name = "פרפר בכבלים (קרוסאובר)", MuscleGroup = "חזה", ImagePath = "cable_crossover.webp" },
        new() { Name = "פרפר חופשי (משקולות)", MuscleGroup = "חזה", ImagePath = "dumbbell_fly.webp" },
        new() { Name = "שכיבות סמיכה", MuscleGroup = "חזה", ImagePath = "push_ups.webp" },
        new() { Name = "פולאובר (משקולת)", MuscleGroup = "חזה", ImagePath = "dumbbell_pullover.webp" },

        // ── גב (Back) ──
        new() { Name = "מתח", MuscleGroup = "גב", ImagePath = "pull_ups.webp" },
        new() { Name = "משיכות פולי עליון", MuscleGroup = "גב", ImagePath = "lat_pulldown.webp" },
        new() { Name = "משיכות פולי עליון (אחיזה צרה)", MuscleGroup = "גב", ImagePath = "lat_pulldown_close_grip.webp" },
        new() { Name = "חתירה בישיבה (כבלים)", MuscleGroup = "גב", ImagePath = "seated_cable_row.webp" },
        new() { Name = "חתירה בהטיית גו (מוט)", MuscleGroup = "גב", ImagePath = "bent_over_row_barbell.webp" },
        new() { Name = "חתירה בהטיית גו (משקולת)", MuscleGroup = "גב", ImagePath = "single_arm_dumbbell_row.webp" },
        new() { Name = "חתירה בטי-באר (T-Bar)", MuscleGroup = "גב", ImagePath = "t_bar_row.webp" },
        new() { Name = "פולי עליון בידיים ישרות", MuscleGroup = "גב", ImagePath = "straight_arm_pulldown.webp" },
        new() { Name = "דדליפט (מסורתי)", MuscleGroup = "גב", ImagePath = "deadlift.webp" },

        // ── יד אחורית (Triceps) ──
        new() { Name = "פשיטת מרפקים בפולי עליון (חבל)", MuscleGroup = "תלת ראשי", ImagePath = "tricep_pushdown_rope.webp" },
        new() { Name = "פשיטת מרפקים בפולי עליון (מוט ישר)", MuscleGroup = "תלת ראשי", ImagePath = "tricep_pushdown_bar.webp" },
        new() { Name = "לחיצה צרפתית (Skull Crushers)", MuscleGroup = "תלת ראשי", ImagePath = "skull_crushers.webp" },
        new() { Name = "פשיטת מרפקים מעל הראש (משקולת)", MuscleGroup = "תלת ראשי", ImagePath = "overhead_tricep_extension.webp" },
        new() { Name = "מקבילים", MuscleGroup = "תלת ראשי", ImagePath = "tricep_dips.webp" },
        new() { Name = "מקבילים בספסל (Bench Dips)", MuscleGroup = "תלת ראשי", ImagePath = "bench_dips.webp" },
        new() { Name = "לחיצת חזה באחיזה צרה", MuscleGroup = "תלת ראשי", ImagePath = "close_grip_bench_press.webp" },
        new() { Name = "קיקבאק (משקולת)", MuscleGroup = "תלת ראשי", ImagePath = "tricep_kickback.webp" },

        // ── תאומים (Calves) ──
        new() { Name = "הרמת עקבים בעמידה", MuscleGroup = "תאומים", ImagePath = "standing_calf_raise.webp" },
        new() { Name = "הרמת עקבים בישיבה", MuscleGroup = "תאומים", ImagePath = "seated_calf_raise.webp" },

        // ── יד קדמית (Biceps) ──
        new() { Name = "כפיפת מרפקים (מוט)", MuscleGroup = "יד קדמית", ImagePath = "barbell_curl.webp" },
        new() { Name = "כפיפת מרפקים (משקולות)", MuscleGroup = "יד קדמית", ImagePath = "dumbbell_curl.webp" },
        new() { Name = "כפיפת מרפקים פטישים (Hammer)", MuscleGroup = "יד קדמית", ImagePath = "hammer_curl.webp" },
        new() { Name = "כפיפת מרפקים בספסל כומר (Preacher)", MuscleGroup = "יד קדמית", ImagePath = "preacher_curl.webp" },
        new() { Name = "כפיפת מרפק ריכוז (Concentration)", MuscleGroup = "יד קדמית", ImagePath = "concentration_curl.webp" },
        new() { Name = "כפיפת מרפקים בכבל התחתון", MuscleGroup = "יד קדמית", ImagePath = "cable_curl.webp" },
        new() { Name = "כפיפת מרפקים בשיפוע חיובי (משקולות)", MuscleGroup = "יד קדמית", ImagePath = "incline_dumbbell_curl.webp" },
        new() { Name = "כפיפת מרפקים באחיזה הפוכה", MuscleGroup = "יד קדמית", ImagePath = "reverse_bicep_curl.webp" },

        // ── כתפיים (Shoulders) ──
        new() { Name = "לחיצת כתפיים (משקולות בישיבה)", MuscleGroup = "כתפיים", ImagePath = "dumbbell_shoulder_press.webp" },
        new() { Name = "לחיצת ארנולד (משקולות)", MuscleGroup = "כתפיים", ImagePath = "arnold_press.webp" },
        new() { Name = "הרחקת זרועות לצדדים (משקולות)", MuscleGroup = "כתפיים", ImagePath = "lateral_raise_dumbbell.webp" },
        new() { Name = "הרחקת זרועות לצדדים (כבלים)", MuscleGroup = "כתפיים", ImagePath = "lateral_raise_cable.webp" },
        new() { Name = "הנפה קדמית (מוט / משקולות)", MuscleGroup = "כתפיים", ImagePath = "front_raise.webp" },
        new() { Name = "פרפר הפוך למכתפיים אחוריות", MuscleGroup = "כתפיים", ImagePath = "rear_delt_fly.webp" },
        new() { Name = "חתירה זקופה לסנטר (Upright Row)", MuscleGroup = "כתפיים", ImagePath = "upright_row.webp" },
        new() { Name = "משיכת כתפיים / שראגס", MuscleGroup = "כתפיים", ImagePath = "shrugs.webp" },

        // ── בטן (Abs) ──
        new() { Name = "כפיפות בטן קלאסיות", MuscleGroup = "בטן", ImagePath = "crunches.webp" },
        new() { Name = "כפיפות בטן באלכסון (אופניים)", MuscleGroup = "בטן", ImagePath = "bicycle_crunches.webp" },
        new() { Name = "הרמת רגליים בתלייה", MuscleGroup = "בטן", ImagePath = "hanging_leg_raises.webp" },
        new() { Name = "פלאנק (בטן סטטי)", MuscleGroup = "בטן", ImagePath = "plank.webp" },
        new() { Name = "כפיפות בטן בכבלים (Cable Crunches)", MuscleGroup = "בטן", ImagePath = "cable_crunches.webp" },

        // ── רגליים וישבן (Legs & Glutes) ──
        new() { Name = "סקוואט חופשי (מוט)", MuscleGroup = "רגליים", ImagePath = "barbell_squat.webp" },
        new() { Name = "סקוואט קדמי (מוט)", MuscleGroup = "רגליים", ImagePath = "front_squat.webp" },
        new() { Name = "סקוואט כוס (Goblet Squat)", MuscleGroup = "רגליים", ImagePath = "goblet_squat.webp" },
        new() { Name = "לחיצת רגליים (מכונה)", MuscleGroup = "רגליים", ImagePath = "leg_press.webp" },
        new() { Name = "פשיטת ברכיים (מכונה)", MuscleGroup = "רגליים", ImagePath = "leg_extension.webp" },
        new() { Name = "כפיפת ברכיים בישיבה (מכונה)", MuscleGroup = "רגליים", ImagePath = "seated_leg_curl.webp" },
        new() { Name = "כפיפת ברכיים בשכיבה (מכונה)", MuscleGroup = "רגליים", ImagePath = "lying_leg_curl.webp" },
        new() { Name = "דדליפט רומני (מוט)", MuscleGroup = "רגליים", ImagePath = "romanian_deadlift_barbell.webp" },
        new() { Name = "דדליפט רומני (משקולות)", MuscleGroup = "רגליים", ImagePath = "romanian_deadlift_dumbbell.webp" },
        new() { Name = "מכרעים / לאנג'ים (הליכה)", MuscleGroup = "רגליים", ImagePath = "walking_lunges.webp" },
        new() { Name = "סקוואט בולגרי (משקולות)", MuscleGroup = "רגליים", ImagePath = "bulgarian_split_squat.webp" },
        new() { Name = "הרמת אגן / היפ טראסט (מוט)", MuscleGroup = "ישבן", ImagePath = "barbell_hip_thrust.webp" },
        new() { Name = "הרמת אגן (מכונה)", MuscleGroup = "ישבן", ImagePath = "machine_hip_thrust.webp" },
        new() { Name = "הרחקת ירכיים (מכונה)", MuscleGroup = "ישבן", ImagePath = "hip_abduction_machine.webp" },
        new() { Name = "קיקבאק לישבן (כבל)", MuscleGroup = "ישבן", ImagePath = "cable_glute_kickback.webp" }
    };

    // ── Computed stats מתוך History ──────────────────────────────────────
    public static int TotalWorkouts => History.Count;

    public static int TotalSets =>
        History.Sum(s => s.Exercises.Sum(e => e.Sets.Count));

    public static double TotalWeightLifted =>
        History.Sum(s => s.Exercises.Sum(e => e.Sets.Sum(set => set.Weight * set.Reps)));

    public static string FavoriteExercise
    {
        get
        {
            var all = History
                .SelectMany(s => s.Exercises)
                .GroupBy(e => e.Exercise.Name)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            return all?.Key ?? "אין עדיין";
        }
    }

    public static string MostTrainedMuscle
    {
        get
        {
            var all = History
                .SelectMany(s => s.Exercises)
                .GroupBy(e => e.Exercise.MuscleGroup)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            return all?.Key ?? "אין עדיין";
        }
    }
}