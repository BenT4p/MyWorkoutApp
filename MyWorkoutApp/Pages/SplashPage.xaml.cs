namespace MyWorkoutApp.Pages;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. כניסה: הלוגו גדל מנקודה (0) לגודל רגיל (1)
        _ = LogoImage.FadeTo(1, 1000, Easing.CubicOut);
        await LogoImage.ScaleTo(1, 1000, Easing.CubicOut);

        // 2. ממתינים חצי מהזמן...
        await Task.Delay(800);

        // 💡 טריק הביצועים: טוענים את האפליקציה לזיכרון בזמן שהלוגו סתם עומד!
        // המעבד יעבוד קשה לחלקיק שנייה, אבל המשתמש לא יראה את זה כי אין אנימציה כרגע.
        AppShell preloadedApp = new AppShell();

        // ממתינים את החצי השני של הזמן...
        await Task.Delay(800);

        // 3. אפקט הפינאלה: מהיר וחד יותר כדי שלא תהיה תחושת ריקנות (500 מילישניות במקום 700)
        _ = LogoImage.FadeTo(0, 400, Easing.CubicIn);
        await LogoImage.ScaleTo(50, 400, Easing.CubicIn);

        // 4. מעבר מיידי לאפליקציה (היא כבר מוכנה בזיכרון!)
        if (Application.Current != null)
        {
            Application.Current.MainPage = preloadedApp;
        }
    }
}