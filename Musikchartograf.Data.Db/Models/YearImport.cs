namespace Musikchartograf.Data.Db.Models;

public class YearImport(
    Guid id,
    string userName,
    int year,
    DateTime start,
    DateTime end)
{
    public Guid Id { get; } = id;

    public string UserName { get; } = userName;
    public User User { get; } = null!;

    public int Year { get; } = year;

    public DateTime Start { get; } = start;
    public DateTime End { get; private set; } = end;

    public void UpdateEnd(DateTime end) => End = end;
}