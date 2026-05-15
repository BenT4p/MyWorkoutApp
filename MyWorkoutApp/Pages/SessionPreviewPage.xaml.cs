using MyWorkoutApp.Models;

namespace MyWorkoutApp.Pages;

public partial class SessionPreviewPage : ContentPage
{
    public SessionPreviewPage(WorkoutSession session)
    {
        InitializeComponent();
        SessionNameLabel.Text = session.Name;
        SessionDateLabel.Text = session.StartTime.ToString("dd/MM/yyyy HH:mm");
        BuildPreviewUI(session);
    }

    private void BuildPreviewUI(WorkoutSession session)
    {
        foreach (var we in session.Exercises)
        {
            var frame = new Frame { BackgroundColor = Color.FromArgb("#252547"), BorderColor = Color.FromArgb("#3a3a6a"), CornerRadius = 14, Padding = 12, HasShadow = false };
            var stack = new VerticalStackLayout { Spacing = 8 };

            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
                ColumnSpacing = 10,
                // הגדלנו מעט את הגובה כדי שאם יש הערה, יהיה לה מקום נוח
                HeightRequest = 75
            };

            var infoStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
            infoStack.Children.Add(new Label { Text = we.Exercise.Name, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, LineBreakMode = LineBreakMode.TailTruncation });
            infoStack.Children.Add(new Label { Text = we.Exercise.MuscleGroup, FontSize = 12, TextColor = Color.FromArgb("#aaaacc") });

            // --- הוספת תצוגת ההערה (אם קיימת) ---
            if (!string.IsNullOrWhiteSpace(we.Notes))
            {
                infoStack.Children.Add(new Label
                {
                    Text = $"📝 {we.Notes}",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#ffaa00"), // צבע כתמתם בולט אבל עדין
                    FontAttributes = FontAttributes.Italic,
                    Margin = new Thickness(0, 4, 0, 0),
                    LineBreakMode = LineBreakMode.TailTruncation
                });
            }

            Grid.SetColumn(infoStack, 0);

            var imgFrame = new Frame { WidthRequest = 90, HeightRequest = 60, CornerRadius = 8, Padding = 0, IsClippedToBounds = true, BackgroundColor = Color.FromArgb("#1a1a2e"), BorderColor = Colors.Transparent, VerticalOptions = LayoutOptions.Center };
            var exerciseImg = new Image { Source = we.Exercise.ImagePath, Aspect = Aspect.AspectFit };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnImageZoomClicked;
            exerciseImg.GestureRecognizers.Add(tapGesture);

            imgFrame.Content = exerciseImg;
            Grid.SetColumn(imgFrame, 1);

            headerGrid.Children.Add(infoStack);
            headerGrid.Children.Add(imgFrame);
            stack.Children.Add(headerGrid);
            stack.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#2a2a50") });

            var tableHeader = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(new GridLength(40)), new(GridLength.Star), new(GridLength.Star) } };
            tableHeader.Children.Add(new Label { Text = "סט", FontSize = 12, TextColor = Color.FromArgb("#aaaacc"), HorizontalOptions = LayoutOptions.Center });
            var wLbl = new Label { Text = "משקל (ק״ג)", FontSize = 12, TextColor = Color.FromArgb("#aaaacc"), HorizontalOptions = LayoutOptions.Center }; Grid.SetColumn(wLbl, 1);
            var rLbl = new Label { Text = "חזרות", FontSize = 12, TextColor = Color.FromArgb("#aaaacc"), HorizontalOptions = LayoutOptions.Center }; Grid.SetColumn(rLbl, 2);
            tableHeader.Children.Add(wLbl); tableHeader.Children.Add(rLbl);
            stack.Children.Add(tableHeader);

            foreach (var set in we.Sets)
            {
                var row = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(new GridLength(40)), new(GridLength.Star), new(GridLength.Star) }, BackgroundColor = Color.FromArgb("#1e1e3f"), Padding = new Thickness(4, 8), Margin = new Thickness(0, 2) };
                row.Children.Add(new Label { Text = set.SetNumber.ToString(), TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center });
                var wl = new Label { Text = set.Weight.ToString(), TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center }; Grid.SetColumn(wl, 1);
                var rl = new Label { Text = set.Reps.ToString(), TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center }; Grid.SetColumn(rl, 2);
                row.Children.Add(wl); row.Children.Add(rl);
                stack.Children.Add(row);
            }

            frame.Content = stack;
            ExercisesContainer.Children.Add(frame);
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

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}