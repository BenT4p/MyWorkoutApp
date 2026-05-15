using System.Text.Json;
using MyWorkoutApp.Models;

namespace MyWorkoutApp.Services;

/// <summary>
/// שומר ומטעין Templates + History + Profile מקבצי JSON בתיקיית AppDataDirectory.
/// </summary>
public static class PersistenceService
{
    private static readonly string TemplatesPath =
        Path.Combine(FileSystem.AppDataDirectory, "templates.json");

    private static readonly string HistoryPath =
        Path.Combine(FileSystem.AppDataDirectory, "history.json");

    // נתיב חדש עבור נתוני הפרופיל
    private static readonly string ProfilePath =
        Path.Combine(FileSystem.AppDataDirectory, "profile.json");

    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    // ── Load ──────────────────────────────────────────────────────────
    public static void Load()
    {
        // 1. טעינת תבניות (Templates)
        if (File.Exists(TemplatesPath))
        {
            try
            {
                var json = File.ReadAllText(TemplatesPath);
                var list = JsonSerializer.Deserialize<List<WorkoutTemplate>>(json, _opts);
                if (list != null)
                {
                    WorkoutStore.Templates.Clear();
                    foreach (var t in list)
                        WorkoutStore.Templates.Add(t);
                }
            }
            catch { /* קובץ פגום */ }
        }

        // 2. טעינת היסטוריה (History)
        if (File.Exists(HistoryPath))
        {
            try
            {
                var json = File.ReadAllText(HistoryPath);
                var list = JsonSerializer.Deserialize<List<WorkoutSession>>(json, _opts);
                if (list != null)
                {
                    WorkoutStore.History.Clear();
                    foreach (var s in list)
                        WorkoutStore.History.Add(s);
                }
            }
            catch { /* קובץ פגום */ }
        }

        // 3. טעינת פרופיל משתמש (Profile)
        if (File.Exists(ProfilePath))
        {
            try
            {
                var json = File.ReadAllText(ProfilePath);
                var profile = JsonSerializer.Deserialize<UserProfile>(json, _opts);
                if (profile != null)
                {
                    WorkoutStore.Profile = profile;
                }
            }
            catch { /* קובץ פגום */ }
        }
    }

    // ── Save ──────────────────────────────────────────────────────────

    public static void SaveTemplates()
    {
        try
        {
            var json = JsonSerializer.Serialize(WorkoutStore.Templates.ToList(), _opts);
            File.WriteAllText(TemplatesPath, json);
        }
        catch { }
    }

    public static void SaveHistory()
    {
        try
        {
            var json = JsonSerializer.Serialize(WorkoutStore.History.ToList(), _opts);
            File.WriteAllText(HistoryPath, json);
        }
        catch { }
    }

    // פונקציה חדשה לשמירת הפרופיל (שם, מטרה, משקל גוף, נתיב תמונה)
    public static void SaveProfile()
    {
        try
        {
            var json = JsonSerializer.Serialize(WorkoutStore.Profile, _opts);
            File.WriteAllText(ProfilePath, json);
        }
        catch { }
    }

    public static void SaveAll()
    {
        SaveTemplates();
        SaveHistory();
        SaveProfile();
    }
}