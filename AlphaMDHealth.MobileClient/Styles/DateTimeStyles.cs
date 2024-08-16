using AlphaMDHealth.Utility;
using DevExpress.Maui.Editors;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateDateTimeStyle()
    {
        CreateNewDateTimeStyle();
        CreateOldDateTimeStyle();
    }

    private void CreateNewDateTimeStyle()
    {
        //CalendarHeaderAppearance pickerHeaderAppearance = new CalendarHeaderAppearance
        //{
        //    BackgroundColor = _primaryAppColor,
        //    HeaderTitleTextColor = _controlBackgroundColor,
        //    HeaderSubtitleTextColor = _controlBackgroundColor,
        //};

        //CalendarDayCellAppearance dayCellAppearance = new CalendarDayCellAppearance
        //{
        //    TodayEllipseBackgroundColor = _secondaryAppColor,
        //    SelectedEllipseBackgroundColor = _primaryAppColor,
        //    TextColor = _primaryTextColor,
        //    TodayTextColor = _controlBackgroundColor,
        //    SelectedTextColor = _controlBackgroundColor,
        //    DisabledTextColor = _tertiaryTextColor,
        //};

        Style defaultDateStyle = new Style(typeof(DateEdit))
        {
            BasedOn = defaultEditBaseStyle,
            Setters =
            {
                new Setter { Property = DateEdit.DateIconColorProperty, Value = _primaryTextColor },
                new Setter { Property = DateEdit.PickerBackgroundColorProperty, Value = _tertiaryAppColor },
                //new Setter { Property = DateEdit.PickerHeaderAppearanceProperty, Value = pickerHeaderAppearance },
                //new Setter { Property = DateEdit.PickerDayCellAppearanceProperty, Value = dayCellAppearance },
                //new Setter { Property = DateEdit.PickerButtonAreaTemplateProperty, Value = _tertiaryAppColor },
                //new Setter { Property = DateEdit.CursorColorProperty, Value = _tertiaryAppColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_DATE_STYLE, defaultDateStyle);


        Style defaultTimeStyle = new Style(typeof(TimeEdit))
        {
            BasedOn = defaultEditBaseStyle,
            Setters =
            {
                new Setter { Property = TimeEdit.TimeIconColorProperty, Value = _primaryTextColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_TIME_STYLE, defaultTimeStyle);

    }

    private void CreateOldDateTimeStyle()
    {
        //Style leftTimePickerStyle = new Style(typeof(CustomTimePicker))
        //{
        //    Setters =
        //    {
        //        new Setter {Property = CustomTimePicker.HeightRequestProperty, Value = _defaultControlHeight},
        //        new Setter {Property = CustomTimePicker.FontEnabledColorProperty , Value = _primaryTextColor },
        //        new Setter {Property = CustomTimePicker.TextColorProperty , Value = _primaryTextColor },
        //        new Setter {Property = CustomTimePicker.FontDisabledColorProperty , Value = _tertiaryTextColor},
        //        new Setter {Property = CustomTimePicker.HorizontalOptionsProperty,Value=LayoutOptions.StartAndExpand },
        //        new Setter {Property = CustomTimePicker.VerticalOptionsProperty ,Value= LayoutOptions.CenterAndExpand },
        //        new Setter {Property = CustomTimePicker.FlowDirectionProperty , Value = DefaultFlowDirection},
        //        new Setter {Property = CustomTimePicker.BackgroundColorProperty , Value = _genericBackgroundColor},
        //        new Setter {Property = CustomTimePicker.BorderColorProperty ,Value = _separatorAndDisableColor},
        //        new Setter {Property = CustomTimePicker.FontSizeProperty, Value= new OnIdiom<double> { Phone = smallLabelSize, Tablet =  smallLabelSize }   },
        //        new Setter {Property = CustomTimePicker.TextAllignmentProperty, Value= TextAlignment.Start},
        //        new Setter {Property = CustomTimePicker.HasUnderlineProperty, Value = true},
        //    }
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_LEFT_TIME_PICKER_STYLE, leftTimePickerStyle);

        //Style timePickerStyle = new(typeof(CustomTimePicker))
        //{
        //    Setters =
        //    {
        //        new Setter {Property = CustomTimePicker.HeightRequestProperty, Value = 35},
        //        new Setter {Property = CustomTimePicker.BorderColorProperty , Value = _separatorAndDisableColor },
        //        new Setter {Property = CustomTimePicker.FontEnabledColorProperty , Value = _primaryTextColor },
        //        new Setter {Property = CustomTimePicker.TextColorProperty , Value = _primaryTextColor },
        //        new Setter {Property = CustomTimePicker.FontDisabledColorProperty , Value = _tertiaryTextColor},
        //        new Setter {Property = CustomTimePicker.HorizontalOptionsProperty,Value=LayoutOptions.FillAndExpand },
        //        new Setter {Property = CustomTimePicker.VerticalOptionsProperty ,Value= LayoutOptions.CenterAndExpand },
        //        new Setter {Property = CustomTimePicker.FlowDirectionProperty , Value = _appFlowDirection},
        //        new Setter {Property = CustomTimePicker.BackgroundColorProperty , Value = _genericBackgroundColor},
        //        new Setter {Property = CustomTimePicker.FontSizeProperty, Value=new OnIdiom<double> { Phone = smallLabelSize, Tablet =  smallLabelSize}   },
        //        new Setter {Property = CustomTimePicker.TextAllignmentProperty, Value= TextAlignment.End},
        //    }
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_TIME_PICKER_STYLE, timePickerStyle);

        //Style defaultDatePickerStyle = new(typeof(CustomDatePicker))
        //{
        //    Setters =
        //    {
        //        new Setter { Property = CustomDatePicker.HeightRequestProperty, Value = 35 },
        //        new Setter { Property = CustomDatePicker.FontEnabledColorProperty, Value = _primaryTextColor },
        //        new Setter { Property = CustomDatePicker.FontDisabledColorProperty, Value = _tertiaryTextColor },
        //        new Setter { Property = CustomDatePicker.FlowDirectionProperty , Value = _appFlowDirection },
        //        new Setter { Property = CustomDatePicker.VerticalOptionsProperty , Value = LayoutOptions.FillAndExpand },
        //        new Setter { Property = CustomDatePicker.HorizontalOptionsProperty,Value=LayoutOptions.FillAndExpand },
        //        new Setter { Property = CustomDatePicker.TextColorProperty , Value = _primaryTextColor },
        //        new Setter { Property = CustomDatePicker.PlaceholderColorProperty , Value = _tertiaryTextColor },
        //        new Setter { Property = CustomDatePicker.BackgroundColorProperty , Value = _genericBackgroundColor },
        //        new Setter { Property = CustomDatePicker.BorderColorProperty , Value = _separatorAndDisableColor },
        //        new Setter { Property = CustomDatePicker.HasUnderlineProperty, Value = true },
        //        new Setter { Property = CustomDatePicker.FontSizeProperty, Value =new OnIdiom<double> { Phone = smallLabelSize, Tablet =  smallLabelSize } },
        //    }
        //};
        //Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_DATE_PICKER_STYLE, defaultDatePickerStyle);

    }
}