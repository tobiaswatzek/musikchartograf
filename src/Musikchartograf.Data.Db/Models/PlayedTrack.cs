namespace Musikchartograf.Data.Db.Models;

public sealed class PlayedTrack(
    Guid trackId,
    string playedByUserName,
    DateTime playedAt,
    int playedInWeekNumber)
{
    public Guid TrackId { get; } = trackId;
    public Track Track { get; } = null!;

    public string PlayedByUserName { get; } = playedByUserName;
    public User PlayedByUser { get; } = null!;

    public int PlayedInWeekNumber { get; } = playedInWeekNumber;
    
    public DateTime PlayedAt { get; } = playedAt;
}