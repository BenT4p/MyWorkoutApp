namespace MyWorkoutApp.Models
{
    public class Exercise
    {
        public string Name { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public string ImagePath { get; set; } = "default_exercise.png";
    }
}