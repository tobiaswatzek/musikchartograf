namespace Musikchartograf.Data.Db.Models;

public class Track(Guid id, string name, Guid artistId)
{
    public Guid Id { get; } = id;

    public string Name { get; } = name;
    public Guid ArtistId { get; } = artistId;
    public Artist Artist { get; } = null!;
    public ICollection<PlayedTrack> Plays { get; } = new List<PlayedTrack>();
}