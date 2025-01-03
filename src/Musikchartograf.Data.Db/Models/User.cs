namespace Musikchartograf.Data.Db.Models;

public class User(string name)
{
    public string Name { get; } = name;

    public IReadOnlyCollection<PlayedTrack> PlayedTracks { get; } =
        new List<PlayedTrack>();
    
    public IReadOnlyCollection<YearImport> YearImports { get; } = new List<YearImport>();
}