namespace FlowDesk.Desktop.Models;

public class SearchResultItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Project", "Task", "Doc", "File"
    public string TargetId { get; set; } = string.Empty;
}
