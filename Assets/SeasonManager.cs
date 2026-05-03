using System;
using UnityEngine;

/// <summary>
/// Tính ID mùa hiện tại + thời gian kết thúc mùa.
/// Mỗi mùa kéo dài 7 ngày, bắt đầu vào Thứ 2 00:00 UTC.
/// Tất cả client cùng tính ra cùng 1 ID nên không cần server-side scheduler.
///
/// Format ID: "2026-W18" (năm-tuần ISO).
/// </summary>
public static class SeasonManager
{
    public const int SEASON_LENGTH_DAYS = 7;

    /// <summary>ID mùa giải hiện tại, dạng "2026-W18".</summary>
    public static string GetCurrentSeasonId()
    {
        return GetSeasonIdAt(DateTime.UtcNow);
    }

    public static string GetSeasonIdAt(DateTime utcNow)
    {
        System.Globalization.Calendar cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        int week = cal.GetWeekOfYear(utcNow, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        int year = utcNow.Year;
        if (week >= 52 && utcNow.Month == 1)
        {
            year -= 1;
        }
        else if (week == 1 && utcNow.Month == 12)
        {
            year += 1;
        }
        return year.ToString("D4") + "-W" + week.ToString("D2");
    }

    /// <summary>Thời gian kết thúc mùa hiện tại (UTC). Là Thứ 2 00:00 UTC kế tiếp.</summary>
    public static DateTime GetCurrentSeasonEndUtc()
    {
        DateTime now = DateTime.UtcNow;
        int daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilNextMonday == 0)
        {
            daysUntilNextMonday = 7;
        }
        DateTime nextMonday = now.Date.AddDays(daysUntilNextMonday);
        return nextMonday;
    }

    public static TimeSpan GetTimeUntilSeasonEnd()
    {
        TimeSpan delta = GetCurrentSeasonEndUtc() - DateTime.UtcNow;
        if (delta.TotalSeconds < 0) delta = TimeSpan.Zero;
        return delta;
    }

    /// <summary>Format "3d 12h 5m" / "12h 5m" / "5m 30s".</summary>
    public static string FormatCountdown(TimeSpan span)
    {
        if (span.TotalSeconds <= 0) return "0s";

        int days = (int)span.TotalDays;
        int hours = span.Hours;
        int minutes = span.Minutes;
        int seconds = span.Seconds;

        if (days > 0) return days + "d " + hours + "h " + minutes + "m";
        if (hours > 0) return hours + "h " + minutes + "m";
        if (minutes > 0) return minutes + "m " + seconds + "s";
        return seconds + "s";
    }
}
