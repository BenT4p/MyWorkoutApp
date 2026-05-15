using MyWorkoutApp.Models;
using MyWorkoutApp.Services;

namespace MyWorkoutApp.Pages;

public partial class ActiveWorkoutPage : ContentPage
{
    // ── State ──────────────────────────────────────────────────────────
    private readonly List<WorkoutExercise> _exercises = new();
    private readonly DateTime _startTime;
    private IDispatcherTimer? _timer;
    private string _workoutName;
    private bool _isEditing = false;

    // ── Colors ─────────────────────────────────────────────────────────
    private static readonly Color BgDark = Color.FromArgb("#1a1a2e");
    private static readonly Color BgCard = Color.FromArgb("#252547");
    private static readonly Color BgRow = Color.FromArgb("#1e1e3f");
    private static readonly Color Purple = Color.FromArgb("#6c47ff");
    private static readonly Color TextWhite = Colors.White;
    private static readonly Color TextDim = Color.FromArgb("#aaaacc");
    private static readonly Color TextMuted = Color.FromArgb("#666688");

    // ── Constructor ────────────────────────────────────────────────────
    public ActiveWorkoutPage(WorkoutTemplate template)
    {
        InitializeComponent();

        _workoutName = template.Name;
        WorkoutNameLabel.Text = _workoutName;
        _startTime = DateTime.Now;

        foreach (var ex in template.Exercises)
            _exercises.Add(CreateExerciseWithMemory(ex));

        StartTimer();
        RebuildUI();
    }

    private WorkoutExercise CreateExerciseWithMemory(Exercise ex)
    {
        var we = new WorkoutExercise { Exercise = ex };

        int previousSetCount = 0;
        string previousNote = string.Empty; // משתנה לשמירת ההערה מהעבר

        if (WorkoutStore.History != null)
        {
            foreach (var session in WorkoutStore.History)
            {
                var pastEx = session.Exercises.FirstOrDefault(e => e.Exercise.Name == ex.Name);
                if (pastEx != null)
                {
                    if (pastEx.Sets.Count > 0)
                    {
                        previousSetCount = pastEx.Sets.Count;
                    }

                    // שולפים את ההערה מהאימון האחרון שבוצע
                    previousNote = pastEx.Notes;
                    break;
                }
            }
        }

        we.Notes = previousNote; // מגדירים את ההערה הישנה כברירת מחדל לאימון הנוכחי

        int setsToCreate = previousSetCount > 0 ? previousSetCount : 1;

        for (int i = 1; i <= setsToCreate; i++)
        {
            we.Sets.Add(new WorkoutSet { SetNumber = i });
        }

        return we;
    }

    private void StartTimer()
    {
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (_, _) =>
        {
            var elapsed = DateTime.Now - _startTime;
            TimerLabel.Text = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
        };
        _timer.Start();
    }

    private void OnEditModeClicked(object sender, EventArgs e)
    {
        _isEditing = !_isEditing;

        if (_isEditing)
        {
            // מצב עריכה פעיל: כפתור מלא בצבע כתום וטקסט לבן
            EditModeButton.Text = "סיום עריכה";
            EditModeButton.BackgroundColor = Color.FromArgb("#ff8800");
            EditModeButton.TextColor = Colors.White;
            EditModeButton.BorderColor = Colors.Transparent;
        }
        else
        {
            // מצב רגיל: כפתור שקוף עם מסגרת כתומה וטקסט כתום
            EditModeButton.Text = "ערוך";
            EditModeButton.BackgroundColor = Colors.Transparent;
            EditModeButton.TextColor = Color.FromArgb("#ff8800");
            EditModeButton.BorderColor = Color.FromArgb("#ff880044");
        }

        AddExerciseButton.IsVisible = _isEditing;
        RebuildUI();
    }

    private async void OnAddExerciseClicked(object sender, EventArgs e)
    {
        var page = new SelectExercisePage();
        page.ExerciseSelected += ex =>
        {
            if (_exercises.Any(we => we.Exercise.Name == ex.Name))
            {
                Dispatcher.Dispatch(async () => await DisplayAlert("שים לב", "התרגיל כבר קיים באימון", "הבנתי"));
                return;
            }

            _exercises.Add(CreateExerciseWithMemory(ex));
            RebuildUI();
        };
        await Navigation.PushAsync(page);
    }

    private async void OnFinishWorkoutClicked(object sender, EventArgs e)
    {
        _timer?.Stop();

        string? name = await DisplayPromptAsync(
            "שם לאימון",
            "תן שם לאימון שסיימת (אפשר להשאיר ריק):",
            placeholder: _workoutName,
            initialValue: _workoutName);

        if (name == null)
        {
            _timer?.Start();
            return;
        }
        if (string.IsNullOrWhiteSpace(name))
            name = _workoutName;

        var session = new WorkoutSession
        {
            Name = name,
            StartTime = _startTime,
            EndTime = DateTime.Now,
            Exercises = new List<WorkoutExercise>(_exercises)
        };

        WorkoutStore.History.Insert(0, session);
        PersistenceService.SaveHistory();

        await Navigation.PopAsync();
    }

