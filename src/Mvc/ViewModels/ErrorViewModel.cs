namespace Mvc.ViewModels;

public class ErrorViewModel
{
    public string? ErrorId { get; set; }

    public string? ErrorMessage { get; set; }

    public bool ShowStackTrace { get; set; }

    public string? StackTrace { get; set; }
}
