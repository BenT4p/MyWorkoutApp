using System.Collections.ObjectModel;

namespace MyWorkoutApp.Models
{
    public class WorkoutExercise
    {
        public Exercise Exercise { get; set; } = new();
        public ObservableCollection<WorkoutSet> Sets { get; set; } = new();

        public string Notes { get; set; } = string.Empty;
    }
}