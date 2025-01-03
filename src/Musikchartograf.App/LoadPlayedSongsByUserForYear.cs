using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Musikchartograf.Data.Db;
using Musikchartograf.Data.Db.Models;
using Musikchartograf.Data.LastFm;
using PlayedTrack = Musikchartograf.Data.LastFm.PlayedTrack;
using Track = Musikchartograf.Data.Db.Models.Track;

namespace Musikchartograf.App;

public sealed record LoadPlayedSongsByUserForYearRequest(
    int Year,
    string User,
    string ApiKey);

public sealed record LoadPlayedSongsByUserForYearResponse(
    DateTimeOffset Start,
    DateTimeOffset End,
    int ImportedPlayedTrackCount);

public sealed class LoadPlayedSongsByUserForYearRequestHandler(
    DataContext dataContext,
    ILastFmApiClient lastFmApiClient)
{
    public async Task<LoadPlayedSongsByUserForYearResponse> Handle(
        LoadPlayedSongsByUserForYearRequest request,
        CancellationToken cancellationToken)
    {
        var (yearStart, yearEnd) = CalculateYearStartAndEnd(request);
        await using var transaction =
            await dataContext.Database.BeginTransactionAsync(cancellationToken);

        var user = await dataContext.Users.FindAsync([request.User],
            cancellationToken);
        if (user is null)
        {
            user = new User(request.User);
            dataContext.Users.Add(user);
        }

        var yearImport = await dataContext.YearImports
            .SingleOrDefaultAsync(
                yi => yi.UserName == user.Name && yi.Year == request.Year,
                cancellationToken);

        if (yearEnd <= yearImport?.End)
        {
            return new LoadPlayedSongsByUserForYearResponse(yearImport.Start,
                yearImport.End, 0);
        }

        if (yearImport is null)
        {
            yearImport = new YearImport(Guid.CreateVersion7(), user.Name,
                request.Year, yearStart.UtcDateTime, yearEnd.UtcDateTime);
            dataContext.YearImports.Add(yearImport);
        }
        else
        {
            yearStart = ISOWeek.ToDateTime(yearImport.End.Year,
                ISOWeek.GetWeekOfYear(yearImport.End) + 1, DayOfWeek.Monday);
            yearImport.UpdateEnd(yearEnd.UtcDateTime);
        }

        var artists = new Dictionary<string, Artist>();
        var tracks = new Dictionary<(Guid ArtistId, string TrackName), Track>();
        var playedTracksCount = 0;
        await foreach (var pt in LoadTracks(request, yearStart,
                           yearEnd, cancellationToken))
        {
            if (!artists.TryGetValue(pt.Artist, out var artist))
            {
                var dbArtist = await
                    dataContext.Artists.SingleOrDefaultAsync(
                        a => a.Name == pt.Artist,
                        cancellationToken);
                if (dbArtist is null)
                {
                    artist = new Artist(Guid.CreateVersion7(),
                        pt.Artist);
                    dataContext.Artists.Add(artist);
                    artists.Add(pt.Artist, artist);
                }
                else
                {
                    artist = dbArtist;
                }
            }

            if (!tracks.TryGetValue((artist.Id, pt.Name), out var track))
            {
                var dbTrack =
                    await dataContext.Tracks.SingleOrDefaultAsync(
                        t => t.ArtistId == artist.Id &&
                             t.Name == pt.Name, cancellationToken);
                if (dbTrack is null)
                {
                    track = new Track(Guid.CreateVersion7(), pt.Name,
                        artist.Id);
                    dataContext.Tracks.Add(track);
                    tracks.Add((artist.Id, pt.Name), track);
                }
                else
                {
                    track = dbTrack;
                }
            }

            dataContext.PlayedTracks.Add(new Data.Db.Models.PlayedTrack(
                track.Id,
                user.Name, pt.ListenedAt.UtcDateTime,
                ISOWeek.GetWeekOfYear(pt.ListenedAt.DateTime)));
            playedTracksCount++;
        }

        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new LoadPlayedSongsByUserForYearResponse(yearStart, yearEnd,
            playedTracksCount);
    }

    private async IAsyncEnumerable<PlayedTrack> LoadTracks(
        LoadPlayedSongsByUserForYearRequest request,
        DateTimeOffset yearStart,
        DateTimeOffset yearEnd,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var page = 1;
        var hasMore = true;
        while (hasMore)
        {
            var tracks = await lastFmApiClient.GetRecentTracks(request.User,
                             yearStart, yearEnd, 200, page,
                             request.ApiKey, cancellationToken) ??
                         throw new InvalidOperationException("No tracks found");

            hasMore = tracks.TotalPages > page;
            page++;
            foreach (var playedTrack in tracks.Tracks.OfType<PlayedTrack>())
            {
                yield return playedTrack;
            }
        }
    }

    private static (DateTimeOffset yearStart, DateTimeOffset yearEnd)
        CalculateYearStartAndEnd(LoadPlayedSongsByUserForYearRequest request)
    {
        var yearStart = new DateTimeOffset(ISOWeek.GetYearStart(request.Year));
        var yearEnd = new DateTimeOffset(ISOWeek.GetYearEnd(request.Year))
            .EndOfDay();
        if (yearEnd > DateTimeOffset.Now)
        {
            // calculate end of previous week
            yearEnd = new DateTimeOffset(ISOWeek.ToDateTime(request.Year,
                ISOWeek.GetWeekOfYear(DateTimeOffset.Now.DateTime) - 1,
                DayOfWeek.Sunday)).EndOfDay();
        }

        return (yearStart, yearEnd);
    }
}