using System.Globalization;
using NodaTime;

namespace Ape.Volo.Common.Extensions;

/// <summary>
/// 时间扩展
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 获取某一日期是该年中的第几周
    /// </summary>
    /// <param name="dateTime">日期</param>
    /// <returns>该日期在该年中的周数</returns>
    public static int GetWeekOfYear(this DateTime dateTime)
    {
        GregorianCalendar gc = new GregorianCalendar();
        return gc.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    /// <summary>
    /// 获取Js格式的timestamp
    /// </summary>
    /// <param name="dateTime">日期</param>
    /// <returns></returns>
    public static long ToJsTimestamp(this DateTime dateTime)
    {
        var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
        long result = (dateTime.Ticks - startTime.Ticks) / 10000; //除10000调整为13位
        return result;
    }

    /// <summary>
    /// 获取js中的getTime()
    /// </summary>
    /// <param name="dt">日期</param>
    /// <returns></returns>
    public static Int64 JsGetTime(this DateTime dt)
    {
        Int64 retval = 0;
        var st = new DateTime(1970, 1, 1);
        TimeSpan t = dt.ToUniversalTime() - st;
        retval = (Int64)(t.TotalMilliseconds + 0.5);
        return retval;
    }

    /// <summary>
    /// 返回默认时间1970-01-01
    /// </summary>
    /// <param name="dt">时间日期</param>
    /// <returns></returns>
    public static DateTime Default(this DateTime dt)
    {
        return DateTime.Parse("1970-01-01");
    }

    /// <summary>
    /// 转为标准时间（北京时间，解决Linux时区问题）
    /// </summary>
    /// <param name="dt">当前时间</param>
    /// <returns></returns>
    public static DateTime ToCstTime(this DateTime dt)
    {
        Instant now = SystemClock.Instance.GetCurrentInstant();
        var shanghaiZone = DateTimeZoneProviders.Tzdb["Asia/Shanghai"];
        return now.InZone(shanghaiZone).ToDateTimeUnspecified();
    }

    /// <summary>
    /// 转为本地时间
    /// </summary>
    /// <param name="time">时间</param>
    /// <returns></returns>
    public static DateTime ToLocalTime(this DateTime time)
    {
        return TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Local);
    }

    /// <summary>
    /// 转为转换为Unix时间戳格式(精确到秒)
    /// </summary>
    /// <param name="time">时间</param>
    /// <returns></returns>
    public static long ToUnixTimeStampSecond(this DateTime time)
    {
        DateTime startTime = new DateTime(1970, 1, 1).ToLocalTime();
        return (long)(time - startTime).TotalSeconds;
    }

    /// <summary>
    /// 转为转换为Unix时间戳格式(精确到毫秒)
    /// </summary>
    /// <param name="time">时间</param>
    /// <returns></returns>
    public static long ToUnixTimeStampMillisecond(this DateTime time)
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
        return (long)(time - startTime).TotalMilliseconds;
    }

    /// <summary>
    /// 时间戳(秒)String转换为DateTime
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime TicksToDateTime(this long timestamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        return dateTimeOffset.LocalDateTime;
    }


    /// <summary>
    /// 时间戳(毫秒)String转换为DateTime类型转换
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime TimeMillisecondToDateTime(this long timestamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        return dateTimeOffset.LocalDateTime;
    }

    /// <summary>
    /// 把毫秒时长格式化为「X天X小时X分钟X秒」的可读文本
    /// </summary>
    /// <param name="time">毫秒数</param>
    /// <returns>可读的时长文本</returns>
    public static string FormatLongToTime(this long time)
    {
        int day = 0;
        int hour = 0;
        int minute = 0;
        int second = 0;
        second = (int)(time / 1000);

        if (second > 60)
        {
            minute = second / 60;
            second %= 60;
        }

        if (minute > 60)
        {
            hour = minute / 60;
            minute %= 60;
        }

        if (hour <= 24) return hour + "小时" + minute + "分钟" + second + "秒";
        day = hour / 24;
        hour %= 24;

        return day + "天" + hour + "小时" + minute + "分钟" + second + "秒";
    }
}
