namespace Musikchartograf.Data.Db.Models;

public class Artist(Guid id, string name)
{
    public Guid Id { get; } = id;

    public string Name { get; } = name;

    public ICollection<Track> Tracks { get; } = new List<Track>();
}