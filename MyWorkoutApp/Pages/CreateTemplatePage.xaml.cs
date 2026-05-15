using MyWorkoutApp.Models;
using MyWorkoutApp.Services;

namespace MyWorkoutApp.Pages;

public partial class CreateTemplatePage : ContentPage
{
    private readonly List<Exercise> _selectedExercises = new();

    public CreateTemplatePage()
    {
        InitializeComponent();
        RefreshList();
    }

    private void RefreshList()
    {
        SelectedExercisesList.ItemsSource = null;
        SelectedExercisesList.ItemsSource = _selectedExercises;
        SaveButton.IsEnabled = _selectedExercises.Count > 0;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnAddExerciseClicked(object sender, EventArgs e)
    {
        var page = new SelectExercisePage();
        page.ExerciseSelected += OnExercisePicked;
        await Navigation.PushAsync(page);
    }

    private void OnExercisePicked(Exercise exercise)
    {
        // מניעת כפילויות
        if (_selectedExercises.Any(ex => ex.Name == exercise.Name))
        {
            Dispatcher.Dispatch(async () => await DisplayAlert("שים לב", "התרגיל כבר קיים בתבנית", "הבנתי"));
            return;
        }

        _selectedExercises.Add(exercise);
        RefreshList();
    }

    private void OnRemoveExerciseClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Exercise ex)
        {
            _selectedExercises.Remove(ex);
            RefreshList();
        }
    }

    private async void OnSaveTemplateClicked(object sender, EventArgs e)
    {
        string name = TemplateNameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
            name = $"תבנית {DateTime.Now:dd/MM HH:mm}";

        WorkoutStore.Templates.Add(new WorkoutTemplate
        {
            Name = name,
            Exercises = new List<Exercise>(_selectedExercises)
        });

        // שמירה לוקאלית
        PersistenceService.SaveTemplates();

        await Navigation.PopAsync();
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