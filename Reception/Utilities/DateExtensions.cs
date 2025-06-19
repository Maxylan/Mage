namespace Reception.Utilities;

public static class DateExtensions
{
    /// <summary>
    /// Convert a `<see cref="DateTime"/>` object to the set 'TIMEZONE_IANA' <see cref="TimeZoneInfo"/>.
    /// <para>
    ///     Gets a `<see cref="DateTime"/>` object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time \(UTC\)\.
    /// </para>
    /// </summary>
    public static DateTime ToServerTime(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);

        string? timeZoneIana = System.Environment.GetEnvironmentVariable("TIMEZONE_IANA");
        if (string.IsNullOrWhiteSpace(timeZoneIana)) {
            return dateTime;
        }

        TimeZoneInfo? timeZone = TimeZoneInfo.GetSystemTimeZones()
            .FirstOrDefault(tz => tz.HasIanaId && tz.Id == timeZoneIana);

        if (timeZone is null) {
            return dateTime;
        }

        switch(dateTime.Kind) {
            case DateTimeKind.Utc:
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZone);
            case DateTimeKind.Local:
                return TimeZoneInfo.ConvertTime(dateTime, timeZone);
        }

        return dateTime;
    }
}
