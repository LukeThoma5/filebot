using System.Collections.Immutable;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using Dapper;

public static class MovieBot
{

    private static Regex MatchImdbId = new Regex("\\[((.)+)\\]", RegexOptions.Compiled);
    public static string GetImdbId(string name)
    {
        var match = MatchImdbId.Match(name);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }
    
    public static void Run(SQLiteConnection db, DirectoryInfo mediaPath)
    {
        // create a map of all the titles
        var sql = "select title_id, primary_title, premiered  from titles where type='movie'";

        // execute the query with dapper
        var titles = db.Query<Title>(sql).Where(t => t.primary_title != null).ToList();

        var titleMap = titles.Select(t => (NameCleaner.CleanName(t.primary_title), Title: t))
            .GroupBy(t => t.Item1)
            .ToImmutableDictionary(t => t.Key, t => t.Select(x => x.Item2).ToList());
        Console.WriteLine("Done");

        // for each folder in the media folder


        foreach (var folder in mediaPath.EnumerateDirectories())
        {
            try
            {
                ProcessFolder(folder);
            }
            catch (Exception ex)
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

            if (folder.Name.Contains("DVD_VIDEO"))
            {
                Console.WriteLine($"Skipping {folder.Name} got DVD_VIDEO in the name");
                return;
            }

            var limitUtc = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(3));

            if (folder.EnumerateFiles().Any(f => f.LastWriteTimeUtc >= limitUtc))
            {
                // skip if still being written to.
                return;
            }

            // get the name of the folder
            var title = GetTitleFromSearch(folder.Name);

            Title? GetTitleFromSearch(string search)
            {
                var folderName = NameCleaner.CleanIncoming(search);
                // if the folder name is in the map
                if (!titleMap.TryGetValue(folderName, out var matchingTitles))
                {
                    Console.WriteLine($"Skipping {folder.Name} no match");
                
                    var imdbId = GetImdbId(folder.Name);

                    if (string.IsNullOrWhiteSpace(imdbId))
                    {
                        return null;
                    }

                    matchingTitles = titleMap.Values.FirstOrDefault(v => v.Any(x => x.title_id == imdbId));
                }
            
                if (matchingTitles == null)
                {
                    return null;
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

                return title;
            }

            ConsoleKeyInfo key;

            if (title == null)
            {
                Console.WriteLine("Would you like to search (s)?:");
                key = Console.ReadKey();
                if (key.Key != ConsoleKey.S)
                {
                    return;
                }

                title = SearchTitle();
            }

            Title? SearchTitle()
            {
                while (true)
                {
                    Console.WriteLine("What would you like to search (next to skip): ");
                    var text = Console.ReadLine();
                    if (text == "next" || text is null)
                    {
                        return null;
                    }

                    var possibleTitle = GetTitleFromSearch(text);
                    if (possibleTitle != null)
                    {
                        return possibleTitle;
                    }
                }
            }

            var newName = $"{title.primary_title} ({title.premiered}) [imdbid-{title.title_id}]";
            Console.WriteLine($"Renaming {folder.Name} to {newName}");
            Console.Write("Accept (y)?: ");
            key = Console.ReadKey();
            if (key.Key != ConsoleKey.Y)
            {
                return;
            }
            
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
            var newMovieName =
                $"{title.primary_title} ({title.premiered}) [imdbid-{title.title_id}]{mainMovieFile.Extension}";
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
    }
}