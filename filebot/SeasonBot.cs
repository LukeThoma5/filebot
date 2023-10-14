using System.Collections.Immutable;
using System.Data.SQLite;
using Dapper;

public static class SeasonBot
{
    public static void Run(DirectoryInfo mediaPath)
    {
        var extrasFolder = Path.Combine(mediaPath.FullName, "Season 00");
        foreach (var seasonFolder in mediaPath.EnumerateDirectories().OrderBy(t => t.Name))
        {
            if (!seasonFolder.Name.StartsWith("Season"))
            {
                continue;
            }

            var seasonNumber = int.Parse(seasonFolder.Name.Split("Season ")[1]);
            var episodeNumber = 1;
           
            foreach (var subFolder in seasonFolder.EnumerateDirectories().OrderBy(t => t.Name))
            {
                foreach (var file in subFolder.EnumerateFiles().OrderBy(t => t.Name))
                {
                    string newPath;
                    if (file.Length < 1000_000_000)
                    {
                        var extraName = $"Extra S{seasonNumber:00} - {Guid.NewGuid():N}.{file.Extension}";
                        newPath = Path.Combine(extrasFolder, extraName);
                    }
                    else
                    {
                        var newFileName = $"S{seasonNumber:00}E{episodeNumber:00}.{file.Extension}";   
                        newPath = Path.Combine(seasonFolder.FullName, newFileName);
                        episodeNumber++;
                    }
                    
                    Console.WriteLine($"Moving {file.FullName} to {newPath}");
                    // file.MoveTo(newPath);
                }
            }
        }
    }
}