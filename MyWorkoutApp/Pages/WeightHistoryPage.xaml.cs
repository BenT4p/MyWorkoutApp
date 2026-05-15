using MyWorkoutApp.Services;

namespace MyWorkoutApp.Pages;

public partial class WeightHistoryPage : ContentPage
{
    public WeightHistoryPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAllWeights();
    }

    private void LoadAllWeights()
    {
        AllWeightsContainer.Children.Clear();
        var allHistory = WorkoutStore.Profile.WeightHistory?.OrderByDescending(w => w.Date).ToList();

        if (allHistory == null || !allHistory.Any())
        {
            AllWeightsContainer.Children.Add(new Label
            {
                Text = "אין שקילות שמורות.",
                TextColor = Color.FromArgb("#aaaacc"),
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20)
            });
            return;
        }

        foreach (var entry in allHistory)
        {
            var frame = new Frame { BackgroundColor = Color.FromArgb("#161630"), BorderColor = Color.FromArgb("#2a2a55"), CornerRadius = 12, Padding = 14, HasShadow = false };
            var row = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto) } };

            string phaseEmoji = "";
            if (!string.IsNullOrEmpty(entry.Phase))
                phaseEmoji = entry.Phase.Contains("מסה") ? "📈 " : (entry.Phase.Contains("חיטוב") ? "📉 " : "⚖️ ");

            var dateLabel = new Label { Text = $"{phaseEmoji}{entry.Date:dd/MM/yyyy}", FontSize = 14, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center };

            var weightFrame = new Frame { BackgroundColor = Color.FromArgb("#22cc6622"), BorderColor = Color.FromArgb("#22cc66"), CornerRadius = 8, Padding = new Thickness(10, 4), HasShadow = false, Margin = new Thickness(10, 0) };
            weightFrame.Content = new Label { Text = $"{entry.Weight:F1} ק\"ג", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#22cc66") };

            var delBtn = new Button { Text = "🗑", TextColor = Color.FromArgb("#ff5555"), BackgroundColor = Colors.Transparent, Padding = 0, WidthRequest = 36, HeightRequest = 36, VerticalOptions = LayoutOptions.Center };
            delBtn.Clicked += (s, e) =>
            {
                WorkoutStore.Profile.WeightHistory.Remove(entry);
                PersistenceService.SaveProfile();
                LoadAllWeights();
            };

            row.Children.Add(dateLabel);
            Grid.SetColumn(weightFrame, 1);
            row.Children.Add(weightFrame);
            Grid.SetColumn(delBtn, 2);
            row.Children.Add(delBtn);

            frame.Content = row;
            AllWeightsContainer.Children.Add(frame);
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}