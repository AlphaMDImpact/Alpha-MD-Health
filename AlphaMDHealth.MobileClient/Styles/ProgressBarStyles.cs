using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateProgressBarStyle()
    {
        CreateMauiProgressBarStyle();
    }

    private void CreateMauiProgressBarStyle()
    {
        Style progressBarStyle = new Style(typeof(ProgressBar))
        {
            Setters =
            {
                new Setter{Property = ProgressBar.ProgressColorProperty , Value = _accentColor },
                new Setter{Property = ProgressBar.BackgroundColorProperty , Value = _primaryAppColor },
                new Setter{Property = ProgressBar.HeightRequestProperty , Value = 3},
                new Setter{Property = ProgressBar.MarginProperty , Value = 0},
                new Setter{Property = ProgressBar.ScaleYProperty , Value = 3},
                new Setter{Property = ProgressBar.HorizontalOptionsProperty , Value = LayoutOptions.FillAndExpand},
                new Setter{Property = ProgressBar.VerticalOptionsProperty , Value = LayoutOptions.End},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PROGRESS_BAR_STYLE, progressBarStyle);
    }
}