// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Spectre.Console;
using TMDbLib.Client;

AnsiConsole.Clear();

var cliOptions = CliParser.Parse(args);


Console.WriteLine($"Media path: {cliOptions.MediaPath.FullName}");

switch (cliOptions)
{
    case ParsedMovieCommandLineOptions movieOptions:
    {
        AnsiConsole.WriteLine("Running movie bot");
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        IConfiguration config = builder.Build();

        var client = new TMDbClient(config["TMDBAPI"]);

        var movieBot = new MovieBot(client);
        await movieBot.RunAsync(movieOptions.MediaPath);
        break;
    }

    case ParsedTvShowCommandLineOptions tvOptions:
    {
        Console.WriteLine("Running tv show bot");
        SeasonBot.Run(tvOptions.MediaPath);
        break;
    }
    
    case ParsedTvCombineCommandLineOptions tvOptions:
    {
        Console.WriteLine("Running tv show bot");
        TvCombineBot.Run(tvOptions.MediaPath, tvOptions.Prefix, tvOptions.Output, tvOptions.DryRun, tvOptions.Season);
        break;
    }
    
    case ParsedCleanupCommandLineOptions tvOptions:
    {
        Console.WriteLine("Running Cleanup");
        CleanupBot.Run(tvOptions.MediaPath);
        break;
    }
}

