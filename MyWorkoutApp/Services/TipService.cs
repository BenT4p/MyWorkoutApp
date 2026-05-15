using System;
using System.Collections.Generic;

namespace MyWorkoutApp.Services;

public static class TipService
{
    private static readonly Random _rand = new();

    public static string GetRandomTip(string gender, string goal, int hour)
    {
        var tips = new List<string>();

        // נבדוק האם מדובר בגבר (כברירת מחדל אם ריק נניח שזה זכר, כי ככה הגדרנו)
        bool isMale = string.IsNullOrEmpty(gender) || gender == "זכר";

        // מטרה בטוחה למקרה שהיא Null
        string safeGoal = goal ?? "";

        // ════ 1. טיפים לפי שעות ════
        if (hour >= 5 && hour < 12) // בוקר
        {
            tips.Add("אימון בוקר מוקדם פותח את היום באנרגיות שיא!");
            tips.Add("אין כמו לסיים את האימון כשהשמש רק זורחת.");
        }
        else if (hour >= 18 || hour < 5) // ערב ולילה
        {
            tips.Add("אימון ערב – הזמן המושלם לפרוק את כל הלחץ של היום!");
            tips.Add(isMale ? "עייף? אחרי הסט הראשון זה יעבור." : "עייפה? אחרי הסט הראשון זה יעבור.");
        }

        // ════ 2. טיפים כלליים (התאמת מגדר) ════
        tips.Add(isMale ? "מוכן לשבור שיאים היום?" : "מוכנה לשבור שיאים היום?");
        tips.Add(isMale ? "אל תשכח לשתות מים בין הסטים!" : "אל תשכחי לשתות מים בין הסטים!");
        tips.Add(isMale ? "התמדה מנצחת כישרון. תן בראש!" : "התמדה מנצחת כישרון. תני בראש!");
        tips.Add("טכניקה נכונה עדיפה על משקל כבד.");
        tips.Add("הקשבה לגוף היא חלק בלתי נפרד מהאימון.");

        // ════ 3. טיפים לפי מטרת אימון ════
        if (safeGoal.Contains("חיטוב"))
        {
            tips.Add(isMale ? "זכור: מאזן קלורי שלילי זה הסוד לחיטוב מוצלח." : "זכרי: מאזן קלורי שלילי זה הסוד לחיטוב מוצלח.");
            tips.Add("חלבון גבוה שומר על מסת השריר שלך בחיטוב.");
            tips.Add("גם בחיטוב, המטרה היא להרים כבד ולשמור על עצימות!");
        }
        else if (safeGoal.Contains("מסה"))
        {
            tips.Add(isMale ? "במסה? אל תפחד לאכול! פחמימות הן הדלק לאימון." : "במסה? אל תפחדי לאכול! פחמימות הן הדלק לאימון.");
            tips.Add("שבירת שיאים דורשת התאוששות. אל תוותר על שעות השינה.");
            tips.Add("פלוס קלורי מתון ועקבי הוא המפתח לעלייה נקייה.");
        }
        else if (safeGoal.Contains("שמירה"))
        {
            tips.Add("שמירה על ההישגים שלך דורשת לא פחות התמדה.");
            tips.Add("זה הזמן המושלם לגוון בתרגילים ולהפתיע את השריר!");
        }

        // הגרלת טיפ אחד מתוך הרשימה שנבנתה
        int index = _rand.Next(tips.Count);
        return tips[index];
    }
}