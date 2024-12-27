namespace Musikchartograf.App;

public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset EndOfDay(this DateTimeOffset date) =>
        date.AddDays(1).AddTicks(-1);
}