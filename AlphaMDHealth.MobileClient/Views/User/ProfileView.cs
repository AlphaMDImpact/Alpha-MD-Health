using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class ProfileView : BaseAttachmentView
{
    private readonly CustomEntryControl _firstNameEntry;
    private readonly CustomEntryControl _middleNameEntry;
    private readonly CustomEntryControl _lastNameEntry;
    private readonly CustomEntryControl _userdegree;
    private readonly CustomMobileControl _mobileEntry;
    private readonly CustomEntryControl _emailEntry;
    private readonly CustomBindablePickerControl _genderPicker;
    private readonly CustomBindablePickerControl _languagePicker;
    private readonly CustomBindablePickerControl _bloodPressurePicker;
    private readonly CustomEntryControl _professionEntry;
    private readonly CustomEntryControl _roleEntry;
    private readonly CustomEntryControl _socialSecurityNumber;
    private readonly CustomEntryControl _internalCode;
    private readonly CustomEntryControl _externalCode;
    private readonly CustomDateTimeControl _dob;
    private readonly CustomLabelControl _headerLabel;
    private readonly CustomLabelControl _instructionLabel;
    private readonly CustomLabelControl _fullNameLabel;
    private readonly CustomLabelControl _ageLabel;
    private readonly CustomImageControl _userProfilePhoto;
    private readonly CustomMessageControl _emptyView;
    private readonly TapGestureRecognizer _profilePhotoTapped;
    private readonly bool _isUser, _isPatient, _isSinglePatientAdd, _isLinkedUser;
    private readonly Grid _mainGrid;
    private readonly UserDTO _userData = new UserDTO { User = new UserModel(), Languages = new List<OptionModel>(), Genders = new List<OptionModel>() };
    private readonly ShareProfilesView _shareProfileView;
    private byte _roleID;

    /// <summary>
    /// Is save in progress flag to restrict cancel button event in between
    /// </summary>
    public bool IsSaveInProgress = false;

    /// <summary>
    /// OnDateSelectedValueChanged Event
    /// </summary>
    public event EventHandler<EventArgs> OnSaveSuccess;

    /// <summary>
    /// View to display profile page components
    /// </summary>
    /// <param name="page">Instance of base page</param>
    /// <param name="parameters">View parameters</param>
    public ProfileView(BasePage page, object parameters) : base(page, parameters)
    {
        if (Parameters?.Count > 0)
        {
            _userData.User.IsLinkedUser = _isLinkedUser = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(UserModel.IsLinkedUser)));
        }
        if (_isLinkedUser)
        {
            _roleID = (int)RoleName.Patient;
        }
        else
        {
            _roleID = (byte)App._essentials.GetPreferenceValue(StorageConstants.PR_ROLE_ID_KEY, 0);
        }

        var padding = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        _isSinglePatientAdd = GenericMethods.MapValueType<bool>(GetParameterValue(Constants.IS_PATIENTS_ADD_PAGE));
        _isUser = _roleID != (int)RoleName.Patient;
        _isPatient = _roleID == (int)RoleName.Patient;
        _parent.PageService = new UserService(App._essentials);
        _headerLabel = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft) { Margin = new Thickness(0, padding) };
        _instructionLabel = new CustomLabelControl(LabelType.PrimarySmallCenter) { Margin = new Thickness(padding, 0, padding, padding) };
        _fullNameLabel = new CustomLabelControl(LabelType.PrimaryMediumCenter) { Margin = new Thickness(padding, padding, padding, 0) };
        _ageLabel = new CustomLabelControl(LabelType.SecondrySmallCenter) { Margin = new Thickness(padding, 0, padding, padding) };
        _userProfilePhoto = new CustomImageControl(AppImageSize.ImageSizeXXL, AppImageSize.ImageSizeXXL, string.Empty, ImageConstants.I_AVTAR_NEW_PNG, false);
        _profilePhotoTapped = new TapGestureRecognizer();
        _userProfilePhoto.GestureRecognizers.Add(_profilePhotoTapped);

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

        _bloodPressurePicker = CreatePickerControl(ResourceConstants.R_BLOOD_GROUP_KEY);
        if (!_userData.User.IsLinkedUser)
        {
            _mobileEntry = new CustomMobileControl
            {
                ControlResourceKey = ResourceConstants.R_MOBILE_NUMBER_KEY,
                IsEnabled = _isSinglePatientAdd
            };
            _emailEntry = new CustomEntryControl
            {
                ControlResourceKey = ResourceConstants.R_EMAIL_ADDRESS_KEY,
                ControlType = FieldTypes.EmailEntryControl,
                IsEnabled = _isSinglePatientAdd,
            };
            _languagePicker = CreatePickerControl(ResourceConstants.R_PREFERRED_LANGUAGE_KEY);

            _professionEntry = new CustomEntryControl
            {
                ControlResourceKey = ResourceConstants.R_PROFESSION_TEXT_KEY,
                ControlType = FieldTypes.TextEntryControl,
                IsEnabled = false
            };
            _userdegree = new CustomEntryControl
            {
                ControlResourceKey = ResourceConstants.R_DEGREE_KEY,
                ControlType = FieldTypes.TextEntryControl,
                IsEnabled = false
            };
            _roleEntry = new CustomEntryControl
            {
                ControlResourceKey = ResourceConstants.R_ROLES_KEY,
                ControlType = FieldTypes.TextEntryControl,
                IsEnabled = false
            };
            _socialSecurityNumber = new CustomEntryControl
            {
                ControlResourceKey = ResourceConstants.R_SOCIAL_SECURITY_NUMBER_KEY,
                ControlType = FieldTypes.TextEntryControl
            };
            _internalCode = new CustomEntryControl
            {
                ControlResourceKey = ResourceConstants.R_INTERNAL_CODE_KEY,
                ControlType = FieldTypes.TextEntryControl
            };
            _externalCode = new CustomEntryControl
            {
                ControlResourceKey = ResourceConstants.R_EXTERNAL_CODE_KEY,
                ControlType = FieldTypes.TextEntryControl
            };
        }

        _dob = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = _isUser && !_isSinglePatientAdd ? ResourceConstants.R_DATE_OF_JOINING_KEY : ResourceConstants.R_DATE_OF_BIRTH_KEY,
            IsEnabled = !_isUser || _isSinglePatientAdd
        };
        _genderPicker = CreatePickerControl(ResourceConstants.R_GENDER_KEY);

        _emptyView = new CustomMessageControl(false) { VerticalOptions = LayoutOptions.CenterAndExpand, IsVisible = false };

        _mainGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions = CalculateRowCalculation(),
            ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
            BackgroundColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR)
        };
        _mainGrid.Add(_headerLabel, 0, 0);
        _mainGrid.Add(_firstNameEntry, 0, 1);
        _mainGrid.Add(_middleNameEntry, 0, 2);
        _mainGrid.Add(_lastNameEntry, 0, 3);

        if (_userData.User.IsLinkedUser)
        {
            _mainGrid.Add(_dob, 0, 4);
            _mainGrid.Add(_genderPicker, 0, 5);
            _mainGrid.Add(_bloodPressurePicker, 0, 6);
        }
        else
        {
            _mainGrid.Add(_mobileEntry, 0, 4);
            _mainGrid.Add(_emailEntry, 0, 5);

            if (_roleID != (int)RoleName.CareTaker)
            {
                _mainGrid.Add(_dob, 0, 6);
                _mainGrid.Add(_genderPicker, 0, 7);
            }
            if (_isUser && !_isSinglePatientAdd)
            {
                if (_roleID != (int)RoleName.CareTaker)
                {
                    _mainGrid.Add(_professionEntry, 0, 8);
                    _mainGrid.Add(_userdegree, 0, 9);

                }
                _mainGrid.Add(_roleEntry, 0, 10);
            }
            else
            {
                _mainGrid.Add(_bloodPressurePicker, 0, 8);
                _mainGrid.Add(_languagePicker, 0, 9);
            }
        }
        Grid bodyGrid = new Grid
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
                new RowDefinition { Height = GridLength.Star }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };
        if (_isSinglePatientAdd || _isLinkedUser)
        {
            if (!_isLinkedUser)
            {
                _mainGrid.Add(_bloodPressurePicker, 0, 11);
                _mainGrid.Add(_socialSecurityNumber, 0, 12);
                _mainGrid.Add(_internalCode, 0, 13);
                _mainGrid.Add(_externalCode, 0, 14);
            }
            bodyGrid.Add(_instructionLabel, 0, 0);
            bodyGrid.Add(_userProfilePhoto, 0, 1);
            bodyGrid.Add(_fullNameLabel, 0, 2);
            bodyGrid.Add(_ageLabel, 0, 3);
            bodyGrid.Add(_mainGrid, 0, 4);
            bodyGrid.Add(_emptyView, 0, 5);
            Content = bodyGrid;
        }
        else
        {
            _parent.SetPageLayoutOption(LayoutOptions.FillAndExpand, false);
            _parent.AddRowColumnDefinition(GridLength.Auto, 7, true);
            _parent.AddRowColumnDefinition(GridLength.Star, 1, true);
            _parent.PageLayout.Add(_instructionLabel, 0, 0);
            _parent.PageLayout.Add(_userProfilePhoto, 0, 1);
            _parent.PageLayout.Add(_fullNameLabel, 0, 2);
            _parent.PageLayout.Add(_ageLabel, 0, 3);
            _parent.PageLayout.Add(_mainGrid, 0, 4);
            _parent.PageLayout.Add(_emptyView, 0, 5);
            if (_isPatient)
            {
                _shareProfileView = new ShareProfilesView();
                _parent.PageLayout.Add(_shareProfileView, 0, 6);
            }

        }
    }

    private RowDefinitionCollection CalculateRowCalculation()
    {
        if (_userData.User.IsLinkedUser)
        {
            return new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            };
        }
        else
        {
            return new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },

            };
        }
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        if (_userData.User != null)
        {
            _userData.User.AccountID = _isSinglePatientAdd || _isLinkedUser
                ? 0
                : App._essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, (long)0);
            _userData.User.RoleID = _isSinglePatientAdd || _isLinkedUser
                ? (byte)RoleName.Patient
                : _roleID;
            if (_isLinkedUser)
            {
                _userData.User.UserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(UserModel.UserID)));
            }
            else
            {
                _userData.User.UserID = _isSinglePatientAdd || _userData.User.RoleID == (int)RoleName.CareTaker
                    ? 0
                    : App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0);
            }
            //await (_parent.PageService as UserService).GetUserAsync(_userData, false, true).ConfigureAwait(true);//todo:
            _parent.PageData = _parent.PageService.PageData;
            if (!_isLinkedUser)
            {
                _parent.PageData.CountryCodes = _userData.CountryCodes;
            }
            if (_isPatient && !_isLinkedUser)
            {
                await _shareProfileView.LoadUIAsync(_userData, _parent);
                _shareProfileView.OnListRefresh += ShareProfileView_OnListRefresh;
            }
            if (_userData.ErrCode == ErrorCode.OK)
            {
                SetControlVisibility(true);
                AssignControlResources();
                await AssignControlValues().ConfigureAwait(true);
                _headerLabel.Text = _parent.GetResourceValueByKey(ResourceConstants.R_PROFILE_EDIT_KEY);
                _instructionLabel.Text = _parent.GetResourceValueByKey(ResourceConstants.R_PROFILE_INFO_KEY);
                _profilePhotoTapped.Tapped += ProfilePhotoTapped_Tapped;
            }
            else
            {
                SetControlVisibility(false);
                _emptyView.IsVisible = true;
                _emptyView.ControlResourceKey = _userData.ErrCode.ToString();
                _emptyView.PageResources = _parent.PageData;
            }
        }
    }
    private async void ShareProfileView_OnListRefresh(object sender, EventArgs e)
    {
        await LoadUIAsync(true).ConfigureAwait(true);
    }

    /// <summary>
    /// Use to unload page data
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        _profilePhotoTapped.Tapped -= ProfilePhotoTapped_Tapped;
        if (_isPatient && !_isLinkedUser)
        {
            _shareProfileView.OnListRefresh -= ShareProfileView_OnListRefresh;
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// refresh list
    /// </summary>
    public void RefreshList()
    {
        InvokeListRefresh(Guid.Empty, new EventArgs());
    }

    /// <summary>
    /// Validate and Save Data
    /// </summary>
    /// <returns>Result of operations</returns>
    internal async Task OnDeleteButtonClickedAsync()
    {
        _userData.User.IsActive = false;
        _userData.User.IsSynced = false;
        await SaveUserDataAsync().ConfigureAwait(true); ;
    }
    /// <summary>
    /// Validate and Save Data
    /// </summary>
    /// <returns>Result of operations</returns>
    internal async Task OnSaveButtonClickedAsync()
    {
        bool isValid = _isSinglePatientAdd || _isLinkedUser
            ? _parent.IsFormValid(this)
            : _parent.IsFormValid();
        if (isValid)
        {
            AppHelper.ShowBusyIndicator = true;
            MapAndSaveData();
            _userData.User.IsMobile = _isSinglePatientAdd;
            await SaveUserDataAsync().ConfigureAwait(true);
        }
        else
        {
            IsSaveInProgress = false;
        }
    }

    private async Task SaveUserDataAsync()
    {
        if (_isLinkedUser)
        {
            _userData.User.IsLinkedUser = true;
        }
        _userData.Users = new List<UserModel> { _userData.User };
        await (_parent.PageService as UserService).SaveUsersAsync(_userData).ConfigureAwait(true);
        if (_userData.ErrCode == ErrorCode.OK)
        {
            if (_isSinglePatientAdd || _isLinkedUser)
            {
                _parent.DisplayOperationStatus(_parent.GetResourceValueByKey(_userData.ErrCode.ToString()), true);
                if (MobileConstants.CheckInternet)
                {
                    await (_parent.PageService as UserService).SyncUserToServerAsync(_userData, CancellationToken.None).ConfigureAwait(true);
                    if (_userData.ErrCode != ErrorCode.OK)
                    {
                        _userData.ErrorDescription = _parent.GetResourceValueByKey(_userData.ErrCode.ToString());
                    }
                }
                OnSaveSuccess?.Invoke(_userData, new EventArgs());
                AppHelper.ShowBusyIndicator = false;
            }
            else
            {
                _ = _parent.SyncDataWithServerAsync(Pages.ProfilePage, false, default).ConfigureAwait(false);
                await ShellMasterPage.CurrentShell.RenderPageAsync().ConfigureAwait(true);
            }
        }
        else
        {
            AppHelper.ShowBusyIndicator = false;
            if (_isSinglePatientAdd)
            {
                _userData.ErrorDescription = _parent.GetResourceValueByKey(_userData.ErrCode.ToString());
                OnSaveSuccess?.Invoke(_userData, new EventArgs());
            }
            else
            {
                _parent.DisplayOperationStatus(_parent.GetResourceValueByKey(_userData.ErrCode.ToString()));
            }
        }
    }

    private async void ProfilePhotoTapped_Tapped(object sender, EventArgs e)
    {
        _profilePhotoTapped.Tapped -= ProfilePhotoTapped_Tapped;
        MaxFileUploadSize = _parent.GetSettingsValueByKey(SettingsConstants.S_IMAGE_COMPRESSED_RESOLUTION_KEY);
        SupportedFileTypes = _parent.GetSettingsValueByKey(SettingsConstants.S_UPLOAD_SUPPORTED_FILE_TYPE_KEY);
        if (_userData.User.ImageName != null && _userData.User.ImageName.Length > 0)
        {
            _base64String = _userData.User.ImageName;
        }
        List<string> actions = Actionlist(FieldTypes.UploadControl);
        await ImageSourceSelectionAsync(actions).ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(_base64String) && !string.IsNullOrWhiteSpace(_uploadedFileExtension) && _base64String != ImageConstants.I_UPLOAD_ICON_PNG)
        {
            if (GenericMethods.IsExtensionSupported(SupportedFileTypes, _uploadedFileExtension))
            {
                _userData.User.ImageName = GetBase64WithPrefix();
                _userData.User.ImageName = FileNameWithExtention;
                //todo:_userData.User.ImageSource = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(_base64String));
            }
            else
            {
                await _parent.DisplayMessagePopupAsync(
                    _parent.GetResourceValueByKey(ResourceConstants.R_SUPPORTED_UPLOAD_FILE_TYPE_KEY).Replace("{0}", SupportedFileTypes),
                    _parent.OnPupupActionClicked, true, false, true).ConfigureAwait(true);
            }
        }
        _base64String = ImageConstants.I_UPLOAD_ICON_PNG;
        _profilePhotoTapped.Tapped += ProfilePhotoTapped_Tapped;
    }

    private void SetControlVisibility(bool isVisible)
    {
        _userProfilePhoto.IsVisible = _instructionLabel.IsVisible = _fullNameLabel.IsVisible = _ageLabel.IsVisible = _mainGrid.IsVisible = isVisible;
    }

    private void MapAndSaveData()
    {
        if (_genderPicker.SelectedValue > 0)
        {
            _userData.User.GenderID = _userData.Genders.FirstOrDefault(x => x.OptionID == _genderPicker.SelectedValue).GroupName;
        }
        if (!_isLinkedUser && _languagePicker.SelectedValue > 0 && (!_isUser || _isSinglePatientAdd))
        {
            _userData.User.PrefferedLanguageID = Convert.ToByte(_languagePicker.SelectedValue, CultureInfo.InvariantCulture);
        }
        if (_isUser && !_isSinglePatientAdd)
        {
            _userData.User.Proffession = _professionEntry.Value;
            _userData.User.RoleName = _roleEntry.Value;
            _userData.User.Doj = _dob.GetSetDate.Value.ToUniversalTime();
            _userData.User.UserDegrees = string.Join(Constants.PIPE_SEPERATOR, _userData.UserDegrees?.Where(y => y.IsSelected)?.Select(x => x.OptionID));
        }
        else
        {
            _userData.User.Dob = _dob.GetSetDate == null ? (DateTimeOffset)_dob.GetSetDate : _dob.GetSetDate.Value.ToUniversalTime();
            if (_bloodPressurePicker?.SelectedValue > 0)
            {
                _userData.User.BloodGroupID = Convert.ToInt16(_bloodPressurePicker.SelectedValue, CultureInfo.InvariantCulture);
            }
            if (_isSinglePatientAdd)
            {
                _userData.User.EmailId = _emailEntry.Value;
                _userData.User.PhoneNo = _mobileEntry.Value;
                _userData.User.SocialSecurityNo = _socialSecurityNumber.Value;
                _userData.User.GeneralMedicalIdenfier = _externalCode.Value;
                _userData.User.HospitalIdenfier = _internalCode.Value;
            }
        }

        _userData.User.FirstName = _firstNameEntry.Value;
        _userData.User.MiddleName = _middleNameEntry.Value;
        _userData.User.LastName = _lastNameEntry.Value;
        _userData.User.IsSynced = false;
        _userData.User.IsActive = true;
        _userData.User.OrganisationID = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 0);

    }

    private void AssignControlResources()
    {
        LibSettings.TryGetDateFormatSettings(_parent.PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        _firstNameEntry.RegexPattern = _parent.GetSettingsValueByKey(SettingsConstants.S_ALPHA_REGEX_KEY);
        _lastNameEntry.RegexPattern = _parent.GetSettingsValueByKey(SettingsConstants.S_ALPHA_REGEX_KEY);
        _firstNameEntry.PageResources = _parent.PageData;
        _middleNameEntry.PageResources = _parent.PageData;
        _lastNameEntry.PageResources = _parent.PageData;
        if (!_isLinkedUser)
        {
            _userdegree.PageResources = _parent.PageData;
            _mobileEntry.CountrySource = _parent.PageData;
            _mobileEntry.PageResources = _parent.PageData;
            _mobileEntry.SetSelectedCountryId(0);
            _emailEntry.PageResources = _parent.PageData;
            _emailEntry.RegexPattern = _parent.GetSettingsValueByKey(SettingsConstants.S_EMAIL_REGEX_KEY);
            _languagePicker.PageResources = _parent.PageData;
            _professionEntry.PageResources = _parent.PageData;
            _roleEntry.PageResources = _parent.PageData;
            _socialSecurityNumber.PageResources = _parent.PageData;
            _internalCode.PageResources = _parent.PageData;
            _externalCode.PageResources = _parent.PageData;
        }
        _dob.PageResources = _parent.PageData;
        _dob.DateFormat = GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat);
        _genderPicker.PageResources = _parent.PageData;
        _bloodPressurePicker.PageResources = _parent.PageData;
    }

    private async Task AssignControlValues()
    {
        AssignPhoto();
        if (_userData.User?.UserID > 0 || _userData.User?.UserTempID != 0)
        {
            AssignAge();
            if (_isUser && _roleID != (int)RoleName.CareTaker)
            {
                _userdegree.Value = string.Join(Constants.PIPE_SEPERATOR_WITH_SPACE, _userData.UserDegrees?.Where(y => y.IsSelected)?.Select(x => x.OptionText));
            }
            _firstNameEntry.Value = _userData.User.FirstName;
            _middleNameEntry.Value = _userData.User.MiddleName;
            _lastNameEntry.Value = _userData.User.LastName;

            if (!_isLinkedUser)
            {
                _mobileEntry.Value = _userData.User.PhoneNo;
                _emailEntry.Value = _userData.User.EmailId;
                _professionEntry.Value = _userData.User.Proffession;
                _roleEntry.Value = _userData.User.RoleName;
            }
            else
            {
                _bloodPressurePicker.ItemSource = _userData.BloodGroups;
                _bloodPressurePicker.SelectedValue = _userData.BloodGroups?.FirstOrDefault(x => x.OptionID == _userData.User.BloodGroupID)?.OptionID ?? 0;
            }
            await Task.Delay(Constants.DATE_RENDER_DELAY).ConfigureAwait(true);
            _dob.GetSetDate = _isUser ? App._essentials.ConvertToLocalTime(_userData.User.Doj.Value.DateTime).Date : App._essentials.ConvertToLocalTime(_userData.User.Dob.Value.DateTime).Date;
            _genderPicker.ItemSource = _userData.Genders;
            _genderPicker.SelectedValue = _userData.Genders.FirstOrDefault(x => x.GroupName == _userData.User.GenderID)?.OptionID ?? 0;
            if (!_isUser && !_isLinkedUser)
            {
                _languagePicker.ItemSource = _userData.Languages;
                _languagePicker.SelectedValue = _userData.User.PrefferedLanguageID;
            }
            if (_roleID == (int)RoleName.Patient)
            {
                _bloodPressurePicker.ItemSource = _userData.BloodGroups;
                _bloodPressurePicker.SelectedValue = _userData.BloodGroups?.FirstOrDefault(x => x.OptionID == _userData.User.BloodGroupID)?.OptionID ?? 0;
            }
        }
        else
        {
            _genderPicker.ItemSource = _userData.Genders;
            _bloodPressurePicker.ItemSource = _userData.BloodGroups;
            if (!_isLinkedUser)
            {
                _languagePicker.ItemSource = _userData.Languages;
            }
        }
    }

    private CustomBindablePickerControl CreatePickerControl(string resourceKey)
    {
        return new CustomBindablePickerControl
        {
            ControlResourceKey = resourceKey,
        };
    }

    private void AssignPhoto()
    {
        _userProfilePhoto.BindingContext = _userData.User;
        //todo:_userProfilePhoto.SetBinding(CustomImageControl.SourceProperty, nameof(UserModel.ImageSource));
        _userProfilePhoto.SetBinding(CustomImageControl.DefaultValueProperty, nameof(UserModel.UserInitials));
        _fullNameLabel.Text = $"{_userData.User.FirstName} {_userData.User.LastName}";
    }

    private void AssignAge()
    {
        var ageGenderText = string.Empty;
        if (_userData.User.UserAge > 0 && _userData.User.UserAge < 120 && !string.IsNullOrWhiteSpace(_userData.User.GenderID))
        {
            ageGenderText = $" {_userData.User.UserAge} {_parent.GetResourceValueByKey(ResourceConstants.R_YEAR_TEXT_KEY)} | {_userData.Genders.FirstOrDefault(x => x.GroupName == _userData.User.GenderID).OptionText}";
        }
        else
        {
            if (_userData.User.UserAge > 0 && _userData.User.UserAge < 120)
            {
                ageGenderText = $"{_userData.User.UserAge}{_parent.GetResourceValueByKey(ResourceConstants.R_YEAR_TEXT_KEY)}";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(_userData.User.GenderID) && _userData.Genders?.Count > 0)
                {
                    ageGenderText = _userData.Genders.First(x => x.GroupName == _userData.User.GenderID).OptionText;
                }
            }
        }
        _ageLabel.Text = ageGenderText;
    }

    protected override void DeleteUploads()
    {
        _userData.User.ImageName = string.Empty;
        _userData.User.ImageName = string.Empty;
        //todo:_userData.User.ImageSource = string.Empty;
        _base64String = string.Empty;
        _uploadedFileExtension = string.Empty;
    }
}