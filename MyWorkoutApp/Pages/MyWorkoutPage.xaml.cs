using MyWorkoutApp.Models;
using MyWorkoutApp.Services;

namespace MyWorkoutApp.Pages;

public partial class MyWorkoutsPage : ContentPage
{
    private bool _showingTemplates = true;

    public MyWorkoutsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshTemplates();
        RefreshHistory();
    }

    // ── Tab switching ─────────────────────────────────────────────────
    // ── Tab switching ─────────────────────────────────────────────────
    private void OnTemplatesTabClicked(object sender, EventArgs e)
    {
        _showingTemplates = true;
        TemplatesTab.IsVisible = true;
        HistoryTab.IsVisible = false;

        TemplatesTabBtn.BackgroundColor = Color.FromArgb("#3322ff");
        TemplatesTabBtn.TextColor = Colors.White;

        HistoryTabBtn.BackgroundColor = Colors.Transparent;
        HistoryTabBtn.TextColor = Color.FromArgb("#6666aa");
    }

    private void OnHistoryTabClicked(object sender, EventArgs e)
    {
        _showingTemplates = false;
        TemplatesTab.IsVisible = false;
        HistoryTab.IsVisible = true;

        HistoryTabBtn.BackgroundColor = Color.FromArgb("#3322ff");
        HistoryTabBtn.TextColor = Colors.White;

        TemplatesTabBtn.BackgroundColor = Colors.Transparent;
        TemplatesTabBtn.TextColor = Color.FromArgb("#6666aa");
    }

    // ── Templates ─────────────────────────────────────────────────────
    private void RefreshTemplates()
    {
        TemplatesList.ItemsSource = null;
        TemplatesList.ItemsSource = WorkoutStore.Templates;
        bool any = WorkoutStore.Templates.Count > 0;
        TemplatesEmpty.IsVisible = !any;
        TemplatesList.IsVisible = any;
    }

    private async void OnTemplateFrameTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is WorkoutTemplate template)
            await Navigation.PushAsync(new EditTemplatePage(template));
    }

    private async void OnCreateTemplateClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateTemplatePage());
    }

    private async void OnDeleteTemplateClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is WorkoutTemplate template)
        {
            // שינוי המלל ל"תוכנית"
            bool confirm = await DisplayAlert("מחיקת תוכנית", $"למחוק את \"{template.Name}\"?", "כן", "לא");
            if (confirm)
            {
                WorkoutStore.Templates.Remove(template);
                RefreshTemplates();

                PersistenceService.SaveTemplates();
            }
        }
    }

    private async void OnStartWorkoutClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is WorkoutTemplate template)
        {
            await Navigation.PushAsync(new ActiveWorkoutPage(template));
        }
    }

    // ── History ───────────────────────────────────────────────────────
    private void RefreshHistory()
    {
        HistoryList.ItemsSource = null;
        HistoryList.ItemsSource = WorkoutStore.History;
        bool any = WorkoutStore.History.Count > 0;
        HistoryEmpty.IsVisible = !any;
        HistoryList.IsVisible = any;
    }

    private async void OnHistorySessionTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is WorkoutSession session)
        {
            await Navigation.PushAsync(new SessionPreviewPage(session));
        }
    }

    private async void OnClearHistoryClicked(object sender, EventArgs e)
    {
        // 1. נוודא שהמשתמש באמת רוצה למחוק הכל
        bool confirm = await DisplayAlert(
            "מחיקת היסטוריה",
            "האם אתה בטוח שברצונך למחוק את כל היסטוריית האימונים?\nלא ניתן לשחזר פעולה זו והיא תאפס את זיכרון המשקלים והסטים.",
            "כן, מחק הכל",
            "לא");

        if (confirm)
        {
            // 2. מנקים את הרשימה במאגר הנתונים
            WorkoutStore.History.Clear();

            // 3. שומרים את השינוי לקובץ/לזיכרון
            PersistenceService.SaveHistory();

            // 4. מרעננים את ה-UI כדי שהרשימה תיעלם ויעלה המסך הריק
            RefreshHistory();

            await DisplayAlert("נמחק", "היסטוריית האימונים נמחקה בהצלחה.", "אישור");
        }
    }

    private async void OnDeleteSessionClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is WorkoutSession session)
        {
            bool confirm = await DisplayAlert("מחיקת אימון", $"למחוק את \"{session.Name}\"?", "כן", "לא");
            if (confirm)
            {
                WorkoutStore.History.Remove(session);
                RefreshHistory();

                PersistenceService.SaveHistory();
            }
        }
    }

    private async void OnRepeatSessionClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is WorkoutSession session)
        {
            // יצירת תבנית זמנית מהsession
            var template = new WorkoutTemplate
            {
                Name = session.Name,
                Exercises = session.Exercises.Select(ex => ex.Exercise).ToList()
            };
            await Navigation.PushAsync(new ActiveWorkoutPage(template));
        }
    }
}