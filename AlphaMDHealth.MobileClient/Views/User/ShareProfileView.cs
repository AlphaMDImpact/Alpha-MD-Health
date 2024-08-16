using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class ShareProfileView : ViewManager
{
    private readonly CustomEntryControl _firstNameEntry;
    private readonly CustomEntryControl _middleNameEntry;
    private readonly CustomEntryControl _lastNameEntry;
    private readonly CustomMobileControl _mobileEntry;
    private readonly CustomEntryControl _emailEntry;
    private readonly CustomLabelControl _shareProgramsHeader;
    private readonly CustomCheckBoxListControl _programOptions;
    private readonly CustomBindablePickerControl _relationTypePicker;
    private readonly CustomButtonControl _actionButton;
    private readonly Grid _bodyGrid;
    private readonly UserDTO _userData = new UserDTO
    {
        User = new UserModel(),
        UserRelation = new UserRelationModel(),
        Programs = new List<PatientProgramModel>(),
        UserRelations = new List<UserRelationModel>()
    };
    public event EventHandler<EventArgs> OnSaveSuccess;

    public ShareProfileView(BasePage page, object param) : base(page, param)
    {

        var padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
        ParentPage.PageService = new UserService(App._essentials);
        _firstNameEntry = new CustomEntryControl
        {
            ControlResourceKey = ResourceConstants.R_FIRST_NAME_KEY,
            ControlType = FieldTypes.AlphaEntryControl
        };
        _middleNameEntry = new CustomEntryControl
        {
            ControlResourceKey = ResourceConstants.R_MIDDLE_NAME_KEY,
            ControlType = FieldTypes.AlphaEntryControl
        };
        _lastNameEntry = new CustomEntryControl
        {
            ControlResourceKey = ResourceConstants.R_LAST_NAME_KEY,
            ControlType = FieldTypes.AlphaEntryControl
        };
        _mobileEntry = new CustomMobileControl
        {
            ControlResourceKey = ResourceConstants.R_MOBILE_NUMBER_KEY,

        };
        _emailEntry = new CustomEntryControl
        {
            ControlResourceKey = ResourceConstants.R_EMAIL_ADDRESS_KEY,
            ControlType = FieldTypes.AlphaEntryControl,

        };
        _relationTypePicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_USER_RELATION_TYPE_KEY,
        };
        _shareProgramsHeader = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _programOptions = new CustomCheckBoxListControl
        {
            CheckBoxType = new OnIdiom<ListStyleType> { Phone = ListStyleType.SeperatorView, Tablet = ListStyleType.BoxView },
            ApplyMargin = true,
            Margin = DeviceInfo.Idiom == DeviceIdiom.Tablet ? new Thickness(-padding, padding) : new Thickness(-padding, padding, 0, padding),
        };
        _actionButton = new CustomButtonControl(ButtonType.PrimaryWithMargin)
        {
            VerticalOptions = LayoutOptions.End,
            IsVisible = false,
            Margin = new Thickness(0, 0, 0, padding),
        };
        _bodyGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
            BackgroundColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR)
        };
        Grid mainGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
            new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
            BackgroundColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR)
        };
        _bodyGrid.Add(_firstNameEntry, 0, 0);
        _bodyGrid.Add(_middleNameEntry, 0, 1);
        _bodyGrid.Add(_lastNameEntry, 0, 2);
        _bodyGrid.Add(_mobileEntry, 0, 3);
        _bodyGrid.Add(_emailEntry, 0, 4);
        _bodyGrid.Add(_relationTypePicker, 0, 5);
        _bodyGrid.Add(_shareProgramsHeader, 0, 6);
        _bodyGrid.Add(_programOptions, 0, 7);
        mainGrid.Add(_bodyGrid, 0, 0);
        mainGrid.Add(_actionButton, 0, 1);
        Content = mainGrid;
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        _userData.UserRelation.PatientCareGiverID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(UserRelationModel.PatientCareGiverID)));
        _userData.SelectedUserID = App._essentials.GetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0);
        _userData.User.RoleID = (byte)RoleName.CareTaker;
        await (ParentPage.PageService as UserService).GetShareProfileDataAsync(_userData).ConfigureAwait(true);
        if (_userData.ErrCode == ErrorCode.OK)
        {
            ParentPage.PageData = ParentPage.PageService.PageData;
            ParentPage.PageData.CountryCodes = _userData.CountryCodes;
            await AssignControlResourcesAndData();
        }
        else
        {
            OnSaveSuccess?.Invoke(_userData.ErrCode.ToString(), new EventArgs());
            //todo:await Navigation.PopAllPopupAsync();
        }
    }

    private Task AssignControlResourcesAndData()
    {
        _firstNameEntry.PageResources = ParentPage.PageData;
        _middleNameEntry.PageResources = ParentPage.PageData;
        _lastNameEntry.PageResources = ParentPage.PageData;
        _emailEntry.PageResources = ParentPage.PageData;
        _mobileEntry.CountrySource = ParentPage.PageData;
        _mobileEntry.PageResources = ParentPage.PageData;
        _relationTypePicker.PageResources = ParentPage.PageData;
        _emailEntry.RegexPattern = ParentPage.GetSettingsValueByKey(SettingsConstants.S_EMAIL_REGEX_KEY);
        _mobileEntry.SetSelectedCountryId(0);
        _relationTypePicker.ItemSource = _userData.UserRelationTypes;
        _shareProgramsHeader.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_SHARE_PROGRAMS_KEY);
        _actionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_SEND_ACTIVATION_AGAIN_KEY);
        _actionButton.Clicked += ResendActionButton_Clicked;
        if (GenericMethods.IsListNotEmpty(_userData.Programs))
        {
            _programOptions.RightViewes.Clear();

            foreach (var a in from program in _userData.Programs
                              select new
                              {
                                  Key = program.ProgramID,
                                  Value = CreateColorView(program) as View
                              })
            {
                if (_programOptions.RightViewes.ContainsKey(a.Key))
                {
                    _programOptions.RightViewes[a.Key] = a.Value;
                }
                else
                {
                    _programOptions.RightViewes.Add(a.Key, a.Value);
                }
            }

            _programOptions.SetOptions((from program in _userData.Programs
                                        select new OptionSelectModel
                                        {
                                            OptionID = program.ProgramID,
                                            Value = program.ProgramID.ToString(CultureInfo.InvariantCulture),
                                            DisplayText = program.Name,
                                            IsSelected = program.IsActive
                                        }).ToList());
            _bodyGrid.Add(_programOptions, 0, 7);
            //  return;
        }
        if (_userData.UserRelation.PatientCareGiverID > 0 && _userData?.User != null)
        {
            _mobileEntry.IsEnabled = false;
            _emailEntry.IsEnabled = false;
            _firstNameEntry.Value = _userData.User.FirstName;
            _middleNameEntry.Value = _userData.User.MiddleName;
            _lastNameEntry.Value = _userData.User.LastName;
            _mobileEntry.Value = _userData.User.PhoneNo;
            _emailEntry.Value = _userData.User.EmailId;
            _relationTypePicker.SelectedValue = _userData.UserRelationTypes.FirstOrDefault(x => x.OptionID == _userData.User.RelationID)?.OptionID ?? 0;
            if (_userData.User.IsTempPassword)
            {
                _actionButton.IsVisible = true;
            }
        }
        return Task.CompletedTask;
    }

    private async void ResendActionButton_Clicked(object sender, EventArgs e)
    {
        _actionButton.Clicked -= ResendActionButton_Clicked;
        AppHelper.ShowBusyIndicator = true;
        UserDTO user = new UserDTO
        {
            User = new UserModel
            {
                UserID = _userData.User.UserID,
                IsUser = _userData.User.IsUser
            }
        };
        await (ParentPage.PageService as UserService).ResendActivationAsync(user, new CancellationToken()).ConfigureAwait(true);
        user.ErrorDescription = ParentPage.GetResourceValueByKey(user.ErrCode.ToString());
        AppHelper.ShowBusyIndicator = false;
        OnSaveSuccess?.Invoke(user, new EventArgs());
        _actionButton.Clicked += ResendActionButton_Clicked;
    }

    private BoxView CreateColorView(PatientProgramModel program)
    {
        const double size = (double)AppImageSize.ImageSizeD;
        var colorView = new BoxView
        {
            StyleId = program.ProgramID.ToString(CultureInfo.InvariantCulture),
            BackgroundColor = program.ProgramGroupIdentifier == null ? (default) : Color.FromArgb(program.ProgramGroupIdentifier),
            HeightRequest = size,
            WidthRequest = size,
            CornerRadius = size / 2,
            VerticalOptions = LayoutOptions.Center,
            FlowDirection = (FlowDirection)App.Current.Resources[StyleConstants.ST_FLOW_DIRECTION]
        };
        return colorView;
    }

    private void MapAndSaveUserSharedProfile()
    {
        _userData.PatientsSharedPrograms = (from x in _userData.Programs
                                            let isActive = _programOptions.SelectedIndexValues.Contains(x.ProgramID.ToString(CultureInfo.InvariantCulture))
                                            where isActive
                                            select new PatientsSharedProgramsModel
                                            {
                                                PatientProgramID = x.PatientProgramID,
                                                PatientCareGiverID = _userData.UserRelation.PatientCareGiverID,
                                                IsActive = isActive
                                            }).ToList();
        _userData.User.RoleID = (byte)RoleName.CareTaker;
        _userData.User.RelationID = Convert.ToInt16(_relationTypePicker.SelectedValue, CultureInfo.InvariantCulture);
        _userData.User.FirstName = _firstNameEntry.Value;
        _userData.User.MiddleName = _middleNameEntry.Value;
        _userData.User.LastName = _lastNameEntry.Value;
        _userData.User.PhoneNo = _mobileEntry.Value;
        _userData.User.EmailId = _emailEntry.Value;
        _userData.User.OrganisationID = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 0);
        _userData.User.RoleAtLevelID = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0);
        _userData.IsActive = false;
        _userData.Users = new List<UserModel> { _userData.User };
    }

    public override Task UnloadUIAsync()
    {
        _actionButton.Clicked -= ResendActionButton_Clicked;
        return Task.CompletedTask;
    }

    internal async Task OnSaveButtonClickedAsync()
    {
        MapAndSaveUserSharedProfile();
        if (ParentPage.IsFormControlValid(_bodyGrid))
        {
            if (_userData.PatientsSharedPrograms?.Any(x => x.IsActive) ?? false)
            {
                _userData.User.IsActive = true;
                _userData.RecordCount = -13;
                _userData.User.IsUser = true;
                await (ParentPage.PageService as UserService).SaveShareProfileDataAsync(_userData, CancellationToken.None).ConfigureAwait(true);
                if (_userData.ErrCode == ErrorCode.OK)
                {
                    _userData.ErrorDescription = ParentPage.GetResourceValueByKey(_userData.ErrCode.ToString());
                }
                else
                {
                    _userData.ErrorDescription = ParentPage.GetResourceValueByKey(_userData.ErrCode.ToString());
                }
            }
            else
            {
                _userData.ErrCode = ErrorCode.InvalidData;
                _userData.ErrorDescription = ParentPage.GetResourceValueByKey(ResourceConstants.R_PROGRAM_SELECTION_MANDATORY_KEY);
            }
            AppHelper.ShowBusyIndicator = true;
            _ = await new BasePage().SyncDataWithServerAsync(Pages.ShareProfilePage, false, default).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
            OnSaveSuccess?.Invoke(_userData, new EventArgs());
        }
    }
}