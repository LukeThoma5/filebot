// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using CommandLine;

using System.Data.SQLite;
using Dapper;

Console.WriteLine("Hello, World!");

var cliOptions = CliParser.Parse(args);

Console.WriteLine($"Database path: {cliOptions.DbPath.FullName}");
Console.WriteLine($"Media path: {cliOptions.MediaPath.FullName}");

// open the db with dapper
using var db = new SQLiteConnection($"Data Source={cliOptions.DbPath.FullName}");
db.Open();

// create a map of all the titles
var sql = "select title_id, primary_title, premiered  from titles where type='movie'";

// execute the query with dapper
var titles = db.Query<Title>(sql).Where(t => t.primary_title != null).ToList();

var titleMap = titles.Select(t => (NameCleaner.CleanName(t.primary_title), Title: t))
    .GroupBy(t => t.Item1)
    .ToImmutableDictionary(t => t.Key, t => t.Select(x => x.Item2).ToList());
    Console.WriteLine("Done");



// for each folder in the media folder
foreach (var folder in cliOptions.MediaPath.EnumerateDirectories())
{
    try
    {
        ProcessFolder(folder);
    }  catch (Exception ex)
    {
        Console.WriteLine($"Failed to process folder {folder.Name} {ex}");
    }
}

void ProcessFolder(DirectoryInfo folder)
{
     Console.WriteLine("Trying folder: " + folder.Name);
    if (folder.Name.Contains("imdbid"))
    {
        Console.WriteLine($"Skipping {folder.Name} got imdbid in the name");
        return;
    }
    
    // get the name of the folder
    var folderName = NameCleaner.CleanIncoming(folder.Name);

    // if the folder name is in the map
    if (!titleMap.TryGetValue(folderName, out var matchingTitles))
    {
        
        Console.WriteLine($"Skipping {folder.Name} no match");
        return;
    }

    Title title = null;

    if (matchingTitles.Count == 1)
    {
        title = matchingTitles.Single();
    }
    else
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Available options:");
                foreach (var (matchingTitle, index) in matchingTitles.Select((t, i) => (t, i)))
                {
                    Console.WriteLine(
                        $"{index}) {matchingTitle.primary_title} ({matchingTitle.premiered}) [imdbid-{matchingTitle.title_id}]");
                }

                var line = Console.ReadLine();
                if (line == "next")
                {
                    break;
                }
                
                var selectedIndex = int.Parse(line);
                title = matchingTitles[selectedIndex];
                if (title != null)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }

    if (title == null)
    {
        return;
    }
    
    var newName = $"{title.primary_title} ({title.premiered}) [imdbid-{title.title_id}]";
    Console.WriteLine($"Renaming {folder.Name} to {newName}");
    folder.MoveTo(Path.Combine(folder.Parent.FullName, newName));
    
    // locate the main movie file
    var mainMovieFile = folder.EnumerateFiles()
        .OrderByDescending(f => f.Length)
        .FirstOrDefault(f => f.Extension == ".mkv" || f.Extension == ".mp4");

    if (mainMovieFile == null)
    {
        return;
    }
    
    // rename the main movie file
    var newMovieName = $"{title.primary_title} ({title.premiered}) [imdbid-{title.title_id}]{mainMovieFile.Extension}";
    Console.WriteLine($"Renaming {mainMovieFile.Name} to {newMovieName}");
    mainMovieFile.MoveTo(Path.Combine(folder.FullName, newMovieName));
    
    // create a folder called extras
    try
    {

        var extrasFolder = folder.CreateSubdirectory("extras");
    }
    catch
    {
        
    }
    
    // move all files but the main movie file into extras
    foreach (var file in folder.EnumerateFiles().Where(f => f.Name != mainMovieFile.Name))
    {
        Console.WriteLine($"Moving {file.Name} to extras");
        file.MoveTo(Path.Combine(folder.FullName, "extras", file.Name));
    } 
}