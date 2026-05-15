using Microsoft.Maui.Graphics;
using MyWorkoutApp.Models;
using MyWorkoutApp.Services;

namespace MyWorkoutApp.Pages;

public partial class HomePage : ContentPage
{
    private bool _showWeight = true;
    private InteractiveLineChartDrawable _chartDrawable;
    private CancellationTokenSource _tooltipTimerCts;

    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        PersistenceService.Load();

        if (WorkoutStore.Profile != null)
        {
            BarWeightPicker.SelectedIndexChanged -= OnPlateTargetChanged;
            BarWeightPicker.SelectedIndex = WorkoutStore.Profile.DefaultBarWeight == 15 ? 0 : 1;
            BarWeightPicker.SelectedIndexChanged += OnPlateTargetChanged;
        }

        UpdateGreeting();
        BuildWeeklyCalendar();
        UpdateStats();
        UpdateFavoriteExercise();
        RefreshChart();

        if (!string.IsNullOrEmpty(PlateTargetEntry.Text))
            OnPlateTargetChanged(this, EventArgs.Empty);
    }

    private void UpdateGreeting()
    {
        int hour = DateTime.Now.Hour;
        string fullName = WorkoutStore.Profile?.Name ?? "מתאמן";
        string firstName = fullName.Split(' ')[0];

        string gender = WorkoutStore.Profile?.Gender ?? "זכר";
        string goal = WorkoutStore.Profile?.Goal ?? "";

        GreetingLabel.Text = hour switch
        {
            >= 5 and < 12 => $"בוקר טוב, {firstName}!",
            >= 12 and < 18 => $"צהריים טובים, {firstName}!",
            _ => $"ערב טוב, {firstName}!"
        };

        TipLabel.Text = TipService.GetRandomTip(gender, goal, hour);
    }

    private void BuildWeeklyCalendar()
    {
        var workoutDates = WorkoutStore.History?.Select(w => w.StartTime.Date).ToHashSet() ?? new HashSet<DateTime>();

        int dailyStreak = 0;
        DateTime target = DateTime.Today;
        if (!workoutDates.Contains(target)) target = target.AddDays(-1);
        while (workoutDates.Contains(target)) { dailyStreak++; target = target.AddDays(-1); }
        StreakLabel.Text = dailyStreak == 0 ? "אין רצף עדיין" : $"{dailyStreak} ימים ברצף";

        DateTime today = DateTime.Today;
        DateTime startOfWeek = StartOfWeek(today);
        int weekCount = Enumerable.Range(0, 7).Count(i => workoutDates.Contains(startOfWeek.AddDays(i)));
        WeeklyCountLabel.Text = $"{weekCount} השבוע";

        WeeklyCalendarContainer.Children.Clear();
        for (int i = 0; i <= 6; i++)
        {
            var day = startOfWeek.AddDays(i);
            bool worked = workoutDates.Contains(day);
            bool isToday = day == today;
            bool isFuture = day > today;

            var stack = new VerticalStackLayout { Spacing = 5, HorizontalOptions = LayoutOptions.Center };
            stack.Children.Add(new Label { Text = GetHebrewDay(day.DayOfWeek), FontSize = 11, TextColor = isToday ? Color.FromArgb("#ffffff") : Color.FromArgb("#6666aa"), HorizontalOptions = LayoutOptions.Center, FontAttributes = isToday ? FontAttributes.Bold : FontAttributes.None });

            if (isFuture) stack.Children.Add(new Frame { WidthRequest = 24, HeightRequest = 24, CornerRadius = 12, BackgroundColor = Color.FromArgb("#1a1a2e"), BorderColor = Color.FromArgb("#252547"), Padding = 0, HasShadow = false, HorizontalOptions = LayoutOptions.Center });
            else if (worked) stack.Children.Add(new Label { Text = "🔥", FontSize = 18, HorizontalOptions = LayoutOptions.Center });
            else stack.Children.Add(new Frame { WidthRequest = 24, HeightRequest = 24, CornerRadius = 12, BackgroundColor = isToday ? Color.FromArgb("#3322ff44") : Color.FromArgb("#1e1e3a"), BorderColor = isToday ? Color.FromArgb("#3322ff") : Color.FromArgb("#2a2a55"), Padding = 0, HasShadow = false, HorizontalOptions = LayoutOptions.Center });

            WeeklyCalendarContainer.Children.Add(stack);
        }
    }

    private DateTime StartOfWeek(DateTime dt)
    {
        int diff = (int)dt.DayOfWeek;
        return dt.AddDays(-diff).Date;
    }

    private void UpdateStats()
    {
        TotalWorkoutsLabel.Text = WorkoutStore.TotalWorkouts.ToString();
        TotalSetsLabel.Text = WorkoutStore.TotalSets.ToString();

        var workoutDates = WorkoutStore.History?.Select(w => w.StartTime.Date).ToHashSet() ?? new HashSet<DateTime>();
        var activeWeeks = workoutDates.Select(d => StartOfWeek(d)).ToHashSet();

        DateTime currentWeek = StartOfWeek(DateTime.Today);
        int weeklyStreak = 0;
        DateTime checkWeek = currentWeek;

        if (!activeWeeks.Contains(checkWeek))
        {
            checkWeek = checkWeek.AddDays(-7);
        }

        while (activeWeeks.Contains(checkWeek))
        {
            weeklyStreak++;
            checkWeek = checkWeek.AddDays(-7);
        }

        GlobalStreakLabel.Text = weeklyStreak.ToString();
    }

    private string GetActualFavoriteExercise()
    {
        if (!string.IsNullOrEmpty(WorkoutStore.Profile?.FavoriteExerciseOverride))
            return WorkoutStore.Profile.FavoriteExerciseOverride;

        var allExercises = WorkoutStore.History?.SelectMany(s => s.Exercises).ToList();
        if (allExercises == null || !allExercises.Any()) return "אין עדיין";

        return allExercises.GroupBy(e => e.Exercise.Name)
                           .OrderByDescending(g => g.Count())
                           .First().Key;
    }

    private void UpdateFavoriteExercise()
    {
        string fav = GetActualFavoriteExercise();
        FavoriteExerciseLabel.Text = fav;
        if (fav == "אין עדיין") return;

        var sessions = WorkoutStore.History.SelectMany(s => s.Exercises).Where(e => e.Exercise.Name == fav).ToList();
        int timesLogged = sessions.Count;
        double bestWeight = sessions.SelectMany(e => e.Sets).Select(s => s.Weight).DefaultIfEmpty(0).Max();
        FavoriteExerciseStatsLabel.Text = $"{timesLogged} סטים • שיא {bestWeight:F1} ק\"ג";
    }

    private void RefreshChart()
    {
        string fav = GetActualFavoriteExercise();
        bool noData = WorkoutStore.History == null || !WorkoutStore.History.Any();

        ChartNoDataLayout.IsVisible = noData;
        ProgressChart.IsVisible = !noData;
        if (noData) return;

        var points = WorkoutStore.History
            .Select(session =>
            {
                var sets = session.Exercises.Where(e => e.Exercise.Name == fav).SelectMany(e => e.Sets).ToList();
                double value = _showWeight ? sets.Select(s => s.Weight).DefaultIfEmpty(0).Max() : sets.Count;
                return (Date: session.StartTime.Date, Value: value);
            })
            .Where(p => p.Value > 0)
            .OrderBy(p => p.Date)
            .TakeLast(12)
            .Reverse()
            .ToList();

        ChartTitleLabel.Text = $"התקדמות — {fav}";
        ChartSubtitleLabel.Text = _showWeight ? "משקל מקסימלי לאימון (ק״ג)" : "סטים לאימון";

        var chartData = points.Select(p => (p.Date.ToString("dd/MM"), p.Value, "")).ToList();

        _chartDrawable = new InteractiveLineChartDrawable(
            chartData,
            _showWeight ? "#3322ff" : "#ff8800",
            _showWeight ? "ק\"ג" : "סטים",
            fixedMin: 0);

        ProgressChart.Drawable = _chartDrawable;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            var point = e.GetPosition(ProgressChart);
            if (point.HasValue)
            {
                _chartDrawable.HandleTap((float)point.Value.X, (float)point.Value.Y);
                ProgressChart.Invalidate();

                _tooltipTimerCts?.Cancel();
                _tooltipTimerCts = new CancellationTokenSource();
                var token = _tooltipTimerCts.Token;

                try
                {
                    await Task.Delay(2500, token);
                    if (!token.IsCancellationRequested)
                    {
                        _chartDrawable.ClearSelection();
                        ProgressChart.Invalidate();
                    }
                }
                catch { }
            }
        };
        ProgressChart.GestureRecognizers.Clear();
        ProgressChart.GestureRecognizers.Add(tapGesture);
    }

    private void OnChartToggleWeight(object sender, EventArgs e)
    {
        _showWeight = true;
        ChartToggleWeightBtn.BackgroundColor = Color.FromArgb("#3322ff");
        ChartToggleWeightBtn.TextColor = Colors.White;
        ChartToggleSetsBtn.BackgroundColor = Colors.Transparent;
        ChartToggleSetsBtn.TextColor = Color.FromArgb("#6666aa");
        RefreshChart();
    }

    private void OnChartToggleSets(object sender, EventArgs e)
    {
        _showWeight = false;
        ChartToggleSetsBtn.BackgroundColor = Color.FromArgb("#ff8800");
        ChartToggleSetsBtn.TextColor = Colors.White;
        ChartToggleWeightBtn.BackgroundColor = Colors.Transparent;
        ChartToggleWeightBtn.TextColor = Color.FromArgb("#6666aa");
        RefreshChart();
    }

    private static readonly double[] AvailablePlates = { 25, 20, 15, 10, 5, 2.5, 1.25 };

    private void OnPlateTargetChanged(object sender, EventArgs e)
    {
        PlatesResultContainer.Children.Clear();
        BarbellView.Drawable = null;
        double barWeight = BarWeightPicker.SelectedIndex == 0 ? 15 : 20;

        if (WorkoutStore.Profile != null && WorkoutStore.Profile.DefaultBarWeight != barWeight)
        {
            WorkoutStore.Profile.DefaultBarWeight = barWeight;
            PersistenceService.SaveProfile();
        }

        if (!double.TryParse(PlateTargetEntry.Text, out double target) || target <= 0)
        {
            PlateInfoLabel.Text = "הזן משקל יעד כדי לחשב";
            PlateInfoLabel.TextColor = Color.FromArgb("#44447a");
            PlateInfoLabel.IsVisible = true;
            return;
        }

        double remaining = target - barWeight;
        if (remaining < 0) { PlateInfoLabel.Text = $"המשקל נמוך ממשקל הבר ({barWeight} ק\"ג)"; PlateInfoLabel.TextColor = Color.FromArgb("#ff4444"); PlateInfoLabel.IsVisible = true; return; }
        if (remaining == 0) { PlateInfoLabel.Text = $"בר בלבד — {barWeight} ק\"ג"; PlateInfoLabel.TextColor = Color.FromArgb("#22cc66"); PlateInfoLabel.IsVisible = true; BarbellView.Drawable = new BarbellDrawable(new List<double>(), barWeight); return; }

        double perSide = remaining / 2.0;
        var platesOneSide = new List<double>();
        double rem = perSide;

        foreach (var plate in AvailablePlates)
            while (rem >= plate - 0.001) { platesOneSide.Add(plate); rem -= plate; }

        bool exact = rem < 0.01;
        double actualTotal = barWeight + platesOneSide.Sum() * 2;
        PlateInfoLabel.IsVisible = false;

        var summaryFrame = new Frame { BackgroundColor = Color.FromArgb(exact ? "#22cc6622" : "#ff880022"), BorderColor = Color.FromArgb(exact ? "#22cc66" : "#ff8800"), CornerRadius = 12, Padding = new Thickness(14, 10), HasShadow = false };
        summaryFrame.Content = new Label { Text = exact ? $"סה\"כ {actualTotal:F2} ק\"ג  ✓" : $"קרוב ביותר: {actualTotal:F2} ק\"ג", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = exact ? Color.FromArgb("#22cc66") : Color.FromArgb("#ff8800"), HorizontalOptions = LayoutOptions.Center };
        PlatesResultContainer.Children.Add(summaryFrame);

        var grouped = platesOneSide.GroupBy(p => p).OrderByDescending(g => g.Key);
        foreach (var grp in grouped)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }, Margin = new Thickness(4, 2) };
            row.Children.Add(new Label { Text = $"{grp.Key} ק\"ג", FontSize = 15, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center });
            var countFrame = new Frame { BackgroundColor = Color.FromArgb("#3322ff33"), BorderColor = Color.FromArgb("#3322ff"), CornerRadius = 8, Padding = new Thickness(10, 4), HasShadow = false, HorizontalOptions = LayoutOptions.End };
            countFrame.Content = new Label { Text = $"×{grp.Count()} כל צד", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#8888ff") };
            Grid.SetColumn(countFrame, 1);
            row.Children.Add(countFrame);
            PlatesResultContainer.Children.Add(row);
        }

        BarbellView.Drawable = new BarbellDrawable(platesOneSide, barWeight);
        BarbellView.Invalidate();
    }

    private static string GetHebrewDay(DayOfWeek day) => day switch { DayOfWeek.Sunday => "א'", DayOfWeek.Monday => "ב'", DayOfWeek.Tuesday => "ג'", DayOfWeek.Wednesday => "ד'", DayOfWeek.Thursday => "ה'", DayOfWeek.Friday => "ו'", DayOfWeek.Saturday => "ש'", _ => "" };
}