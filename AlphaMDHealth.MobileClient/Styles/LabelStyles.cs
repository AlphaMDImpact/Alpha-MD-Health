using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls;
using Device = Microsoft.Maui.Controls.Device;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Represents AppStyles
/// </summary>
public partial class AppStyles
{
    private void CreateLabelStyle()
    {
        CreateNewLabelStyle();
        CreateOldLabelStyles();
    }

    private void CreateNewLabelStyle()
    {
        var largeTextSizeSetter = new Setter { Property = Label.FontSizeProperty, Value = largeLabelSize };
        var mediumTextSizeSetter = new Setter { Property = Label.FontSizeProperty, Value = mediumLabelSize };
        var smallTextSizeSetter = new Setter { Property = Label.FontSizeProperty, Value = smallLabelSize };
        var microTextSizeSetter = new Setter { Property = Label.FontSizeProperty, Value = microLabelSize };
        var fontBoldSetter = new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold };

        var pTextColorSetter = new Setter { Property = Label.TextColorProperty, Value = _primaryTextColor };
        var sTextColorSetter = new Setter { Property = Label.TextColorProperty, Value = _secondaryTextColor };
        var tTextColorSetter = new Setter { Property = Label.TextColorProperty, Value = _tertiaryTextColor };
        var lightTextColorSetter = new Setter { Property = Label.TextColorProperty, Value = _genericBackgroundColor };
        var pAppTextColorSetter = new Setter { Property = Label.TextColorProperty, Value = _primaryAppColor };
        var errorTextColorSetter = new Setter { Property = Label.TextColorProperty, Value = _errorColor };

        var htmlLabelSetter = new Setter { Property = Label.TextTypeProperty, Value = TextType.Html };
        var underlineTextDecorationSetter = new Setter { Property = Label.TextDecorationsProperty, Value = TextDecorations.Underline };

        #region Default Label Styles
        Style defaultLabelStyle = new Style(typeof(Label))
        {
            Setters =
            {
                new Setter { Property = Label.FlowDirectionProperty , Value = _appFlowDirection },
                new Setter { Property = Label.LineBreakModeProperty , Value = LineBreakMode.WordWrap },
            }
        };

        Style HVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = defaultLabelStyle,
            Setters =
            {
                new Setter { Property = Label.VerticalOptionsProperty , Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
            }
        };

