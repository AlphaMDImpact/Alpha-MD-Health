using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class UserAccountSettingViewCell : ContentView
{
    private readonly UserAccountSettingsView _userAccountSettingsView;
    public UserAccountSettingViewCell(UserAccountSettingsView view)
    {
        _userAccountSettingsView = view;
        CustomLabelControl headerLabel = new CustomLabelControl(LabelType.SecondrySmallLeft);
        headerLabel.SetBinding(CustomLabelControl.TextProperty, nameof(UserAccountSettingsModel.ReadingType));
        CustomBindablePicker pickerControl = new CustomBindablePicker
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_BINDABLE_PICKER_STYLE],
            HeightRequest = 35,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            BorderColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE],
            BorderType = "Line",
            Margin = new Thickness(GenericMethods.GetPlatformSpecificValue(0, -1, 0), 0)
        };
        pickerControl.SetBinding(CustomBindablePicker.ClassIdProperty, nameof(UserAccountSettingsModel.ReadingTypeID));
        pickerControl.SetBinding(CustomBindablePicker.ItemsSourceProperty, nameof(UserAccountSettingsModel.ReadingUnitOptions));
        pickerControl.SetBinding(CustomBindablePicker.SelectedItemProperty, nameof(UserAccountSettingsModel.SelectedReadingUnit));
        pickerControl.SelectedIndexChanged += PickerControl_SelectedValuesChanged;
        Grid mainGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };
        mainGrid.Add(headerLabel, 0, 0);
        mainGrid.Add(pickerControl, 0, 1);
        Content = mainGrid;
    }

    private void PickerControl_SelectedValuesChanged(object sender, EventArgs e)
    {
        UserAccountSettingsModel bindItem = (sender as CustomBindablePicker).BindingContext as UserAccountSettingsModel;
        CustomBindablePicker typePicker = (CustomBindablePicker)sender;
        long readingTypeID = Convert.ToInt64(typePicker.ClassId, CultureInfo.InvariantCulture);
        if (readingTypeID > 0 && typePicker.SelectedItem != null && bindItem.ReadingUnit != (string)typePicker.SelectedItem)
        {
            _userAccountSettingsView.Picker_SelectedValuesChanged(readingTypeID, (string)typePicker.SelectedItem);
            bindItem.ReadingUnit = (string)typePicker.SelectedItem;
        }
    }
}