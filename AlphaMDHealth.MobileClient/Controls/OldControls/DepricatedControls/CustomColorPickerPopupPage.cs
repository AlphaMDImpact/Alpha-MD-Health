using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class CustomColorPickerPopupPage : ContentPage //todo: PopupPage
{
    private readonly CustomColorPickerControl _customColorPicker;
    private readonly CustomButtonControl _saveButton;
    private readonly CustomButtonControl _cancelButton;
    private readonly ProgramModel _program;
    private readonly BasePage _parent;

    /// <summary>
    /// on click event of Save Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    public CustomColorPickerPopupPage(ProgramModel program, BasePage parent)
    {
        _program = program;
        _parent = parent;
        var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        Color selectedColor = Color.FromArgb(_program.ProgramGroupIdentifier);
        _customColorPicker = new CustomColorPickerControl
        {
            //todo:SelectedColor = selectedColor,
            ColorValue = selectedColor,
            //todo:Padding = new Thickness(0, padding)
        };
        _saveButton = new CustomButtonControl(ButtonType.PrimaryWithMargin)
        {
            VerticalOptions = LayoutOptions.End
        };

        _cancelButton = new CustomButtonControl(ButtonType.TransparentWithMargin)
        {
            VerticalOptions = LayoutOptions.End
        };
        // Padding added for popup view
        Padding = new OnIdiom<Thickness>
        {
            Phone = new Thickness(0, padding * 3.5, 0, 0),
            Tablet = new Thickness(0.15 * App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0), 0.05 * App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0))
        };
        var mainLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
        };
        mainLayout.Add(_customColorPicker, 0, 0);
        mainLayout.Add(_saveButton, 0, 1);
        mainLayout.Add(_cancelButton, 0, 2);
        //todo:
        //CloseWhenBackgroundIsClicked = false;
        //Content = new PancakeView
        //{
        //    Style = (Style)Application.Current.Resources[LibStyleConstants.ST_PANCAKE_STYLE],
        //    CornerRadius = new OnIdiom<CornerRadius> { Phone = new CornerRadius(padding * 2, padding * 2, 0, 0), Tablet = new CornerRadius(10) },
        //    BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR],
        Content = mainLayout;
        //};
    }
    /// <summary>
    /// On Appearing
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _saveButton.Text = _parent.GetResourceValueByKey(ResourceConstants.R_SAVE_ACTION_KEY);
        _cancelButton.Text = _parent.GetResourceValueByKey(ResourceConstants.R_CANCEL_ACTION_KEY);
        _saveButton.Clicked += OnSaveClick;
        _cancelButton.Clicked += OnPopupClose;
    }

    /// <summary>
    /// On Disappearing
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async void OnPopupClose(object sender, EventArgs e)
    {
        //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
    }

    private async void OnSaveClick(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        _program.ProgramGroupIdentifier = _customColorPicker.GetHexValue();
        OnSaveButtonClicked.Invoke(_program, new EventArgs());
        //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }
}