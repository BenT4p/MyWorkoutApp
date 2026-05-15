using Microsoft.Maui.Graphics;
using MyWorkoutApp.Models;
using MyWorkoutApp.Services;

namespace MyWorkoutApp.Pages;

public partial class ProfilePage : ContentPage
{
    private InteractiveLineChartDrawable _chartDrawable;
    private CancellationTokenSource _tooltipTimerCts;
    private bool _isEditingWeight = false;

    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProfileData();
        UpdateFavoriteExerciseUI();
        RefreshWeightChart();
        LoadWeightHistory();
    }

    private void LoadProfileData()
    {
        var p = WorkoutStore.Profile;
        NameEntry.Text = p.Name;

        GenderPicker.SelectedItem = string.IsNullOrEmpty(p.Gender) ? "זכר" : p.Gender;
        BarWeightPicker.SelectedIndex = p.DefaultBarWeight == 15 ? 0 : 1;
        if (!string.IsNullOrEmpty(p.Goal)) GoalPicker.SelectedItem = p.Goal;

        if (!string.IsNullOrEmpty(p.ProfileImagePath) && File.Exists(p.ProfileImagePath))
            ProfileImage.Source = ImageSource.FromFile(p.ProfileImagePath);
        else
            ProfileImage.Source = "tab_person.png";

        if (p.WeightHistory != null && p.WeightHistory.Any())
        {
            var latest = p.WeightHistory.OrderByDescending(w => w.Date).First();
            CurrentWeightPill.Text = $"{latest.Weight:F1} ק\"ג";
        }
        else
        {
            CurrentWeightPill.Text = "--";
        }
    }

    // ─── ניהול תרגיל אהוב עם התפריט החדש ───
    private string GetAutoFavoriteExercise()
    {
        var all = WorkoutStore.History?.SelectMany(s => s.Exercises).ToList() ?? new();
        if (!all.Any()) return "אין עדיין";
        return all.GroupBy(e => e.Exercise.Name).OrderByDescending(g => g.Count()).First().Key;
    }

    private void UpdateFavoriteExerciseUI()
    {
        var p = WorkoutStore.Profile;
        string activeFavName = p.FavoriteExerciseOverride;
        bool isManual = !string.IsNullOrEmpty(activeFavName);

        if (!isManual)
        {
            activeFavName = GetAutoFavoriteExercise();
        }

        if (activeFavName == "אין עדיין")
        {
            FavExerciseLabel.Text = "אוטומטי (אין אימונים)";
            FavExerciseImg.Source = "tab_workouts.png";
        }
        else
        {
            var ex = WorkoutStore.AvailableExercises.FirstOrDefault(e => e.Name == activeFavName);
            if (ex != null) FavExerciseImg.Source = ex.ImagePath;

            FavExerciseLabel.Text = isManual ? activeFavName : $"אוטומטי ({activeFavName})";
        }
    }

    private void OnChangeFavoriteClicked(object sender, EventArgs e)
    {
        FavExerciseOverlay.IsVisible = true;
        FavExerciseOverlay.Opacity = 0;
        FavExerciseOverlay.FadeTo(1, 150);
    }

    private void OnCancelFavOverlay(object sender, EventArgs e)
    {
        FavExerciseOverlay.FadeTo(0, 150).ContinueWith(t =>
            MainThread.BeginInvokeOnMainThread(() => FavExerciseOverlay.IsVisible = false));
    }

    private async void OnManualFavSelected(object sender, EventArgs e)
    {
        OnCancelFavOverlay(sender, e);

        var page = new SelectExercisePage();
        page.ExerciseSelected += (ex) =>
        {
            WorkoutStore.Profile.FavoriteExerciseOverride = ex.Name;
            PersistenceService.SaveProfile();
            UpdateFavoriteExerciseUI();
        };
        await Navigation.PushAsync(page);
    }

    private void OnAutoFavSelected(object sender, EventArgs e)
    {
        OnCancelFavOverlay(sender, e);
        WorkoutStore.Profile.FavoriteExerciseOverride = "";
        PersistenceService.SaveProfile();
        UpdateFavoriteExerciseUI();
    }

    // ─── תמונות (Overlay רגיל לזום) ───
    private void OnImageZoomClicked(object sender, EventArgs e)
    {
        if (sender is Image img && img.Source != null)
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

    private void OnProfileDataChanged(object sender, EventArgs e)
    {
        var p = WorkoutStore.Profile;
        p.Name = NameEntry.Text;

        if (GenderPicker.SelectedItem != null)
            p.Gender = GenderPicker.SelectedItem.ToString();

        if (BarWeightPicker.SelectedIndex == 0)
            p.DefaultBarWeight = 15;
        else if (BarWeightPicker.SelectedIndex == 1)
            p.DefaultBarWeight = 20;

        if (GoalPicker.SelectedItem != null)
            p.Goal = GoalPicker.SelectedItem.ToString();

        PersistenceService.SaveProfile();
    }

    private async void OnPickImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result != null)
            {
                var localPath = Path.Combine(FileSystem.AppDataDirectory, "profile_img.png");
                using (var stream = await result.OpenReadAsync())
                using (var newStream = File.OpenWrite(localPath))
                {
                    await stream.CopyToAsync(newStream);
                }
                WorkoutStore.Profile.ProfileImagePath = localPath;
                ProfileImage.Source = ImageSource.FromFile(localPath);
                PersistenceService.SaveProfile();
            }
        }
        catch { await DisplayAlert("שגיאה", "לא ניתן היה לטעון את התמונה", "אישור"); }
    }

    private void OnAddWeightClicked(object sender, EventArgs e)
    {
        if (!double.TryParse(NewWeightEntry.Text, out double weight) || weight < 30 || weight > 300)
        {
            DisplayAlert("שגיאה", "המשקל חייב להיות בין 30 ל-300 ק״ג", "אישור");
            return;
        }

        var entry = new WeightEntry
        {
            Date = WeightDatePicker.Date,
            Weight = weight,
            Phase = WorkoutStore.Profile.Goal ?? ""
        };

        WorkoutStore.Profile.WeightHistory ??= new List<WeightEntry>();
        WorkoutStore.Profile.WeightHistory.RemoveAll(w => w.Date.Date == entry.Date.Date);
        WorkoutStore.Profile.WeightHistory.Add(entry);
        PersistenceService.SaveProfile();

        NewWeightEntry.Text = "";
        LoadProfileData();
        RefreshWeightChart();
        LoadWeightHistory();
    }

    private void OnEditWeightClicked(object sender, EventArgs e)
    {
        _isEditingWeight = !_isEditingWeight;
        EditWeightBtn.Text = _isEditingWeight ? "סיום" : "ערוך";
        ClearAllWeightsBtn.IsVisible = _isEditingWeight;
        LoadWeightHistory();
    }

    private async void OnClearAllWeightsClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("אזהרה", "האם אתה בטוח שברצונך למחוק את כל היסטוריית השקילות שלך?", "כן, מחק", "ביטול");
        if (confirm)
        {
            WorkoutStore.Profile.WeightHistory?.Clear();
            PersistenceService.SaveProfile();
            _isEditingWeight = false;
            EditWeightBtn.Text = "ערוך";
            ClearAllWeightsBtn.IsVisible = false;
            LoadProfileData();
            RefreshWeightChart();
            LoadWeightHistory();
        }
    }

    private async void OnSeeAllWeightsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new WeightHistoryPage());
    }

    private void RefreshWeightChart()
    {
        var history = WorkoutStore.Profile.WeightHistory?.OrderBy(w => w.Date).TakeLast(20).ToList();

        if (history == null || history.Count < 2)
        {
            ChartNoDataLabel.IsVisible = true;
            WeightChartView.IsVisible = false;
            return;
        }

        ChartNoDataLabel.IsVisible = false;
        WeightChartView.IsVisible = true;

        var chartData = history.Select(w => {
            string emoji = "";
            if (!string.IsNullOrEmpty(w.Phase))
                emoji = w.Phase.Contains("מסה") ? "📈" : (w.Phase.Contains("חיטוב") ? "📉" : "⚖️");
            return (w.Date.ToString("dd/MM"), w.Weight, emoji);
        }).ToList();

        // ─── החישוב הדינמי והחכם של הגבולות ───
        double minWeight = history.Min(w => w.Weight);
        double maxWeight = history.Max(w => w.Weight);

        // לוקחים 20 ק"ג למעלה ו-20 ק"ג למטה, אבל לא חורגים מהגבולות ההגיוניים של 30 ו-300
        double dynamicMin = Math.Max(30, minWeight - 10);
        double dynamicMax = Math.Min(300, maxWeight + 10);

        _chartDrawable = new InteractiveLineChartDrawable(chartData, "#6c47ff", "ק\"ג", fixedMin: dynamicMin, fixedMax: dynamicMax);
        WeightChartView.Drawable = _chartDrawable;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            var point = e.GetPosition(WeightChartView);
            if (point.HasValue)
            {
                _chartDrawable.HandleTap((float)point.Value.X, (float)point.Value.Y);
                WeightChartView.Invalidate();

                _tooltipTimerCts?.Cancel();
                _tooltipTimerCts = new CancellationTokenSource();
                var token = _tooltipTimerCts.Token;

                try
                {
                    await Task.Delay(2500, token);
                    if (!token.IsCancellationRequested)
                    {
                        _chartDrawable.ClearSelection();
                        WeightChartView.Invalidate();
                    }
                }
                catch { }
            }
        };
        WeightChartView.GestureRecognizers.Clear();
        WeightChartView.GestureRecognizers.Add(tapGesture);
    }

    private void LoadWeightHistory()
    {
        WeightHistoryContainer.Children.Clear();
        var allHistory = WorkoutStore.Profile.WeightHistory?.OrderByDescending(w => w.Date).ToList();

        if (allHistory == null || !allHistory.Any())
        {
            SeeAllWeightsBtn.IsVisible = false;
            return;
        }

        var displayHistory = allHistory.Take(5).ToList();
        SeeAllWeightsBtn.IsVisible = allHistory.Count > 5 && !_isEditingWeight;

        foreach (var entry in displayHistory)
        {
            var row = new Grid { Margin = new Thickness(0, 4) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            if (_isEditingWeight) row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string phaseEmoji = "";
            if (!string.IsNullOrEmpty(entry.Phase))
                phaseEmoji = entry.Phase.Contains("מסה") ? "📈 " : (entry.Phase.Contains("חיטוב") ? "📉 " : "⚖️ ");

            var dateLabel = new Label { Text = $"{phaseEmoji}{entry.Date:dd/MM/yy}", FontSize = 13, TextColor = Color.FromArgb("#aaaacc"), VerticalOptions = LayoutOptions.Center };
            var weightFrame = new Frame { BackgroundColor = Color.FromArgb("#22cc6622"), BorderColor = Color.FromArgb("#22cc66"), CornerRadius = 8, Padding = new Thickness(10, 4), HasShadow = false };
            weightFrame.Content = new Label { Text = $"{entry.Weight:F1} ק\"ג", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#22cc66") };

            row.Children.Add(dateLabel);
            Grid.SetColumn(weightFrame, 1);
            row.Children.Add(weightFrame);

            if (_isEditingWeight)
            {
                var delBtn = new Button { Text = "✕", TextColor = Color.FromArgb("#ff5555"), BackgroundColor = Colors.Transparent, Padding = 0, WidthRequest = 30, HeightRequest = 30, Margin = new Thickness(10, 0, 0, 0) };
                delBtn.Clicked += (s, e) =>
                {
                    WorkoutStore.Profile.WeightHistory.Remove(entry);
                    PersistenceService.SaveProfile();
                    LoadProfileData();
                    RefreshWeightChart();
                    LoadWeightHistory();
                };
                Grid.SetColumn(delBtn, 2);
                row.Children.Add(delBtn);
            }

            WeightHistoryContainer.Children.Add(row);
        }
    }

    private void OnCalculateRmClicked(object sender, EventArgs e)
    {
        if (double.TryParse(RmWeightEntry.Text, out double w) && int.TryParse(RmRepsEntry.Text, out int r))
        {
            if (r <= 0) return;
            if (r == 1) { RmResultLabel.Text = $"ה-1RM שלך הוא {w} ק\"ג"; return; }
            double oneRm = w / (1.0278 - (0.0278 * r));
            RmResultLabel.Text = $"1RM משוער: {oneRm:F1} ק\"ג";
        }
    }
}