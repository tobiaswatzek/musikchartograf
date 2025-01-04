using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Musikchartograf.Data.Db;

namespace Musikchartograf.App;

public sealed record CalculateWeeklyChartsForUserRequest(
    int Year,
    int Week,
    string User);

public sealed record CalculateWeeklyChartsForUserResponse(
    IReadOnlyList<ChartedTrack> Charts);

public sealed record ChartedTrack(
    int Rank,
    int Plays,
    Guid TrackId,
    string Name,
    string Artist);

public sealed class CalculateWeeklyChartsForUserRequestHandler(
    DataContext dataContext)
{
    public async Task<CalculateWeeklyChartsForUserResponse> Handle(
        CalculateWeeklyChartsForUserRequest request,
        CancellationToken cancellationToken)
    {
        var weekStart = new DateTimeOffset(
                ISOWeek.ToDateTime(request.Year, request.Week,
                    DayOfWeek.Monday))
            .UtcDateTime;
        var weekEnd = new DateTimeOffset(weekStart.AddDays(6)).EndOfDay()
            .UtcDateTime;

        var yearImport = await dataContext.YearImports.FirstOrDefaultAsync(yi =>
                yi.Year == request.Year && yi.UserName == request.User,
            cancellationToken);
        if (yearImport is null)
        {
            throw new InvalidOperationException("No data imported for year");
        }

        if (weekStart < yearImport.Start || weekStart >= yearImport.End ||
            weekEnd <= yearImport.Start || weekEnd > yearImport.End)
        {
            throw new InvalidOperationException(
                "No data imported for week in year");
        }

        var queryable = dataContext.PlayedTracks
            .Include(t => t.Track).ThenInclude(t => t.Artist)
            .Where(pt =>
                pt.PlayedByUserName == request.User &&
                pt.PlayedInYear == request.Year &&
                pt.PlayedInWeekNumber == request.Week)
            .Select(pt => new
            {
                pt.TrackId,
                TrackName = pt.Track.Name,
                ArtistName = pt.Track.Artist.Name,
                pt.PlayedAt
            })
            .GroupBy(pt => new { pt.TrackId, pt.TrackName, pt.ArtistName })
            .Select(g => new
            {
                g.Key.TrackId,
                g.Key.TrackName,
                g.Key.ArtistName,
                LastPlayedAt =
                    g.Select(t => t.PlayedAt).Max(),
                Plays = g.Count()
            })
            .OrderByDescending(x => x.Plays)
            .ThenByDescending(x => x.LastPlayedAt)
            .Take(20);
        var charts = await queryable
            .ToListAsync(cancellationToken);

        return new CalculateWeeklyChartsForUserResponse(charts.Select((x, i) =>
                new ChartedTrack(i + 1, x.Plays, x.TrackId, x.TrackName,
                    x.ArtistName))
            .ToList());
    }
}