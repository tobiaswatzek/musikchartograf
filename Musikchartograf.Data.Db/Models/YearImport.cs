using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Musikchartograf.Data.Db.Models;

public class YearImport(
    Guid id,
    string userName,
    int year,
    DateTimeOffset start,
    DateTimeOffset end)
{
    public Guid Id { get; } = id;

    public string UserName { get; } = userName;
    public User User { get; } = null!;

    public int Year { get; } = year;

    public DateTimeOffset Start { get; } = start;
    public DateTimeOffset End { get; private set; } = end;

    public void UpdateEnd(DateTimeOffset end) => End = end;
}