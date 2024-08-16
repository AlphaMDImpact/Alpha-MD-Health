using DevExpress.Maui.Controls;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateTabStyle()
    {
        Style defaultTabViewStyle = new Style(typeof(TabView))
        {
            Setters =
            {
                new Setter{Property = TabView.HeaderPanelIndentProperty , Value = Constants.ZERO_VALUE },
                new Setter{Property = TabView.SelectedItemIndicatorColorProperty , Value = _accentColor },
                new Setter{Property = TabView.HeaderPanelMinHeightProperty , Value = Constants.ZERO_VALUE },
                new Setter{Property = TabView.HeaderPanelPaddingProperty , Value = Constants.ZERO_VALUE}
            }
        };
        Application.Current.Resources.Add(StyleConstants.TABVIEW_STYLE, defaultTabViewStyle);
    }
}