        Style HStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = defaultLabelStyle,
            Setters =
            {
                new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.Start },
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start },
                new Setter { Property = Label.VerticalOptionsProperty , Value = LayoutOptions.Center },
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
            }
        };

        Style HEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = defaultLabelStyle,
            Setters =
            {
                new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.End },
                new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.End },
                new Setter { Property = Label.VerticalOptionsProperty , Value = LayoutOptions.Center },
                new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
            }
        };
        #endregion

        #region Primary
        #region LargeLabelSize
        Style primaryLargeHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { pTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_LARGE_HV_CENTER_LABEL_STYLE, primaryLargeHVCenterLabelStyle);

        Style primaryLargeHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { pTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_LARGE_H_START_V_CENTER_LABEL_STYLE, primaryLargeHStartVCenterLabelStyle);

        Style primaryLargeHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { pTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_LARGE_H_END_V_CENTER_LABEL_STYLE, primaryLargeHEndVCenterLabelStyle);

        Style primaryLargeHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryLargeHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_LARGE_HV_CENTER_BOLD_LABEL_STYLE, primaryLargeHVCenterBoldLabelStyle);

        Style primaryLargeHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryLargeHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE, primaryLargeHStartVCenterBoldLabelStyle);

        Style primaryLargeHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryLargeHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE, primaryLargeHEndVCenterBoldLabelStyle);

        Style cardLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryLargeHStartVCenterLabelStyle,
            Setters = 
            { 
               fontBoldSetter ,
               new Setter { Property = Label.FontSizeProperty , Value = 40 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_CARD_LABEL_STYLE, cardLabelStyle);
        #endregion

        #region MediumLabelStyle
        Style primaryMediumHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { pTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MEDIUM_HV_CENTER_LABEL_STYLE, primaryMediumHVCenterLabelStyle);

        Style primaryMediumHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { pTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MEDIUM_H_START_V_CENTER_LABEL_STYLE, primaryMediumHStartVCenterLabelStyle);

        Style primaryMediumHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { pTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MEDIUM_H_END_V_CENTER_LABEL_STYLE, primaryMediumHEndVCenterLabelStyle);

        Style primaryMediumHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryMediumHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE, primaryMediumHVCenterBoldLabelStyle);

        Style primaryMediumHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryMediumHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE, primaryMediumHStartVCenterBoldLabelStyle);

        Style primaryMediumHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryMediumHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE, primaryMediumHEndVCenterBoldLabelStyle);
        #endregion

        #region SmallLabelStyle
        Style primarySmallHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { pTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_SMALL_HV_CENTER_LABEL_STYLE, primarySmallHVCenterLabelStyle);

        Style primarySmallHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { pTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_SMALL_H_START_V_CENTER_LABEL_STYLE, primarySmallHStartVCenterLabelStyle);

        Style primarySmallHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { pTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_SMALL_H_END_V_CENTER_LABEL_STYLE, primarySmallHEndVCenterLabelStyle);

        Style primarySmallHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primarySmallHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_SMALL_HV_CENTER_BOLD_LABEL_STYLE, primarySmallHVCenterBoldLabelStyle);

        Style primarySmallHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primarySmallHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE, primarySmallHStartVCenterBoldLabelStyle);

        Style primarySmallHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primarySmallHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE, primarySmallHEndVCenterBoldLabelStyle);


        #endregion

        #region MicroLabelSize
        Style primaryMicroHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { pTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MICRO_HV_CENTER_LABEL_STYLE, primaryMicroHVCenterLabelStyle);

        Style primaryMicroHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { pTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MICRO_H_START_V_CENTER_LABEL_STYLE, primaryMicroHStartVCenterLabelStyle);

        Style primaryMicroHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { pTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MICRO_H_END_V_CENTER_LABEL_STYLE, primaryMicroHEndVCenterLabelStyle);

        Style primaryMicroHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryMicroHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MICRO_HV_CENTER_BOLD_LABEL_STYLE, primaryMicroHVCenterBoldLabelStyle);

        Style primaryMicroHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryMicroHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE, primaryMicroHStartVCenterBoldLabelStyle);

        Style primaryMicroHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryMicroHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE, primaryMicroHEndVCenterBoldLabelStyle);

        #endregion
        #endregion

        #region Secondary 
        #region LargeLabelSize
        Style secondaryLargeHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { sTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_LARGE_HV_CENTER_LABEL_STYLE, secondaryLargeHVCenterLabelStyle);

        Style secondaryLargeHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { sTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_LARGE_H_START_V_CENTER_LABEL_STYLE, secondaryLargeHStartVCenterLabelStyle);

        Style secondaryLargeHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { sTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_LARGE_H_END_V_CENTER_LABEL_STYLE, secondaryLargeHEndVCenterLabelStyle);

        Style secondaryLargeHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondaryLargeHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_LARGE_HV_CENTER_BOLD_LABEL_STYLE, secondaryLargeHVCenterBoldLabelStyle);

        Style secondaryLargeHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondaryLargeHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE, secondaryLargeHStartVCenterBoldLabelStyle);

        Style secondaryLargeHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondaryLargeHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE, secondaryLargeHEndVCenterBoldLabelStyle);

        #endregion

        #region MediumLabelStyle
        Style secondaryMediumHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { sTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MEDIUM_HV_CENTER_LABEL_STYLE, secondaryMediumHVCenterLabelStyle);

        Style secondaryMediumHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { sTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MEDIUM_H_START_V_CENTER_LABEL_STYLE, secondaryMediumHStartVCenterLabelStyle);

        Style secondaryMediumHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { sTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MEDIUM_H_END_V_CENTER_LABEL_STYLE, secondaryMediumHEndVCenterLabelStyle);

        Style secondaryMediumHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondaryMediumHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE, secondaryMediumHVCenterBoldLabelStyle);

        Style secondaryMediumHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondaryMediumHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE, secondaryMediumHStartVCenterBoldLabelStyle);

        Style secondaryMediumHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondaryMediumHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE, secondaryMediumHEndVCenterBoldLabelStyle);
        #endregion

        #region SmallLabelStyle
        Style secondarySmallHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { sTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_SMALL_HV_CENTER_LABEL_STYLE, secondarySmallHVCenterLabelStyle);

        Style secondarySmallHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { sTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_SMALL_H_START_V_CENTER_LABEL_STYLE, secondarySmallHStartVCenterLabelStyle);

        Style secondarySmallHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { sTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_SMALL_H_END_V_CENTER_LABEL_STYLE, secondarySmallHEndVCenterLabelStyle);

        Style secondarySmallHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondarySmallHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_SMALL_HV_CENTER_BOLD_LABEL_STYLE, secondarySmallHVCenterBoldLabelStyle);

        Style secondarySmallHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondarySmallHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE, secondarySmallHStartVCenterBoldLabelStyle);

        Style secondarySmallHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondarySmallHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE, secondarySmallHEndVCenterBoldLabelStyle);


        #endregion

        #region MicroLabelSize
        Style secondaryMicroHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { sTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MICRO_HV_CENTER_LABEL_STYLE, secondaryMicroHVCenterLabelStyle);

        Style secondaryMicroHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { sTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MICRO_H_START_V_CENTER_LABEL_STYLE, secondaryMicroHStartVCenterLabelStyle);

        Style secondaryMicroHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { sTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MICRO_H_END_V_CENTER_LABEL_STYLE, secondaryMicroHEndVCenterLabelStyle);

        Style secondaryMicroHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondaryMicroHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MICRO_HV_CENTER_BOLD_LABEL_STYLE, secondaryMicroHVCenterBoldLabelStyle);

        Style secondaryMicroHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondaryMicroHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE, secondaryMicroHStartVCenterBoldLabelStyle);

        Style secondaryMicroHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = secondaryMicroHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SECONDARY_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE, secondaryMicroHEndVCenterBoldLabelStyle);


        #endregion

        #endregion

        #region Tertiary
        #region LargeLabelSize
        Style tertiaryLargeHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { tTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_LARGE_HV_CENTER_LABEL_STYLE, tertiaryLargeHVCenterLabelStyle);

        Style tertiaryLargeHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { tTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_LARGE_H_START_V_CENTER_LABEL_STYLE, tertiaryLargeHStartVCenterLabelStyle);

        Style tertiaryLargeHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { tTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_LARGE_H_END_V_CENTER_LABEL_STYLE, tertiaryLargeHEndVCenterLabelStyle);

        Style tertiaryLargeHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiaryLargeHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_LARGE_HV_CENTER_BOLD_LABEL_STYLE, tertiaryLargeHVCenterBoldLabelStyle);

        Style tertiaryLargeHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiaryLargeHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE, tertiaryLargeHStartVCenterBoldLabelStyle);

        Style tertiaryLargeHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiaryLargeHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE, tertiaryLargeHEndVCenterBoldLabelStyle);

        #endregion

        #region MediumLabelStyle
        Style tertiaryMediumHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { tTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MEDIUM_HV_CENTER_LABEL_STYLE, tertiaryMediumHVCenterLabelStyle);

        Style tertiaryMediumHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { tTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MEDIUM_H_START_V_CENTER_LABEL_STYLE, tertiaryMediumHStartVCenterLabelStyle);

        Style tertiaryMediumHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { tTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MEDIUM_H_END_V_CENTER_LABEL_STYLE, tertiaryMediumHEndVCenterLabelStyle);

        Style tertiaryMediumHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiaryMediumHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE, tertiaryMediumHVCenterBoldLabelStyle);

        Style tertiaryMediumHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiaryMediumHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE, tertiaryMediumHStartVCenterBoldLabelStyle);

        Style tertiaryMediumHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiaryMediumHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE, tertiaryMediumHEndVCenterBoldLabelStyle);
        #endregion

        #region SmallLabelStyle
        Style tertiarySmallHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { tTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_SMALL_HV_CENTER_LABEL_STYLE, tertiarySmallHVCenterLabelStyle);

        Style tertiarySmallHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { tTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_SMALL_H_START_V_CENTER_LABEL_STYLE, tertiarySmallHStartVCenterLabelStyle);

        Style tertiarySmallHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { tTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_SMALL_H_END_V_CENTER_LABEL_STYLE, tertiarySmallHEndVCenterLabelStyle);

        Style tertiarySmallHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiarySmallHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_SMALL_HV_CENTER_BOLD_LABEL_STYLE, tertiarySmallHVCenterBoldLabelStyle);

        Style tertiarySmallHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiarySmallHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE, tertiarySmallHStartVCenterBoldLabelStyle);

        Style tertiarySmallHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiarySmallHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE, tertiarySmallHEndVCenterBoldLabelStyle);


        #endregion

        #region MicroLabelSize
        Style tertiaryMicroHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { tTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MICRO_HV_CENTER_LABEL_STYLE, tertiaryMicroHVCenterLabelStyle);

        Style tertiaryMicroHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { tTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MICRO_H_START_V_CENTER_LABEL_STYLE, tertiaryMicroHStartVCenterLabelStyle);

        Style tertiaryMicroHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { tTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MICRO_H_END_V_CENTER_LABEL_STYLE, tertiaryMicroHEndVCenterLabelStyle);

        Style tertiaryMicroHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiaryMicroHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MICRO_HV_CENTER_BOLD_LABEL_STYLE, tertiaryMicroHVCenterBoldLabelStyle);

        Style tertiaryMicroHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiaryMicroHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE, tertiaryMicroHStartVCenterBoldLabelStyle);

        Style tertiaryMicroHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = tertiaryMicroHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_TERTIARY_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE, tertiaryMicroHEndVCenterBoldLabelStyle);


        #endregion

        #endregion

        #region PrimaryApp
        #region LargeLabelSize
        Style primaryAppLargeHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { pAppTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_LARGE_HV_CENTER_LABEL_STYLE, primaryAppLargeHVCenterLabelStyle);

        Style primaryAppLargeHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { pAppTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_LARGE_H_START_V_CENTER_LABEL_STYLE, primaryAppLargeHStartVCenterLabelStyle);

        Style primaryAppLargeHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { pAppTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_LARGE_H_END_V_CENTER_LABEL_STYLE, primaryAppLargeHEndVCenterLabelStyle);

        Style primaryAppLargeHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppLargeHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_LARGE_HV_CENTER_BOLD_LABEL_STYLE, primaryAppLargeHVCenterBoldLabelStyle);

        Style primaryAppLargeHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppLargeHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE, primaryAppLargeHStartVCenterBoldLabelStyle);

        Style primaryAppLargeHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppLargeHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE, primaryAppLargeHEndVCenterBoldLabelStyle);

        #endregion

        #region MediumLabelStyle
        Style primaryAppMediumHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { pAppTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MEDIUM_HV_CENTER_LABEL_STYLE, primaryAppMediumHVCenterLabelStyle);

        Style primaryAppMediumHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { pAppTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MEDIUM_H_START_V_CENTER_LABEL_STYLE, primaryAppMediumHStartVCenterLabelStyle);

        Style primaryAppMediumHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { pAppTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MEDIUM_H_END_V_CENTER_LABEL_STYLE, primaryAppMediumHEndVCenterLabelStyle);

        Style primaryAppMediumHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppMediumHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE, primaryAppMediumHVCenterBoldLabelStyle);

        Style primaryAppMediumHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppMediumHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE, primaryAppMediumHStartVCenterBoldLabelStyle);

        Style primaryAppMediumHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppMediumHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE, primaryAppMediumHEndVCenterBoldLabelStyle);
        #endregion

        #region SmallLabelStyle
        Style primaryAppSmallHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { pAppTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_SMALL_HV_CENTER_LABEL_STYLE, primaryAppSmallHVCenterLabelStyle);

        Style primaryAppSmallHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { pAppTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_SMALL_H_START_V_CENTER_LABEL_STYLE, primaryAppSmallHStartVCenterLabelStyle);

        Style primaryAppSmallHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { pAppTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_SMALL_H_END_V_CENTER_LABEL_STYLE, primaryAppSmallHEndVCenterLabelStyle);

        Style primaryAppSmallHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppSmallHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_SMALL_HV_CENTER_BOLD_LABEL_STYLE, primaryAppSmallHVCenterBoldLabelStyle);

        Style primaryAppSmallHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppSmallHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE, primaryAppSmallHStartVCenterBoldLabelStyle);

        Style primaryAppSmallHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppSmallHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE, primaryAppSmallHEndVCenterBoldLabelStyle);


        #endregion

        #region MicroLabelSize
        Style primaryAppMicroHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { pAppTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MICRO_HV_CENTER_LABEL_STYLE, primaryAppMicroHVCenterLabelStyle);

        Style primaryAppMicroHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { pAppTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MICRO_H_START_V_CENTER_LABEL_STYLE, primaryAppMicroHStartVCenterLabelStyle);

        Style primaryAppMicroHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { pAppTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MICRO_H_END_V_CENTER_LABEL_STYLE, primaryAppMicroHEndVCenterLabelStyle);

        Style primaryAppMicroHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppMicroHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MICRO_HV_CENTER_BOLD_LABEL_STYLE, primaryAppMicroHVCenterBoldLabelStyle);

        Style primaryAppMicroHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppMicroHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE, primaryAppMicroHStartVCenterBoldLabelStyle);

        Style primaryAppMicroHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = primaryAppMicroHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE, primaryAppMicroHEndVCenterBoldLabelStyle);
        #endregion

        #endregion

        #region ErrorLabelStyles
        Style errorHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { errorTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_ERROR_HV_CENTER_LABEL_STYLE, errorHVCenterLabelStyle);

        Style errorHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { errorTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_ERROR_H_START_V_CENTER_LABEL_STYLE, errorHStartVCenterLabelStyle);

        Style errorHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { errorTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_ERROR_H_END_V_CENTER_LABEL_STYLE, errorHEndVCenterLabelStyle);

        Style errorHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = errorHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_ERROR_HV_CENTER_BOLD_LABEL_STYLE, errorHVCenterBoldLabelStyle);

        Style errorHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = errorHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_ERROR_H_START_V_CENTER_BOLD_LABEL_STYLE, errorHStartVCenterBoldLabelStyle);

        Style errorHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = errorHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_ERROR_H_END_V_CENTER_BOLD_LABEL_STYLE, errorHEndVCenterBoldLabelStyle);
        #endregion

        #region LinkLabelStyles
        Style linkHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { underlineTextDecorationSetter, pAppTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LINK_HV_CENTER_LABEL_STYLE, linkHVCenterLabelStyle);

        Style linkHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { underlineTextDecorationSetter, pAppTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LINK_H_START_V_CENTER_LABEL_STYLE, linkHStartVCenterLabelStyle);

        Style linkHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { underlineTextDecorationSetter, pAppTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LINK_H_END_V_CENTER_LABEL_STYLE, linkHEndVCenterLabelStyle);

        Style linkHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = linkHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LINK_HV_CENTER_BOLD_LABEL_STYLE, linkHVCenterBoldLabelStyle);

        Style linkHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = linkHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LINK_H_START_V_CENTER_BOLD_LABEL_STYLE, linkHStartVCenterBoldLabelStyle);

        Style linkHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = linkHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LINK_H_END_V_CENTER_BOLD_LABEL_STYLE, linkHEndVCenterBoldLabelStyle);
        #endregion
    
        #region InitialImageStyle
        Style initialImageStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { lightTextColorSetter, fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_INITIAL_IMAGE_STYLE, initialImageStyle);
        #endregion

        #region LightLabelStyles

        #region LargeLabelSize
        Style lightLargeHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { lightTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_LARGE_HV_CENTER_LABEL_STYLE, lightLargeHVCenterLabelStyle);

        Style lightLargeHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { lightTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_LARGE_H_START_V_CENTER_LABEL_STYLE, lightLargeHStartVCenterLabelStyle);

        Style lightLargeHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { lightTextColorSetter, largeTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_LARGE_H_END_V_CENTER_LABEL_STYLE, lightLargeHEndVCenterLabelStyle);

        Style lightLargeHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightLargeHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_LARGE_HV_CENTER_BOLD_LABEL_STYLE, lightLargeHVCenterBoldLabelStyle);

        Style lightLargeHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightLargeHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_LARGE_H_START_V_CENTER_BOLD_LABEL_STYLE, lightLargeHStartVCenterBoldLabelStyle);

        Style lightLargeHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightLargeHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_LARGE_H_END_V_CENTER_BOLD_LABEL_STYLE, lightLargeHEndVCenterBoldLabelStyle);

        #endregion

        #region MediumLabelStyle
        Style lightMediumHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { lightTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MEDIUM_HV_CENTER_LABEL_STYLE, lightMediumHVCenterLabelStyle);

        Style lightMediumHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { lightTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MEDIUM_H_START_V_CENTER_LABEL_STYLE, lightMediumHStartVCenterLabelStyle);

        Style lightMediumHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { lightTextColorSetter, mediumTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MEDIUM_H_END_V_CENTER_LABEL_STYLE, lightMediumHEndVCenterLabelStyle);

        Style lightMediumHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightMediumHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MEDIUM_HV_CENTER_BOLD_LABEL_STYLE, lightMediumHVCenterBoldLabelStyle);

        Style lightMediumHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightMediumHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MEDIUM_H_START_V_CENTER_BOLD_LABEL_STYLE, lightMediumHStartVCenterBoldLabelStyle);

        Style lightMediumHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightMediumHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MEDIUM_H_END_V_CENTER_BOLD_LABEL_STYLE, lightMediumHEndVCenterBoldLabelStyle);
        #endregion

        #region SmallLabelStyle
        Style lightSmallHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { lightTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_SMALL_HV_CENTER_LABEL_STYLE, lightSmallHVCenterLabelStyle);

        Style lightSmallHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { lightTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_SMALL_H_START_V_CENTER_LABEL_STYLE, lightSmallHStartVCenterLabelStyle);

        Style lightSmallHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { lightTextColorSetter, smallTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_SMALL_H_END_V_CENTER_LABEL_STYLE, lightSmallHEndVCenterLabelStyle);

        Style lightSmallHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightSmallHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_SMALL_HV_CENTER_BOLD_LABEL_STYLE, lightSmallHVCenterBoldLabelStyle);

        Style lightSmallHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightSmallHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_SMALL_H_START_V_CENTER_BOLD_LABEL_STYLE, lightSmallHStartVCenterBoldLabelStyle);

        Style lightSmallHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightSmallHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_SMALL_H_END_V_CENTER_BOLD_LABEL_STYLE, lightSmallHEndVCenterBoldLabelStyle);


        #endregion

        #region MicroLabelSize
        Style lightMicroHVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { lightTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MICRO_HV_CENTER_LABEL_STYLE, lightMicroHVCenterLabelStyle);

        Style lightMicroHStartVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HStartVCenterLabelStyle,
            Setters = { lightTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MICRO_H_START_V_CENTER_LABEL_STYLE, lightMicroHStartVCenterLabelStyle);

        Style lightMicroHEndVCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HEndVCenterLabelStyle,
            Setters = { lightTextColorSetter, microTextSizeSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MICRO_H_END_V_CENTER_LABEL_STYLE, lightMicroHEndVCenterLabelStyle);

        Style lightMicroHVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightMicroHVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MICRO_HV_CENTER_BOLD_LABEL_STYLE, lightMicroHVCenterBoldLabelStyle);

        Style lightMicroHStartVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightMicroHStartVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MICRO_H_START_V_CENTER_BOLD_LABEL_STYLE, lightMicroHStartVCenterBoldLabelStyle);

        Style lightMicroHEndVCenterBoldLabelStyle = new(typeof(Label))
        {
            BasedOn = lightMicroHEndVCenterLabelStyle,
            Setters = { fontBoldSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIGHT_MICRO_H_END_V_CENTER_BOLD_LABEL_STYLE, lightMicroHEndVCenterBoldLabelStyle);


        #endregion

        #endregion

        #region HtmlLabelStyles

        Style htmlPrimaryLabelStyle = new(typeof(Label))
        {
            BasedOn = defaultLabelStyle,
            Setters = { htmlLabelSetter, pTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_HTML_PRIMARY_LABEL_STYLE, htmlPrimaryLabelStyle);

        Style htmlPrimaryCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { htmlLabelSetter, pTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_HTML_PRIMARY_CENTER_LABEL_STYLE, htmlPrimaryCenterLabelStyle);

        Style htmlSecondaryLabelStyle = new(typeof(Label))
        {
            BasedOn = defaultLabelStyle,
            Setters = { htmlLabelSetter, sTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_HTML_SECONDARY_LABEL_STYLE, htmlSecondaryLabelStyle);

        Style htmlSecondaryCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { htmlLabelSetter , sTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_HTML_SECONDARY_CENTER_LABEL_STYLE, htmlSecondaryCenterLabelStyle);

        Style htmlTertiaryLabelStyle = new(typeof(Label))
        {
            BasedOn = defaultLabelStyle,
            Setters = { htmlLabelSetter, tTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_HTML_TERTIARY_LABEL_STYLE, htmlTertiaryLabelStyle);

        Style htmlTertiaryCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { htmlLabelSetter, tTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_HTML_TERTIARY_CENTER_LABEL_STYLE, htmlTertiaryCenterLabelStyle);

        Style htmlLightLabelStyle = new(typeof(Label))
        {
            Setters = { htmlLabelSetter, lightTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_HTML_LIGHT_LABEL_STYLE, htmlLightLabelStyle);

        Style htmlLightCenterLabelStyle = new(typeof(Label))
        {
            BasedOn = HVCenterLabelStyle,
            Setters = { htmlLabelSetter, lightTextColorSetter }
        };
        Application.Current.Resources.Add(StyleConstants.ST_HTML_LIGHT_CENTER_LABEL_STYLE, htmlLightCenterLabelStyle);
        #endregion

    }

    private void CreateOldLabelStyles()
    {
        //Style defaultSmallLabelStyle = new Style(typeof(CustomLabel))
        //{
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value = new OnIdiom<double> { Phone = smallLabelSize, Tablet =  smallLabelSize }      },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.NoWrap },
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.FlowDirectionProperty , Value = _appFlowDirection},
        //}
        //};

        //var defaultMediumLabelStyle = new Style(typeof(CustomLabel))
        //{
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value = new OnIdiom<double> { Phone = mediumLabelSize, Tablet =  mediumLabelSize }     },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.NoWrap },
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.FlowDirectionProperty , Value = _appFlowDirection},
        //}
        //};

        //var defaultLeftLabelStyle = new Style(typeof(Label))
        //{
        //    Setters =
        //{
        //    new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap },
        //    new Setter { Property = Label.VerticalOptionsProperty , Value = LayoutOptions.End},
        //    new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start },
        //    new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start },
        //    new Setter { Property = Label.FlowDirectionProperty , Value = _appFlowDirection},
        //}
        //};
        //_defaultExtraSmallLabelStyle = new Style(typeof(CustomLabel))
        //{
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value = GenericMethods.GetPlatformSpecificValue(microLabelSize, microLabelSize, 0) },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.NoWrap },
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.FlowDirectionProperty , Value = _appFlowDirection},
        //}
        //};
        //Style formattedLabelSecondaryColorSmallStyle = new(typeof(Label))
        //{
        //    Setters =
        //{
        //    new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap },
        //    new Setter { Property = Label.VerticalOptionsProperty , Value = LayoutOptions.End},
        //    new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start },
        //    new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start },
        //    new Setter { Property = Label.FlowDirectionProperty , Value = DefaultFlowDirection},
        //    new Setter { Property = Label.FontSizeProperty,  Value = new OnIdiom<double> { Phone = smallLabelSize, Tablet = smallLabelSize } },
        //    new Setter { Property = Label.TextColorProperty , Value = _secondaryTextColor }
        //}
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_FORMATTED_LABEL_SECONDARY_TEXT_COLOR_SMALL_STYLE, formattedLabelSecondaryColorSmallStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_LEFT_LABEL_STYLE, defaultLeftLabelStyle);

        //Style spanPrimaryTextColorSmallStyle = new(typeof(Span))
        //{
        //    Setters =
        //{
        //    new Setter { Property = Span.FontSizeProperty , Value = smallLabelSize},
        //    new Setter { Property = Span.TextColorProperty , Value = _secondaryTextColor},
        //}
        //};
        //Style spanPrimaryTextColorLargeStyle = new(typeof(Span))
        //{
        //    Setters =
        //{
        //    new Setter { Property = Span.FontSizeProperty , Value = largeLabelSize},
        //    new Setter { Property = Span.TextColorProperty , Value = _primaryTextColor},
        //}
        //};
        //Style labelPrimaryTextColorSmallStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.StartAndExpand },
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start },
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryTextColor},
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.TailTruncation },
        //}
        //};
        //Style labelPrimaryTextColorLargeRightStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultMediumLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value =  Device.GetNamedSize(NamedSize.Large, typeof(CustomLabel)) },
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.End },
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.End },
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryTextColor},
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.TailTruncation },
        //}
        //};
        //Style labelPrimaryTextColorMediumStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultMediumLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryTextColor},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.Start },
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start },
        //}
        //};
        //Style labelMessageTextColorMediumStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultMediumLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryAppColor},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.Start },
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start },
        //}
        //};
        //Style labelPrimaryTextColorBoldMediumStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultMediumLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryTextColor},
        //    new Setter { Property = CustomLabel.FontAttributesProperty , Value = FontAttributes.Bold },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.WordWrap },
        //}
        //};
        //Style labelSecondaryColorSmallStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.StartAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start},
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _secondaryTextColor},
        //}
        //};
        //Style secontryExtraSmallLabelStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = _defaultExtraSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _secondaryTextColor},
        //}
        //};
        //Style primaryAppExtraSmallStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = _defaultExtraSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.End},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.End},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryAppColor},
        //}
        //};
        //Style primaryAppSmallSmallStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryAppColor},
        //    new Setter { Property = CustomLabel.TextDecorationsProperty , Value = TextDecorations.Underline},
        //}
        //};
        //Style labelTertiaryColorSmallStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.StartAndExpand},
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _tertiaryTextColor},
        //}
        //};
        //Style errorlabelStyle = new(typeof(CustomLabel))
        //{
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.StartAndExpand},
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value = new OnIdiom<double> { Phone = smallLabelSize, Tablet = smallLabelSize }},
        //    new Setter { Property = CustomLabel.HeightRequestProperty, Value = new OnIdiom<double> { Phone = smallLabelSize+10, Tablet = smallLabelSize+10 }},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _errorColor},
        //    new Setter { Property = CustomLabel.PaddingProperty, Value = new Thickness(2,3)},
        //    new Setter { Property = CustomLabel.MarginProperty, Value =  new Thickness(0, 5, 0, 0)},
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.WordWrap },
        //    new Setter { Property = CustomLabel.FlowDirectionProperty , Value = _appFlowDirection},
        //}
        //};
        //Style serverErrorlabelStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = errorlabelStyle,
        //    Setters =
        //{
        //    new Setter {Property = CustomLabel.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
        //    new Setter {Property = CustomLabel.VerticalTextAlignmentProperty, Value = TextAlignment.Center},
        //    new Setter {Property = CustomLabel.HorizontalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
        //    new Setter {Property = CustomLabel.VerticalOptionsProperty, Value = LayoutOptions.StartAndExpand},
        //    new Setter {Property = CustomLabel.HeightRequestProperty, Value = -1},
        //    new Setter {Property = CustomLabel.MarginProperty, Value =  new Thickness(0)},
        //    new Setter {Property = CustomLabel.TextColorProperty, Value = _genericBackgroundColor},
        //    new Setter {Property = CustomLabel.BackgroundColorProperty, Value = _errorColor},
        //}
        //};
        //Style htmlErrorlabelStyle = new(typeof(Label))
        //{
        //    Setters =
        //{
        //    new Setter { Property = Label.TextTypeProperty, Value = TextType.Html},
        //    new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.StartAndExpand},
        //    new Setter { Property = Label.FontSizeProperty, Value = new OnIdiom<double> { Phone = smallLabelSize, Tablet = smallLabelSize }},
        //    new Setter { Property = Label.TextColorProperty , Value = _errorColor},
        //    new Setter { Property = Label.PaddingProperty, Value = new Thickness(2,3)},
        //    new Setter { Property = Label.MarginProperty, Value =  new Thickness(0, 5, 0, 0)},
        //    new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap },
        //    new Setter { Property = Label.FlowDirectionProperty , Value = _appFlowDirection},
        //}
        //};
        //Style htmlPrimarylabelStyle = new(typeof(Label))
        //{
        //    Setters =
        //{
        //    new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.CenterAndExpand},
        //    new Setter { Property = Label.HorizontalTextAlignmentProperty , Value = TextAlignment.Center},
        //    new Setter { Property = Label.VerticalOptionsProperty , Value = LayoutOptions.Fill},
        //    new Setter { Property = Label.FontSizeProperty, Value = new OnIdiom<double> { Phone = smallLabelSize, Tablet = smallLabelSize }},
        //    new Setter { Property = Label.TextColorProperty , Value = _primaryTextColor},
        //    new Setter { Property = Label.PaddingProperty, Value = new Thickness(2,3)},
        //    new Setter { Property = Label.MarginProperty, Value =  new Thickness(0, 5, 0, 0)},
        //    new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap },
        //    new Setter { Property = Label.FlowDirectionProperty , Value = _appFlowDirection},
        //}
        //};
        //Style htmlSecondrylabelStyle = new(typeof(Label))
        //{
        //    Setters =
        //{
        //    new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.StartAndExpand},
        //    new Setter { Property = Label.FontSizeProperty, Value = new OnIdiom<double> { Phone = smallLabelSize, Tablet = smallLabelSize }},
        //    new Setter { Property = Label.TextColorProperty , Value = _secondaryTextColor},
        //    new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation },
        //    new Setter { Property = Label.FlowDirectionProperty , Value = _appFlowDirection},
        //}

        //};
        //Style labelWhiteColorSmallStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _genericBackgroundColor},
        //}
        //};
        //Style labelWhiteColorExtraSmallStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value = new OnIdiom<double> { Phone = microLabelSize, Tablet = microLabelSize }},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _genericBackgroundColor},
        //}
        //};
        //Style labelWhiteColorMediumStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultMediumLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _genericBackgroundColor},
        //}
        //};
        //Style labelWhiteColorBoldLargeStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultMediumLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value =  Device.GetNamedSize(NamedSize.Large, typeof(CustomLabel)) },
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _genericBackgroundColor},
        //    new Setter { Property = CustomLabel.FontAttributesProperty , Value = FontAttributes.Bold },
        //}
        //};
        //Style noMasterHeaderStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultMediumLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontAttributesProperty , Value = FontAttributes.Bold},
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryTextColor},
        //    new Setter { Property = CustomLabel.MarginProperty , Value = new Thickness(0, 20, 0, 40)},
        //}
        //};
        //Style listHeaderStyle = new(typeof(CustomLabel))
        //{
        //    BasedOn = defaultSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.StartAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start},
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryTextColor},
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.TailTruncation },
        //}
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_LARGE_STYLE, spanPrimaryTextColorLargeStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_SPAN_PRIMARY_TEXT_COLOR_SMALL_STYLE, spanPrimaryTextColorSmallStyle);

        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_LARGE_STYLE, labelPrimaryTextColorLargeRightStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_SMALL_STYLE, labelPrimaryTextColorSmallStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_MEDIUM_STYLE, labelPrimaryTextColorMediumStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_MESSAGE_TEXT_COLOR_MEDIUM_STYLE, labelMessageTextColorMediumStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_BOLD_MEDIUM_STYLE, labelPrimaryTextColorBoldMediumStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_SECONDARY_TEXT_COLOR_SMALL_STYLE, labelSecondaryColorSmallStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_SECONTRY_EXTRA_SMALL_LABEL_STYLE, secontryExtraSmallLabelStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_TERTIARY_TEXT_COLOR_SMALL_STYLE, labelTertiaryColorSmallStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_SERVER_ERROR_LABEL_STYLE, serverErrorlabelStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_WHITE_COLOR_SMALL_STYLE, labelWhiteColorSmallStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_WHITE_COLOR_EXTRA_SMALL_STYLE, labelWhiteColorExtraSmallStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_NO_MASTER_HEADER_STYLE_KEY, noMasterHeaderStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_OLD_ERROR_LABEL_STYLE, errorlabelStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LIST_HEADER_STYLE, listHeaderStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_HTML_ERROR_LABEL_STYLE, htmlErrorlabelStyle);
        ////Application.Current.Resources.Add(StyleConstants.ST_HTML_PRIMARY_LABEL_STYLE, htmlPrimarylabelStyle);
        ////Application.Current.Resources.Add(StyleConstants.ST_HTML_SECONDRY_LABEL_STYLE, htmlSecondrylabelStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_WHITE_COLOR_MEDIUM_STYLE, labelWhiteColorMediumStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_WHITE_COLOR_BOLD_LARGE_STYLE, labelWhiteColorBoldLargeStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_COLOR_EXTRA_SMALL_STYLE, primaryAppExtraSmallStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_PRIMARY_APP_COLOR_SMALL_STYLE, primaryAppSmallSmallStyle);

        //Style defaultExtraLargeLabelStyle = new Style(typeof(CustomLabel))
        //{
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value =  new OnIdiom<double> { Phone = largeLabelSize, Tablet = largeLabelSize  } },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.NoWrap },
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.FlowDirectionProperty , Value = DefaultFlowDirection},
        //}
        //};

        //Style defaultLargeLabelStyle = new Style(typeof(CustomLabel))
        //{
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value =  new OnIdiom<double> { Phone = largeLabelSize, Tablet =  largeLabelSize }  },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.NoWrap },
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.FlowDirectionProperty , Value = DefaultFlowDirection},
        //}
        //};

        //Style defaultMicroLabelStyle = new Style(typeof(CustomLabel))
        //{
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value = new OnIdiom<double> { Phone = microLabelSize, Tablet =  microLabelSize }   },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.NoWrap },
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
        //    new Setter { Property = CustomLabel.FlowDirectionProperty , Value = DefaultFlowDirection},
        //}
        //};

        //Style labelWhiteColorLargeStyle = new Style(typeof(CustomLabel))
        //{
        //    BasedOn = defaultLargeLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _genericBackgroundColor},
        //}
        //};

        //Style labelWhiteColorExtraLargeStyle = new Style(typeof(CustomLabel))
        //{
        //    BasedOn = defaultExtraLargeLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _genericBackgroundColor},
        //}
        //};


        //Style labelPrimaryTextColorBoldSmallStyle = new Style(typeof(CustomLabel))
        //{
        //    BasedOn = defaultSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.StartAndExpand },
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start },
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryTextColor },
        //    new Setter { Property = CustomLabel.FontAttributesProperty , Value = FontAttributes.Bold },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty , Value = LineBreakMode.TailTruncation },
        //}
        //};

        //Style labelPrimaryTextColorMicroStyle = new Style(typeof(CustomLabel))
        //{
        //    BasedOn = defaultMicroLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryTextColor},
        //}
        //};


        //Style labelPrimaryAppColorMediumStyle = new Style(typeof(CustomLabel))
        //{
        //    BasedOn = defaultMediumLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryAppColor },
        //}
        //};

        //Style labelPrimaryAppColorSmallStyle = new Style(typeof(CustomLabel))
        //{
        //    BasedOn = defaultSmallLabelStyle,
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryAppColor },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty , Value = LineBreakMode.TailTruncation},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.StartAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start},
        //}
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_MEDIUM_LABEL_STYLE, defaultMediumLabelStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_WHITE_COLOR_LARGE_STYLE, labelWhiteColorLargeStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_WHITE_COLOR_EXTRA_LARGE_STYLE, labelWhiteColorExtraLargeStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_PRIMARY_APP_COLOR_MEDIUM_STYLE, labelPrimaryAppColorMediumStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_PRIMARY_APP_COLOR_SMALL_STYLE, labelPrimaryAppColorSmallStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_BOLD_SMALL_STYLE, labelPrimaryTextColorBoldSmallStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_MICRO_STYLE, labelPrimaryTextColorMicroStyle);

        //Style instructionLabelStyle = new Style(typeof(CustomLabel))
        //{
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.TextColorProperty, Value = _genericBackgroundColor},
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty, Value = LayoutOptions.StartAndExpand},
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty, Value = LayoutOptions.Start},
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.WordWrap},
        //    new Setter { Property = CustomLabel.MarginProperty, Value = new Thickness(0)},
        //    new Setter { Property = CustomLabel.PaddingProperty, Value = new Thickness(0)},
        //    new Setter { Property = CustomLabel.OpacityProperty, Value = 1},
        //    new Setter { Property = CustomLabel.FlowDirectionProperty , Value = DefaultFlowDirection},
        //}
        //};

        //Style htmlSmallLabelStyle = new Style(typeof(Label))
        //{
        //    Setters =
        //{
        //    new Setter { Property = Label.FontSizeProperty, Value =  new OnIdiom<double> { Phone = smallLabelSize, Tablet =  smallLabelSize } },
        //    new Setter { Property = Label.LineBreakModeProperty, Value = LineBreakMode.NoWrap },
        //    new Setter { Property = Label.VerticalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = Label.HorizontalOptionsProperty , Value = LayoutOptions.FillAndExpand},
        //    new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start },
        //    new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start },
        //    new Setter { Property = Label.FlowDirectionProperty , Value = DefaultFlowDirection},
        //    new Setter { Property = Label.TextColorProperty , Value = _primaryAppColor},
        //    new Setter { Property = Label.MaxLinesProperty, Value = 1000 },
        //    new Setter { Property = Label.TextTypeProperty, Value = TextType.Html },
        //}
        //};

        //Application.Current.Resources.Add(StyleConstants.ST_INSTRUCTION_LABEL_STYLE, instructionLabelStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_HTML_SMALL_LABEL_STYLE_KEY, htmlSmallLabelStyle);


        //Style dayLabelStyle = new(typeof(CustomLabel))
        //{
        //    Setters =
        //{
        //    new Setter { Property = CustomLabel.FontSizeProperty, Value = new OnIdiom<double> { Phone = microLabelSize, Tablet =  microLabelSize } },
        //    new Setter { Property = CustomLabel.LineBreakModeProperty, Value = LineBreakMode.NoWrap },
        //    new Setter { Property = CustomLabel.LineHeightProperty, Value = 0 },
        //    new Setter { Property = CustomLabel.VerticalOptionsProperty , Value = LayoutOptions.Fill },
        //    new Setter { Property = CustomLabel.VerticalTextAlignmentProperty, Value = TextAlignment.Start },
        //    new Setter { Property = CustomLabel.HorizontalOptionsProperty , Value = LayoutOptions.Start },
        //    new Setter { Property = CustomLabel.HorizontalTextAlignmentProperty , Value = TextAlignment.Start },
        //    new Setter { Property = CustomLabel.TextColorProperty , Value = _primaryTextColor },
        //    new Setter { Property = CustomLabel.FlowDirectionProperty , Value = _appFlowDirection }
        //}
        //};
        //Style defaultHTMLLabelStyle = new(typeof(Label))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = Label.FlowDirectionProperty , Value = _appFlowDirection},
        //        new Setter { Property = Label.FontSizeProperty, Value =Device.GetNamedSize(NamedSize.Small, typeof(Label))},
        //        new Setter { Property = Label.LineHeightProperty, Value =2},
        //        new Setter { Property = Label.MarginProperty, Value =new Thickness(0, GenericMethods.GetPlatformSpecificValue(-20, 0, 0), 0, Convert.ToDouble(_controlPadding, CultureInfo.CurrentCulture))}
        //    }
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_HTML_LABEL_STYLE, defaultHTMLLabelStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_DAY_LABEL_STYLE, dayLabelStyle);
    }
}