// App.xaml.cs — עדכן את הקובץ הקיים שלך כך:

using MyWorkoutApp.Services;

namespace MyWorkoutApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        PersistenceService.Load();
        MainPage = new Pages.SplashPage();
    }

    protected override void OnStart()
    {
        // טוען נתונים שמורים בהפעלת האפליקציה
        PersistenceService.Load();
    }

    protected override void OnSleep()
    {
        // שומר כשהאפליקציה עוברת לרקע
        PersistenceService.SaveAll();
    }

    protected override void OnResume()
    {
        // אפשר לטעון מחדש אם צריך
    }
}