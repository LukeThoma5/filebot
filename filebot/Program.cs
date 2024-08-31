// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using CommandLine;
using System.Data.SQLite;
using Dapper;

Console.WriteLine("Hello, World!");

var cliOptions = CliParser.Parse(args);


Console.WriteLine($"Media path: {cliOptions.MediaPath.FullName}");

switch (cliOptions)
{
    case ParsedMovieCommandLineOptions movieOptions:
    {
        Console.WriteLine("Running movie bot");
        Console.WriteLine($"Database path: {movieOptions.DbPath.FullName}");

// open the db with dapper
        using var db = new SQLiteConnection($"Data Source={movieOptions.DbPath.FullName}");
        db.Open();

        MovieBot.Run(db, movieOptions.MediaPath);
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
}