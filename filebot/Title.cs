using System.ComponentModel.DataAnnotations.Schema;

public class Title
{
    public string title_id { get; set; }
    
    public string primary_title { get; set; }   
    public int? premiered { get; set; }
}