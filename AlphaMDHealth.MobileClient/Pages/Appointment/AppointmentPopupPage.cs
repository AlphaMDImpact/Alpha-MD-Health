using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Core.Internal;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class AppointmentPopupPage : BasePopupPage
{
    private readonly CustomBindablePickerControl _appointmentTypePicker;
    private readonly CustomDropDownMultiSelectControl _appointmentParticipantsMultiSelect;
    private readonly CollectionView _appointmentSelectedParticipantsList;
    private readonly CustomLabel _careGiverErrorLabel;
    private readonly CustomDateTimeControl _fromDateTime;
    private readonly Grid _bodyGrid;
    private readonly StackLayout _languageStack;
    private readonly CustomDateTimeControl _toDateTime;
    private readonly AppointmentDTO _appointmentData = new AppointmentDTO { Appointment = new AppointmentModel(), RecordCount = -1 };
    private readonly CustomLabelControl _languageErrorLabel;
    private AppointmentDTO appointmentRes;
    private readonly CustomCheckBox _externalParticipant;
    private readonly CustomEntryControl _firstNameEntry;
    private readonly CustomMobileControl _mobileEntry;
    private readonly CustomEntryControl _emailEntry;
    private bool _isExternalParticipant;
    private readonly DateTime _selectedDate;

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    public AppointmentPopupPage(string appointmentID, long selectedUserID, DateTime selectedDate) : base(new BasePage())
    {
        _selectedDate = selectedDate;
        _parentPage.PageService = new AppointmentService(App._essentials);
        if (selectedUserID == 0)
        { 
          selectedUserID = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0);
        }
        _appointmentData.SelectedUserID = selectedUserID;
        //todo: CloseWhenBackgroundIsClicked = false;
        //todo: Padding = _parentPage.GetPopUpPagePadding(PopUpPageType.Long);
        if (!string.IsNullOrWhiteSpace(appointmentID))
        {
            _appointmentData.Appointment.AppointmentID = Convert.ToInt64(appointmentID, CultureInfo.InvariantCulture);
        }
        _fromDateTime = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateTimeControl,
            ControlResourceKey = ResourceConstants.R_STARTS_TEXT_KEY,
        };
        _toDateTime = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateTimeControl,
            ControlResourceKey = ResourceConstants.R_ENDS_TEXT_KEY,
        };
        _appointmentTypePicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_APPOINTMENT_TYPE_TEXT_KEY,
        };
        CustomCellModel participantModel = new CustomCellModel
        {
            CellID = nameof(ParticipantsModel.ParticipantID),
            CellHeader = nameof(ParticipantsModel.FullName),
            CellDescription = nameof(ParticipantsModel.Profession),
            //todo: CellLeftSourceIcon = nameof(ParticipantsModel.ImageSource),
            CellLeftDefaultIcon = nameof(ParticipantsModel.NameInitials),
            CellRightContentHeader = nameof(ParticipantsModel.ShowRemoveButtonText),
            ShowRemoveButton = nameof(ParticipantsModel.ShowRemoveButton),
            CellFirstMiddleContentHeaderColor = nameof(ParticipantsModel.CellFirstMiddleContentHeaderColor),
            CellSecondMiddleSatusContentHeaderColor = nameof(ParticipantsModel.CellSecondMiddleSatusContentHeaderColor),
            CellFirstMiddleSatusContentHeader = nameof(ParticipantsModel.CellFirstMiddleSatusContentHeader),
            CellSecondMiddleSatusContentHeader = nameof(ParticipantsModel.CellSecondMiddleSatusContentHeader)
        };
        _appointmentSelectedParticipantsList = new CollectionView
        {
            ItemsLayout = ItemLayoutCreate(false),
            ItemTemplate = new DataTemplate(() =>
            {
                ResponsiveView view = new ResponsiveView(participantModel);
                view.OnItemClicked += OnRemoveParticipentClicked;
                return new ContentView { Content = view };
            })
        };
        _careGiverErrorLabel = new CustomLabelControl(LabelType.ClientErrorLabel)
        {
            Margin = new Thickness(0, 0, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture)),
            IsVisible = false,
        };
        _appointmentParticipantsMultiSelect = new CustomDropDownMultiSelectControl(participantModel)
        {
            ControlResourceKey = ResourceConstants.R_PARTICIPANTS_TEXT_KEY
        };
        _externalParticipant = new CustomCheckBox()
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_CHECKBOX_KEY],
            Margin = new Thickness(Device.RuntimePlatform == Device.Android ?30:40,0,0,0)
        };
        _firstNameEntry = new CustomEntryControl
        {
            IsVisible=false,
            ControlResourceKey = ResourceConstants.R_NAME_KEY,
            ControlType = FieldTypes.AlphaEntryControl
        };
        _mobileEntry = new CustomMobileControl
        {
            ControlResourceKey = ResourceConstants.R_PHONE_NUMBER_KEY,
            IsEnabled = _appointmentData.Appointment.AppointmentID<1,
            IsVisible = false,
        };
        _emailEntry = new CustomEntryControl
        {
            ControlResourceKey = ResourceConstants.R_APPOINTMENT_EMAIL_KEY,
            ControlType = FieldTypes.EmailEntryControl,
            IsEnabled = _appointmentData.Appointment.AppointmentID < 1,
            IsVisible = false,
        };
        _languageStack = new StackLayout { Spacing = 0, FlowDirection = (FlowDirection)App.Current.Resources[StyleConstants.ST_FLOW_DIRECTION] };
        _languageErrorLabel = new CustomLabelControl(LabelType.ClientErrorLabel) { IsVisible = false, Margin = new Thickness(0, 0, 0, 10) };
        _parentPage.PageLayout.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star } };
        _parentPage.AddRowColumnDefinition(GridLength.Auto, 1, true);
        _bodyGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
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
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
            }
        };
        _bodyGrid.Add(_languageStack, 0, 0);
        _bodyGrid.Add(_languageErrorLabel, 0, 1);
        _bodyGrid.Add(_appointmentTypePicker, 0, 2);
        _bodyGrid.Add(_fromDateTime, 0, 3);
        _bodyGrid.Add(_toDateTime, 0, 4);
        _bodyGrid.Add(_appointmentParticipantsMultiSelect, 0, 5);
        _bodyGrid.Add(_careGiverErrorLabel, 0, 6);
        _bodyGrid.Add(_appointmentSelectedParticipantsList, 0, 7);
        _bodyGrid.Add(_externalParticipant, 0, 8);
        _bodyGrid.Add(_firstNameEntry, 0, 9);
        _bodyGrid.Add(_mobileEntry, 0, 10);
        _bodyGrid.Add(_emailEntry, 0, 11);
        _parentPage.PageLayout.Add(_bodyGrid, 0, 1);
        Grid.SetColumnSpan(_bodyGrid, 2);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        base.OnAppearing();
        await Task.WhenAll(
           _parentPage.GetResourcesAsync(GroupConstants.RS_APPOINTMENT_PAGE_GROUP, GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_TASK_STATUS_GROUP,
           GroupConstants.RS_APPOINTMENT_TYPES_GROUP, GroupConstants.RS_YES_NO_TYPE_GROUP),
           _parentPage.GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_APPOINTMENT_PAGE_GROUP),
           _parentPage.GetFeaturesAsync(AppPermissions.AppointmentCancelMeeting.ToString(), AppPermissions.AppointmentAddEdit.ToString()),
           (_parentPage.PageService as AppointmentService).GetAppointmentsAsync(_appointmentData)
       ).ConfigureAwait(true);
        if (_appointmentData.ErrCode == ErrorCode.OK)
        {
            _isExternalParticipant = _appointmentData.ExternalParticipant?.AppointmentID > 0;
            _externalParticipant.IsChecked = _isExternalParticipant;
            _parentPage.PageData.CountryCodes = _appointmentData.CountryCodes;
            AssignControlResources();
            if (_appointmentData.Appointment.AppointmentID > 0)
            {
                SetTitle(_appointmentData.AppointmentTypes.Find(x => x.IsSelected).OptionText);
                await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
                _fromDateTime.GetSetDate = _appointmentData.Appointment.FromDateTime.Value.Date;
                _toDateTime.GetSetDate = _appointmentData.Appointment.ToDateTime.Value.Date;
                _fromDateTime.SetTime = _appointmentData.Appointment.FromDateTime.Value.TimeOfDay;
                _toDateTime.SetTime = _appointmentData.Appointment.ToDateTime.Value.TimeOfDay;
                if (_parentPage.CheckFeaturePermissionByCode(AppPermissions.AppointmentCancelMeeting.ToString()))
                {
                    _parentPage.CreateSepratorView(_bodyGrid, 0, 12, 1);
                    DisplayBottomButton(ResourceConstants.R_CANCEL_MEETING_TEXT_KEY, FieldTypes.DeleteTransparentExButtonControl);
                    OnBottomButtonClickedEvent += CancelButton_Clicked;
                }
                if (_isExternalParticipant)
                {
                    _externalParticipant.Color = _externalParticipant.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
                    ShowExternalParticipant();
                    _firstNameEntry.Value = _appointmentData.ExternalParticipant.FirstName;
                    _mobileEntry.Value = _appointmentData.ExternalParticipant.MobileNo;
                    _emailEntry.Value = _appointmentData.ExternalParticipant.EmailID;
                }
                else
                {
                    _mobileEntry.IsEnabled = true;
                    _emailEntry.IsEnabled = true;
                    ApplyResourceToExternalUser(); 
                }
            }
            else
            {
                await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
                _fromDateTime.GetSetDate = _selectedDate.Date;
                _toDateTime.GetSetDate = _selectedDate.Date;
            }
        }
        else
        {
            await RefreshAppointmentAsync(_appointmentData.ErrCode.ToString()).ConfigureAwait(true);
        }
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _appointmentParticipantsMultiSelect.OnAddEvent -= ParticipantAddButton_Clicked;
        _appointmentParticipantsMultiSelect.OnSearchList -= OnSearchedData;
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        OnBottomButtonClickedEvent -= CancelButton_Clicked;
    }

    private async void OnMessgeDeleteActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                _parentPage.OnClosePupupAction(sender, e);
                await SaveAppointmentAsync(false).ConfigureAwait(true);
                break;
            case 2:
                _parentPage.OnClosePupupAction(sender, e);
                break;
            default:// to do
                break;
        }
    }

    private async void OnMessgeCaregiverNotAvailableActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                _parentPage.OnClosePupupAction(sender, e);
                await SaveBasedOnActionAsync(appointmentRes).ConfigureAwait(true);
                break;
            case 2:
                _parentPage.OnClosePupupAction(sender, e);
                break;
            default:// to do
                break;
        }
    }

    private void AssignControlResources()
    {
        _externalParticipant.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_APPOINTMENT_INVITE_EXTERNAL_KEY);
        _externalParticipant.CheckedChanged += ExternalParticipant_CheckedChanged;
        _appointmentTypePicker.PageResources = _parentPage.PageData;
        _firstNameEntry.PageResources = _parentPage.PageData;
        ApplyResourceToExternalUser();
        _fromDateTime.PageResources = _parentPage.PageData;
        _toDateTime.PageResources = _parentPage.PageData;
        _appointmentParticipantsMultiSelect.PageResources = _parentPage.PageData;
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.AppointmentAddEdit.ToString()));
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
        _appointmentTypePicker.ItemSource = _appointmentData.AppointmentTypes;
        _appointmentTypePicker.SelectedValue = _appointmentData.Appointment.AppointmentTypeID > 0 ? _appointmentData.AppointmentTypes.Find(x => x.IsSelected).OptionID : 0;
        _appointmentParticipantsMultiSelect.ItemsSource = _appointmentData.AppointmentParticipants;
        _appointmentParticipantsMultiSelect.SelectedItems = _appointmentData.AppointmentParticipants.Where(x => x.IsSelected).Cast<object>().ToList();
        _appointmentSelectedParticipantsList.ItemsSource = _appointmentParticipantsMultiSelect.SelectedItems;
        _careGiverErrorLabel.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_CAREGIVER_SELECTION_ERROR_MESSAGE_KEY);
        _appointmentParticipantsMultiSelect.OnAddEvent += ParticipantAddButton_Clicked;
        _appointmentParticipantsMultiSelect.OnSearchList += OnSearchedData;
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
        _appointmentSelectedParticipantsList.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _appointmentParticipantsMultiSelect.SelectedItems.Count;
        GenerateLanguageTab();
    }

    private void ApplyResourceToExternalUser()
    {
        _emailEntry.PageResources = _parentPage.PageData;
        _emailEntry.RegexPattern = _parentPage.GetSettingsValueByKey(SettingsConstants.S_EMAIL_REGEX_KEY);
        _mobileEntry.CountrySource = _parentPage.PageData;
        _mobileEntry.PageResources = _parentPage.PageData;
        _mobileEntry.SetSelectedCountryId(0);
    }

    private void ExternalParticipant_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        _isExternalParticipant = ((CustomCheckBox)sender).IsChecked;
        if (_isExternalParticipant)
        {
            _externalParticipant.Color = _externalParticipant.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
            ShowExternalParticipant();
        }
        else
        {
            _externalParticipant.Color = _externalParticipant.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR];
            _firstNameEntry.IsVisible = false;
            _mobileEntry.IsVisible = false;
            _emailEntry.IsVisible = false;
        }
    }

    private void ShowExternalParticipant()
    {
        _firstNameEntry.IsVisible = true;
        _mobileEntry.IsVisible = true;
        _emailEntry.IsVisible = true;
    }

    private LinearItemsLayout ItemLayoutCreate(bool isHorizontal)
    {
        return new LinearItemsLayout(isHorizontal ? ItemsLayoutOrientation.Horizontal : ItemsLayoutOrientation.Vertical)
        {
            SnapPointsType = SnapPointsType.None,
            SnapPointsAlignment = SnapPointsAlignment.Center,
        };
    }

    private void ParticipantAddButton_Clicked(object sender, EventArgs e)
    {
        _appointmentParticipantsMultiSelect.SelectedItems.Cast<ParticipantsModel>().ForEach(x => x.ShowRemoveButton = true);
        AssignSourceToAppointmentParticipants();
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        if (SaveLanguageSpecificData())
        {
            await SaveAppointmentAsync(true).ConfigureAwait(false);
        }
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        //todo:
        //await Navigation.PopPopupAsync().ConfigureAwait(true);
    }

    private async void OnSearchedData(object sender, EventArgs e)
    {
        if (e is TextChangedEventArgs item)
        {
            await Task.Delay(1);
            _appointmentParticipantsMultiSelect.ItemsSource = string.IsNullOrWhiteSpace(item.NewTextValue)
                ? _appointmentData.AppointmentParticipants
                : _appointmentData.AppointmentParticipants.FindAll(x => x.FullName.ToLower(CultureInfo.InvariantCulture).Contains(item.NewTextValue.ToLower(CultureInfo.InvariantCulture)));
        }
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeDeleteActionClicked, true, true, false).ConfigureAwait(true);
    }

    private bool SaveLanguageSpecificData()
    {
        bool isEmptyDetail = string.IsNullOrWhiteSpace(_appointmentData.AppointmentDetails.FirstOrDefault(x => x.LanguageID == Convert.ToByte(_appointmentData.LanguageTabs.FirstOrDefault().LanguageID, CultureInfo.InvariantCulture)).PageData);
        if(_appointmentData?.LanguageTabs?.Count == 1)
        {
            _languageErrorLabel.IsVisible = false;
            //return true;
        }
        foreach (LanguageModel language in _appointmentData.LanguageTabs)
        {
            var viewData = _languageStack.Children.Where(x => x.AutomationId == 1.ToString(CultureInfo.InvariantCulture)).ToList();
            ContentDetailModel contentDetail = _appointmentData.AppointmentDetails.FirstOrDefault(x => x.LanguageID == Convert.ToByte(language.LanguageID, CultureInfo.InvariantCulture));
            contentDetail.PageHeading = (viewData[0] as CustomEntryControl).Value;
            contentDetail.PageData = (viewData[1] as CustomMultiLineEntryControl).Value;
            
        }
        return true;
    }

    private void AssignSourceToAppointmentParticipants()
    {
        _appointmentSelectedParticipantsList.ItemsSource = null;
        _appointmentSelectedParticipantsList.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _appointmentParticipantsMultiSelect.SelectedItems.Count;
        _appointmentSelectedParticipantsList.ItemsSource = _appointmentParticipantsMultiSelect.SelectedItems;
    }

    private async Task SaveAppointmentAsync(bool isActive)
    {
        AppointmentDTO appointmentDTO = new AppointmentDTO();
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            appointmentDTO.IsExternalParticipant = _isExternalParticipant;
            if (!isActive)
            {
                appointmentDTO.Appointment = new AppointmentModel
                {
                    AppointmentID = _appointmentData.Appointment.AppointmentID,
                    AppointmentStatusID = ResourceConstants.R_CANCELLED_STATUS_KEY
                };
                AppHelper.ShowBusyIndicator = true;
                await SyncAppointmentDataAsync(appointmentDTO);
                await (_parentPage.PageService as AppointmentService).UpdateAppointmentAsync(appointmentDTO, CancellationToken.None).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
            else
            {
                _careGiverErrorLabel.IsVisible = false;
                if (_parentPage.IsFormValid())
                {
                    if (ValidateDateTime())
                    {
                        DisplayAlert(ErrorCode.InvalidData.ToString());
                        return;
                    }
                    var count = _isExternalParticipant ? 1 : 2;
                    if (!(_appointmentParticipantsMultiSelect.SelectedItems.Cast<ParticipantsModel>().Any(x => x.RoleID == (int)RoleName.Doctor) && _appointmentParticipantsMultiSelect.SelectedItems.Cast<ParticipantsModel>().Count() >= count))
                    {
                        _careGiverErrorLabel.IsVisible = true;
                        return;
                    }
                    AppHelper.ShowBusyIndicator = true;
                    MapAppointmentData(appointmentDTO, isActive);
                    await SyncAppointmentDataAsync(appointmentDTO); 
                }
                else
                {
                    return;
                }
            }
            if (appointmentDTO.ErrCode == ErrorCode.OK)
            {
                await RefreshAppointmentAsync(appointmentDTO.ErrCode.ToString()).ConfigureAwait(true);
            }
            else if (appointmentDTO.ErrCode == ErrorCode.DuplicateData)
            {
                await DuplicateDataHandleAsync(appointmentDTO).ConfigureAwait(true);
            }
            else
            {
                DisplayAlert(appointmentDTO.ErrCode.ToString());
            }
        }
    }

    private async Task SyncAppointmentDataAsync(AppointmentDTO appointmentDTO)
    {
        appointmentDTO.LocaltoUtcTimeInSeconds = Convert.ToInt32((appointmentDTO.Appointment.FromDateTime.Value.ToLocalTime().DateTime - appointmentDTO.Appointment.FromDateTime.Value.DateTime).TotalSeconds);
        await (_parentPage.PageService as AppointmentService).SyncAppointmentToServerAsync(appointmentDTO, CancellationToken.None).ConfigureAwait(true);
        await _parentPage.SyncDataWithServerAsync(Pages.AppointmentsView, false, App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0)).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    private async Task DuplicateDataHandleAsync(AppointmentDTO appointmentDTO)
    {
        SetUnavailableStatus(appointmentDTO);
        appointmentRes = appointmentDTO;
        await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_CAREGIVER_AVAILABLE_ERROR_MESSAGE_KEY, OnMessgeCaregiverNotAvailableActionClicked, true, true, false).ConfigureAwait(true);
    }

    private async Task SaveBasedOnActionAsync(AppointmentDTO appointmentDTO)
    {
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            appointmentDTO.Appointment.IsRepeatRequest = true;
            appointmentDTO.AppointmentParticipants = GetAppointmentParticipents(appointmentDTO);
            appointmentDTO.ErrCode = ErrorCode.OK;
            await SyncAppointmentDataAsync(appointmentDTO);
            if (appointmentDTO.ErrCode == ErrorCode.OK)
            {
                await RefreshAppointmentAsync(appointmentDTO.ErrCode.ToString()).ConfigureAwait(true);
            }
            else
            {
                DisplayAlert(appointmentDTO.ErrCode.ToString());
            }
        }
    }

    private async Task RefreshAppointmentAsync(String errorCode)
    {
        OnSaveButtonClicked?.Invoke(errorCode, new EventArgs());
        //todo:
        //await Navigation.PopAllPopupAsync().ConfigureAwait(true);
    }

    private void MapAppointmentData(AppointmentDTO appointmentDTO, bool isActive)
    {
        appointmentDTO.Appointment = _appointmentData.Appointment;
        appointmentDTO.IsActive = isActive;
        appointmentDTO.Appointment.IsRepeatRequest = false;
        appointmentDTO.Appointment.IsActive = isActive;
        appointmentDTO.Appointment.AccountID = App._essentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, (long)0);
        appointmentDTO.Appointment.FromDateTime = _fromDateTime.GetSetDate.Value.ToUniversalTime();
        appointmentDTO.Appointment.ToDateTime = _toDateTime.GetSetDate.Value.ToUniversalTime();
        appointmentDTO.Appointment.AppointmentStatusID = ResourceConstants.R_NEW_STATUS_KEY;
        appointmentDTO.AppointmentDetails = _appointmentData.AppointmentDetails;
        appointmentDTO.Appointment.AppointmentTypeID = (short)_appointmentTypePicker.SelectedValue;
        appointmentDTO.AppointmentParticipants = GetAppointmentParticipents(appointmentDTO);
        foreach (var item in appointmentDTO.AppointmentDetails)
        {
            item.PageID = appointmentDTO.Appointment.AppointmentID;
        }
        appointmentDTO.ExternalParticipant = new ParticipantsModel
        {
            FirstName = _firstNameEntry.Value,
            MobileNo = _mobileEntry.Value,
            EmailID = _emailEntry.Value,
        };
    }

    private List<ParticipantsModel> GetAppointmentParticipents(AppointmentDTO appointmentDTO)
    {
        return (from participent in _appointmentParticipantsMultiSelect.SelectedItems.Cast<ParticipantsModel>().ToList()
                select new ParticipantsModel
                {
                    ParticipantID = participent.ParticipantID,
                    AppointmentID = appointmentDTO.Appointment.AppointmentID,
                    AppointmentStatusID = ResourceConstants.R_NEW_STATUS_KEY,
                    AccountID = participent.AccountID,
                    IsActive = true
                }).ToList();
    }

    private bool ValidateDateTime()
    {
        return _fromDateTime.GetSetDate.Value > _toDateTime.GetSetDate.Value || _fromDateTime.GetSetDate.Value.Day != _toDateTime.GetSetDate.Value.Day
                            || _fromDateTime.GetSetDate.Value.Month != _toDateTime.GetSetDate.Value.Month
                            || _fromDateTime.GetSetDate.Value.Year != _toDateTime.GetSetDate.Value.Year
                            || _fromDateTime.GetSetDate.Value == _toDateTime.GetSetDate.Value
                            || (_fromDateTime.GetSetDate.Value - DateTime.Now).TotalMinutes < Convert.ToInt32(_parentPage.GetSettingsValueByKey(SettingsConstants.S_APPOINTMENT_DEFAULT_TIMEOUT_KEY), CultureInfo.InvariantCulture);
    }

    private void GenerateLanguageTab()
    {
        //_customTabs.LoadUIData((from language in _appointmentData.LanguageTabs select new OptionModel { OptionID = Convert.ToInt64(language.LanguageID, CultureInfo.InvariantCulture), OptionText = language.LanguageName , GroupName = language.LanguageID}).ToList(), false);
        //_customTabs.TabClicked += Language_TabClicked;
        //_parentPage.PageLayout.Add(_customTabs, 0, 0);
        foreach (LanguageModel language in _appointmentData.LanguageTabs)
        {
            if(language.LanguageID == 1)
            {
                CreateLanguageEntry(Convert.ToByte(language.LanguageID, CultureInfo.InvariantCulture));
            }
        }
        ShowHideLanguageView(_appointmentData.LanguageTabs[0].LanguageID);
    }

    private void Language_TabClicked(object sender, EventArgs e)
    {
        ShowHideLanguageView(Convert.ToInt16(sender, CultureInfo.InvariantCulture));
    }

    private void CreateLanguageEntry(byte languageID)
    {
        CustomEntryControl subjectEntry = new CustomEntryControl
        {
            PageResources = _parentPage.PageData,
            IsUnderLine = true,
            ControlResourceKey = ResourceConstants.R_APPOINTMENT_SUBJECT_TEXT_KEY,
            AutomationId = languageID.ToString(CultureInfo.InvariantCulture)
        };
        CustomMultiLineEntryControl multilineEntry = new CustomMultiLineEntryControl
        {
            PageResources = _parentPage.PageData,
            ControlResourceKey = ResourceConstants.R_INFORMATION_TEXT_KEY,
            EditorHeightRequest = EditorHeight.Chat,
            AutomationId = languageID.ToString(CultureInfo.InvariantCulture)
        };
        _languageStack.Add(subjectEntry);
        _languageStack.Add(multilineEntry);
        subjectEntry.Value = _appointmentData.AppointmentDetails.Find(x => x.LanguageID == languageID).PageHeading;
        multilineEntry.Value = _appointmentData.AppointmentDetails.Find(x => x.LanguageID == languageID).PageData;
    }

    private void ShowHideLanguageView(short languageID)
    {
        foreach (View view in _languageStack.Children)
        {
            view.IsVisible = view.AutomationId == languageID.ToString();
        }
    }

    private void SetUnavailableStatus(AppointmentDTO appointmentDTO)
    {
        string unavailableText = _parentPage.GetResourceValueByKey(ResourceConstants.R_UNAVAILABLE_TEXT_KEY);
        string availableText = _parentPage.GetResourceValueByKey(ResourceConstants.R_AVAILABLE_TEXT_KEY);
        foreach (var item in _appointmentData.AppointmentParticipants)
        {
            if (appointmentDTO.AppointmentParticipants.Exists(x => x.ParticipantID == item.ParticipantID))
            {
                item.CellFirstMiddleSatusContentHeader = string.Empty;
                item.CellSecondMiddleSatusContentHeader = unavailableText;
                item.CellSecondMiddleSatusContentHeaderColor = StyleConstants.ERROR_COLOR;
                item.IsSelected = true;
            }
            else
            {
                item.CellFirstMiddleSatusContentHeader = string.Empty;
                item.CellSecondMiddleSatusContentHeader = availableText;
                item.CellSecondMiddleSatusContentHeaderColor = StyleConstants.SUCCESS_COLOR;
            }
        }
        AssignSourceToAppointmentParticipants();
    }

    private void OnRemoveParticipentClicked(object sender, EventArgs e)
    {
        ParticipantsModel selectedParticipant = _appointmentParticipantsMultiSelect.SelectedItems.Cast<ParticipantsModel>().FirstOrDefault(x => x.ParticipantID == Convert.ToInt64((sender as CustomLabelControl).ClassId, CultureInfo.InvariantCulture));
        _appointmentParticipantsMultiSelect.SelectedItems.Remove(selectedParticipant);
        ParticipantsModel participant = _appointmentData.AppointmentParticipants.FirstOrDefault(x => x.ParticipantID == Convert.ToInt64((sender as CustomLabelControl).ClassId, CultureInfo.InvariantCulture));
        participant.IsSelected = false;
        participant.ShowRemoveButton = false;
        AssignSourceToAppointmentParticipants();
    }

    private void DisplayAlert(string errorConstant)
    {
        _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(errorConstant));
    }
}
