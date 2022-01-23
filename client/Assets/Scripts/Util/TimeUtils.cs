using System;

public static class TimeUtils
{
    public static DateTime T0 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long GetTimeStampFromT0()
    {
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
    }
}