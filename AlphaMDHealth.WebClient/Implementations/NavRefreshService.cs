namespace AlphaMDHealth.WebClient;

public class NavRefreshService : INavRefreshService
{
    public event Action RefreshRequested;

    public void CallRequestRefresh()
    {
        RefreshRequested?.Invoke();
    }
}