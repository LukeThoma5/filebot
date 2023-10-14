using CommandLine;

public abstract class CommandLineOptionsBase
{
    [Option("media-path", Required = true, HelpText = "Path to the media files.")]
    public string MediaPath { get; set; }
}

[Verb("movies", HelpText = "Run the movie bot.")]
public class MovieCommandLineOptions : CommandLineOptionsBase
{
    [Option("db-path", Required = true, HelpText = "Path to the database file.")]
    public string DbPath { get; set; }
}


[Verb("tv")]
public class TvShowCommandLineOptions : CommandLineOptionsBase
{
    
}


public abstract class ParsedCommandLineOptionsBase
{
    public DirectoryInfo MediaPath { get; set; }
}

public class ParsedMovieCommandLineOptions : ParsedCommandLineOptionsBase
{
    public FileInfo DbPath { get; set; }
}


public class ParsedTvShowCommandLineOptions : ParsedCommandLineOptionsBase
{
    
}

public static class CliParser
{
public static ParsedCommandLineOptionsBase Parse(string[] args)
    {
        var result = Parser.Default.ParseArguments<MovieCommandLineOptions, TvShowCommandLineOptions>(args);
        if (result.Tag == ParserResultType.NotParsed)
        {
            throw new Exception("Failed to parse command line options.");
        }

        DirectoryInfo GetMediaPath(CommandLineOptionsBase opts)
        {
            var mediaPath = new DirectoryInfo(opts.MediaPath);
            return mediaPath;
        }
        
        

        var parsed = result.MapResult<MovieCommandLineOptions, TvShowCommandLineOptions, ParsedCommandLineOptionsBase>((MovieCommandLineOptions opts) =>
        {
            var dbPath = new FileInfo(opts.DbPath);
            if (!dbPath.Exists)
            {
                throw new Exception("Database file does not exist.");
            }

            return(ParsedCommandLineOptionsBase) new ParsedMovieCommandLineOptions
            {
                DbPath = dbPath,
                MediaPath = GetMediaPath(opts)
            };
        }, (TvShowCommandLineOptions opts) =>
        {
            return(ParsedCommandLineOptionsBase) new ParsedTvShowCommandLineOptions
            {
                MediaPath = GetMediaPath(opts)
            };
        }, errs => null)
            ?? throw new Exception("failed to parse command line options");
        


        return parsed;
    }
}