using Cocona;
using Cocona.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Musikchartograf.App;
using Musikchartograf.Data.Db;
using Musikchartograf.Data.LastFm;

var builder = CoconaApp.CreateBuilder();

builder.Services.AddHttpClient<ILastFmApiClient, LastFmApiClient>();
builder.Services.AddScoped<LoadPlayedSongsByUserForYearRequestHandler>();
builder.Services.AddSingleton<DbConnectionContext>();
builder.Services.AddTransient<PopulateDbConnectionContextFilter>();
builder.Services.AddDbContext<DataContext>((services, opt) =>
{
    var dbConnectionContext =
        services.GetRequiredService<DbConnectionContext>();
    opt.UseSqlite($"Data Source={dbConnectionContext.GetPath()}");
});

var app = builder.Build();
app.UseFilter(new PopulateDbConnectionContextFilterAttribute());

app.AddCommand("load-year",
    async (CommonParameters common, string apiKey,
        int year,
        LoadPlayedSongsByUserForYearRequestHandler handler,
        CoconaAppContext ctx) =>
    {
        var resp = await handler.Handle(
            new LoadPlayedSongsByUserForYearRequest(year, common.User, apiKey),
            ctx.CancellationToken);

        Console.WriteLine(resp);
    });

await app.RunAsync();

public sealed class PopulateDbConnectionContextFilterAttribute : Attribute,
    IFilterFactory
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return serviceProvider
            .GetRequiredService<PopulateDbConnectionContextFilter>();
    }
}

public sealed class PopulateDbConnectionContextFilter(
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