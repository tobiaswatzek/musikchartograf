using Cocona;
using Cocona.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Musikchartograf.App;
using Musikchartograf.Data.Db;
using Musikchartograf.Data.LastFm;
using Spectre.Console;

var builder = CoconaApp.CreateBuilder();
builder.Logging.ClearProviders();
builder.Services.AddHttpClient<ILastFmApiClient, LastFmApiClient>();
builder.Services.AddScoped<LoadPlayedSongsByUserForYearRequestHandler>();
builder.Services.AddScoped<CalculateWeeklyChartsForUserRequestHandler>();
builder.Services.AddSingleton<DbConnectionContext>();
builder.Services.AddTransient<PopulateDbConnectionCommandFilter>();
builder.Services.AddDbContext<DataContext>((services, opt) =>
{
    var dbConnectionContext =
        services.GetRequiredService<DbConnectionContext>();
    opt.UseSqlite($"Data Source={dbConnectionContext.GetPath()}");
});

var app = builder.Build();
app.UseFilter(new PrettyPrintExceptionCommandFilter());
app.UseFilter(new PopulateDbConnectionContextFilterAttribute());

app.AddCommand("load-year",
    async (CommonParameters common,
        int year,
        LoadPlayedSongsByUserForYearRequestHandler handler,
        CoconaAppContext ctx) =>
    {
        var apiKey = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter Last.fm API Key:")
                .Secret());

        var resp = await AnsiConsole.Status()
            .StartAsync("Importing data...", async _ => await handler.Handle(
                new LoadPlayedSongsByUserForYearRequest(year, common.User,
                    apiKey),
                ctx.CancellationToken));

        AnsiConsole.MarkupLine(
            $"[bold]:check_mark_button: Imported all available data for {year}[/]");
        AnsiConsole.WriteLine();
        var grid = new Grid();
        grid.AddColumn().AddColumn();
        grid.AddRow("Start", $"{resp.Start.ToLocalTime():F}")
            .AddRow("End", $"{resp.End.ToLocalTime():F}")
            .AddRow("Number of added tracks",
                $"{resp.ImportedPlayedTrackCount}");
        AnsiConsole.Write(grid);
    });

app.AddCommand("weekly",
    async (CommonParameters common, int year, int week,
        CalculateWeeklyChartsForUserRequestHandler handler,
        CoconaAppContext ctx) =>
    {
        var response = await handler.Handle(
            new CalculateWeeklyChartsForUserRequest(year, week, common.User),
            ctx.CancellationToken);

        var table = new Table();
        table.Title = new TableTitle( $"Week {year} {week} Charts for {common.User}");
        table.AddColumn(new TableColumn("Rank").RightAligned());
        table.AddColumn(new TableColumn("Name").LeftAligned());
        table.AddColumn(new TableColumn("Artist").LeftAligned());
        table.AddColumn(new TableColumn("Plays").RightAligned());

        foreach (var c in response.Charts.OrderBy(c => c.Rank))
        {
            table.AddRow(c.Rank.ToString(), c.Name, c.Artist,
                c.Plays.ToString());
        }
        
        AnsiConsole.Write(table);
    });

await app.RunAsync();

public sealed class PopulateDbConnectionContextFilterAttribute : IFilterFactory
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return serviceProvider
            .GetRequiredService<PopulateDbConnectionCommandFilter>();
    }
}

public sealed class PopulateDbConnectionCommandFilter(
    DbConnectionContext connectionContext,
    IServiceProvider serviceProvider) : ICommandFilter
{
    public async ValueTask<int> OnCommandExecutionAsync(
        CoconaCommandExecutingContext ctx,
        CommandExecutionDelegate next)
    {
        var databaseOption =
            ctx.ParsedCommandLine.Options
                .Single(c => c.Option.Name == "database").Value ??
            throw new InvalidOperationException("No database option provided");
        connectionContext.SetPath(databaseOption);
        var dataContext = serviceProvider.GetRequiredService<DataContext>();
        await dataContext.Database.MigrateAsync();

        return await next(ctx);
    }
}

public sealed class PrettyPrintExceptionCommandFilter : ICommandFilter
{
    public async ValueTask<int> OnCommandExecutionAsync(
        CoconaCommandExecutingContext ctx,
        CommandExecutionDelegate next)
    {
        try
        {
            return await next(ctx);
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 1;
        }
    }
}

public sealed class DbConnectionContext
{
    private string? _path = null;

    public void SetPath(string path)
    {
        if (_path is not null)
        {
            throw new InvalidOperationException("Path is already set.");
        }

        _path = path;
    }

    public string GetPath() => _path ??
                               throw new InvalidOperationException(
                                   $"Path has to be set using {nameof(SetPath)}");
}

public sealed record CommonParameters(
    [Option('d', Description = "Specifies the database file path.")]
    string Database,
    [Option('u', Description = "Specifies the user name.")]
    string User
) : ICommandParameterSet;