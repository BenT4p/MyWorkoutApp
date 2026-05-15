using MyWorkoutApp.Models;
using MyWorkoutApp.Services;

namespace MyWorkoutApp.Pages;

public partial class EditTemplatePage : ContentPage
{
    private readonly WorkoutTemplate _template;
    private readonly List<Exercise> _exercises;

    public EditTemplatePage(WorkoutTemplate template)
    {
        InitializeComponent();
        _template = template;
        _exercises = new List<Exercise>(template.Exercises);
        TemplateNameEntry.Text = template.Name;
        RefreshList();
    }

    private void RefreshList()
    {
        ExercisesList.ItemsSource = null;
        ExercisesList.ItemsSource = _exercises;
    }

    private async void OnAddExerciseClicked(object sender, EventArgs e)
    {
        var page = new SelectExercisePage();
        page.ExerciseSelected += ex =>
        {
            if (_exercises.Any(e => e.Name == ex.Name))
            {
                Dispatcher.Dispatch(async () => await DisplayAlert("שים לב", "התרגיל כבר קיים בתבנית", "הבנתי"));
                return;
            }
            _exercises.Add(ex);
            RefreshList();
        };
        await Navigation.PushAsync(page);
    }

    private void OnRemoveExerciseClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Exercise ex)
        {
            _exercises.Remove(ex);
            RefreshList();
        }
    }

    private void OnMoveUpClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Exercise ex)
        {
            int i = _exercises.IndexOf(ex);
            if (i > 0)
            {
                _exercises.RemoveAt(i);
                _exercises.Insert(i - 1, ex);
                RefreshList();
            }
        }
    }

    private void OnMoveDownClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Exercise ex)
        {
            int i = _exercises.IndexOf(ex);
            if (i >= 0 && i < _exercises.Count - 1)
            {
                _exercises.RemoveAt(i);
                _exercises.Insert(i + 1, ex);
                RefreshList();
            }
        }
    }

    private bool HasChanges()
    {
        if (TemplateNameEntry.Text?.Trim() != _template.Name) return true;
        if (_exercises.Count != _template.Exercises.Count) return true;
        for (int i = 0; i < _exercises.Count; i++)
        {
            if (_exercises[i].Name != _template.Exercises[i].Name) return true;
        }
        return false;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await OnBackClickedAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        Dispatcher.Dispatch(async () => await OnBackClickedAsync());
        return true;
    }

    private async Task OnBackClickedAsync()
    {
        if (HasChanges())
        {
            bool discard = await DisplayAlert(
                "לצאת ללא שמירה?",
                "ביצעת שינויים בתבנית. אם תחזור השינויים לא יישמרו.",
                "צא ללא שמירה", "המשך עריכה");
            if (!discard) return;
        }
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string name = TemplateNameEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(name))
            _template.Name = name;

        _template.Exercises = new List<Exercise>(_exercises);

        int idx = WorkoutStore.Templates.IndexOf(_template);
        if (idx >= 0)
        {
            WorkoutStore.Templates.RemoveAt(idx);
            WorkoutStore.Templates.Insert(idx, _template);
        }

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