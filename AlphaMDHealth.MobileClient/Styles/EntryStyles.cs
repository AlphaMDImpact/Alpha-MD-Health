using AlphaMDHealth.Utility;
using DevExpress.Maui.Core;
using DevExpress.Maui.Editors;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    Style defaultEditBaseStyle;

    private void CreateEntryStyle()
    {
        CreateDevExpressEntryStyle();
        //CreateXamarinEntryStyles();
    }

    private void CreateDevExpressEntryStyle()
    {
        defaultEditBaseStyle = new(typeof(EditBase))
        {
            Setters =
            {
                new Setter { Property = EditBase.ErrorColorProperty, Value = _errorColor },
                new Setter { Property = EditBase.ErrorIconColorProperty, Value = _errorColor },
                new Setter { Property = EditBase.ErrorTextProperty, Value = _errorColor },
                new Setter { Property = EditBase.LabelColorProperty, Value = _primaryTextColor },
                new Setter { Property = EditBase.FocusedLabelColorProperty, Value = _primaryAppColor },
                new Setter { Property = EditBase.DisabledLabelColorProperty, Value = _tertiaryTextColor },
                new Setter { Property = EditBase.TextColorProperty, Value = _primaryTextColor },
                new Setter { Property = EditBase.DisabledTextColorProperty, Value = _tertiaryTextColor },
                new Setter { Property = EditBase.PlaceholderColorProperty, Value = _tertiaryTextColor },
                new Setter { Property = EditBase.HelpTextColorProperty, Value = _primaryTextColor },
                new Setter { Property = EditBase.DisabledHelpTextColorProperty, Value = _tertiaryTextColor },
                new Setter { Property = EditBase.BackgroundColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = EditBase.DisabledBackgroundColorProperty, Value = _genericBackgroundColor },
                new Setter { Property = EditBase.BorderColorProperty, Value = _separatorAndDisableColor },
                new Setter { Property = EditBase.FocusedBorderColorProperty, Value = _primaryAppColor },
                new Setter { Property = EditBase.DisabledBorderColorProperty, Value = _tertiaryAppColor },
                new Setter { Property = EditBase.IconColorProperty , Value = _primaryTextColor},
                new Setter { Property = EditBase.DisabledIconColorProperty, Value = _tertiaryAppColor },
                new Setter { Property = EditBase.StartIconColorProperty, Value = _primaryTextColor },
                new Setter { Property = EditBase.EndIconColorProperty, Value = _primaryTextColor },
                new Setter { Property = EditBase.ClearIconColorProperty , Value = _primaryTextColor },
                new Setter { Property = EditBase.ClearIconVisibilityProperty , Value = IconVisibility.Auto },
                new Setter { Property = EditBase.IconVerticalAlignmentProperty , Value = LayoutAlignment.Center },
                new Setter { Property = EditBase.LabelFontSizeProperty, Value = smallLabelSize },
                new Setter { Property = EditBase.TextFontSizeProperty, Value = smallLabelSize },
                new Setter { Property = EditBase.BottomTextFontSizeProperty, Value = smallLabelSize },
                new Setter { Property = EditBase.CornerRadiusProperty, Value = _controlCornerRadius },
                new Setter { Property = EditBase.FlowDirectionProperty , Value = _appFlowDirection },

                //new Setter { Property = EditBase.HeightRequestProperty, Value = _defaultEnrtyHeight },
                //new Setter { Property = EditBase.TextHorizontalAlignmentProperty, Value = TextAlignment.Start },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_MY_ENTRY_STYLE_KEY, defaultEditBaseStyle);

        Style numericEntryStyle = new(typeof(NumericEdit))
        {
            BasedOn = defaultEditBaseStyle,
            Setters =
            {
                new Setter { Property = NumericEdit.UpIconColorProperty , Value = _primaryTextColor },
                new Setter { Property = NumericEdit.DownIconColorProperty , Value = _primaryTextColor },
                new Setter { Property = NumericEdit.AffixColorProperty , Value = _primaryTextColor },

                //new Setter { Property = NumericEdit.HeightRequestProperty, Value = _defaultEnrtyHeight },
                //new Setter { Property = NumericEdit.TextHorizontalAlignmentProperty, Value = TextAlignment.Start },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_NUMERIC_MY_ENTRY_STYLE_KEY, numericEntryStyle);
    }

    ////todo: we will remove following styles once will start using new entry styles
    //private void CreateXamarinEntryStyles()
    //{
        //Style afterLoginEntryStyle = new(typeof(CustomEntry))
        //{
        //    Setters =
        //    {
        //        new Setter {Property = CustomEntry.HeightRequestProperty, Value = _defaultControlHeight},
        //        new Setter {Property = CustomEntry.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //        new Setter {Property = CustomEntry.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //        new Setter {Property = CustomEntry.BackgroundColorProperty, Value = _genericBackgroundColor },
        //        new Setter {Property = CustomEntry.TextHorizontalAlignmentProperty, Value = TextAlignment.Start},
        //        new Setter {Property = CustomEntry.PlaceholderColorProperty, Value = _tertiaryTextColor},
        //        new Setter {Property = CustomEntry.DisabledTextColorProperty, Value = _tertiaryTextColor},//todo:
        //        new Setter {Property = CustomEntry.TextColorProperty,Value= _primaryTextColor },
        //        new Setter {Property = CustomEntry.FlowDirectionProperty , Value = DefaultFlowDirection},
        //        new Setter {Property = CustomEntry.TextFontSizeProperty, Value =  new OnIdiom<double> { Phone = smallLabelSize, Tablet =  smallLabelSize} } ,
        //        new Setter {Property = CustomEntry.MarginProperty, Value =  new Thickness(0) },
        //    }
        //};

        //Application.Current.Resources.Add(StyleConstants.ST_AFTER_LOGIN_ENTRY_STYLE_KEY, afterLoginEntryStyle);

        //Style defaultEntryStyle = new(typeof(CustomEntry))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = CustomEntry.HeightRequestProperty, Value = _defaultControlHeight },
        //        new Setter { Property = CustomEntry.TextHorizontalAlignmentProperty, Value = TextAlignment.Start },
        //        new Setter { Property = CustomEntry.PlaceholderColorProperty, Value = _tertiaryTextColor },
        //        new Setter { Property = CustomEntry.DisabledTextColorProperty, Value = _tertiaryTextColor },
        //        new Setter { Property = CustomEntry.TextFontSizeProperty, Value =  Device.GetNamedSize(NamedSize.Small, typeof(CustomEntry)) },
        //        new Setter { Property = CustomEntry.TextColorProperty,Value= _primaryTextColor },
        //        new Setter { Property = CustomEntry.FlowDirectionProperty , Value = _appFlowDirection }
        //    }
        //};
        //Style underLineEntryStyle = new(typeof(CustomEntry))
        //{
        //    BasedOn = defaultEntryStyle,
        //    Setters =
        //    {
        //        new Setter { Property = CustomEntry.HeightRequestProperty, Value = 35 },
        //        new Setter { Property = CustomEntry.BoxModeProperty, Value = BoxMode.Filled},
        //        new Setter { Property = CustomEntry.FocusedBorderColorProperty, Value =_primaryAppColor },
        //        new Setter { Property = CustomEntry.BorderColorProperty, Value =_separatorAndDisableColor },
        //        new Setter { Property = CustomEntry.CursorColorProperty, Value = _primaryAppColor },
        //        new Setter { Property = CustomEntry.MarginProperty, Value =  new Thickness(0) }
        //    }
        //};
        //Style defaultSmallEditorStyle = new(typeof(CustomMultiLineEntry))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = CustomMultiLineEntry.FontSizeProperty, Value = new OnIdiom<double> { Phone = smallLabelSize, Tablet =  smallLabelSize } },
        //        new Setter { Property = CustomMultiLineEntry.FlowDirectionProperty , Value = _appFlowDirection },
        //        new Setter { Property = CustomMultiLineEntry.TextColorProperty , Value = _primaryTextColor },
        //        new Setter { Property = CustomMultiLineEntry.PlaceholderColorProperty , Value = _tertiaryTextColor },
        //        new Setter { Property = CustomMultiLineEntry.MarginProperty , Value = new Thickness(8,0,0,0) },
        //        new Setter { Property = CustomMultiLineEntry.EntryFocusColorProperty, Value =_primaryAppColor },
        //        new Setter { Property = CustomMultiLineEntry.EntryUnFocusColorProperty, Value = _separatorAndDisableColor },
        //        new Setter { Property = CustomMultiLineEntry.BackgroundColorProperty, Value = new AppThemeBindingExtension { Light=Colors.Transparent, Dark= _genericBackgroundColor } }
        //    }
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_ENTRY_STYLE_KEY, defaultEntryStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_UNDER_LINE_ENTRY_STYLE, underLineEntryStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_SMALL_EDITOR_STYLE, defaultSmallEditorStyle);
    //}
}