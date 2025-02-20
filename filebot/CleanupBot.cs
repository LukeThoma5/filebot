public static class CleanupBot
{
    public static void Run(DirectoryInfo mediaPath)
    {

        var emptyDirectories = mediaPath.EnumerateDirectories()
            .Where(n => !n.EnumerateDirectories().Any() && !n.EnumerateFiles().Any())
            .OrderBy(s => s.FullName)
            .ToArray();
        var items = new List<Action>();
        foreach (var dir in emptyDirectories)
        {
            Console.WriteLine($"Deleting {dir.FullName}");
            items.Add(() => dir.Delete());
        }
        
        Console.WriteLine("Accept?:");
        var result = Console.ReadLine();
        if (result != "yes")
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