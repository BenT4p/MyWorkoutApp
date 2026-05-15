namespace MyWorkoutApp.Models
{
    public class WorkoutSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<WorkoutExercise> Exercises { get; set; } = new();

        public string DateDisplay => StartTime.ToString("dd/MM/yyyy HH:mm");
        public string DurationDisplay
        {
            get
            {
                var d = EndTime - StartTime;
                return $"{(int)d.TotalHours:D2}:{d.Minutes:D2}:{d.Seconds:D2}";
            }
        }
        public string ExerciseSummary =>
            Exercises.Count == 0 ? "ללא תרגילים" :
            string.Join(" • ", Exercises.Take(3).Select(e => e.Exercise.Name)) +
            (Exercises.Count > 3 ? $" +{Exercises.Count - 3}" : "");
    }
}