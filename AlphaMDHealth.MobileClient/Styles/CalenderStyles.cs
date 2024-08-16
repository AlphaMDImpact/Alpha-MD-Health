using AlphaMDHealth.Utility;
using DevExpress.Maui.Scheduler;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateCalenderStyle()
    {
        Style defaultCalandarViewBaseStyle = new Style(typeof(ViewBase))
        {
            Setters =
            {
                new Setter { Property = ViewBase.VerticalOptionsProperty, Value= LayoutOptions.StartAndExpand },
                new Setter { Property = ViewBase.HorizontalOptionsProperty, Value= LayoutOptions.StartAndExpand },
                new Setter { Property = ViewBase.WidthRequestProperty, Value= App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0) - 20 },
                new Setter { Property = ViewBase.HeightRequestProperty, Value= App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0) - 20 },
                new Setter { Property = ViewBase.StartProperty, Value= DateTime.Now },
                new Setter { Property = ViewBase.FirstDayOfWeekProperty, Value= DayOfWeek.Monday },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_CALANDAR_VIEWBASE_STYLE, defaultCalandarViewBaseStyle);
    }
}