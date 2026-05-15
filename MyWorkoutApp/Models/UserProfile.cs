namespace MyWorkoutApp.Models;

public class UserProfile
{
    public string Name { get; set; } = "מתאמן";
    public string Gender { get; set; } = "זכר";
    public string Goal { get; set; } = "";
    public double DefaultBarWeight { get; set; } = 20;
    public string ProfileImagePath { get; set; } = "";
    public string FavoriteExerciseOverride { get; set; } = "";

    // ── Weight tracking history ──
    public List<WeightEntry> WeightHistory { get; set; } = new();
}

public class WeightEntry
{
    public DateTime Date { get; set; }
    public double Weight { get; set; }
    public string Phase { get; set; } = "";
}