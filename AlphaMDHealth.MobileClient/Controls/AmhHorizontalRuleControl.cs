using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class AmhHorizontalRuleControl : BoxView
{
    public AmhHorizontalRuleControl()
    {
        Style = (Style)App.Current.Resources[StyleConstants.ST_LIGHT_BACKGROUND_SEPARATOR_STYLE];
    }
}
