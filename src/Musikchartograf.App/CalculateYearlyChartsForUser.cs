using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Musikchartograf.Data.Db;

namespace Musikchartograf.App;

public sealed record CalculateYearlyChartsForUserRequest(
    int Year,
    string User);

public sealed record CalculateYearlyChartsForUserResponse(
    IReadOnlyList<YearlyChartedTrack> Charts);

public sealed record YearlyChartedTrack(
    int Rank,
    int Plays,
    int Points,
    Guid TrackId,
    string Name,
    string Artist);
public sealed class CalculateYearlyChartsForUserRequestHandler(
    DataContext dataContext)
{
    public async Task<CalculateYearlyChartsForUserResponse> Handle(
        CalculateYearlyChartsForUserRequest request,
        CancellationToken cancellationToken)
    {
        var yearStart = new DateTimeOffset(ISOWeek.GetYearStart(request.Year));
        var yearEnd = new DateTimeOffset(ISOWeek.GetYearEnd(request.Year))
            .EndOfDay();

        var yearImport = await dataContext.YearImports.FirstOrDefaultAsync(yi =>
                yi.Year == request.Year && yi.UserName == request.User,
            cancellationToken);
        if (yearImport is null)
        {
            throw new InvalidOperationException("No data imported for year");
        }

        if (yearStart.UtcDateTime != yearImport.Start ||
            yearEnd.UtcDateTime != yearImport.End)
        {
            throw new InvalidOperationException(
                "Not enough data imported for year");
        }

        var queryable = dataContext.PlayedTracks
            .Include(t => t.Track).ThenInclude(t => t.Artist)
            .Where(pt =>
                pt.PlayedByUserName == request.User &&
                pt.PlayedInYear == request.Year)
            .Select(pt => new
            {
                pt.TrackId,
                TrackName = pt.Track.Name,
                ArtistName = pt.Track.Artist.Name,
                pt.PlayedInWeekNumber,
                pt.PlayedAt
            })
            .GroupBy(pt => new
            {
                pt.TrackId, pt.TrackName, pt.ArtistName, pt.PlayedInWeekNumber
            })
            .Select(g =>
                new
                {
                    g.Key.TrackId,
                    g.Key.TrackName,
                    g.Key.ArtistName,
                    g.Key.PlayedInWeekNumber,
                    LastPlayedAt =
                        g.Select(t => t.PlayedAt).Max(),
                    Plays = g.Count()
                });
            var charts = await queryable
            .AsAsyncEnumerable()
            .GroupBy(x => x.PlayedInWeekNumber)
            .SelectMany(g => g.OrderByDescending(x => x.Plays)
                .ThenByDescending(x => x.LastPlayedAt)
                .Take(20)
                .Reverse()
                .Select((x, i) => new
                {
                    Points = i + 1, x.TrackId, x.TrackName, x.ArtistName,
                    x.Plays,
                    x.LastPlayedAt
                }))
            .GroupBy(x => new { x.TrackId, x.TrackName, x.ArtistName })
            .SelectAwait(async g => new
            {
                g.Key.TrackId,
                g.Key.TrackName,
                g.Key.ArtistName,
                LastPlayedAt =
                    await g.Select(t => t.LastPlayedAt)
                        .MaxAsync(cancellationToken),
                Points = await g.SumAsync(t => t.Points, cancellationToken),
                Plays = await g.SumAsync(t => t.Plays, cancellationToken)
            })
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.LastPlayedAt)
            .Take(20)
            .Select((t, i) => new YearlyChartedTrack(i+ 1, t.Plays, t.Points, t.TrackId, t.TrackName, t.ArtistName))
            .ToListAsync(cancellationToken);

        return new CalculateYearlyChartsForUserResponse(charts);
    }
}