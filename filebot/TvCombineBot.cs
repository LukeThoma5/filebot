public static class TvCombineBot
{
    public static void Run(DirectoryInfo mediaPath, string prefix, string output)
    {
        var outputRoot = mediaPath.CreateSubdirectory(output);
        outputRoot.CreateSubdirectory("Season 00");
        outputRoot.CreateSubdirectory("Season 01");
        var extrasFolder = Path.Combine(mediaPath.FullName, output, "Season 00");
        var seasonFolder = Path.Combine(mediaPath.FullName, output, "Season 01");

        var seasonNumber = 1;
        var episodeNumber = 1;
        foreach (var file in mediaPath.EnumerateDirectories()
                     .Where(n => n.Name.StartsWith(prefix))
                     .SelectMany(s => s.EnumerateFiles())
                     .OrderBy(s => s.FullName)
                )
        {

            string newPath;
            if (file.Length < 1000_000_000)
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
            // file.MoveTo(newPath);
        }

    }
}