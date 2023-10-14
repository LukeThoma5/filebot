using System.Text.RegularExpressions;

public static class NameCleaner
{
    private static Regex _cleanRegex = new(@"[^a-zA-Z0-9]|(the)|(retail)|(dvd)|(directors cut)|(extended edition)", RegexOptions.Compiled);
    // regex to remove all spaces, _ and - from the name, words including the word "the"
    private static Regex regex = new Regex(@"[^a-zA-Z0-9]|(the)", RegexOptions.Compiled);
    public static string CleanName(string name) => regex.Replace(name.ToLower(), "");
    public static string CleanIncoming(string name) => _cleanRegex.Replace(name.Split("202").First().ToLower(), "");    
}