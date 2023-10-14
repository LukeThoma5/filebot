using CommandLine;

public class CommandLineOptions
{
    [Option("db-path", Required = true, HelpText = "Path to the database file.")]
    public string DbPath { get; set; }
    [Option("media-path", Required = true, HelpText = "Path to the media files.")]
    public string MediaPath { get; set; }
}

public class ParsedCommandLineOptions
{
    public FileInfo DbPath { get; set; }
    public DirectoryInfo MediaPath { get; set; }
}

public static class CliParser
{
public static ParsedCommandLineOptions Parse(string[] args)
    {
        var result = Parser.Default.ParseArguments<CommandLineOptions>(args);
        if (result.Tag == ParserResultType.NotParsed)
        {
            throw new Exception("Failed to parse command line options.");
        }
        var options = (Parsed<CommandLineOptions>)result;
        var parsed = new ParsedCommandLineOptions
        {
            DbPath = new FileInfo(options.Value.DbPath),
            MediaPath = new DirectoryInfo(options.Value.MediaPath)
        };

        if (!parsed.DbPath.Exists)
        {
            throw new Exception("Database file does not exist.");
        }
        
        if (!parsed.MediaPath.Exists)
        {
            throw new Exception("Media path does not exist.");
        }

        return parsed;
    }
}