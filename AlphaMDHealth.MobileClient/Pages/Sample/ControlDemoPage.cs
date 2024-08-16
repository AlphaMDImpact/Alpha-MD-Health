using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Mopups.Services;
using Plugin.Fingerprint;

namespace AlphaMDHealth.MobileClient;

public class ControlDemoPage : BasePage
{
    private readonly AmhButtonControl _labelControlDemoButton;
    private readonly AmhButtonControl _messageControlDemo;
    private readonly AmhButtonControl _buttonControlDemo;
    private readonly AmhButtonControl _webViewControlDemoButton;
    private readonly AmhButtonControl _uploadControlDemoButton;
    private readonly AmhButtonControl _calenderControlDemoButton;
    private readonly AmhButtonControl _calenderDemoButton;
    private readonly AmhButtonControl _carouselDemoPage;
    private readonly AmhProgressBarControl _progressBarControl;
    private readonly AmhButtonControl _generatePdfButton;
    private readonly AmhButtonControl _authentication;
    private PincodeView _pincodeView;
    private AmhCardsControl _cardsControl;
    List<OptionModel> cardsData;

    private readonly AmhEntryControl _textEntry;
    private readonly AmhEntryControl _numericDisabledEntry;
    private readonly AmhEntryControl _emailEntry;
    private readonly AmhEntryControl _passwordEntry;
    private readonly AmhEntryControl _editorControl;

    private readonly AmhNumericEntryControl _numericEntry;
    private readonly AmhNumericEntryControl _decimalEntry;
    private readonly AmhNumericEntryControl _counterEntry;
    private readonly AmhMobileNumberControl _mobileNumberEntry;

    private readonly AmhDateTimeControl _datetimeEntry;
    private readonly AmhDateTimeControl _dateEntry;
    private readonly AmhDateTimeControl _timeEntry;

    private readonly AmhCheckBoxControl _checkBoxList;
    private readonly AmhCheckBoxControl _colorBoxCheckBoxList;

    private readonly AmhRadioButtonControl _radioList;

    private readonly AmhColorPickerControl _colorPicker;


    private readonly AmhSingleSelectDropDownControl _singleDropdown;
    private readonly AmhSingleSelectDropDownControl _singleEditableDropdown;
    private readonly AmhMultiSelectDropDownControl _multiDropdown;
    private readonly AmhMultiSelectDropDownControl _multiEditableDropdown;
    private readonly AmhSingleSelectDropDownControl _singleDisableDropdown;
    private readonly AmhSingleSelectDropDownControl _singleEditableDisableDropdown;
    private readonly AmhMultiSelectDropDownControl _multiDisableDropdown;
    private readonly AmhMultiSelectDropDownControl _multiEditableDisableDropdown;

    private readonly AmhSliderControl _horizontalslider;
    private readonly AmhButtonControl _goToPopUpPageButton;
    private readonly AmhButtonControl _primaryButton;
    private readonly AmhButtonControl _secondaryButton;
    private readonly AmhButtonControl _tertiaryButton;
    private readonly AmhButtonControl _transparentButton;
    private readonly AmhButtonControl _deleteButton;
    private readonly AmhButtonControl _tabsControl;

    private readonly AmhImageControl _imageControl;
    private readonly AmhImageControl _imageControl1;
    private readonly AmhImageControl _circleImageControl;
    private readonly AmhImageControl _initialCircleImage;
    private readonly AmhImageControl _initialRectangleImageControl;
    private readonly AmhImageControl _circleImageWithBorderControl;
    private readonly AmhImageControl _rectangleImageWithBorderBase64Control;
    private readonly AmhImageControl _rectangleImageWithBase64Control;
    private readonly AmhImageControl _circleImageWithoutBackgroundControl;
    private readonly AmhImageControl _rectangleImageWithoutBackgroundControl;

    private readonly AmhBadgeCountControl _amhBadgeCountControl;

    private readonly AmhSwitchControl _switchcontrol;
    private readonly AmhSwitchControl _disableswitchcontrol;

