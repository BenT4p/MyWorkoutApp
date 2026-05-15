using System.ComponentModel;
namespace MyWorkoutApp.Models
{
    public class WorkoutSet : INotifyPropertyChanged
    {
        public int SetNumber { get; set; }

        private double _weight;
        public double Weight
        {
            get => _weight;
            set { _weight = value; OnPropertyChanged(nameof(Weight)); }
        }

        private int _reps;
        public int Reps
        {
            get => _reps;
            set { _reps = value; OnPropertyChanged(nameof(Reps)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}