namespace Musikchartograf.Data.Db.Models;

public sealed class PlayedTrack(
    Guid trackId,
    string playedByUserName,
    DateTimeOffset playedAt)
{
    public Guid TrackId { get; } = trackId;
    public Track Track { get; } = null!;

    public string PlayedByUserName { get; } = playedByUserName;
    public User PlayedByUser { get; } = null!;

    public DateTimeOffset PlayedAt { get; } = playedAt;
}