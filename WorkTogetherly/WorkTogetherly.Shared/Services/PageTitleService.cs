namespace WorkTogetherly.Shared.Services;

public class PageTitleService
{
    public string Title { get; private set; } = string.Empty;
    public event Action? TitleChanged;

    public void SetTitle(string title)
    {
        Title = title;
        TitleChanged?.Invoke();
    }
}
