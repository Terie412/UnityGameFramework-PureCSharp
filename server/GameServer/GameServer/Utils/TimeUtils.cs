using System;

public static class TimeUtils
{
    public static ulong GetTimeStamp()
    {
        return (ulong) new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
    }
}