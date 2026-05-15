namespace MyWorkoutApp.Models
{
    public class WorkoutTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new();

        public string ExerciseSummary =>
            Exercises.Count == 0 ? "ללא תרגילים" :
            string.Join(" • ", Exercises.Take(3).Select(e => e.Name)) +
            (Exercises.Count > 3 ? $" +{Exercises.Count - 3}" : "");
    }
}