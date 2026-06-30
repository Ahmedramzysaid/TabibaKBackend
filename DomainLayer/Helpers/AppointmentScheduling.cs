namespace DomainLayer.Helpers;

public static class AppointmentScheduling
{
    public static DateTime ToUtcDateTime(DateOnly date, TimeSpan time)
        => date.ToDateTime(TimeOnly.FromTimeSpan(time), DateTimeKind.Utc);

    public static bool IsInFuture(DateOnly date, TimeSpan time)
        => ToUtcDateTime(date, time) > DateTime.UtcNow;

    public static bool IsOnOrAfterNow(DateOnly date, TimeSpan time)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        if (date > today) return true;
        if (date < today) return false;
        return time >= now.TimeOfDay;
    }

    public static bool IsBeforeNow(DateOnly date, TimeSpan time)
        => ToUtcDateTime(date, time) < DateTime.UtcNow;

    public static string FormatDisplay(DateOnly date, TimeSpan time)
    {
        var dt = ToUtcDateTime(date, time);
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        if (dt.Date == today)
            return $"Today, {dt:hh:mm tt}";
        if (dt.Date == tomorrow)
            return $"Tomorrow, {dt:hh:mm tt}";
        return dt.ToString("MMM d, h:mm tt");
    }

    public static string FormatTime12Hour(TimeSpan time)
        => TimeOnly.FromTimeSpan(time).ToString("hh:mm tt");

    public static (DateOnly Date, TimeSpan Time) FromUtcDateTime(DateTime dateTime)
    {
        var utc = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return (DateOnly.FromDateTime(utc), utc.TimeOfDay);
    }
}
