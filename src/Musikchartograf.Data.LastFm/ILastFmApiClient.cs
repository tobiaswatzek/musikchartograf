namespace Musikchartograf.Data.LastFm;

public record Track(string Name, string Artist, string Album);

public sealed record PlayedTrack(
    string Name,
    string Artist,
    string Album,
    DateTimeOffset ListenedAt) : Track(Name, Artist, Album);

public sealed record PlayingTrack(string Name, string Artist, string Album)
    : Track(Name, Artist, Album);

public record RecentTracksResponse(
    string User,
    int Page,
    int PerPage,
    int TotalPages,
    int Total,
    IReadOnlyCollection<Track> Tracks);

public interface ILastFmApiClient
{
    public Task<RecentTracksResponse?> GetRecentTracks(string user,
        DateTimeOffset from, DateTimeOffset to, int limit,
        int page, string apiKey, CancellationToken cancellationToken);
}