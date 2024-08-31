public static class TvCombineBot
{
    public static void Run(DirectoryInfo mediaPath, string prefix, string output, bool dryRun, int season)
    {
        if (season < 1)
        {
            return;
        }
        
        var outputRoot = mediaPath.CreateSubdirectory(output);
        outputRoot.CreateSubdirectory("Season 00");
        outputRoot.CreateSubdirectory($"Season {season:00}");
        var extrasFolder = Path.Combine(mediaPath.FullName, output, "Season 00");
        var seasonFolder = Path.Combine(mediaPath.FullName, output, $"Season {season:00}");

        var seasonNumber = season;
        var episodeNumber = 1;
        var files = mediaPath.EnumerateDirectories()
            .Where(n => n.Name.StartsWith(prefix))
            .SelectMany(s => s.EnumerateFiles())
            .OrderBy(s => s.FullName)
            .ToArray();
        var averageFileSize = files.Select(s => (int?)s.Length).Average();
        var items = new List<Action>();
        foreach (var file in files)
        {

            string newPath;
            if (file.Length < averageFileSize / 3)
            {
                var extraName = $"Extra S{seasonNumber:00} - {Guid.NewGuid():N}{file.Extension}";
                newPath = Path.Combine(extrasFolder, extraName);
            }
            else if (file.Length > 15000_000_000)
            {
                Console.WriteLine($"Skipping {file.FullName} file too big");
                // Console.ReadLine();
                continue;
            }
            else
            {
                var newFileName = $"S{seasonNumber:00}E{episodeNumber:00}{file.Extension}";
                newPath = Path.Combine(seasonFolder, newFileName);
                episodeNumber++;
            }

            Console.WriteLine($"Moving {file.FullName} to {newPath}");
            items.Add(() => file.MoveTo(newPath));
        }
        
        Console.WriteLine("Accept?:");
        var result = Console.ReadLine();
        if (result != "yes" || dryRun)
        {
            Console.WriteLine("Exiting");
            return;
        }

        foreach (var item in items)
        {
            item.Invoke();
        }
    }
}