    private void RebuildUI()
    {
        ExercisesContainer.Children.Clear();
        for (int i = 0; i < _exercises.Count; i++)
        {
            ExercisesContainer.Children.Add(BuildExerciseCard(_exercises[i], i));
        }
    }

    private View BuildExerciseCard(WorkoutExercise we, int index)
    {
        var frame = new Frame { BackgroundColor = BgCard, BorderColor = Color.FromArgb("#3a3a6a"), CornerRadius = 14, Padding = 12, HasShadow = false };
        var outerStack = new VerticalStackLayout { Spacing = 8 };

        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Auto),
                new(GridLength.Star),
                new(GridLength.Auto),
                new(GridLength.Auto),
            },
            ColumnSpacing = 10,
            HeightRequest = 90 // הגדלנו קצת כדי שיהיה מקום להערה
        };

        var arrowStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        var upBtn = new Button { Text = "▲", FontSize = 11, BackgroundColor = index > 0 ? Purple : Color.FromArgb("#3a3a6a"), TextColor = TextWhite, WidthRequest = 32, HeightRequest = 28, CornerRadius = 6, Padding = 0, IsEnabled = index > 0 };
        upBtn.Clicked += (_, _) => { MoveExercise(index, -1); };
        var downBtn = new Button { Text = "▼", FontSize = 11, BackgroundColor = index < _exercises.Count - 1 ? Purple : Color.FromArgb("#3a3a6a"), TextColor = TextWhite, WidthRequest = 32, HeightRequest = 28, CornerRadius = 6, Padding = 0, IsEnabled = index < _exercises.Count - 1 };
        downBtn.Clicked += (_, _) => { MoveExercise(index, +1); };
        arrowStack.Children.Add(upBtn); arrowStack.Children.Add(downBtn);
        arrowStack.IsVisible = _isEditing;
        Grid.SetColumn(arrowStack, 0);

        var nameStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Spacing = 2 };
        nameStack.Children.Add(new Label { Text = we.Exercise.Name, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = TextWhite, LineBreakMode = LineBreakMode.TailTruncation });
        nameStack.Children.Add(new Label { Text = we.Exercise.MuscleGroup, FontSize = 12, TextColor = TextDim });

        // --- הוספת תיבת ההערות ---
        var notesEntry = new Entry
        {
            Text = we.Notes, // טוען את ההערה ששמרנו בזיכרון
            Placeholder = "הוסף הערה (למשל: ספסל בשיפוע עליון)...",
            PlaceholderColor = Color.FromArgb("#555577"),
            TextColor = Color.FromArgb("#aaaacc"),
            BackgroundColor = Colors.Transparent,
            FontSize = 12,
            MaxLength = 100, // הגבלה ל-100 תווים כמו שביקשת
            Margin = new Thickness(0, 5, 0, 0),
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing // מוסיף כפתור מחיקה קטן בצד
        };
        notesEntry.TextChanged += (_, args) => { we.Notes = args.NewTextValue ?? string.Empty; };
        nameStack.Children.Add(notesEntry);

        Grid.SetColumn(nameStack, 1);

        var imgFrame = new Frame
        {
            WidthRequest = 90,
            HeightRequest = 60,
            CornerRadius = 8,
            Padding = 0,
            IsClippedToBounds = true,
            BackgroundColor = BgDark,
            BorderColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center
        };
        var exerciseImg = new Image
        {
            Source = we.Exercise.ImagePath,
            Aspect = Aspect.AspectFit,
            WidthRequest = 90,
            HeightRequest = 60
        };
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnImageZoomClicked;
        exerciseImg.GestureRecognizers.Add(tapGesture);
        imgFrame.Content = exerciseImg;
        Grid.SetColumn(imgFrame, 2);

        var delBtn = new Button { Text = "✕", FontSize = 14, BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#ff5555"), WidthRequest = 36, HeightRequest = 36, Padding = 0 };
        delBtn.Clicked += (_, _) => { _exercises.Remove(we); RebuildUI(); };
        delBtn.IsVisible = _isEditing;
        Grid.SetColumn(delBtn, 3);

        headerGrid.Children.Add(arrowStack);
        headerGrid.Children.Add(nameStack);
        headerGrid.Children.Add(imgFrame);
        headerGrid.Children.Add(delBtn);

        outerStack.Children.Add(headerGrid);
        outerStack.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#2a2a50") });

        var setsContainer = new VerticalStackLayout { Spacing = 3 };
        var tableHeader = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(new GridLength(38)), new(GridLength.Star), new(GridLength.Star), new(new GridLength(40)) } };
        tableHeader.Children.Add(MakeHeaderLabel("סט", 0));
        tableHeader.Children.Add(MakeHeaderLabel("משקל", 1));
        tableHeader.Children.Add(MakeHeaderLabel("חזרות", 2));

        var addSetBtn = new Button { Text = "+", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = TextWhite, BackgroundColor = Purple, WidthRequest = 34, HeightRequest = 34, CornerRadius = 17, Padding = 0 };
        addSetBtn.Clicked += (_, _) => { AddSet(we, setsContainer); };
        Grid.SetColumn(addSetBtn, 3);
        tableHeader.Children.Add(addSetBtn);

        outerStack.Children.Add(tableHeader);
        foreach (var s in we.Sets) setsContainer.Children.Add(BuildSetRow(s, we, setsContainer));
        outerStack.Children.Add(setsContainer);

        frame.Content = outerStack;
        return frame;
    }

    private (double weight, int reps) GetPreviousSetData(string exerciseName, int setNumber)
    {
        if (WorkoutStore.History == null) return (0, 0);

        foreach (var session in WorkoutStore.History)
        {
            var pastExercise = session.Exercises.FirstOrDefault(e => e.Exercise.Name == exerciseName);
            if (pastExercise != null)
            {
                var pastSet = pastExercise.Sets.FirstOrDefault(s => s.SetNumber == setNumber);
                if (pastSet != null)
                {
                    return (pastSet.Weight, pastSet.Reps);
                }
                return (0, 0);
            }
        }
        return (0, 0);
    }

    private View BuildSetRow(WorkoutSet set, WorkoutExercise we, VerticalStackLayout container)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(new GridLength(38)), new(GridLength.Star), new(GridLength.Star), new(new GridLength(40)) }, BackgroundColor = BgRow, Padding = new Thickness(4, 5) };
        grid.Children.Add(new Label { Text = set.SetNumber.ToString(), FontSize = 14, TextColor = TextDim, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center });

        var (prevWeight, prevReps) = GetPreviousSetData(we.Exercise.Name, set.SetNumber);

        string weightPlaceholder = prevWeight > 0 ? prevWeight.ToString() : "0";
        string repsPlaceholder = prevReps > 0 ? prevReps.ToString() : "0";

        var weightEntry = new Entry { Placeholder = weightPlaceholder, Keyboard = Keyboard.Numeric, TextColor = TextWhite, BackgroundColor = Colors.Transparent, FontSize = 14, HorizontalTextAlignment = TextAlignment.Center, Text = set.Weight > 0 ? set.Weight.ToString() : "" };
        weightEntry.TextChanged += (_, args) => { if (double.TryParse(args.NewTextValue, out double w)) set.Weight = w; };
        Grid.SetColumn(weightEntry, 1);

        var repsEntry = new Entry { Placeholder = repsPlaceholder, Keyboard = Keyboard.Numeric, TextColor = TextWhite, BackgroundColor = Colors.Transparent, FontSize = 14, HorizontalTextAlignment = TextAlignment.Center, Text = set.Reps > 0 ? set.Reps.ToString() : "" };
        repsEntry.TextChanged += (_, args) => { if (int.TryParse(args.NewTextValue, out int r)) set.Reps = r; };
        Grid.SetColumn(repsEntry, 2);

        var delBtn = new Button { Text = "✕", FontSize = 12, BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#ff5555"), WidthRequest = 36, HeightRequest = 34, Padding = 0 };
        delBtn.Clicked += (_, _) => { we.Sets.Remove(set); for (int i = 0; i < we.Sets.Count; i++) we.Sets[i].SetNumber = i + 1; RebuildSetsContainer(container, we); };
        Grid.SetColumn(delBtn, 3);

        grid.Children.Add(weightEntry); grid.Children.Add(repsEntry); grid.Children.Add(delBtn);
        return grid;
    }

    private void AddSet(WorkoutExercise we, VerticalStackLayout container)
    {
        var set = new WorkoutSet { SetNumber = we.Sets.Count + 1 };
        we.Sets.Add(set);
        container.Children.Add(BuildSetRow(set, we, container));
    }

    private void RebuildSetsContainer(VerticalStackLayout container, WorkoutExercise we)
    {
        container.Children.Clear();
        foreach (var s in we.Sets) container.Children.Add(BuildSetRow(s, we, container));
    }

    private void MoveExercise(int fromIndex, int direction)
    {
        int toIndex = fromIndex + direction;
        if (toIndex < 0 || toIndex >= _exercises.Count) return;

        var tmp = _exercises[fromIndex];
        _exercises[fromIndex] = _exercises[toIndex];
        _exercises[toIndex] = tmp;

        RebuildUI();
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

    private static Label MakeHeaderLabel(string text, int col)
    {
        var lbl = new Label
        {
            Text = text,
            FontSize = 12,
            TextColor = Color.FromArgb("#666688"),
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(lbl, col);
        return lbl;
    }

    protected override bool OnBackButtonPressed()
    {
        Dispatcher.Dispatch(async () =>
        {
            bool quit = await DisplayAlert(
                "יציאה מהאימון",
                "אם תצא עכשיו האימון לא יישמר. להמשיך?",
                "צא ללא שמירה", "המשך אימון");

            if (quit)
            {
                _timer?.Stop();
                await Navigation.PopAsync();
            }
        });
        return true;
    }
}