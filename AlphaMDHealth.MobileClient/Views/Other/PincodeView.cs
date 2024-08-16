using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Controls;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;


/// <summary>
/// Pincode view
/// </summary>
public class PincodeView
{
    private readonly BasePage _parentPage;
    /// <summary>
    /// Event to submit pincode
    /// </summary>
    public EventHandler<int> OnSubmitPincode { get; set; }

    internal Grid _pincodeGrid;
    internal Grid _buttonGrid;
    internal double _imageBoxSize;
    internal int _noOfDots;
    internal double _buttonHeight;
    internal Thickness _svgMarginPercent;

    /// <summary>
    /// variable for pincode
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Pincode view
    /// </summary>
    /// <param name="page">base content page</param>
    public PincodeView(BasePage page)
    {
        _parentPage = page;
        _pincodeGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            RowSpacing = 0,
            ColumnSpacing = 5,
            Margin = new Thickness(0),
            //Padding = new Thickness(0, Convert.ToInt32(Application.Current.Resources[StyleConstants.ST_CONTROL_PADDING_MARGIN], CultureInfo.CurrentCulture) / 2),
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            }
        };
        _buttonGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            VerticalOptions = LayoutOptions.EndAndExpand,
            //Added to prevent animation when page launched after app sleep
            HeightRequest = CalculateHeight()
        };
        GenerateRowsAndColumns();
    }

    /// <summary>
    /// Reset pincode view
    /// </summary>
    public void ResetPincodeGrid()
    {
        Code = string.Empty;
        if (_pincodeGrid.Count > 0)
        {
            _pincodeGrid.Clear();
        }
        _pincodeGrid.RowDefinitions = GenerateRows(1);
        _pincodeGrid.ColumnDefinitions = GenerateColumns(_noOfDots + 1);

        for (int i = 0; i < _noOfDots; i++)
        {
            _pincodeGrid.Add(CreateImageBox(i, ImageConstants.I_EMPTY_ENTRY_PNG), i, 0);
        }
    }

    private AmhImageControl CreateImageBox(int i, string icon)
    {
        return new AmhImageControl(FieldTypes.SecondaryBorderTransparentButtonControl)
        {
            StyleId = $"box_{i}",
            ImageHeight = AppImageSize.ImageSizeL,
            ImageWidth = AppImageSize.ImageSizeL,
            Icon = icon,
        };
    }

    private double CalculateHeight()
    {
        var keyboardHeight = (App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0) - 30) * new OnIdiom<double> { Phone = 0.35, Tablet = 0.3 };
        _buttonHeight = new OnIdiom<double> { Phone = keyboardHeight / 4, Tablet = keyboardHeight / 3 };
        _imageBoxSize = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_IMAGE_SIZE_XLARGE], CultureInfo.InvariantCulture);
        _svgMarginPercent = new Thickness(
            GenericMethods.GetPlatformSpecificValue(
                new OnIdiom<double> { Phone = 0.57, Tablet = 0.77 },
                new OnIdiom<double> { Phone = 0.37, Tablet = 0.47 },
                new OnIdiom<double> { Phone = 0.37, Tablet = 0.47 }),
            0.10, 0, 0.10
        );
        return keyboardHeight;
    }

    private void GenerateRowsAndColumns()
    {
        _buttonGrid.RowDefinitions = GenerateRows(new OnIdiom<int> { Phone = 4, Tablet = 3 });
        _buttonGrid.ColumnDefinitions = GenerateColumns(new OnIdiom<int> { Phone = 3, Tablet = 4 });

        _buttonGrid.Add(CreateKeys(Convert.ToString(1, CultureInfo.InvariantCulture)), 0, 0);
        _buttonGrid.Add(CreateKeys(Convert.ToString(2, CultureInfo.InvariantCulture)), 1, 0);
        _buttonGrid.Add(CreateKeys(Convert.ToString(3, CultureInfo.InvariantCulture)), 2, 0);

        if (MobileConstants.IsDevicePhone)
        {
            _buttonGrid.Add(CreateKeys(Convert.ToString(4, CultureInfo.InvariantCulture)), 0, 1);
            _buttonGrid.Add(CreateKeys(Convert.ToString(5, CultureInfo.InvariantCulture)), 1, 1);
            _buttonGrid.Add(CreateKeys(Convert.ToString(6, CultureInfo.InvariantCulture)), 2, 1);
            _buttonGrid.Add(CreateKeys(Convert.ToString(7, CultureInfo.InvariantCulture)), 0, 2);
            _buttonGrid.Add(CreateKeys(Convert.ToString(8, CultureInfo.InvariantCulture)), 1, 2);
            _buttonGrid.Add(CreateKeys(Convert.ToString(9, CultureInfo.InvariantCulture)), 2, 2);
            _buttonGrid.Add(CreateKeys(string.Empty), 0, 3);
            _buttonGrid.Add(CreateKeys(Convert.ToString(0, CultureInfo.InvariantCulture)), 1, 3);
            _buttonGrid.Add(CreateKeys(ImageConstants.I_DELETE_PNG), 2, 3);
        }
        else
        {
            _buttonGrid.Add(CreateKeys(Convert.ToString(4, CultureInfo.InvariantCulture)), 3, 0);
            _buttonGrid.Add(CreateKeys(Convert.ToString(5, CultureInfo.InvariantCulture)), 0, 1);
            _buttonGrid.Add(CreateKeys(Convert.ToString(6, CultureInfo.InvariantCulture)), 1, 1);
            _buttonGrid.Add(CreateKeys(Convert.ToString(7, CultureInfo.InvariantCulture)), 2, 1);
            _buttonGrid.Add(CreateKeys(Convert.ToString(8, CultureInfo.InvariantCulture)), 3, 1);
            _buttonGrid.Add(CreateKeys(string.Empty), 0, 2);
            _buttonGrid.Add(CreateKeys(Convert.ToString(9, CultureInfo.InvariantCulture)), 1, 2);
            _buttonGrid.Add(CreateKeys(Convert.ToString(0, CultureInfo.InvariantCulture)), 2, 2);
            _buttonGrid.Add(CreateKeys(ImageConstants.I_DELETE_PNG), 3, 2);
        }
    }

    private ColumnDefinitionCollection GenerateColumns(int count)
    {
        var cols = new ColumnDefinitionCollection();
        for (int i = 0; i < count; i++)
        {
            cols.Add(new ColumnDefinition { Width = GridLength.Star });
        }
        return cols;
    }

    private RowDefinitionCollection GenerateRows(int count)
    {
        var rows = new RowDefinitionCollection();
        for (int i = 0; i < count; i++)
        {
            rows.Add(new RowDefinition { Height = new GridLength(_buttonHeight, GridUnitType.Absolute) });
        }
        return rows;
    }

    private AmhButtonControl CreateKeys(string text)
    {
        var button = new AmhButtonControl(FieldTypes.PrimaryTransparentButtonControl)
        {
            StyleName = StyleConstants.ST_PINCODE_BUTTON_STYLE,
            UniqueID = text,
            Icon = text.Contains(Constants.PNG_EXTENSION) ? text : string.Empty,
            Value = text.Contains(Constants.PNG_EXTENSION) ? string.Empty : text,
            HeightRequest = _buttonHeight,
            IsControlEnabled = !string.IsNullOrWhiteSpace(text),
        };
        button.ResourceKey = text;
        button.PageResources = _parentPage.PageData;
        return button;
    }

    private void UpdatePincodeGrid(int iconCount, string removePin)
    {
        _pincodeGrid.Add(string.IsNullOrWhiteSpace(removePin)
            ? CreateImageBox(iconCount - 1, ImageConstants.I_FILLED_ENTRY_PNG) //new SvgImageView(ImageConstants.I_FILLED_ENTRY_PNG, _imageBoxSize, _imageBoxSize)
            : CreateImageBox(iconCount - 1, ImageConstants.I_EMPTY_ENTRY_PNG) //new SvgImageView(ImageConstants.I_EMPTY_ENTRY_PNG, _imageBoxSize, _imageBoxSize)
        , iconCount - 1, 0);
    }

    private void BtnClicked(object sender, EventArgs e)
    {
        var button = sender as SimpleButton;
        //button.OnValueChanged -= BtnClicked;
        if (!string.IsNullOrWhiteSpace(button.StyleId))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Code.Length < _noOfDots)
                {
                    if (button.StyleId.ToUpper(CultureInfo.InvariantCulture) == ImageConstants.I_DELETE_PNG.ToUpper(CultureInfo.InvariantCulture))
                    {
                        if (Code.Length > 0)
                        {
                            UpdatePincodeGrid(Code.Length, Constants.BASEKEYPAD_DELETE_BUTTON_TEXT);
                            Code = Code.Substring(0, Code.Length - 1);
                        }
                    }
                    else
                    {
                        Code += button.StyleId.Split('.')[0];
                        if (Code.Length < _noOfDots + 1)
                        {
                            UpdatePincodeGrid(Code.Length, null);
                        }
                        if (Code.Length == _noOfDots)
                        {
                            AppHelper.ShowBusyIndicator = true;
                            // submit input to take action on page
                            OnSubmitPincode.Invoke(sender, 0);
                        }
                    }
                }
            });
        }
        //button.OnValueChanged += BtnClicked;
    }

    private async void OnAuthenticate(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await AuthenticateWithBiometricsAsync(sender).ConfigureAwait(false);
    }

    internal async Task CreateFingerPrintButtonAsync(bool showFingerprintOption)
    {
        if (showFingerprintOption && await CrossFingerprint.Current.IsAvailableAsync().ConfigureAwait(true))
        {
            var authType = await CrossFingerprint.Current.GetAuthenticationTypeAsync().ConfigureAwait(true);
            AmhButtonControl btnFingerprint = authType.ToString() == Constants.FACE_TYPE
                ? CreateKeys(ImageConstants.I_FACE_ID_PNG)
                : CreateKeys(ImageConstants.I_FINGER_PRINT_PNG);
            _buttonGrid.Add(btnFingerprint, 0, 3);
        }
    }

    internal async Task RegisterClickEventAsync(bool isPincodeLogin)
    {
        await CreateFingerPrintButtonAsync(isPincodeLogin).ConfigureAwait(true);
        foreach (AmhButtonControl button in _buttonGrid.Children.OfType<AmhButtonControl>())
        {
            button.PageResources = _parentPage.PageData;
            if (button.UniqueID?.ToString() == ImageConstants.I_FINGER_PRINT_PNG
                || button.UniqueID?.ToString() == ImageConstants.I_FACE_ID_PNG)
            {
                button.OnValueChanged += OnAuthenticate;
            }
            else
            {
                if (!button.UniqueID.Contains(Constants.PNG_EXTENSION))
                {
                    button.Value = button.UniqueID;
                }
                button.OnValueChanged += BtnClicked;
            }
        }
    }

    internal void UnRegisterButtonEvents()
    {
        if (_buttonGrid != null && _buttonGrid.Children != null && _buttonGrid.Children.Count > 0)
        {
            foreach (AmhButtonControl button in _buttonGrid.Children.OfType<AmhButtonControl>())
            {
                if (button.UniqueID?.ToString() == ImageConstants.I_FINGER_PRINT_PNG
                    || button.UniqueID?.ToString() == ImageConstants.I_FACE_ID_PNG)
                {
                    button.OnValueChanged -= OnAuthenticate;
                }
                else
                {
                    button.OnValueChanged -= BtnClicked;
                }
            }
        }
    }

    internal async Task AuthenticateWithBiometricsAsync(object sender)
    {
        string biometricRequestMessage = string.Empty;
        if (MobileConstants.IsIosPlatform)
        {
            biometricRequestMessage = LibResources.GetResourceValueByKey(_parentPage.PageData?.Resources, 
                await CrossFingerprint.Current.GetAuthenticationTypeAsync().ConfigureAwait(true) == AuthenticationType.Face
                ? ResourceConstants.R_VERIFY_BIOMETRIC_MESSAGE_BODY_FACE_KEY
                : ResourceConstants.R_VERIFY_BIOMETRIC_MESSAGE_BODY_FINGER_KEY);
        }
        AuthenticationRequestConfiguration dialogConfig = new AuthenticationRequestConfiguration(LibResources.GetResourceValueByKey(_parentPage.PageData?.Resources, ResourceConstants.R_VERIFY_FINGER_PRINT_MESSAGE_KEY), biometricRequestMessage)
        {
            AllowAlternativeAuthentication = false
        };
        FingerprintAuthenticationResult biometricResult = await CrossFingerprint.Current.AuthenticateAsync(dialogConfig).ConfigureAwait(true);
        if (biometricResult.Authenticated)
        {
            // Submit result to take action on page                
            OnSubmitPincode.Invoke(sender, 1);
        }
        else
        {
            if (biometricResult.ErrorMessage != Constants.NOTIFICATION_ALERT_BOX_CANCEL)
            {
                switch (biometricResult.Status)
                {
                    case FingerprintAuthenticationResultStatus.Failed:
                    case FingerprintAuthenticationResultStatus.Canceled:
                        await _parentPage.DisplayMessagePopupAsync(LibResources.GetResourceValueByKey(_parentPage.PageData?.Resources, ResourceConstants.R_BIOMETRIC_AUTHENTICATION_FAILED_STATUS_KEY), false, true, true).ConfigureAwait(true);
                        break;
                    case FingerprintAuthenticationResultStatus.TooManyAttempts:
                        await _parentPage.DisplayMessagePopupAsync(LibResources.GetResourceValueByKey(_parentPage.PageData?.Resources, ResourceConstants.R_BIOMETRIC_AUTHENTICATION_RESULT_TOO_MANY_ATTEMPTS_STATUS_KEY), false, true, true).ConfigureAwait(true);
                        break;
                    case FingerprintAuthenticationResultStatus.NotAvailable:
                        await _parentPage.DisplayMessagePopupAsync(LibResources.GetResourceValueByKey(_parentPage.PageData?.Resources, ResourceConstants.R_VERIFY_BIOMETRIC_MESSAGE_BODY_FINGER_KEY), false, true, true).ConfigureAwait(true);
                        break;
                    default:
                        // Not required
                        break;
                }
            }
            AppHelper.ShowBusyIndicator = false;
        }
    }
}