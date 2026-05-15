using MyWorkoutApp.Models;
using MyWorkoutApp.Services;

namespace MyWorkoutApp.Pages;

public partial class SelectExercisePage : ContentPage
{
    public event Action<Exercise>? ExerciseSelected;

    // מנעול שמונע קריסות אם המשתמש לחץ פעמיים מהר
    private bool _isNavigating = false;

    public SelectExercisePage()
    {
        InitializeComponent();
        ExercisesCollection.ItemsSource = WorkoutStore.AvailableExercises;
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue?.ToLowerInvariant() ?? "";

        if (string.IsNullOrWhiteSpace(keyword))
        {
            ExercisesCollection.ItemsSource = WorkoutStore.AvailableExercises;
        }
        else
        {
            ExercisesCollection.ItemsSource = WorkoutStore.AvailableExercises
                .Where(ex => ex.Name.ToLowerInvariant().Contains(keyword) ||
                             ex.MuscleGroup.ToLowerInvariant().Contains(keyword))
                .ToList();
        }
    }

    // הפונקציה החדשה והבטוחה לבחירת תרגיל בלי קריסות
    private async void OnExerciseTapped(object sender, EventArgs e)
    {
        if (_isNavigating) return;

        if (sender is Frame frame && frame.BindingContext is Exercise exercise)
        {
            _isNavigating = true; // נועלים כדי למנוע לחיצה נוספת

            ExerciseSelected?.Invoke(exercise);
            await Navigation.PopAsync();

            _isNavigating = false; // משחררים
        }
    }

    private void OnImageZoomClicked(object sender, EventArgs e)
    {
        if (sender is Image img)
        {
            ZoomImage.Source = img.Source;
            ZoomOverlay.IsVisible = true;
            ZoomOverlay.Opacity = 0;
            ZoomOverlay.FadeTo(1, 200);
        }
    }

    private void OnCloseZoomClicked(object sender, EventArgs e)
    {
        ZoomOverlay.FadeTo(0, 150).ContinueWith(t =>
            MainThread.BeginInvokeOnMainThread(() => ZoomOverlay.IsVisible = false));
    }
}