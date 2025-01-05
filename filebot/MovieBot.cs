using System.Text.RegularExpressions;
using Spectre.Console;
using TMDbLib.Client;
using TMDbLib.Objects.Search;

public class MovieBot
{

    private readonly TMDbClient _client;
    private static Regex MatchImdbId = new Regex("\\[((.)+)\\]", RegexOptions.Compiled);

    public MovieBot(TMDbClient client)
    {
        _client = client;
    }

    public static string GetImdbId(string name)
    {
        var match = MatchImdbId.Match(name);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }
 

    private IEnumerable<DirectoryInfo> GetRemainingDirectories(DirectoryInfo mediaPath)
    {
        foreach (var folder in mediaPath.EnumerateDirectories())
        {
            if (folder.Name.Contains("imdbid"))
            {
                continue;
            }
            
            var limitUtc = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(3));

            if (folder.EnumerateFiles().Any(f => f.LastWriteTimeUtc >= limitUtc))
            {
                // skip if still being written to.
                continue;
            }

            if (!folder.EnumerateFiles().Any())
            {
                // skip empty folders
                continue;
            }

            yield return folder;
        }
    }

    private PromptResult<DirectoryInfo> GetFolderChoice(DirectoryInfo[] directories)
    {
        var selection = new SelectionPrompt<PromptResult<DirectoryInfo>>()
            .Title("Which folder do you want to process?")
            .PageSize(10)
            .EnableSearch()
            .AddChoices(AsPromptResult(directories))
            .AddChoices(PromptResult<DirectoryInfo>.ExitResult)
            .UseConverter(d => d.Display(dir => dir.Name))
            ;
        var dir = AnsiConsole.Prompt(
            selection
        );

        return dir;
    }

    private static IEnumerable<PromptResult<T>> AsPromptResult<T>(IEnumerable<T> items)
        => items.Select(d => new PromptResult<T>.Selection(d));

    private async Task<PromptResult<SearchMovie>> SearchMovieAsync(string query)
    {
        var movies = new List<SearchMovie>();
        var maxMovies = 100;
        var pageNumber = 1;
        var hasMore = true;
        do
        {
            var potentialMovies = await _client.SearchMovieAsync(query, page: pageNumber);
            movies.AddRange(potentialMovies.Results);
            hasMore = pageNumber < potentialMovies.TotalPages && movies.Count < maxMovies;
            pageNumber += 1;
        } while (hasMore);
        
        var selection = new SelectionPrompt<PromptResult<SearchMovie>>()
            .Title("Which movie is it?")
            .PageSize(10)
            .EnableSearch()
            .UseConverter(d => d.Display(mov => $"{mov.Title} ({mov.ReleaseDate:yyyy}) ({mov.Popularity})"))
            .AddChoices(AsPromptResult(movies))
            .AddChoices(new PromptResult<SearchMovie>.Manual())
            .AddChoices(PromptResult<SearchMovie>.ExitResult);

        
        var mov = AnsiConsole.Prompt(
            selection
        );

        return mov;
    }

    private bool GetConfirmation(string prompt)
    {
        var confirmation = AnsiConsole.Prompt(
            new TextPrompt<bool>(prompt)
                .AddChoice(true)
                .AddChoice(false)
                .DefaultValue(true)
                .WithConverter(choice => choice ? "y" : "n"));

        return confirmation;
    }

    private async Task ApplyMovieAsync(DirectoryInfo folder, SearchMovie searchMovie)
    {
        var movie = await _client.GetMovieAsync(searchMovie.Id);
        var newName = $"{movie.Title} ({movie.ReleaseDate:yyyy}) [imdbid-{movie.ImdbId}]";
        AnsiConsole.WriteLine($"Renaming {folder.Name} to {newName}");
        var accepted = GetConfirmation("Accept?");
        if (!accepted)
        {
            return;
        }
            
        folder.MoveTo(Path.Combine(folder.Parent.FullName, newName));

        // locate the main movie file
        var mainMovieFile = folder.EnumerateFiles()
            .OrderByDescending(f => f.Length)
            .FirstOrDefault(f => f.Extension is ".mkv" or ".mp4");

        if (mainMovieFile == null)
        {
            return;
        }

        // rename the main movie file
        var newMovieName =
            $"{movie.Title} ({movie.ReleaseDate:yyyy}) [imdbid-{movie.ImdbId}]{mainMovieFile.Extension}";
        AnsiConsole.WriteLine($"Renaming {mainMovieFile.Name} to {newMovieName}");
        mainMovieFile.MoveTo(Path.Combine(folder.FullName, newMovieName));

        // create a folder called extras
        try
        {
            var extrasFolder = folder.CreateSubdirectory("extras");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }

        // move all files but the main movie file into extras
        foreach (var file in folder.EnumerateFiles().Where(f => f.Name != mainMovieFile.Name))
        {
            AnsiConsole.WriteLine($"Moving {file.Name} to extras");
            file.MoveTo(Path.Combine(folder.FullName, "extras", file.Name));
        }
    }
    
    public async Task RunAsync(DirectoryInfo mediaPath)
    {
        while (true)
        {
            var options = GetRemainingDirectories(mediaPath).ToArray();
            if (!options.Any())
            {
                return;
            }

            var folderChoice = GetFolderChoice(options);

            if (folderChoice is not PromptResult<DirectoryInfo>.Selection folder)
            {
                // choose to exit
                return;
            }
            
            try
            {
                await ProcessFolderAsync(folder.Item);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
        }
    }

    private async Task ProcessFolderAsync(DirectoryInfo folder)
    {
        AnsiConsole.WriteLine("Trying folder: " + folder.Name);

        // get the name of the folder
        var name = NameCleaner.CleanIncoming(folder.Name)
                   ?? throw new Exception("Name not set");

        while (true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                // exit because no name entered
                return;
            }
                
            var movieResult = await SearchMovieAsync(name);

            if (movieResult is PromptResult<SearchMovie>.Selection movieSelection)
            {
                await ApplyMovieAsync(folder, movieSelection.Item);
                return;
            }

            if (movieResult is PromptResult<SearchMovie>.Exit)
            {
                return;
            }

            if (movieResult is PromptResult<SearchMovie>.Manual)
            {
                name = AnsiConsole.Ask("What do you want to search for?", "");
            }
        }
    }
}