    public ControlDemoPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);

        _messageControlDemo = new AmhButtonControl(FieldTypes.PrimaryButtonControl);
        AddView(_messageControlDemo);

        _buttonControlDemo = new AmhButtonControl(FieldTypes.PrimaryButtonControl);
        AddView(_buttonControlDemo);

        _cardsControl = new AmhCardsControl(FieldTypes.CardsControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY
        };
        AddView(_cardsControl);
        _uploadControlDemoButton = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY
        };
        AddView(_uploadControlDemoButton);

        AddBadgeControl(FieldTypes.PrimaryBadgeControl);
        AddBadgeControl(FieldTypes.SecondaryBadgeControl);
        AddBadgeControl(FieldTypes.DarkBadgeControl);
        AddBadgeControl(FieldTypes.LightBadgeControl);
        AddBadgeControl(FieldTypes.SuccessBadgeControl);
        AddBadgeControl(FieldTypes.DangerBadgeControl);
        AddBadgeControl(FieldTypes.InfoBadgeControl);
        AddBadgeControl(FieldTypes.WarningBadgeControl);

        #region Images

        _imageControl = new AmhImageControl(FieldTypes.ImageControl);
        AddView(_imageControl);

        _imageControl1 = new AmhImageControl(FieldTypes.ImageControl);
        AddView(_imageControl1);

        _circleImageControl = new AmhImageControl(FieldTypes.CircleImageControl);
        AddView(_circleImageControl);

        _circleImageWithoutBackgroundControl = new AmhImageControl(FieldTypes.CircleWithBackgroundImageControl);
        AddView(_circleImageWithoutBackgroundControl);

        _initialCircleImage = new AmhImageControl(FieldTypes.CircleWithBackgroundImageControl) { ImageHeight = AppImageSize.ImageSizeL, ImageWidth = AppImageSize.ImageSizeL };
        AddView(_initialCircleImage);

        _circleImageWithBorderControl = new AmhImageControl(FieldTypes.CircleWithBorderImageControl);
        AddView(_circleImageWithBorderControl);

        _rectangleImageWithBase64Control = new AmhImageControl(FieldTypes.SquareImageControl);
        AddView(_rectangleImageWithBase64Control);

        _rectangleImageWithBorderBase64Control = new AmhImageControl(FieldTypes.SquareWithBorderImageControl);
        AddView(_rectangleImageWithBorderBase64Control);  

        _initialRectangleImageControl = new AmhImageControl(FieldTypes.SquareWithBackgroundImageControl) { ImageHeight = AppImageSize.ImageSizeXXXL, ImageWidth = AppImageSize.ImageSizeXXXL };
        AddView(_initialRectangleImageControl);

        _rectangleImageWithoutBackgroundControl = new AmhImageControl(FieldTypes.SquareWithBackgroundImageControl);
        AddView(_rectangleImageWithoutBackgroundControl);
        #endregion

        _switchcontrol = new AmhSwitchControl(FieldTypes.SwitchControl)
        {
            ResourceKey = FieldTypes.SwitchControl.ToString()
        };
        AddView(_switchcontrol);

        _disableswitchcontrol = new AmhSwitchControl(FieldTypes.SwitchControl)
        {
            ResourceKey = FieldTypes.SwitchControl.ToString()
        };
        AddView(_disableswitchcontrol);

        _amhBadgeCountControl = new AmhBadgeCountControl(FieldTypes.BadgeCountControl)
        {
            ResourceKey = FieldTypes.SwitchControl.ToString(),
            Value = "786",
        };
        AddView(_amhBadgeCountControl);

        _labelControlDemoButton = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY
        };
        AddView(_labelControlDemoButton);

        _webViewControlDemoButton = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY
        };
        AddView(_webViewControlDemoButton);

        _calenderControlDemoButton = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY
        };
        AddView(_calenderControlDemoButton);

        _calenderDemoButton = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY
        };
        AddView(_calenderDemoButton);

        _carouselDemoPage = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY
        };
        AddView(_carouselDemoPage);

        _generatePdfButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            Value = "Create Pdf",
        };
        _generatePdfButton.OnValueChanged += OnGeneratePdfButtonClicked;
        AddView(_generatePdfButton);

        _authentication = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            Value = "AuthScan",
        };
        _authentication.OnValueChanged += OnAuthClick;
        AddView(_authentication);

        _textEntry = new AmhEntryControl(FieldTypes.TextEntryControl)
        {
            ResourceKey = FieldTypes.TextEntryControl.ToString(),
            Icon = ImageConstants.I_USER_ID_PNG,
        };
        AddView(_textEntry);

        _emailEntry = new AmhEntryControl(FieldTypes.EmailEntryControl)
        {
            ResourceKey = FieldTypes.EmailEntryControl.ToString(),
            Icon = ImageConstants.I_USER_ID_PNG,
            RegexExpression = "^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,4}$"
        };
        AddView(_emailEntry);

        _passwordEntry = new AmhEntryControl(FieldTypes.PasswordEntryControl)
        {
            ResourceKey = FieldTypes.PasswordEntryControl.ToString(),
            Icon = ImageConstants.I_PASSWORD_ICON_PNG,
        };
        AddView(_passwordEntry);
        _tabsControl = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY
        };
        AddView(_tabsControl);

        _numericEntry = new AmhNumericEntryControl(FieldTypes.NumericEntryControl)
        {
            ResourceKey = FieldTypes.NumericEntryControl.ToString(),
            Icon = ImageConstants.I_USER_ID_PNG,
        };
        AddView(_numericEntry);

        _numericDisabledEntry = new AmhEntryControl(FieldTypes.NumericEntryControl)
        {
            ResourceKey = "NumericEntryControlDisabled",
            Icon = ImageConstants.I_USER_ID_PNG,
            IsControlEnabled = false,
        };
        AddView(_numericDisabledEntry);


        _decimalEntry = new AmhNumericEntryControl(FieldTypes.DecimalEntryControl)
        {
            ResourceKey = FieldTypes.DecimalEntryControl.ToString(),
            Icon = ImageConstants.I_USER_ID_PNG,
        };
        AddView(_decimalEntry);

        _counterEntry = new AmhNumericEntryControl(FieldTypes.CounterEntryControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY,
            Icon = ImageConstants.I_USER_ID_PNG,
        };
        AddView(_counterEntry);

        _mobileNumberEntry = new AmhMobileNumberControl(FieldTypes.MobileNumberControl)
        {
            ResourceKey = ResourceConstants.R_MOBILE_NUMBER_KEY,
            Icon = ImageConstants.I_PASSWORD_ICON_PNG,
        };
        AddView(_mobileNumberEntry);

        _editorControl = new AmhEntryControl(FieldTypes.MultiLineEntryControl)
        {
            ResourceKey = ResourceConstants.R_PASSWORD_KEY,
            Icon = ImageConstants.I_PASSWORD_ICON_PNG,
        };
        AddView(_editorControl);


        _datetimeEntry = new AmhDateTimeControl(FieldTypes.DateTimeControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_datetimeEntry);
        _dateEntry = new AmhDateTimeControl(FieldTypes.DateControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_dateEntry);
        _timeEntry = new AmhDateTimeControl(FieldTypes.TimeControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_timeEntry);

        _colorPicker = new AmhColorPickerControl(FieldTypes.ColorPickerControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_colorPicker);

        _progressBarControl = new AmhProgressBarControl(FieldTypes.ProgressBarControl);
        AddView(_progressBarControl);

        #region Dropdowns

        _singleDropdown = new AmhSingleSelectDropDownControl(FieldTypes.SingleSelectDropdownControl)
        {
            ResourceKey = "SingleSelectKey"
        };
        AddView(_singleDropdown);

        _singleDisableDropdown = new AmhSingleSelectDropDownControl(FieldTypes.SingleSelectDropdownControl)
        {
            ResourceKey = "SingleSelectKey",
            IsControlEnabled = false
        };
        AddView(_singleDisableDropdown);

        _singleEditableDropdown = new AmhSingleSelectDropDownControl(FieldTypes.SingleSelectEditableDropdownControl)
        {
            ResourceKey = "EditableSingleSelectKey"
        };
        AddView(_singleEditableDropdown);

        _singleEditableDisableDropdown = new AmhSingleSelectDropDownControl(FieldTypes.SingleSelectEditableDropdownControl)
        {
            ResourceKey = "EditableSingleSelectKey",
            IsControlEnabled = false
        };
        AddView(_singleEditableDisableDropdown);

        _multiDropdown = new AmhMultiSelectDropDownControl(FieldTypes.MultiSelectDropdownControl)
        {
            ResourceKey = "MultiSelectKey"
        };
        AddView(_multiDropdown);

        _multiDisableDropdown = new AmhMultiSelectDropDownControl(FieldTypes.MultiSelectDropdownControl)
        {
            ResourceKey = "MultiSelectKey",
            IsControlEnabled = false
        };
        AddView(_multiDisableDropdown);

        _multiEditableDropdown = new AmhMultiSelectDropDownControl(FieldTypes.MultiSelectEditableDropdownControl)
        {
            ResourceKey = "EditableMultiSelectKey"
        };
        AddView(_multiEditableDropdown);

        _multiEditableDisableDropdown = new AmhMultiSelectDropDownControl(FieldTypes.MultiSelectEditableDropdownControl)
        {
            ResourceKey = "EditableMultiSelectKey",
            IsControlEnabled = false
        };
        AddView(_multiEditableDisableDropdown);

        #endregion

        _horizontalslider = new AmhSliderControl(FieldTypes.HorizontalSliderControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY,
        };
        AddView(_horizontalslider);


        _checkBoxList = new AmhCheckBoxControl(FieldTypes.VerticalCheckBoxControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_checkBoxList);

        _colorBoxCheckBoxList = new AmhCheckBoxControl(FieldTypes.ColorBoxCheckBoxControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_colorBoxCheckBoxList);

        _radioList = new AmhRadioButtonControl(FieldTypes.VerticalRadioButtonControl)
        {
            ResourceKey = ResourceConstants.R_USER_NAME_KEY
        };
        AddView(_radioList);

        _goToPopUpPageButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            Icon = ImageConstants.I_USER_ID_PNG,
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_goToPopUpPageButton);
        _primaryButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            Icon = ImageConstants.I_USER_ID_PNG,
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_primaryButton);
        _secondaryButton = new AmhButtonControl(FieldTypes.SecondaryButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_secondaryButton);
        _tertiaryButton = new AmhButtonControl(FieldTypes.TertiaryButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_tertiaryButton);
        _transparentButton = new AmhButtonControl(FieldTypes.TransparentButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_transparentButton);
        _deleteButton = new AmhButtonControl(FieldTypes.DeleteButtonControl)
        {
            ResourceKey = ResourceConstants.R_LOGIN_ACTION_KEY,
        };
        AddView(_deleteButton);
    }

    private void AddBadgeControl(FieldTypes primaryBadgeControl)
    {
        var badgeControl = new AmhBadgeControl(primaryBadgeControl)
        {
            Value = primaryBadgeControl.ToString()
        };
        AddView(badgeControl);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new AuthService(App._essentials).GetAccountDataAsync(PageData).ConfigureAwait(false);

        var demoService = new ControlDemoService(App._essentials);
        demoService.GetControlDemoPageResources(PageData);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _pincodeView = new PincodeView(this);
            _pincodeView.OnSubmitPincode += ShowAlert;

            //Apply Resources
            cardsData = new List<OptionModel>();
            //cardsData = demoService.GenerateLargeDataset(0);
            _cardsControl.Options = cardsData;


            _textEntry.PageResources = _emailEntry.PageResources = _passwordEntry.PageResources
                = _numericEntry.PageResources = _numericDisabledEntry.PageResources = _decimalEntry.PageResources
                = _counterEntry.PageResources = _mobileNumberEntry.PageResources = _editorControl.PageResources = PageData;
            _datetimeEntry.PageResources = _dateEntry.PageResources = _timeEntry.PageResources = PageData;

            _switchcontrol.PageResources = _disableswitchcontrol.PageResources = PageData;
            _disableswitchcontrol.IsControlEnabled = false;
            _disableswitchcontrol.Value = true;

            List<OptionModel> optionsList = demoService.GetOptionsList();
            _checkBoxList.PageResources = PageData;
            _checkBoxList.Options = optionsList;
            _checkBoxList.Value = "1|3|5";

            List<OptionModel> optionsListWithColor = demoService.GetOptionsListWithColor();
            _colorBoxCheckBoxList.PageResources = PageData;
            _colorBoxCheckBoxList.Options = optionsListWithColor;
            _colorBoxCheckBoxList.Value = "4";

            _radioList.PageResources = PageData;
            _radioList.Options = optionsList;
            //_radioList.Value = "5";

            _colorPicker.PageResources = PageData;
            _colorPicker.Value = "#FFA07A";
            Color color = Color.FromHex(_colorPicker.Value);
            _progressBarControl.StyleName = _colorPicker.Value;

            _singleDropdown.PageResources = _singleEditableDropdown.PageResources = _multiDropdown.PageResources = _multiEditableDropdown.PageResources = PageData;
            _singleDisableDropdown.PageResources = _singleEditableDisableDropdown.PageResources = _multiDisableDropdown.PageResources = _multiEditableDisableDropdown.PageResources = PageData;

            _singleDropdown.Options = _singleEditableDropdown.Options = _multiDropdown.Options = _multiEditableDropdown.Options = optionsList;
            _singleDisableDropdown.Options = _singleEditableDisableDropdown.Options = _multiDisableDropdown.Options = _multiEditableDisableDropdown.Options = optionsList;

            _singleDisableDropdown.Value = _singleEditableDisableDropdown.Value = 4;
            _multiDisableDropdown.Value = _multiEditableDisableDropdown.Value = "1|3";



            _progressBarControl.PageResources = PageData;

            _horizontalslider.PageResources = PageData;

            _primaryButton.PageResources = _secondaryButton.PageResources = _tertiaryButton.PageResources = _transparentButton.PageResources
                = _deleteButton.PageResources = _goToPopUpPageButton.PageResources = PageData;
            _goToPopUpPageButton.Value = "Go To Popup Demo Page";
            _tertiaryButton.Value = "Go to Charts Demo Page";
            _transparentButton.Value = "Go to List Demo Page";

            _carouselDemoPage.Value = "Carousel control Demo";
            _carouselDemoPage.OnValueChanged += _carouselButtonCLick;

            #region Image

            //Flat image without any circle or border
            _imageControl.Icon = ImageConstants.I_IMAGE_ICON_PNG;
            _imageControl1.Icon = ImageConstants.I_FINGER_PRINT_PNG;

            //image in circle  without border
            _circleImageControl.Icon = ImageConstants.I_VIDEO_CAMERA_ICON_PNG;
            _circleImageControl.ImageHeight = AppImageSize.ImageSizeXXL;
            _circleImageControl.ImageWidth = AppImageSize.ImageSizeXXL;

            // initial in circle with border
            _initialCircleImage.Value = "CI";

            // image in circle with border
            _circleImageWithBorderControl.Icon = ImageConstants.I_FINGER_PRINT_PNG;
            _circleImageWithBorderControl.ImageHeight = AppImageSize.ImageSizeXXXL;
            _circleImageWithBorderControl.ImageWidth = AppImageSize.ImageSizeXXXL;

            // initial in rectangle without border
            _initialRectangleImageControl.Value = "DEMO";

            // image in rectangle without border using base 64
            _rectangleImageWithBase64Control.Source = Convert.FromBase64String("/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAoHCBIVFRgVERUYGRgYGRgaGBgYGBgYGBoYGhgaGRoYGRgcIS4lHB4rIxgcJjgmKy8xNTU1GiQ7QDszPy40NTEBDAwMEA8QHhISHzQrJSs2NDQ0NDQ0NDQ0NDQ0NDQxNDQ0NDQ0NDQ0NDQ0NDQ0NDQ0NzQ0NDQ0NDE0NDE0NDQ0P//AABEIARMAtwMBIgACEQEDEQH/xAAcAAACAgMBAQAAAAAAAAAAAAAAAQIGAwUHBAj/xAA+EAACAQIDBQUECQMDBQEAAAABAgADEQQSIQUxQVFhBiJxgaEHE5HBFCMyQlJigrHRcuHwM5KiJGOywvEV/8QAGQEBAAMBAQAAAAAAAAAAAAAAAAECAwQF/8QAJREBAQACAgICAQQDAAAAAAAAAAECESExAxJBUSITYYHBBHGR/9oADAMBAAIRAxEAPwDp4jhHJQI4RwCOEIBHCEAjhCAQhCAQhCAQhCAQhCSFFJRQFImSiMCJhAwgEYhCQGI4o4DhCEBwhKd2z7XfRr0aFjVt3mOopg7v1SLdJk2321dvYfD/AOo+v4VBY+dt3nKZtj2khbjDoB1fU+Sj+85xjtq1HYlnLuTc+M1hNVzoD4kgD1PylN2tJjIvLduMY51d/EAqPSYT2lrvo1aqD/W6/uflKklWqh1H/M/KbWi5cd9b+A1+MirRuF2vikIani66ka2Zi6n46fGWzY3tCAATF2J4Ogtf9N7HyPlKMuGAGjsvRl+bD9jMGJw5QXPeU8bajx5iJdFm3eMBtClWXNScMOhnqnBti7Xq4Zw9Jjb7wG5h1HHynZtg7Yp4mkr02F7DOt9UbiCPIy8y2yyx02UI4S6pQhCAojJRQIGEZhAQjiEcgEcUYgOEUcDX7d2kuGw9Ss33F0HNjoo8yRPnvH4x6jszMSWYl24s51PlOl+13axC0cKh1dveP0VdFB87nyE5bUYC9uGg8Znl20xnDEDl0A14ngOnUxnEcPmdfGY1pE6sdOQ/zWbDAYVSdQbdBp8ZFrSY7RwzAm7qbdAD/M2rimtmpVAG/Cy28hlF/wDjPXh9nG31dM/BT/Eyns7iamhUZeRH97j4yntFvStT/wDpBxld8jDjoyt1BGoPgJKjiyvdZ1qJzFww/qUj1HrN3S7C1CLsR6/OQxXY51F0j2iZhk0uOw5SzU75WvbmLdfhMuxO0v0asj3I1GbKbBt4PQ75kTDVEBSoPju0+es1+IpZbjKCDpfeLW329JfC7Z542PojAYunWppUpm6OoZT0Pzmect9l+2nD/RnJyMDkBG5wSTx46nynUprKws0Io4SQoo4QIGEZhAjHFCQGI5GOA45GOBw/2i1ycbV/LZR0ARd3x9TKpa5AHDSXH2o4cJjCwFg6q9+Z+yf2ld2JQzseh9DM61xZ9m7JL624y77H2RTAFwJj2XhALzeU1sLCc+WTt8eEk29uGwqAbhNhTQW3Tw0AZsMOsjFfJmRBymHEU1mdzMFRryclMd7aHaey6b8NZznbezWpsRewJ4eg6DfOsskq3aXCBlJtvHqNR84wy1UebGXFSeyof6TS92xDCqljy7wHnv1n0LODdkcN/wBdQBv/AKi+guDO8zqxcGQijil1ShAwgIwgYQMcIoSBKEjHAccjC8DkvtiT6+i1t6EX4Gzf3lZ7PizHy+Mu/tfwxK4dxbQup8wD8pSNiaOBz+QMpk1x+F42e95uEpnSVVMWyaIPG0hidvYtfsLoOdrn4znuO67cc9Og4anYT3Imk53sLtPWY5a2Xy3y64XGZxdZEkibfZsMgMx1EA3TR7Y2oyAkEDSVGj2lxjOSGBXqbfCOKjmL7Vmj26Lp5ww203YXYWPG+6LbLg0Sw5iMceUZ5bxVnsvT/wCvodXB+AJnaZyPsjTvjaXRideiNOuTqx6cOfZQhCXUKIxxQEYQMIGGF4oXgO8LxXheA7x3kbwBgUj2jOlal7pLl0cObDS1iCt777G859sSl9YBbUBr/tLHtCnUGIrOGJu5bKSbWzHQDwnkwVNfesw4j5zD23a7P0pjI9SI6KzAa8OPpMTUsUDTdVJBIzKgUvlvY98q1jbXUWlnwtAMtuk9tPDWsb+g/eZezX140rmL2G3u1qPbPYk2AVl1OXNlADC1rjS0sPZt7JZvCY9oMApG/wDzlPRsSnemT4yLdr446nLxbV2capJuN4AvuA524zQPgsajulO5UlcjWQIFzHNnXLcgi3EWIO+XZaSt3WAIguEy/Ydh0PeHrGKMorWGpVAxVly8Db7J/MnLwno2rTK4e3Jh+83xonjr4zx7QpBkK+H73iXVVzm40vY6hkre+ZWKhWtYcTpx850im4YBhuIuJTjhw6ZAcqiykA2vzGn+ayzbFpZKCLyWw8L6Tbx5W3Tn83jmOMvy90UcU3cxRRxQAwgYQPNeF5G8LwJXheRvFeBO8LyF47wKd2nwao7MNA4Deeob+fOVfD0glgDfS9733zoHaXC50DjehJI5oftA+h8jKBWsr2XcbkdNRceGs58prJ245e2E/ZZ9l1gVGs2qjSVTY9bW0sZr2HPSY5cV048x4NouS9h0v5zd7KSyW4Sk7WxeIUuKaAgkEMb6adJsdg9oqhUU3Wz7iBuPW8tMeNot+FnfutfhPUCDNHgziy/1pTIddAbgcr318dJtC5XXgd/SV6LNslQzX1mubTPiHnkBuR1IiXlXKPZh8IgclRqx1tuubD4yyKthYcJq9m0wSDb7HH8x+f8AM2s6fFjqbcnnz9rJ9CEITVzlFHEYCMIGEDx3heRvC8B3heRvFeBO8LyF47wJHXQyr9odjYenReoiWe663Y2BYAhQTYDXhLNeePbaBsPVB/Ax81GYftK2bi2GVlc6wFTK977rnx36TfU8WMtyd8qzPYgzf0UpVKdnG/cbkEHoROXKc7ehhldaj2oiuSOnlNrgcBTADLlJ4mVehspc9jWqW5Fl/jWbinsVNLV2136KPlJkaScctvUqJ91gbb9RMQxSk5QRflPBW2NSYhSXbrmIPkBae+nhKVNO6NeLEksfFjrK5VWyTp56h0yg+HS82exKKsXzAEZQLEXGpvu8pqQ9yTLFsSnZCx++dPAaD5yfDPyY+fL8WxRAosoAHICwkoo51uE4oQkgiMDCAjCBhA8F4rwivALwvC8jAcd4rxQJXmLG2NNweKOPipmS81rYyhiA9Km6vl7j5TcAkHu5hoT0G6NbS5ir37pmw2fWI7s8m19nvSco4swOnJhwYHiDPPhMVqOY4c/Cc1nw7JdcrYmGL6g6z009mMTvbyY2/eePBbQUjfrN5hsUoG+95nGu99MmGoFBrCpMjYgcT5TX4jFXNl3yNbLlqJ7zlEuaLYADgAPhKlgaOq33swHxMt038M7cvnu9JQijmznOEUJIIQhAUIjCBroXiivAd4XivFeA7wvFeULtn2zqUnNDCFbgd+po1ifurwBHEnnJktRbpP2h9pvdocNQbvt/qMp1RPwX4M3oPGej2a0VGFJXe1Ry3iAFHoonJqtRmYsxJJNyTqSTvJPEzpPsox6/WUGPKonorD0U/Gaa1NK75XjaWy6eITJUXd9lhoynmDOadoeztbDnNbMnB1G7+ofdPXdOwilMVTDA6ETLLGVrjnY4jhNoMp76k9QbHz4GbqhtIHcHHkP5lv2n2KpvdqICN+H7pP8A6zR09jPSfI6EHrxHMHiJhlLj26MbMuksK1R+YHXfNvhsOF0A8Txjw2GAGk99GlM60kSw5ysh5Mp876CWL3ilmUEEqRmHEZgGFx4GabC4RqlRLfZVlZj0XUAdSQPK8qParbVTBbV96LlKlJM6fiQFl0/MCtx/eb+HHcrn/wAiyZR0mOafZXaTB4mwpVVzH7jHK/8AtO/yvNvNNaYnCEIBFHEYCMIjCBrLxXmjxnazA075q6kjgl39VFvWVrantGUAjC0yT+OpoPEIpufMiTMbVfaL9UqKoLMQoG8kgAeJMqu1u3uEpXWlmrN+TRPNzv8AIGc02ptnEYg3rVGbkL2UeCjQTwXmkw+0XJYdudscXiQVze7Q/cS4uOTNvPoOkrtPcfGBhR+94/IS2tKsbTb9mto/R8RTq8FYZ+qHRvQn4TVOJKkdY0l9LUgQBxB1BG63Az0qsrXs12r9Iwaq5u1E+7PMqAMh+Gn6ZbfdTK8XS85jGiR4jBpUWzqDy5g8weEyZY3qBVJYgAAkkmwAG8k8BKXlMuuVfxWx3W5TvL6jxHHymHD4ZicoBufK3U8hMW2FqYxA9Ku6UbAqtNSKjNmsHZr6LxAtyPh7iwSkKaB3ZAAabPndlOmZy1ybnXT9pW+GWyb5+vppP8nKY7s4+K29DDimmUeZ5nnOT+1vCt7yhV+6UamTyYHOB5gn/aZa8FtV6NZFOf6NVBstQkvSfNYZb65DroSRYC1tRPZ202N9JwdRFF3W1RLbyya2HiLr+qbYz0umNvtPZwoG46jdN/sXttjMNZQ+dB9ypdhbo29fjK4dPKRccRuM1s2o7VsTt9g69lqH3Lng57hPR93xtLYjAgEEEHcRqD5z5nDTb7J7RYvDEe4qso/Ae8h/SdJS4fS0yfQUU59sP2k03smLTIfxpcp4ld48ry8YTH0aovSqI4/Iwa3iBumdxs7WllegwiMJCXzJcxwAjnQyKMCSEclCBEwiqFJDaXIseG62+egiRYc5FTEb3GmvhHTWQFBQbgW6Dj5SYMDoPsn2kExJosbCsnd/rS7D4rn9J2hDffv/AM1E+Ydn4t6VRKqGzI6uvipBH7T6U2bi1rUkqoe66q6nowv/AGmXknyvjfh7DYb5qNsYZ66FUbKPD7RG64PDpNoVvv8AhwjYcpnOFryo+FxDoXXFEISyKHpkq7a215C4tcc5ZFqkjOrrkUNc3udAB9q+/TW/HlMO28EWUsgGdBmW40PAqen8ygY7atfCW90MrVdSxVq1PVmIppUIHfNtV1Pd3zfHG52aY+SzGc7++FwvSxFYrp3r2BBDDug3AuRrbXcdRpPdgKlSnalVBK7kf9lb+Zr9lvVOHpYhgFbKpqqaIR9CFYBRrYkEjmLaSy0KqVEV0OZWAKnmDuMplqXicL4W65vLivtP2P7jEioi2SuM+m7OLBx53B8zKad07X7U8GHwLNbvUnRx4fZbysfScUQ667uMvhdxGU1UAvKAjcWMV5dU1M9eDxdSm6vTYq66qwOoPLqDynivJK0DvXZTbi4zDrU0Dju1BwDjf5HePGE5r7N9rijiTTY2SspvyDKCyn4Bh5wmFx5ay8KXeOYab8DJhprtmyiK8V4pKEzPPVezrfcT/wDJmmHE07r14RekxmIiJkKD5lB48fHjJNAamdp9ku1g+HfDse9Sa6j8j3I+DBviJxUS2ezranuMalzZan1bcu8RlP8AuA+JlcpuJl1XfVkopIzBq82JHdJ4WN/MfzaUvDbOWoHZHORHZlzoGy1AysjqN2g/zWXHaFQKuu66g33WLC80uGVCpKqy0yQnu1F+8xBzsTv4r4Wl8MpJqs88csrNMtLCVPfmpvRrknTdbda55+l57tkVS1NCbC6KbDg2t+J06cLTHi0IC06bAMtjbcbaLcW3KLmafYm16bVKlMWp+7ckAuGV1ZyGZG0vqets9pbdzl46ZyTCznvbcbf2aMRh6tEmxZGCnkSNPK8+b3QqSraFSQR1BsZ9RVOfxnzl2rw/u8ZXQbs5Yfq73zjCtcmrd9LHy6THJSM1ZgmK8ZmNpAyq5uLHUXimEGEjaWJTMhaYTJE7oSzK0lMSGTBiITvCRvGDJGGl3WK8G1Hj/n7TOZixCaXG8ajymRWzAEcZEKcy0nIIINiNQRvBGoMwxqZI+l+z+0RicNSrDe6Atbg40ceTAzaKZzD2O7XzJUwzHVD7xB+VtGAHQ2P6p01TrOfKarTG7jXbefLSza91lOmh0YHfPLh8fmpmoLIQGUAkBS7WKm7DQ3PLjrPR2iW9FvI/A3ms2vSpsppplQI2a7MFQ3GUgki99bDhL4+vEvzWfk9ubPhl2VUrmkGrKfelmVXKjRWXMGYDTLfTTgBxleGxqKOG94tquZEyJmRGBpktcn7OcN0IccpZsHUIWmEYOACpYEkEobEa7tRa2s1mNwdGgFDFnzVS1x3cost1sAbru003dJrjlljnrH5Z3HC+O3K8zWv7WtHuqnXUA66HUcROIe1bBinjsy7qlNH/AFAsp/8AEfGdowpfIvvPt63t4mx+Fpyr2yJ9bQb8jr6qf5mWHbfLpzpZEySyJM2Zk0iwjaNRpIGIDWEBvhAwXgdx8ojJDUEdJCxoZlBnlRp6EaRKVK8LxREyyGQGQpHKSvA6r8x/nOAaKqLjTeNRIE7x3kVe4uOPpEIFi7GbW+jYujVJsobI/wDQ/da/hcN+kT6KvPlVWn0B7O9tfScGmY3en9U995KgZW81K+d5TOb5Wxrd7e/0m8JotqUqj4JUYqalUKoJQkFQC+UAA5WtuPOWHaKZkYdJowzLhi1G+fuKbAsbjuXQfdYAg3sZGF9dX65Tlh7313rfDS7AbFIt6YdaedCEZQQEdcxbOw+ycxueFh5bbAbWwVdXyrdlqEsh3hrsquDuKsE4cDrMVZXeoKlde49Ee9UNqEdMrUyh1uWIPDjNd2d2WmFeo1ZwUqMURAA7KtN81N3cbu64uN/em+eXvLZxdSzTlxw/Ty1l1LZdrxgi1iKhUuLFsu4X3acLgXnNfbLT0oN1Yek6Ph3VagUgB2VjYHeqMqg28HH+WlD9sy/UUT/3CP8AgZhj3y6br14ciUxQWQvNlDaSVrb57Nn4JKhJqV6dJRxc3Y/0oNT6ec9+2G2etL3eFvUfMGaqwINlubLfdew3AaEyBXaTXMJCidbxSNpQJgptIlog0qsjexI6mZkaeSq3eMyo8SpsesGExq0leTtRK8YaY7xgxtIByt+VvQybSDi4tI03v3TvHrJGUNL37J9se6xZosbLXXL+tLsnpnHmJz8mejBYp6bpUQ2ZGV18VII/aV/YfUWK1Qkcpodk4wMlQUdaiE917qpJJsc34bgrpymy2btFa1CnWp/ZqIrjwYXseo3eUoPa0vRpo9FSiPVYtUzEstRKjGmiLfRT3m3G55Wk+OTL8ar5crjJZ/K3YnBFcR9IqsopFQHvbeVCZbW11Ij2hgUrZhomRizNk0Isubx0A15iebs+9WtToV6zK6vTUuWGUFrZSSu4tccgOM2OLcrTAqjK7nISnVtOdgdLyJnlvrmcRW+Ka74vN+3jwOPc400ywKAEKBlsoyAghhz108Pwyr+2ip3KK/nJ/wCJE2a7VqUsZTw6upRGpI4CgNeoCMl7d5bupv0E0ftrezYZeYqH4ZR/7ScpzLrXB47xZvfLl67pjO+TG6Q43llkSIVmsnibfOTKzzYt9QvIepkXiJk3SptCY1MJXayTTHGZEGExjqtrGhkH3wUzOXlbXD1I0zAzyI09CGaSqWMkV4ooQkDI1F4jePXpHeAaA8wIuPhIgyN8pvwO+ScWhLsnsm2yHw74Zj36JLoOJpubm3gxP+4SwUNnllR3ZGHvVqIrgkKVY3YcA2u+cT7KbYOFxNOtc5Qcrjmj6OPhqOqidu2UqOiorsShzNqVFnJdQDuK2IiSbltVy363U2wbLx1TPW+k5vdqCzBgxVSGXLlGXda3lrMmzffNUr4hCHp1AvukY5bsptlKsboVuw00JueUNuYwHPTqHJRCAs5UuQVuVcZddSANeUxbHVPcgUHzhXYO4UqC+lwL7xYra3CazKZy5Sa51+zmmNwymNu+N7Y8Vh8KazM7u9Sk6kAkLYviEta4u2Sw6Wa0p/tjxYbFUkH3KV/N3PyQS949aaM9Ve9VY0kdScyqrtTW+X7v2NOs5P7RMVnx9b8mVP8AaoJ9SZX8rJa6L6TLWHWpv/auE6SAkmOgEiIE0E1tV7sT1mxZrKTyBmqWUyXxZlhBISEmZAQhJHnaIQhMb2uyJPTTihNIrkzQhCWUKIwhAH3QpfZEIR8pnQE792NqFtnUCxufdrrx0YqPQWhCFcmq23WZdnIVYgvUCMbkllOc5STwmhw2MqUs4pOyDPR0U2H2Wbd46whO3wz8f+/04fL8/wAOomgmdjlGtr9bHSfP3aI3xeIJ1+uqf+ZhCccdzWNvhCElCGJ+wfKa5YQlcu2mPT0U4QhIRX//2Q==");
            _rectangleImageWithBase64Control.ImageHeight = AppImageSize.ImageSizeXXL;
            _rectangleImageWithBase64Control.ImageWidth = AppImageSize.ImageSizeXXXL;

            // image in rectangle with border using base 64
            _rectangleImageWithBorderBase64Control.Source = Convert.FromBase64String("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");
            _rectangleImageWithBorderBase64Control.ImageHeight = AppImageSize.ImageSizeXXL;
            _rectangleImageWithBorderBase64Control.ImageWidth = AppImageSize.ImageSizeXXL;

            //image in circle without background 
            _circleImageWithoutBackgroundControl.Icon = "data:image/png;base64,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";
            _circleImageWithoutBackgroundControl.ImageHeight = AppImageSize.ImageSizeXXL;
            _circleImageWithoutBackgroundControl.ImageWidth = AppImageSize.ImageSizeXXL;

            //image in rectangle without background 
            _rectangleImageWithoutBackgroundControl.Value = "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";
            _rectangleImageWithoutBackgroundControl.ImageHeight = AppImageSize.ImageSizeXXL;
            _rectangleImageWithoutBackgroundControl.ImageWidth = AppImageSize.ImageSizeXXL;

            #endregion

            // Register events
            _goToPopUpPageButton.OnValueChanged += _goToPopUpPageButton_OnValueChanged;
            _primaryButton.OnValueChanged += OnSignInButtonClicked;
            _secondaryButton.OnValueChanged += OnSignInButtonClicked;
            _tertiaryButton.OnValueChanged += OnViewChartsDemoClicked;
            _transparentButton.OnValueChanged += OnViewListViewDemoClicked;
            _deleteButton.OnValueChanged += OnBackButtonClicked;

            _colorPicker.OnValueChanged += ColorPicker_PickedColorChanged1;
            _tabsControl.PageResources = PageData;
            _tabsControl.Value = "TabsControl Demo";
            _tabsControl.OnValueChanged += _tabsControl_OnValueChanged;
            _labelControlDemoButton.PageResources = _webViewControlDemoButton.PageResources = _calenderDemoButton.PageResources = _calenderControlDemoButton.PageResources = _uploadControlDemoButton.PageResources = PageData; _webViewControlDemoButton.Value = "WebView Controls Demo";
            _webViewControlDemoButton.OnValueChanged += _webViewControlDemoButton_OnValueChanged;
            _uploadControlDemoButton.Value = "Upload control Demo";
            _uploadControlDemoButton.OnValueChanged += _uploadControlDemoButton_OnValueChanged;
            _calenderControlDemoButton.Value = "Calendar Controls Demo";
            _calenderControlDemoButton.OnValueChanged += _calenderControlDemoButton_OnValueChanged;
            _calenderDemoButton.Value = "Calendar Demo";
            _calenderDemoButton.OnValueChanged += _calenderDemoButton_OnValueChanged;
            _labelControlDemoButton.Value = "Label Demo";
            _labelControlDemoButton.OnValueChanged += _labelControlDemoButton_OnValueChanged;
            _messageControlDemo.Value = "Message Control Demo";
            _messageControlDemo.OnValueChanged += _messageControlDemo_OnValueChanged;
            _buttonControlDemo.Value = "Button Control Demo";
            _buttonControlDemo.OnValueChanged += _buttonControlDemo_OnValueChanged;

            AppHelper.ShowBusyIndicator = false;
        });
    }

    private async void _messageControlDemo_OnValueChanged(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        //await ShellMasterPage.CurrentShell.PushMainPageAsync(new MessageControlDemo()).ConfigureAwait(false);
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(ErrorCode.NoInternetConnection.ToString())).ConfigureAwait(false);
    }

    private async void _buttonControlDemo_OnValueChanged(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new ButtonDemoPage()).ConfigureAwait(false);
    }

    protected override void OnDisappearing()
    {
        _goToPopUpPageButton.OnValueChanged -= _goToPopUpPageButton_OnValueChanged;
        _primaryButton.OnValueChanged -= OnSignInButtonClicked;
        _secondaryButton.OnValueChanged -= OnSignInButtonClicked;
        _tertiaryButton.OnValueChanged -= OnViewChartsDemoClicked;
        _transparentButton.OnValueChanged -= OnViewListViewDemoClicked;
        _deleteButton.OnValueChanged -= OnBackButtonClicked;
        //_linkLabel.OnValueChanged -= _linkLabel_OnValueChanged;

        base.OnDisappearing();
    }

    private async void OnSignInButtonClicked(object sender, EventArgs e)
    {
        var checkBoxListValues = _checkBoxList.Value;
        var colorBoxcheckBoxListValues = _colorBoxCheckBoxList.Value;
        var radioListValue = _radioList.Value;
        var selectedColorValue = _colorPicker.Value;

        var _singleDropdownValue = _singleDropdown.Value;
        var _singleDropdownValueValue = _singleEditableDropdown.Value;
        var _multiDropdownValue = _multiDropdown.Value;
        var _multiEditableDropdownValue = _multiEditableDropdown.Value;

        var _singleDisableDropdownValue = _singleDisableDropdown.Value;
        var _singleEditableDisableDropdownValue = _singleEditableDisableDropdown.Value;
        var _multiDisableDropdownValue = _multiDisableDropdown.Value;
        var _multiEditableDisableDropdownValue = _multiEditableDisableDropdown.Value;


        if (IsFormValid())
        {
            AppHelper.ShowBusyIndicator = true;
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
        }
    }

    private async void OnAuthClick(object sender, EventArgs e)
    {
        if (await CrossFingerprint.Current.IsAvailableAsync())
        {
            _pincodeView.ResetPincodeGrid();
            await _pincodeView.AuthenticateWithBiometricsAsync("").ConfigureAwait(true);
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new LoginPage()).ConfigureAwait(false);
    }

    private async void ShowAlert(object sender, int actionIndex)
    {
        if (actionIndex == 1)
        {
            await DisplayAlert("BioMetrics", "User Authenticated", "Ok");
        }
        else
        {
            await DisplayAlert("BioMetrics", "Authentication Failed", "Ok");
        }
    }

    private async void _goToPopUpPageButton_OnValueChanged(object sender, EventArgs e)
    {
        await MopupService.Instance.PushAsync(new PopUpDemoPage(this, null)).ConfigureAwait(false);
    }

    private async void OnViewListViewDemoClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new ListViewDemoPage()).ConfigureAwait(false);
    }

    private async void OnViewChartsDemoClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new ChartDemoPage()).ConfigureAwait(false);
    }

    private async void _tabsControl_OnValueChanged(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new TabsViewControlDemo()).ConfigureAwait(false);
    }
    private async void _webViewControlDemoButton_OnValueChanged(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new WebViewControlDemoPage()).ConfigureAwait(false);
    }

    private async void _uploadControlDemoButton_OnValueChanged(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new UploadControlDemoPage()).ConfigureAwait(false);
    }

    private async void _calenderControlDemoButton_OnValueChanged(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new CalenderDemoPage()).ConfigureAwait(false);

    }

    private async void _calenderDemoButton_OnValueChanged(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new CalenderDemoPage()).ConfigureAwait(false);

    }
    private async void _labelControlDemoButton_OnValueChanged(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new LabelControlDemoPage()).ConfigureAwait(false);
    }

    private void ColorPicker_PickedColorChanged1(object sender, EventArgs e)
    {
        _progressBarControl.StyleName = (string)_colorPicker.Value;
        var progress = (double)_progressBarControl.Value;
        progress += .1;
        if (progress > 1)
        {
            progress = 0.1;
        }
        // Color color = Color.FromHex(_colorPicker.Value as string);
        _progressBarControl.Value = progress;
    }

    private async void OnGeneratePdfButtonClicked(object sender, EventArgs e)
    {
        string FileName = "Invoice001";
        List<string> OrgDetail = new List<string>
        {
            "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",
        };

        Dictionary<string, string> Header = new Dictionary<string, string>
        {
            { "Program", "My Program" },
            { "Doctor Name", "Raj Shaan" },
            { "Patient Name", "Chetan Tej" },
            { "Enter Date", "April 12, 2023" },
        };

        List<TableModel> tableData = new List<TableModel>
        {
            new TableModel { Item = "Item 1", Amount = "50.00" },
            new TableModel { Item = "Item 2", Amount = "30.00" },
            new TableModel { Item = "Item 3", Amount = "20.00" },
            new TableModel { Item = "Total Amount", Amount = "100.00" },
            new TableModel { Item = "Discount", Amount = "10.00" },
            new TableModel { Item = "Amount Paid", Amount = "90.00" },
            new TableModel { Item = "Payment Mode", Amount = "Credit Card" },
        };

        //todo:
        //var stream = DocumentUtility.CreatePdf(FileName, OrgDetail, Header, tableData);
        //await ShellMasterPage.CurrentShell.PushMainPageAsync(new PdfViewerPage(stream)).ConfigureAwait(false);
    }

    private async void _linkLabel_OnValueChanged(object sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://www.google.com");
    }
    private async void _carouselButtonCLick(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new CarousalDemoPage()).ConfigureAwait(false);
    }
    private void AddView(View view)
    {
        AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 1, true);
        int index = PageLayout.Children?.Count() ?? 0;
        PageLayout.Add(view, 0, index);
    }
}