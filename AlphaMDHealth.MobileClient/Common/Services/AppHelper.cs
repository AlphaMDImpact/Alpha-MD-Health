using Controls.UserDialogs.Maui;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Represents App helper
/// </summary>
public static class AppHelper
{
    private static bool _showBusyIndicator;

    /// <summary>
    /// Text to be displayed along with the indicator
    /// </summary>
    public static string IndicatorText { get; set; } = string.Empty;

    /// <summary>
    /// Show/Hide busy indicator in screen
    /// </summary>
    public static bool ShowBusyIndicator
    {
        get => _showBusyIndicator;
        set
        {
            if (_showBusyIndicator != value)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _showBusyIndicator = value;
                    if (value)
                    {
                        UserDialogs.Instance.Loading(IndicatorText).Show();
                        ClearIndicatorText();
                    }
                    else
                    {
                        ClearIndicatorText();
                        UserDialogs.Instance.Loading().Hide();
                        UserDialogs.Instance.Loading().Dispose();
                    }
                });
            }
        }
    }

    private static void ClearIndicatorText()
    {
        if (!string.IsNullOrWhiteSpace(IndicatorText))
        {
            IndicatorText = string.Empty;
        }
    }
}