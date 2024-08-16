using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class RegisterMoreInfoPage : LoginBasePage
{
    private readonly AmhSingleSelectDropDownControl _genderPicker;
    private readonly AmhSingleSelectDropDownControl _languagePicker;
    private readonly AmhDateTimeControl _dob;
    private readonly AmhLabelControl _headerLabel;
    private readonly AmhButtonControl _saveButton;
    private readonly UserDTO _userData = new UserDTO { User = new UserModel(), Languages = new List<OptionModel>(), Genders = new List<OptionModel>() };

    public RegisterMoreInfoPage() : base(PageLayoutType.LoginFlowPageLayout, false)
    {
        double appPadding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
        _headerLabel = new AmhLabelControl(FieldTypes.PrimaryLargeHVCenterLabelControl);
        _dob = new AmhDateTimeControl
        {
            // IsUnderLine = false,
            // ShowHeader = false,
            FieldType = FieldTypes.DateControl,
            // IsBackGroundTransparent = false,
            ResourceKey = ResourceConstants.R_DATE_OF_BIRTH_KEY
        };
        _genderPicker = new AmhSingleSelectDropDownControl
        {
            ResourceKey = ResourceConstants.R_GENDER_KEY
        };
        _languagePicker = new AmhSingleSelectDropDownControl
        {
            ResourceKey = ResourceConstants.R_PREFERRED_LANGUAGE_KEY
        };
        //_genderPicker = CreatePickerControl(ResourceConstants.R_GENDER_KEY);
        //_languagePicker = CreatePickerControl(ResourceConstants.R_PREFERRED_LANGUAGE_KEY);
        _saveButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, appPadding),
        };
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        AddRowColumnDefinition(GridLength.Auto, 1, true);
        AddRowColumnDefinition(GridLength.Star, 1, true);
        AddRowColumnDefinition(GridLength.Auto, 1, true);

        Grid _maingrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
            }
        };

        PageLayout.Add(_headerLabel, 0, 0);
        _maingrid.Add(_dob, 0, 0);
        _maingrid.Add(_genderPicker, 0, 1);
        _maingrid.Add(_languagePicker, 0, 2);
        PageLayout.Add(new ScrollView { Content = _maingrid }, 0, 1);
        PageLayout.Add(_saveButton, 0, 2);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await new UserService(App._essentials).GetRegisterUserInfoAsync(_userData).ConfigureAwait(true);
        PageData.Resources = _userData.Resources;
        PageData.Settings = _userData.Settings;
        if (_userData.ErrCode == ErrorCode.OK)
        {
            DisplayControls(true);
            AssignControlResources();
            _saveButton.IsVisible = true;
            await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true);
            _headerLabel.Value = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_REGISTER_MORE_KEY);
            _saveButton.Value = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_NEXT_ACTION_KEY);
            _saveButton.OnValueChanged += OnNextButtonClicked;
        }

        else
        {
            _saveButton.IsVisible = false;
            DisplayControls(false);
            //todo
            //PageLayout.Add(new AmhMessageControl(false)
            //{
            //    PageResources = PageData,
            //    VerticalOptions = LayoutOptions.CenterAndExpand,
            //    ControlResourceKey = _userData.ErrCode.ToString()
            //}, 0, 6);
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private void DisplayControls(bool isVisible)
    {
        _dob.IsVisible = _genderPicker.IsVisible = _languagePicker.IsVisible = isVisible;
    }

    private async void OnNextButtonClicked(object sender, EventArgs e)
    {
        // in case of complete profile, user cannot continue without internet connection
        if (!await CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            return;
        }
        _saveButton.OnValueChanged -= OnNextButtonClicked;
        if (IsFormValid())
        {
            AppHelper.ShowBusyIndicator = true;
            MapAndSaveData();
            await new UserService(App._essentials).SaveUsersAsync(_userData).ConfigureAwait(true);
            if (_userData.ErrCode == ErrorCode.OK)
            {
                ////, App._essentials.SetPreferenceValue(StorageConstants.PR_IS_REGISTRATION_FLOW_KEY, false);
                await NavigateOnNextPageAsync(false, true, LoginFlow.ProfilePage).ConfigureAwait(true);
            }
            else
            {
                AppHelper.ShowBusyIndicator = false;
                DisplayOperationStatus(LibResources.GetResourceValueByKey(PageData?.Resources, _userData.ErrCode.ToString()));
            }
        }
        _saveButton.OnValueChanged += OnNextButtonClicked;
    }

    protected override void OnDisappearing()
    {
        _saveButton.OnValueChanged -= OnNextButtonClicked;
        base.OnDisappearing();
    }

    private void MapAndSaveData()
    {
        _userData.User.UserID = App._essentials.GetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0);
        if (_genderPicker.Value > 0)
        {
            _userData.User.GenderID = _userData.Genders.FirstOrDefault(x => x.OptionID == _genderPicker.Value).GroupName;
        }
        if (_languagePicker.Value > 0)
        {
            _userData.User.PrefferedLanguageID = Convert.ToByte(_languagePicker.Value, CultureInfo.InvariantCulture);
        }
        _userData.User.Dob = _dob.Value.Value.ToUniversalTime();
        _userData.User.IsSynced = false;
        _userData.Users = new List<UserModel> { _userData.User };
    }

    private void AssignControlResources()
    {
        _dob.PageResources = PageData;
        _genderPicker.PageResources = PageData;
        _languagePicker.PageResources = PageData;
        _languagePicker.Options = _userData.Languages;
        _genderPicker.Options = _userData.Genders;
    }

    /*  private CustomBindablePickerControl CreatePickerControl(string resourceKey)
      {
          return new CustomBindablePickerControl
          {
              IsUnderLine = false,
              ShowHeader = false,
              IsBackGroundTransparent = false,
              ControlResourceKey = resourceKey,
          };
      }*/
}
