using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class LabelControlDemoPage : BasePage
{
    private readonly AmhButtonControl _backButton;

    private readonly AmhLabelControl primaryLargeHVCenterLabelStyle;
    private readonly AmhLabelControl primaryLargeHStartVCenterLabelStyle;
    private readonly AmhLabelControl primaryLargeHEndVCenterLabelStyle;
    private readonly AmhLabelControl primaryLargeHVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryLargeHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryLargeHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl primaryMediumHVCenterLabelStyle;
    private readonly AmhLabelControl primaryMediumHStartVCenterLabelStyle;
    private readonly AmhLabelControl primaryMediumHEndVCenterLabelStyle;
    private readonly AmhLabelControl primaryMediumHVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryMediumHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryMediumHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl primarySmallHVCenterLabelStyle;
    private readonly AmhLabelControl primarySmallHStartVCenterLabelStyle;
    private readonly AmhLabelControl primarySmallHEndVCenterLabelStyle;
    private readonly AmhLabelControl primarySmallHVCenterBoldLabelStyle;
    private readonly AmhLabelControl primarySmallHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl primarySmallHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl primaryMicroHVCenterLabelStyle;
    private readonly AmhLabelControl primaryMicroHStartVCenterLabelStyle;
    private readonly AmhLabelControl primaryMicroHEndVCenterLabelStyle;
    private readonly AmhLabelControl primaryMicroHVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryMicroHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryMicroHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl secondaryLargeHVCenterLabelStyle;
    private readonly AmhLabelControl secondaryLargeHStartVCenterLabelStyle;
    private readonly AmhLabelControl secondaryLargeHEndVCenterLabelStyle;
    private readonly AmhLabelControl secondaryLargeHVCenterBoldLabelStyle;
    private readonly AmhLabelControl secondaryLargeHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl secondaryLargeHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl secondaryMediumHVCenterLabelStyle;
    private readonly AmhLabelControl secondaryMediumHStartVCenterLabelStyle;
    private readonly AmhLabelControl secondaryMediumHEndVCenterLabelStyle;
    private readonly AmhLabelControl secondaryMediumHVCenterBoldLabelStyle;
    private readonly AmhLabelControl secondaryMediumHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl secondaryMediumHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl secondarySmallHVCenterLabelStyle;
    private readonly AmhLabelControl secondarySmallHStartVCenterLabelStyle;
    private readonly AmhLabelControl secondarySmallHEndVCenterLabelStyle;
    private readonly AmhLabelControl secondarySmallHVCenterBoldLabelStyle;
    private readonly AmhLabelControl secondarySmallHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl secondarySmallHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl secondaryMicroHVCenterLabelStyle;
    private readonly AmhLabelControl secondaryMicroHStartVCenterLabelStyle;
    private readonly AmhLabelControl secondaryMicroHEndVCenterLabelStyle;
    private readonly AmhLabelControl secondaryMicroHVCenterBoldLabelStyle;
    private readonly AmhLabelControl secondaryMicroHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl secondaryMicroHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl tertiaryLargeHVCenterLabelStyle;
    private readonly AmhLabelControl tertiaryLargeHStartVCenterLabelStyle;
    private readonly AmhLabelControl tertiaryLargeHEndVCenterLabelStyle;
    private readonly AmhLabelControl tertiaryLargeHVCenterBoldLabelStyle;
    private readonly AmhLabelControl tertiaryLargeHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl tertiaryLargeHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl tertiaryMediumHVCenterLabelStyle;
    private readonly AmhLabelControl tertiaryMediumHStartVCenterLabelStyle;
    private readonly AmhLabelControl tertiaryMediumHEndVCenterLabelStyle;
    private readonly AmhLabelControl tertiaryMediumHVCenterBoldLabelStyle;
    private readonly AmhLabelControl tertiaryMediumHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl tertiaryMediumHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl tertiarySmallHVCenterLabelStyle;
    private readonly AmhLabelControl tertiarySmallHStartVCenterLabelStyle;
    private readonly AmhLabelControl tertiarySmallHEndVCenterLabelStyle;
    private readonly AmhLabelControl tertiarySmallHVCenterBoldLabelStyle;
    private readonly AmhLabelControl tertiarySmallHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl tertiarySmallHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl tertiaryMicroHVCenterLabelStyle;
    private readonly AmhLabelControl tertiaryMicroHStartVCenterLabelStyle;
    private readonly AmhLabelControl tertiaryMicroHEndVCenterLabelStyle;
    private readonly AmhLabelControl tertiaryMicroHVCenterBoldLabelStyle;
    private readonly AmhLabelControl tertiaryMicroHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl tertiaryMicroHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl primaryAppLargeHVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppLargeHStartVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppLargeHEndVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppLargeHVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryAppLargeHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryAppLargeHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl primaryAppMediumHVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppMediumHStartVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppMediumHEndVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppMediumHVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryAppMediumHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryAppMediumHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl primaryAppSmallHVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppSmallHStartVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppSmallHEndVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppSmallHVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryAppSmallHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryAppSmallHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl primaryAppMicroHVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppMicroHStartVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppMicroHEndVCenterLabelStyle;
    private readonly AmhLabelControl primaryAppMicroHVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryAppMicroHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl primaryAppMicroHEndVCenterBoldLabelStyle;

    private readonly Grid views;

    private readonly AmhLabelControl lightLargeHVCenterLabelStyle;
    private readonly AmhLabelControl lightLargeHStartVCenterLabelStyle;
    private readonly AmhLabelControl lightLargeHEndVCenterLabelStyle;
    private readonly AmhLabelControl lightLargeHVCenterBoldLabelStyle;
    private readonly AmhLabelControl lightLargeHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl lightLargeHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl lightMediumHVCenterLabelStyle;
    private readonly AmhLabelControl lightMediumHStartVCenterLabelStyle;
    private readonly AmhLabelControl lightMediumHEndVCenterLabelStyle;
    private readonly AmhLabelControl lightMediumHVCenterBoldLabelStyle;
    private readonly AmhLabelControl lightMediumHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl lightMediumHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl lightSmallHVCenterLabelStyle;
    private readonly AmhLabelControl lightSmallHStartVCenterLabelStyle;
    private readonly AmhLabelControl lightSmallHEndVCenterLabelStyle;
    private readonly AmhLabelControl lightSmallHVCenterBoldLabelStyle;
    private readonly AmhLabelControl lightSmallHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl lightSmallHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl lightMicroHVCenterLabelStyle;
    private readonly AmhLabelControl lightMicroHStartVCenterLabelStyle;
    private readonly AmhLabelControl lightMicroHEndVCenterLabelStyle;
    private readonly AmhLabelControl lightMicroHVCenterBoldLabelStyle;
    private readonly AmhLabelControl lightMicroHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl lightMicroHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl htmlWhiteLabelControl;
    private readonly AmhLabelControl htmlWhiteCenterLabelControl;

    private readonly AmhLabelControl linkHVCenterLabelStyle;
    private readonly AmhLabelControl linkHStartVCenterLabelStyle;
    private readonly AmhLabelControl linkHEndVCenterLabelStyle;
    private readonly AmhLabelControl linkHVCenterBoldLabelStyle;
    private readonly AmhLabelControl linkHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl linkHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl errorHVCenterLabelStyle;
    private readonly AmhLabelControl errorHStartVCenterLabelStyle;
    private readonly AmhLabelControl errorHEndVCenterLabelStyle;
    private readonly AmhLabelControl errorHVCenterBoldLabelStyle;
    private readonly AmhLabelControl errorHStartVCenterBoldLabelStyle;
    private readonly AmhLabelControl errorHEndVCenterBoldLabelStyle;

    private readonly AmhLabelControl _htmlPrimaryLabel;
    private readonly AmhLabelControl _htmlPrimaryCenterLabel;
    private readonly AmhLabelControl _htmlSecondaryLabel;
    private readonly AmhLabelControl _htmlSecondaryCenterLabel;
    private readonly AmhLabelControl _htmlTertiaryLabel;
    private readonly AmhLabelControl _htmlTertiaryCenterLabel;

    public LabelControlDemoPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);

        views = new Grid()
        {
            RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto } },
            BackgroundColor = Colors.Black
        };

        primaryLargeHVCenterLabelStyle = AddLabel(FieldTypes.PrimaryLargeHVCenterLabelControl);
        primaryLargeHStartVCenterLabelStyle = AddLabel(FieldTypes.PrimaryLargeHStartVCenterLabelControl);
        primaryLargeHEndVCenterLabelStyle = AddLabel(FieldTypes.PrimaryLargeHEndVCenterLabelControl);
        primaryLargeHVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryLargeHVCenterBoldLabelControl);
        primaryLargeHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryLargeHStartVCenterBoldLabelControl);
        primaryLargeHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryLargeHEndVCenterBoldLabelControl);
        primaryMediumHVCenterLabelStyle = AddLabel(FieldTypes.PrimaryMediumHVCenterLabelControl);
        primaryMediumHStartVCenterLabelStyle = AddLabel(FieldTypes.PrimaryMediumHStartVCenterLabelControl);
        primaryMediumHEndVCenterLabelStyle = AddLabel(FieldTypes.PrimaryMediumHEndVCenterLabelControl);
        primaryMediumHVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryMediumHVCenterBoldLabelControl);
        primaryMediumHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryMediumHStartVCenterBoldLabelControl);
        primaryMediumHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryMediumHEndVCenterBoldLabelControl);
        primarySmallHVCenterLabelStyle = AddLabel(FieldTypes.PrimarySmallHVCenterLabelControl);
        primarySmallHStartVCenterLabelStyle = AddLabel(FieldTypes.PrimarySmallHStartVCenterLabelControl);
        primarySmallHEndVCenterLabelStyle = AddLabel(FieldTypes.PrimarySmallHEndVCenterLabelControl);
        primarySmallHVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimarySmallHVCenterBoldLabelControl);
        primarySmallHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimarySmallHStartVCenterBoldLabelControl);
        primarySmallHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimarySmallHEndVCenterBoldLabelControl);
        primaryMicroHVCenterLabelStyle = AddLabel(FieldTypes.PrimaryMicroHVCenterLabelControl);
        primaryMicroHStartVCenterLabelStyle = AddLabel(FieldTypes.PrimaryMicroHStartVCenterLabelControl);
        primaryMicroHEndVCenterLabelStyle = AddLabel(FieldTypes.PrimaryMicroHEndVCenterLabelControl);
        primaryMicroHVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryMicroHVCenterBoldLabelControl);
        primaryMicroHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryMicroHStartVCenterBoldLabelControl);
        primaryMicroHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryMicroHEndVCenterBoldLabelControl);

        secondaryLargeHVCenterLabelStyle = AddLabel(FieldTypes.SecondaryLargeHVCenterLabelControl);
        secondaryLargeHStartVCenterLabelStyle = AddLabel(FieldTypes.SecondaryLargeHStartVCenterLabelControl);
        secondaryLargeHEndVCenterLabelStyle = AddLabel(FieldTypes.SecondaryLargeHEndVCenterLabelControl);
        secondaryLargeHVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondaryLargeHVCenterBoldLabelControl);
        secondaryLargeHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondaryLargeHStartVCenterBoldLabelControl);
        secondaryLargeHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondaryLargeHEndVCenterBoldLabelControl);
        secondaryMediumHVCenterLabelStyle = AddLabel(FieldTypes.SecondaryMediumHVCenterLabelControl);
        secondaryMediumHStartVCenterLabelStyle = AddLabel(FieldTypes.SecondaryMediumHStartVCenterLabelControl);
        secondaryMediumHEndVCenterLabelStyle = AddLabel(FieldTypes.SecondaryMediumHEndVCenterLabelControl);
        secondaryMediumHVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondaryMediumHVCenterBoldLabelControl);
        secondaryMediumHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondaryMediumHStartVCenterBoldLabelControl);
        secondaryMediumHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondaryMediumHEndVCenterBoldLabelControl);
        secondarySmallHVCenterLabelStyle = AddLabel(FieldTypes.SecondarySmallHVCenterLabelControl);
        secondarySmallHStartVCenterLabelStyle = AddLabel(FieldTypes.SecondarySmallHStartVCenterLabelControl);
        secondarySmallHEndVCenterLabelStyle = AddLabel(FieldTypes.SecondarySmallHEndVCenterLabelControl);
        secondarySmallHVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondarySmallHVCenterBoldLabelControl);
        secondarySmallHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondarySmallHStartVCenterBoldLabelControl);
        secondarySmallHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondarySmallHEndVCenterBoldLabelControl);
        secondaryMicroHVCenterLabelStyle = AddLabel(FieldTypes.SecondaryMicroHVCenterLabelControl);
        secondaryMicroHStartVCenterLabelStyle = AddLabel(FieldTypes.SecondaryMicroHStartVCenterLabelControl);
        secondaryMicroHEndVCenterLabelStyle = AddLabel(FieldTypes.SecondaryMicroHEndVCenterLabelControl);
        secondaryMicroHVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondaryMicroHVCenterBoldLabelControl);
        secondaryMicroHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondaryMicroHStartVCenterBoldLabelControl);
        secondaryMicroHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.SecondaryMicroHEndVCenterBoldLabelControl);

        tertiaryLargeHVCenterLabelStyle = AddLabel(FieldTypes.TertiaryLargeHVCenterLabelControl);
        tertiaryLargeHStartVCenterLabelStyle = AddLabel(FieldTypes.TertiaryLargeHStartVCenterLabelControl);
        tertiaryLargeHEndVCenterLabelStyle = AddLabel(FieldTypes.TertiaryLargeHEndVCenterLabelControl);
        tertiaryLargeHVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiaryLargeHVCenterBoldLabelControl);
        tertiaryLargeHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiaryLargeHStartVCenterBoldLabelControl);
        tertiaryLargeHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiaryLargeHEndVCenterBoldLabelControl);
        tertiaryMediumHVCenterLabelStyle = AddLabel(FieldTypes.TertiaryMediumHVCenterLabelControl);
        tertiaryMediumHStartVCenterLabelStyle = AddLabel(FieldTypes.TertiaryMediumHStartVCenterLabelControl);
        tertiaryMediumHEndVCenterLabelStyle = AddLabel(FieldTypes.TertiaryMediumHEndVCenterLabelControl);
        tertiaryMediumHVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiaryMediumHVCenterBoldLabelControl);
        tertiaryMediumHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiaryMediumHStartVCenterBoldLabelControl);
        tertiaryMediumHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiaryMediumHEndVCenterBoldLabelControl);
        tertiarySmallHVCenterLabelStyle = AddLabel(FieldTypes.TertiarySmallHVCenterLabelControl);
        tertiarySmallHStartVCenterLabelStyle = AddLabel(FieldTypes.TertiarySmallHStartVCenterLabelControl);
        tertiarySmallHEndVCenterLabelStyle = AddLabel(FieldTypes.TertiarySmallHEndVCenterLabelControl);
        tertiarySmallHVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiarySmallHVCenterBoldLabelControl);
        tertiarySmallHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiarySmallHStartVCenterBoldLabelControl);
        tertiarySmallHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiarySmallHEndVCenterBoldLabelControl);
        tertiaryMicroHVCenterLabelStyle = AddLabel(FieldTypes.TertiaryMicroHVCenterLabelControl);
        tertiaryMicroHStartVCenterLabelStyle = AddLabel(FieldTypes.TertiaryMicroHStartVCenterLabelControl);
        tertiaryMicroHEndVCenterLabelStyle = AddLabel(FieldTypes.TertiaryMicroHEndVCenterLabelControl);
        tertiaryMicroHVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiaryMicroHVCenterBoldLabelControl);
        tertiaryMicroHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiaryMicroHStartVCenterBoldLabelControl);
        tertiaryMicroHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.TertiaryMicroHEndVCenterBoldLabelControl);

        primaryAppLargeHVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppLargeHVCenterLabelControl);
        primaryAppLargeHStartVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppLargeHStartVCenterLabelControl);
        primaryAppLargeHEndVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppLargeHEndVCenterLabelControl);
        primaryAppLargeHVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppLargeHVCenterBoldLabelControl);
        primaryAppLargeHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppLargeHStartVCenterBoldLabelControl);
        primaryAppLargeHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppLargeHEndVCenterBoldLabelControl);
        primaryAppMediumHVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppMediumHVCenterLabelControl);
        primaryAppMediumHStartVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppMediumHStartVCenterLabelControl);
        primaryAppMediumHEndVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppMediumHEndVCenterLabelControl);
        primaryAppMediumHVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppMediumHVCenterBoldLabelControl);
        primaryAppMediumHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppMediumHStartVCenterBoldLabelControl);
        primaryAppMediumHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppMediumHEndVCenterBoldLabelControl);
        primaryAppSmallHVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppSmallHVCenterLabelControl);
        primaryAppSmallHStartVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppSmallHStartVCenterLabelControl);
        primaryAppSmallHEndVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppSmallHEndVCenterLabelControl);
        primaryAppSmallHVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppSmallHVCenterBoldLabelControl);
        primaryAppSmallHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppSmallHStartVCenterBoldLabelControl);
        primaryAppSmallHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppSmallHEndVCenterBoldLabelControl);
        primaryAppMicroHVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppMicroHVCenterLabelControl);
        primaryAppMicroHStartVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppMicroHStartVCenterLabelControl);
        primaryAppMicroHEndVCenterLabelStyle = AddLabel(FieldTypes.PrimaryAppMicroHEndVCenterLabelControl);
        primaryAppMicroHVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppMicroHVCenterBoldLabelControl);
        primaryAppMicroHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppMicroHStartVCenterBoldLabelControl);
        primaryAppMicroHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.PrimaryAppMicroHEndVCenterBoldLabelControl);

        errorHVCenterLabelStyle = AddLabel(FieldTypes.ErrorHVCenterLabelControl);
        errorHStartVCenterLabelStyle = AddLabel(FieldTypes.ErrorHStartVCenterLabelControl);
        errorHEndVCenterLabelStyle = AddLabel(FieldTypes.ErrorHEndVCenterLabelControl);
        errorHVCenterBoldLabelStyle = AddLabel(FieldTypes.ErrorHVCenterBoldLabelControl);
        errorHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.ErrorHStartVCenterBoldLabelControl);
        errorHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.ErrorHEndVCenterBoldLabelControl);

        linkHVCenterLabelStyle = AddLabel(FieldTypes.LinkHVCenterLabelControl);
        linkHStartVCenterLabelStyle = AddLabel(FieldTypes.LinkHStartVCenterLabelControl);
        linkHEndVCenterLabelStyle = AddLabel(FieldTypes.LinkHEndVCenterLabelControl);
        linkHVCenterBoldLabelStyle = AddLabel(FieldTypes.LinkHVCenterBoldLabelControl);
        linkHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.LinkHStartVCenterBoldLabelControl);
        linkHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.LinkHEndVCenterBoldLabelControl);

        _htmlPrimaryLabel = AddLabel(FieldTypes.HtmlPrimaryLabelControl);
        _htmlPrimaryCenterLabel = AddLabel(FieldTypes.HtmlPrimaryCenterLabelControl);
        _htmlSecondaryLabel = AddLabel(FieldTypes.HtmlSecondaryLabelControl);
        _htmlSecondaryCenterLabel = AddLabel(FieldTypes.HtmlSecondaryCenterLabelControl);
        _htmlTertiaryLabel = AddLabel(FieldTypes.HtmlTertiaryLabelControl);
        _htmlTertiaryCenterLabel = AddLabel(FieldTypes.HtmlTertiaryCenterLabelControl);

        lightLargeHVCenterLabelStyle = AddLabel(FieldTypes.LightLargeHVCenterLabelControl, true);
        lightLargeHStartVCenterLabelStyle = AddLabel(FieldTypes.LightLargeHStartVCenterLabelControl, true);
        lightLargeHEndVCenterLabelStyle = AddLabel(FieldTypes.LightLargeHEndVCenterLabelControl, true);
        lightLargeHVCenterBoldLabelStyle = AddLabel(FieldTypes.LightLargeHVCenterBoldLabelControl, true);
        lightLargeHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.LightLargeHStartVCenterBoldLabelControl, true);
        lightLargeHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.LightLargeHEndVCenterBoldLabelControl, true);
        lightMediumHVCenterLabelStyle = AddLabel(FieldTypes.LightMediumHVCenterLabelControl, true);
        lightMediumHStartVCenterLabelStyle = AddLabel(FieldTypes.LightMediumHStartVCenterLabelControl, true);
        lightMediumHEndVCenterLabelStyle = AddLabel(FieldTypes.LightMediumHEndVCenterLabelControl, true);
        lightMediumHVCenterBoldLabelStyle = AddLabel(FieldTypes.LightMediumHVCenterBoldLabelControl, true);
        lightMediumHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.LightMediumHStartVCenterBoldLabelControl, true);
        lightMediumHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.LightMediumHEndVCenterBoldLabelControl, true);
        lightSmallHVCenterLabelStyle = AddLabel(FieldTypes.LightSmallHVCenterLabelControl, true);
        lightSmallHStartVCenterLabelStyle = AddLabel(FieldTypes.LightSmallHStartVCenterLabelControl, true);
        lightSmallHEndVCenterLabelStyle = AddLabel(FieldTypes.LightSmallHEndVCenterLabelControl, true);
        lightSmallHVCenterBoldLabelStyle = AddLabel(FieldTypes.LightSmallHVCenterBoldLabelControl, true);
        lightSmallHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.LightSmallHStartVCenterBoldLabelControl, true);
        lightSmallHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.LightSmallHEndVCenterBoldLabelControl, true);
        lightMicroHVCenterLabelStyle = AddLabel(FieldTypes.LightMicroHVCenterLabelControl, true);
        lightMicroHStartVCenterLabelStyle = AddLabel(FieldTypes.LightMicroHStartVCenterLabelControl, true);
        lightMicroHEndVCenterLabelStyle = AddLabel(FieldTypes.LightMicroHEndVCenterLabelControl, true);
        lightMicroHVCenterBoldLabelStyle = AddLabel(FieldTypes.LightMicroHVCenterBoldLabelControl, true);
        lightMicroHStartVCenterBoldLabelStyle = AddLabel(FieldTypes.LightMicroHStartVCenterBoldLabelControl, true);
        lightMicroHEndVCenterBoldLabelStyle = AddLabel(FieldTypes.LightMicroHEndVCenterBoldLabelControl, true);

        htmlWhiteLabelControl = AddLabel(FieldTypes.HtmlLightLabelControl, true);
        htmlWhiteCenterLabelControl = AddLabel(FieldTypes.HtmlLightCenterLabelControl, true);

        AddView(views);

        _backButton = new AmhButtonControl(FieldTypes.DeleteButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_backButton);
    }

    private AmhLabelControl AddLabel(FieldTypes fieldType, bool isLight = false)
    {
        var lbl = new AmhLabelControl(fieldType) { ResourceKey = ResourceConstants.R_LOGINPAGE_TERMS_OFUSE_SENTENCE_KEY };
        if (isLight)
        {
            AddSubView(lbl);
        }
        else
        {
            AddView(lbl);
        }
        return lbl;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(true);

        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);

        _backButton.PageResources = PageData;
        _backButton.OnValueChanged += OnBackButtonClicked;

        primaryLargeHVCenterLabelStyle.PageResources = PageData;
        primaryLargeHStartVCenterLabelStyle.PageResources = PageData;
        primaryLargeHEndVCenterLabelStyle.PageResources = PageData;
        primaryLargeHVCenterBoldLabelStyle.PageResources = PageData;
        primaryLargeHStartVCenterBoldLabelStyle.PageResources = PageData;
        primaryLargeHEndVCenterBoldLabelStyle.PageResources = PageData;
        primaryMediumHVCenterLabelStyle.PageResources = PageData;
        primaryMediumHStartVCenterLabelStyle.PageResources = PageData;
        primaryMediumHEndVCenterLabelStyle.PageResources = PageData;
        primaryMediumHVCenterBoldLabelStyle.PageResources = PageData;
        primaryMediumHStartVCenterBoldLabelStyle.PageResources = PageData;
        primaryMediumHEndVCenterBoldLabelStyle.PageResources = PageData;
        primarySmallHVCenterLabelStyle.PageResources = PageData;
        primarySmallHStartVCenterLabelStyle.PageResources = PageData;
        primarySmallHEndVCenterLabelStyle.PageResources = PageData;
        primarySmallHVCenterBoldLabelStyle.PageResources = PageData;
        primarySmallHStartVCenterBoldLabelStyle.PageResources = PageData;
        primarySmallHEndVCenterBoldLabelStyle.PageResources = PageData;
        primaryMicroHVCenterLabelStyle.PageResources = PageData;
        primaryMicroHStartVCenterLabelStyle.PageResources = PageData;
        primaryMicroHEndVCenterLabelStyle.PageResources = PageData;
        primaryMicroHVCenterBoldLabelStyle.PageResources = PageData;
        primaryMicroHStartVCenterBoldLabelStyle.PageResources = PageData;
        primaryMicroHEndVCenterBoldLabelStyle.PageResources = PageData;

        secondaryLargeHVCenterLabelStyle.PageResources = PageData;
        secondaryLargeHStartVCenterLabelStyle.PageResources = PageData;
        secondaryLargeHEndVCenterLabelStyle.PageResources = PageData;
        secondaryLargeHVCenterBoldLabelStyle.PageResources = PageData;
        secondaryLargeHStartVCenterBoldLabelStyle.PageResources = PageData;
        secondaryLargeHEndVCenterBoldLabelStyle.PageResources = PageData;
        secondaryMediumHVCenterLabelStyle.PageResources = PageData;
        secondaryMediumHStartVCenterLabelStyle.PageResources = PageData;
        secondaryMediumHEndVCenterLabelStyle.PageResources = PageData;
        secondaryMediumHVCenterBoldLabelStyle.PageResources = PageData;
        secondaryMediumHStartVCenterBoldLabelStyle.PageResources = PageData;
        secondaryMediumHEndVCenterBoldLabelStyle.PageResources = PageData;
        secondarySmallHVCenterLabelStyle.PageResources = PageData;
        secondarySmallHStartVCenterLabelStyle.PageResources = PageData;
        secondarySmallHEndVCenterLabelStyle.PageResources = PageData;
        secondarySmallHVCenterBoldLabelStyle.PageResources = PageData;
        secondarySmallHStartVCenterBoldLabelStyle.PageResources = PageData;
        secondarySmallHEndVCenterBoldLabelStyle.PageResources = PageData;
        secondaryMicroHVCenterLabelStyle.PageResources = PageData;
        secondaryMicroHStartVCenterLabelStyle.PageResources = PageData;
        secondaryMicroHEndVCenterLabelStyle.PageResources = PageData;
        secondaryMicroHVCenterBoldLabelStyle.PageResources = PageData;
        secondaryMicroHStartVCenterBoldLabelStyle.PageResources = PageData;
        secondaryMicroHEndVCenterBoldLabelStyle.PageResources = PageData;

        tertiaryLargeHVCenterLabelStyle.PageResources = PageData;
        tertiaryLargeHStartVCenterLabelStyle.PageResources = PageData;
        tertiaryLargeHEndVCenterLabelStyle.PageResources = PageData;
        tertiaryLargeHVCenterBoldLabelStyle.PageResources = PageData;
        tertiaryLargeHStartVCenterBoldLabelStyle.PageResources = PageData;
        tertiaryLargeHEndVCenterBoldLabelStyle.PageResources = PageData;
        tertiaryMediumHVCenterLabelStyle.PageResources = PageData;
        tertiaryMediumHStartVCenterLabelStyle.PageResources = PageData;
        tertiaryMediumHEndVCenterLabelStyle.PageResources = PageData;
        tertiaryMediumHVCenterBoldLabelStyle.PageResources = PageData;
        tertiaryMediumHStartVCenterBoldLabelStyle.PageResources = PageData;
        tertiaryMediumHEndVCenterBoldLabelStyle.PageResources = PageData;
        tertiarySmallHVCenterLabelStyle.PageResources = PageData;
        tertiarySmallHStartVCenterLabelStyle.PageResources = PageData;
        tertiarySmallHEndVCenterLabelStyle.PageResources = PageData;
        tertiarySmallHVCenterBoldLabelStyle.PageResources = PageData;
        tertiarySmallHStartVCenterBoldLabelStyle.PageResources = PageData;
        tertiarySmallHEndVCenterBoldLabelStyle.PageResources = PageData;
        tertiaryMicroHVCenterLabelStyle.PageResources = PageData;
        tertiaryMicroHStartVCenterLabelStyle.PageResources = PageData;
        tertiaryMicroHEndVCenterLabelStyle.PageResources = PageData;
        tertiaryMicroHVCenterBoldLabelStyle.PageResources = PageData;
        tertiaryMicroHStartVCenterBoldLabelStyle.PageResources = PageData;
        tertiaryMicroHEndVCenterBoldLabelStyle.PageResources = PageData;

        primaryAppLargeHVCenterLabelStyle.PageResources = PageData;
        primaryAppLargeHStartVCenterLabelStyle.PageResources = PageData;
        primaryAppLargeHEndVCenterLabelStyle.PageResources = PageData;
        primaryAppLargeHVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppLargeHStartVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppLargeHEndVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppMediumHVCenterLabelStyle.PageResources = PageData;
        primaryAppMediumHStartVCenterLabelStyle.PageResources = PageData;
        primaryAppMediumHEndVCenterLabelStyle.PageResources = PageData;
        primaryAppMediumHVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppMediumHStartVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppMediumHEndVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppSmallHVCenterLabelStyle.PageResources = PageData;
        primaryAppSmallHStartVCenterLabelStyle.PageResources = PageData;
        primaryAppSmallHEndVCenterLabelStyle.PageResources = PageData;
        primaryAppSmallHVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppSmallHStartVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppSmallHEndVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppMicroHVCenterLabelStyle.PageResources = PageData;
        primaryAppMicroHStartVCenterLabelStyle.PageResources = PageData;
        primaryAppMicroHEndVCenterLabelStyle.PageResources = PageData;
        primaryAppMicroHVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppMicroHStartVCenterBoldLabelStyle.PageResources = PageData;
        primaryAppMicroHEndVCenterBoldLabelStyle.PageResources = PageData;

        lightLargeHVCenterLabelStyle.PageResources = PageData;
        lightLargeHStartVCenterLabelStyle.PageResources = PageData;
        lightLargeHEndVCenterLabelStyle.PageResources = PageData;
        lightLargeHVCenterBoldLabelStyle.PageResources = PageData;
        lightLargeHStartVCenterBoldLabelStyle.PageResources = PageData;
        lightLargeHEndVCenterBoldLabelStyle.PageResources = PageData;
        lightMediumHVCenterLabelStyle.PageResources = PageData;
        lightMediumHStartVCenterLabelStyle.PageResources = PageData;
        lightMediumHEndVCenterLabelStyle.PageResources = PageData;
        lightMediumHVCenterBoldLabelStyle.PageResources = PageData;
        lightMediumHStartVCenterBoldLabelStyle.PageResources = PageData;
        lightMediumHEndVCenterBoldLabelStyle.PageResources = PageData;
        lightSmallHVCenterLabelStyle.PageResources = PageData;
        lightSmallHStartVCenterLabelStyle.PageResources = PageData;
        lightSmallHEndVCenterLabelStyle.PageResources = PageData;
        lightSmallHVCenterBoldLabelStyle.PageResources = PageData;
        lightSmallHStartVCenterBoldLabelStyle.PageResources = PageData;
        lightSmallHEndVCenterBoldLabelStyle.PageResources = PageData;
        lightMicroHVCenterLabelStyle.PageResources = PageData;
        lightMicroHStartVCenterLabelStyle.PageResources = PageData;
        lightMicroHEndVCenterLabelStyle.PageResources = PageData;
        lightMicroHVCenterBoldLabelStyle.PageResources = PageData;
        lightMicroHStartVCenterBoldLabelStyle.PageResources = PageData;
        lightMicroHEndVCenterBoldLabelStyle.PageResources = PageData;

        linkHVCenterLabelStyle.PageResources = PageData;
        linkHStartVCenterLabelStyle.PageResources = PageData;
        linkHEndVCenterLabelStyle.PageResources = PageData;
        linkHVCenterBoldLabelStyle.PageResources = PageData;
        linkHStartVCenterBoldLabelStyle.PageResources = PageData;
        linkHEndVCenterBoldLabelStyle.PageResources = PageData;

        errorHVCenterLabelStyle.PageResources = PageData;
        errorHStartVCenterLabelStyle.PageResources = PageData;
        errorHEndVCenterLabelStyle.PageResources = PageData;
        errorHVCenterBoldLabelStyle.PageResources = PageData;
        errorHStartVCenterBoldLabelStyle.PageResources = PageData;
        errorHEndVCenterBoldLabelStyle.PageResources = PageData;

        _htmlPrimaryLabel.Value = _htmlPrimaryCenterLabel.Value = _htmlSecondaryLabel.Value = _htmlSecondaryCenterLabel.Value =
           _htmlTertiaryLabel.Value = _htmlTertiaryCenterLabel.Value =htmlWhiteLabelControl.Value = 
           htmlWhiteCenterLabelControl.Value = "<p> <h1>HtmlLabel</h1> <nbsp> <i>HtmlLabel</i> <nbsp> <b>HtmlLabel</b> <nbsp> <u>HtmlLabel</u></p>";

        AppHelper.ShowBusyIndicator = false;
    }
    protected override void OnDisappearing()
    {
        _backButton.OnValueChanged -= OnBackButtonClicked;
        base.OnDisappearing();
    }
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new ControlDemoPage()).ConfigureAwait(false);
    }

    private void AddSubView(View view)
    {
        views.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        int index = views.Children?.Count() ?? 0;
        views.Add(view, 0, index);
    }

    private void AddView(View view)
    {
        AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 1, true);
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }
}