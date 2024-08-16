using AlphaMDHealth.Utility;
using DevExpress.Maui.Editors;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Represents checkbox AppStyles
/// </summary>
public partial class AppStyles
{
    private void CreateCheckBoxStyle()
    {
        CreateDevExpressCheckboxStyle();
        //CreateXamarinCheckboxStyle();
    }

    private void CreateDevExpressCheckboxStyle()
    {
        Style defaultCheckEditStyle = new(typeof(CheckEdit))
        {
            Setters =
            {
                new Setter { Property = CheckEdit.FlowDirectionProperty , Value = DefaultFlowDirection},
                //new Setter { Property = CheckEdit.TextColorProperty, Value = _tertiaryTextColor },
                //new Setter { Property = CheckEdit.FontSizeProperty, Value = Device.GetNamedSize(NamedSize.Small, typeof(CustomCheckBox))*0.9 },
                //new Setter { Property = CheckEdit.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                //new Setter { Property = CheckEdit.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                //new Setter { Property = CheckEdit.ColorProperty, Value = _tertiaryTextColor },
                //new Setter { Property = CheckEdit.ScaleXProperty, Value = GenericMethods.GetPlatformSpecificValue(1.1, 1.1, 0)},
                //new Setter { Property = CheckEdit.ScaleYProperty, Value = GenericMethods.GetPlatformSpecificValue(1.1, 1.1, 0)},
                //new Setter { Property = CheckEdit.MarginProperty, Value = new Thickness(0,-5,0,-10)},
                
                new Setter { Property = CheckEdit.CheckBoxColorProperty , Value = _primaryTextColor },
                new Setter { Property = CheckEdit.CheckedCheckBoxColorProperty , Value = _primaryAppColor },
                new Setter { Property = CheckEdit.DisabledCheckBoxColorProperty , Value = _tertiaryTextColor },
                new Setter { Property = CheckEdit.DisabledCheckedCheckBoxColorProperty , Value = _tertiaryAppColor },
                new Setter { Property = CheckEdit.LabelColorProperty , Value = _primaryTextColor },
                //new Setter { Property = CheckEdit.CheckedLabelColorProperty , Value = _primaryAppColor }, //todo: property is not available in control
                new Setter { Property = CheckEdit.DisabledLabelColorProperty , Value = _tertiaryTextColor },
                new Setter { Property = CheckEdit.CheckBoxPositionProperty , Value = CheckBoxPosition.Start },
                new Setter { Property = CheckEdit.CheckBoxAlignmentProperty , Value = TextAlignment.Start },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_CHECK_EDIT_STYLE, defaultCheckEditStyle);
    }

    //private void CreateXamarinCheckboxStyle()
    //{
        //Style defaultCheckBoxStyle = new(typeof(CustomCheckBox))
        //{
        //    Setters =
        //    {
        //        new Setter {Property = CustomCheckBox.TextColorProperty, Value = _tertiaryTextColor },
        //        new Setter {Property = CustomCheckBox.FontSizeProperty, Value =Device.GetNamedSize(NamedSize.Small, typeof(CustomCheckBox))*0.9 },
        //        new Setter {Property = CustomCheckBox.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //        new Setter {Property = CustomCheckBox.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
        //        new Setter {Property = CustomCheckBox.FlowDirectionProperty , Value = DefaultFlowDirection},
        //        new Setter {Property = CustomCheckBox.ColorProperty, Value = _tertiaryTextColor },
        //        new Setter {Property = CustomCheckBox.ScaleXProperty, Value = GenericMethods.GetPlatformSpecificValue(1.1, 1.1, 0)},
        //        new Setter {Property = CustomCheckBox.ScaleYProperty, Value = GenericMethods.GetPlatformSpecificValue(1.1, 1.1, 0)},
        //        new Setter {Property = CustomCheckBox.MarginProperty, Value = new Thickness(0,-5,0,-10)},
        //    }
        //};
        //Style selectedCheckBoxStyle = new(typeof(CustomCheckBox))
        //{
        //    BasedOn = defaultCheckBoxStyle,
        //    Setters =
        //    {
        //        new Setter {Property = CustomCheckBox.TextColorProperty, Value = _primaryAppColor },
        //        new Setter {Property = CustomCheckBox.ColorProperty, Value = _primaryAppColor },
        //    }
        //};

        //Style disabledCheckBoxStyle = new(typeof(CustomCheckBox))
        //{
        //    BasedOn = defaultCheckBoxStyle,
        //    Setters =
        //    {
        //        new Setter {Property = CustomCheckBox.TextColorProperty, Value = _separatorAndDisableColor },
        //        new Setter {Property = CustomCheckBox.ColorProperty, Value = _separatorAndDisableColor },
        //        new Setter {Property = CustomBindablePicker.FontDisabledColorProperty, Value = _separatorAndDisableColor },
        //    }
        //};

        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_CHECKBOX_KEY, defaultCheckBoxStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_SELECTED_CHECKBOX_KEY, selectedCheckBoxStyle);
        //Application.Current.Resources.Add(StyleConstants.ST_DISABLED_CHECKBOX_KEY, disabledCheckBoxStyle);
    //}
}
