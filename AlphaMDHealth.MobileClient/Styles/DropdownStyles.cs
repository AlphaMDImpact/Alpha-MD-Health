using AlphaMDHealth.Utility;
using DevExpress.Maui.Core;
using DevExpress.Maui.Editors;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateDropdownStyle()
    {
        CreateDevExpressDropdownStyle();
        //CreateOldDropdownStyle();
    }

    private void CreateDevExpressDropdownStyle()
    {
        Style singleSelectDropdownStyle = new(typeof(ComboBoxEdit))
        {
            BasedOn = defaultEditBaseStyle,
            Setters =
            {
                new Setter { Property = ComboBoxEdit.PickerShowModeProperty , Value = DropDownShowMode.DropDown },
                new Setter { Property = ComboBoxEdit.IsFilterEnabledProperty , Value =false },
                new Setter { Property = ComboBoxEdit.DropDownBackgroundColorProperty, Value = Color.FromHex(StyleConstants.GENERIC_BACKGROUND_COLOR) },
                new Setter { Property = ComboBoxEdit.BorderColorProperty, Value = _separatorAndDisableColor},
                new Setter { Property = ComboBoxEdit.IsLabelFloatingProperty , Value = false},
                 new Setter { Property = ComboBoxEdit.DropDownSelectedItemTextColorProperty , Value = _primaryAppColor },
                new Setter { Property = ComboBoxEdit.DropDownSelectedItemBackgroundColorProperty , Value = _tertiaryAppColor },
                new Setter { Property = ComboBoxEdit.ClearIconVisibilityProperty , Value = IconVisibility.Never },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SINGLE_SELECT_DROPDOWN_STYLE, singleSelectDropdownStyle);

        Style singleSelectEditableDropdownStyle = new(typeof(ComboBoxEdit))
        {
            BasedOn = defaultEditBaseStyle,
            Setters =
            {
                new Setter { Property = ComboBoxEdit.PickerShowModeProperty , Value = DropDownShowMode.DropDown },
                new Setter { Property = ComboBoxEdit.IsFilterEnabledProperty , Value = true },
                new Setter { Property = ComboBoxEdit.BorderColorProperty, Value = _separatorAndDisableColor},
                new Setter { Property = ComboBoxEdit.DropDownBackgroundColorProperty, Value = Color.FromHex(StyleConstants.GENERIC_BACKGROUND_COLOR) },
                new Setter { Property = ComboBoxEdit.FilterConditionProperty, Value =  DataFilterCondition.Contains},
                 new Setter { Property = ComboBoxEdit.FilterTextChangedCommandProperty, Value =  "Value Not Found"},
                new Setter { Property = ComboBoxEdit.IsLabelFloatingProperty , Value = false},
                 new Setter { Property = ComboBoxEdit.DropDownSelectedItemTextColorProperty , Value = _primaryAppColor },
                new Setter { Property = ComboBoxEdit.DropDownSelectedItemBackgroundColorProperty , Value = _tertiaryAppColor },
                //new Setter { Property= ComboBoxEdit.IsReadOnlyProperty, Value = true},
                //new Setter { Property = ComboBoxEdit.LabelColorProperty , Value = _tertiaryTextColor},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SINGLE_SELECT_EDITABLE_DROPDOWN_STYLE, singleSelectEditableDropdownStyle);

        Style multiSelectDropdownStyle = new(typeof(TokenEdit))
        {
            BasedOn = defaultEditBaseStyle,
            Setters =
            {
                new Setter { Property = TokenEdit.IsFilterEnabledProperty , Value = false },
                new Setter { Property = TokenEdit.ClearIconVisibilityProperty , Value = IconVisibility.Never },
                new Setter { Property = TokenEdit.DropDownBackgroundColorProperty, Value = Color.FromHex(StyleConstants.GENERIC_BACKGROUND_COLOR) },
                new Setter { Property = TokenEdit.IsLabelFloatingProperty , Value = false},
                //new Setter { Property = ComboBoxEdit.LabelColorProperty , Value = _tertiaryTextColor},
                new Setter { Property = TokenEdit.BorderColorProperty, Value = _separatorAndDisableColor},
                 new Setter { Property = ComboBoxEdit.DropDownSelectedItemTextColorProperty , Value = _primaryAppColor },
                new Setter { Property = ComboBoxEdit.DropDownSelectedItemBackgroundColorProperty , Value = _tertiaryAppColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_MULTI_SELECT_DROPDOWN_STYLE, multiSelectDropdownStyle);

        Style multiSelectEditableDropdownStyle = new(typeof(TokenEdit))
        {
            BasedOn = defaultEditBaseStyle,
            Setters =
            {
                new Setter { Property = TokenEdit.IsFilterEnabledProperty , Value = true },
                new Setter { Property = TokenEdit.IsLabelFloatingProperty , Value = false},
                //new Setter { Property = TokenEdit.LabelColorProperty , Value = _tertiaryTextColor},
                new Setter { Property = TokenEdit.PlaceholderColorProperty, Value = _defaultBackgroundColor},
                new Setter { Property = TokenEdit.BorderColorProperty, Value = _separatorAndDisableColor},
                new Setter { Property = TokenEdit.DropDownBackgroundColorProperty, Value = Color.FromHex(StyleConstants.GENERIC_BACKGROUND_COLOR) },
                new Setter { Property = TokenEdit.FilterConditionProperty, Value =  DataFilterCondition.Contains},
                 new Setter { Property = ComboBoxEdit.DropDownSelectedItemTextColorProperty , Value = _primaryAppColor },
                new Setter { Property = ComboBoxEdit.DropDownSelectedItemBackgroundColorProperty , Value = _tertiaryAppColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_MULTI_SELECT_EDITABLE_DROPDOWN_STYLE, multiSelectEditableDropdownStyle);

        Style singleSelectWithoutBorderDropdownStyle = new(typeof(ComboBoxEdit))
        {
            BasedOn = defaultEditBaseStyle,
            Setters =
            {
                new Setter { Property = ComboBoxEdit.PickerShowModeProperty , Value = DropDownShowMode.DropDown },
                new Setter { Property = ComboBoxEdit.IsFilterEnabledProperty , Value =false },
                new Setter { Property = ComboBoxEdit.BorderThicknessProperty, Value = 0 },
                new Setter { Property = ComboBoxEdit.HorizontalOptionsProperty, Value = LayoutOptions.Center },
                new Setter { Property = ComboBoxEdit.HeightRequestProperty, Value=32 },
                new Setter { Property = ComboBoxEdit.FocusedBorderThicknessProperty, Value=0 },
                new Setter {Property = ComboBoxEdit.MarginProperty, Value= new Thickness(7,4,0,0) },
                new Setter { Property = ComboBoxEdit.ClearIconVisibilityProperty , Value = IconVisibility.Never },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_SINGLE_SELECT_WITHOUT_BORDER_DROPDOWN_STYLE, singleSelectWithoutBorderDropdownStyle);

    }

    //private void CreateOldDropdownStyle()
    //{
    //    Style bindablePickerStyle = new(typeof(CustomBindablePicker))
    //    {
    //        Setters =
    //        {
    //            new Setter { Property = CustomBindablePicker.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand },
    //            new Setter { Property = CustomBindablePicker.HorizontalOptionsProperty, Value = LayoutOptions.StartAndExpand },
    //            new Setter { Property = CustomBindablePicker.TitleColorProperty, Value=_tertiaryTextColor },
    //            new Setter { Property = CustomBindablePicker.PlaceholderColorProperty, Value=_tertiaryTextColor },
    //            new Setter { Property = CustomBindablePicker.FontEnabledColorProperty , Value = _primaryTextColor },
    //            new Setter { Property = CustomBindablePicker.FontDisabledColorProperty , Value = _primaryTextColor },
    //            new Setter { Property = CustomBindablePicker.BackgroundColorProperty , Value = _genericBackgroundColor },
    //            new Setter { Property = CustomBindablePicker.BorderColorProperty , Value = _genericBackgroundColor },
    //            new Setter { Property = CustomBindablePicker.FontSizeProperty, Value =  new OnIdiom<double> { Phone = smallLabelSize, Tablet =  smallLabelSize } },
    //            new Setter { Property = CustomBindablePicker.FlowDirectionProperty , Value = _appFlowDirection }
    //        }
    //    };
    //    Application.Current.Resources.Add(StyleConstants.ST_BINDABLE_PICKER_STYLE, bindablePickerStyle);
    //}
}
