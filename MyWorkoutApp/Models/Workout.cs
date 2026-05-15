using System.Collections.ObjectModel;

namespace MyWorkoutApp.Models
{
    public class Workout
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public ObservableCollection<WorkoutExercise> Exercises { get; set; } = new();

        public string DateDisplay => Date.ToString("dd/MM/yyyy HH:mm");
    }
}