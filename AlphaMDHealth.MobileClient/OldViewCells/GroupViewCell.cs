using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Group view cell for menu groups
/// </summary>
public class GroupViewCell : ViewCell
{
    private readonly CustomLabelControl _groupTitleLabel;

    /// <summary>
    /// Group view cell for menu groups
    /// </summary>
    public GroupViewCell()
    {
        bool isNotAllowSpace = (bool)Application.Current.Resources[StyleConstants.ST_IS_PROFILE_CIRCULAR_STYLE];
        double spacing = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
        _groupTitleLabel = new CustomLabelControl(GetLabelStyle(isNotAllowSpace))
        {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = TextAlignment.Center,
        };
        _groupTitleLabel.Padding = new Thickness(spacing, isNotAllowSpace ? 0 : spacing);
        if (!isNotAllowSpace)
        {
            _groupTitleLabel.HeightRequest = -1;
        }
        View = _groupTitleLabel;
        On<iOS>().SetDefaultBackgroundColor((Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR]);
    }

    private static LabelType GetLabelStyle(bool isNotAllowSpace)
    {
        if (isNotAllowSpace)
        {
            //if (AppStyles.IsTabletScaledView)
            //{
            //    return LabelType.HeaderPrimarySmallLeftWithoutPadding;
            //}
            //else
            //{
                return MobileConstants.IsTablet ? LabelType.PrimarySmallLeft : LabelType.HeaderPrimarySmallLeftWithoutPadding;
            //}
        }
        else
        {
            //if (AppStyles.IsTabletScaledView)
            //{
            //    return LabelType.HeaderPrimaryMediumBoldLeftWithoutPadding;
            //}
            //else
            //{
                return MobileConstants.IsTablet ? LabelType.PrimaryMediumBoldLeft : LabelType.HeaderPrimaryMediumBoldLeftWithoutPadding;
            //}
        }
    }

    /// <summary>
    /// Called when binding context is changed
    /// </summary>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        IGrouping<(byte SequenceNo, long MenuGroupID, string Content), MenuModel> menuData
            = BindingContext as IGrouping<(byte SequenceNo, long MenuGroupID, string Content), MenuModel>;
        if (menuData != null)
        {
            _groupTitleLabel.Text = menuData.Key.Content;
            _groupTitleLabel.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
            // Null or empty is used so that empty groups can also have full height based on requirement
            if (string.IsNullOrEmpty(_groupTitleLabel.Text))
            {
                _groupTitleLabel.HeightRequest = Height = 5;
            }
        }
    }
}