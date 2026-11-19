namespace TechNova.ViewModels;

public class AdminListViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public List<string> Headers { get; set; } = [];
    public List<AdminTableRowViewModel> Rows { get; set; } = [];
}

public class AdminTableRowViewModel
{
    public List<string> Cells { get; set; } = [];
}
