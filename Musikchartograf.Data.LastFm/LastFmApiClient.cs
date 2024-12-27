using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Musikchartograf.Data.LastFm;

public class LastFmApiClient(HttpClient httpClient) : ILastFmApiClient
{
    private readonly Uri baseUri = new("https://ws.audioscrobbler.com/2.0/");

    public async Task<RecentTracksResponse?> GetRecentTracks(string user,
        DateTimeOffset from, DateTimeOffset to,
        int limit,
        int page, string apiKey,
        CancellationToken cancellationToken)
    {
        var uriBuilder = new UriBuilder(baseUri);
        uriBuilder.Query = $"?api_key={apiKey}" +
                           $"&format=json" +
                           $"&method=user.getRecentTracks" +
                           $"&user={user}" +
                           $"&from={from.ToUnixTimeSeconds()}" +
                           $"&to={to.ToUnixTimeSeconds()}" +
                           $"&limit={limit}" +
                           $"&page={page}";

        var res =
            await httpClient.GetFromJsonAsync<RecentTracksResponseDto>(
                uriBuilder.Uri, cancellationToken);

        if (res is null)
        {
            return null;
        }

        var tracks = res.RecentTracks.Tracks.Select<TrackDto, Track>(t =>
        {
            if (t.Attr?.NowPlaying is not null)
            {
                return new PlayingTrack(t.Name, t.Artist.Text, t.Album.Text);
            }

            if (t.Date is not null)
            {
                var playedAt =
                    DateTimeOffset.FromUnixTimeSeconds(int.Parse(t.Date.Uts));
                return new PlayedTrack(t.Name, t.Artist.Text, t.Album.Text,
                    playedAt);
            }

            throw new UnreachableException(
                "NowPlaying is null and Date is null. " +
                "This is not supposed to happen.");
        }).ToList();

        return new RecentTracksResponse(res.RecentTracks.Attr.User,
            int.Parse(res.RecentTracks.Attr.Page),
            int.Parse(res.RecentTracks.Attr.PerPage),
            int.Parse(res.RecentTracks.Attr.TotalPages),
            int.Parse(res.RecentTracks.Attr.Total), tracks);
    }
}

internal sealed class ArtistDto
{
    [JsonPropertyName("#text")] public required string Text { get; init; }
}

internal sealed class AlbumDto
{
    [JsonPropertyName("#text")] public required string Text { get; init; }
}

internal sealed class TrackAttrDto
{
    [JsonPropertyName("nowplaying")]
    public required string NowPlaying { get; init; }
}

internal sealed class DateDto
{
    [JsonPropertyName("uts")] public required string Uts { get; init; }
}

internal sealed class TrackDto
{
    [JsonPropertyName("name")] public required string Name { get; init; }

    [JsonPropertyName("url")] public required string Url { get; init; }

    [JsonPropertyName("artist")] public required ArtistDto Artist { get; init; }

    [JsonPropertyName("album")] public required AlbumDto Album { get; init; }

    [JsonPropertyName("@attr")] public TrackAttrDto? Attr { get; init; }

    [JsonPropertyName("date")] public DateDto? Date { get; init; }
}

internal sealed class RecentTracksContainerAttrDto
{
    [JsonPropertyName("user")] public required string User { get; init; }

    [JsonPropertyName("totalPages")]
    public required string TotalPages { get; init; }

    [JsonPropertyName("page")] public required string Page { get; init; }
    [JsonPropertyName("perPage")] public required string PerPage { get; init; }
    [JsonPropertyName("total")] public required string Total { get; init; }
}

internal sealed class RecentTracksContainerDto
{
    [JsonPropertyName("track")]
    public required IReadOnlyCollection<TrackDto> Tracks { get; init; }

    [JsonPropertyName("@attr")]
    public required RecentTracksContainerAttrDto Attr { get; init; }
}

internal sealed class RecentTracksResponseDto
{
    [JsonPropertyName("recenttracks")]
    public required RecentTracksContainerDto RecentTracks { get; init; }
}