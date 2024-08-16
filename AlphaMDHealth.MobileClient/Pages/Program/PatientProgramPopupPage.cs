using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class PatientProgramPopupPage : BasePopupPage
{
    private string _patientprogramId;
    private readonly CustomButtonControl _deleteButton;
    private readonly CustomLabelControl _headerLabel;
    private readonly PatientProgramDTO _programData = new PatientProgramDTO { RecordCount = -1, PatientProgram = new PatientProgramModel() };
    private readonly CustomBindablePickerControl _programOption;
    private readonly CustomBindablePickerControl _endPointTypes;
    private readonly CustomBindablePickerControl _trackerTypes;
    private readonly CustomDateTimeControl _dateControl;
    private readonly CustomEntryControl _entryPoint;
    private List<OptionModel> _trackerTypeDataSource = new List<OptionModel>();
    private Grid _bodyGrid; 

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    /// <summary>
    /// constructor of page
    /// </summary>
    public PatientProgramPopupPage(string patientprogramID) : base(new BasePage())
    {
        _patientprogramId = patientprogramID;
        _parentPage.PageService = new PatientProgramService(App._essentials);
        _headerLabel = new CustomLabelControl(LabelType.BeforeLoginHeaderWithNoTopMargin)
        {
            IsVisible = false
        };
        _deleteButton = new CustomButtonControl(ButtonType.TransparentWithMargin)
        {
            TextColor = Color.FromArgb(StyleConstants.ERROR_COLOR),
            IsVisible = false,
            VerticalOptions = LayoutOptions.End
        };
        _programOption = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_PROGRAM_NAME_KEY,
            IsUnderLine = true,
        };
        _endPointTypes = new CustomBindablePickerControl 
        { 
            ControlResourceKey = ResourceConstants.R_END_POINT_TYPE_KEY,
        };
        _trackerTypes = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_PATIENT_PROGRAM_TRACKER_KEY,
            IsVisible = false
        };
        _dateControl = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = ResourceConstants.R_PATIENT_PROGRAM_DATE_KEY,
            IsVisible = false
        };
        _entryPoint = new CustomEntryControl
        {
            ControlType = FieldTypes.NumericEntryControl,
            ControlResourceKey = ResourceConstants.R_PATIENT_PROGRAM_DAYS_KEY,
            IsVisible = false
        };
        _bodyGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };

        _endPointTypes.SelectedValuesChanged += OnEndPointChange;
        _programOption.SelectedValuesChanged += OnProgramChange;
        _bodyGrid.Add(_programOption, 0, 0);
        _bodyGrid.Add(_endPointTypes, 0, 1);
        _bodyGrid.Add(_trackerTypes, 0, 2);
        _bodyGrid.Add(_dateControl, 0, 3);
        _bodyGrid.Add(_deleteButton, 0, 7);
        ScrollView content = new ScrollView { Content = _bodyGrid };
        _parentPage.PageLayout.Add(content, 0, 0);
    }

    private void GridSetter()
    {
        _bodyGrid.Add(_entryPoint, 0, 2);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        base.OnAppearing();
        _programData.PatientPrograms = new List<PatientProgramModel>();
        _programData.PatientProgram.PatientProgramID = Convert.ToInt64(_patientprogramId);
        _programData.SelectedUserID = _programData.RecordCount == -2
            ? App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0)
            : App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await (_parentPage.PageService as PatientProgramService).GetPatientProgramsAsync(_programData).ConfigureAwait(true);
        _parentPage.PageData = (_parentPage.PageService as PatientProgramService).PageData;
        await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            AssignControlResources();
            await Task.Delay(8);
            if (_programData.PatientProgram.PatientProgramID > 0)
            {
                _endPointTypes.SelectedValue = Convert.ToInt64(_programData.PatientProgram.EntryTypeID);
                LoadUiBasedOnTrackerTypeID(_programData.PatientProgram.EntryTypeID ?? 0);
                ValuSetterBaseOnEndPointType();
            }
        }
        else
        {
            OnSaveButtonClicked.Invoke(_programData.ErrCode.ToString(), new EventArgs());
            //todo:await Navigation.PopAllPopupAsync();
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private async void OnEndPointChange(object sender, EventArgs e)
    {
        LoadUiBasedOnTrackerTypeID(Convert.ToInt32(_endPointTypes.SelectedValue));
    }

    private async void OnProgramChange(object sender, EventArgs e)
    {
        _programOption.IsEnabled = false;
        _trackerTypeDataSource = _programData.TrackerTypes.FindAll(y => y.ParentOptionID == _programOption.SelectedValue);
        _trackerTypes.ItemSource = _trackerTypeDataSource;
    }

    private void ValuSetterBaseOnEndPointType()
    {
        if (_programData.PatientProgram.EntryTypeID == 812)  
        {
            _trackerTypeDataSource = _programData.TrackerTypes.FindAll(x => x.ParentOptionID == _programData.PatientProgram.ProgramID);
            _dateControl.GetSetDate = _programData.PatientProgram.EntryDate.Value.Date;
            _trackerTypes.ItemSource = _trackerTypeDataSource;
            _trackerTypes.SelectedValue = (long)_programData.PatientProgram.TrackerID;
        }
        else if (_programData.PatientProgram.EntryTypeID == 813)  
        {
            _dateControl.GetSetDate = _programData.PatientProgram.EntryDate.Value.Date;
        }
        else if (_programData.PatientProgram.EntryTypeID == 814)  
        {
            _entryPoint.Value = _programData.PatientProgram.ProgramEntryPoint;
        }
    }

    private void LoadUiBasedOnTrackerTypeID(int entryTypeID)
    {
        // trackers
        if (entryTypeID == 812)   
        {
            ResouceValueAndVisibilitySetter(true, true, false);
        }
        // date
        else if (entryTypeID == 813)  
        {
            ResouceValueAndVisibilitySetter(false, true,false);
        }
        // days 
        else if (entryTypeID == 814)  
        {
            ResouceValueAndVisibilitySetter(false, false, true);
            GridSetter();
        }
        _trackerTypes.PageResources = _parentPage.PageData;
        _dateControl.PageResources = _parentPage.PageData;
        _entryPoint.PageResources = _parentPage.PageData;
    }

    private void ResouceValueAndVisibilitySetter(bool isTracker, bool isDate, bool isDay)
    {
        _parentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_PATIENT_PROGRAM_TRACKER_KEY).IsRequired = isTracker;
        _parentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_PATIENT_PROGRAM_DATE_KEY).IsRequired = isDate;
        _parentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_PATIENT_PROGRAM_DAYS_KEY).IsRequired = isDay;
        _trackerTypes.IsVisible = isTracker;
        _dateControl.IsVisible = isDate;
        _entryPoint.IsVisible = isDay;
    }

    private void AssignControlResources()
    {
        _programOption.PageResources = _parentPage.PageData;
        _entryPoint.PageResources = _parentPage.PageData;
        _endPointTypes.PageResources = _parentPage.PageData;
        _endPointTypes.ItemSource = _programData.EndPointTypes;
        _programOption.ItemSource = _programData.OrganizationPrograms;
        _trackerTypes.PageResources = _parentPage.PageData;
        _dateControl.PageResources = _parentPage.PageData;
        SelectProgram(_programData.PatientProgram.ProgramID);
        _entryPoint.Value = (_programData.PatientProgram.PatientProgramID > 0) ? _programData.PatientProgram.ProgramEntryPoint : Constants.CONSTANT_ZERO;
        _deleteButton.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY);
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.PatientProgramsView.ToString()));
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
        if (_programData.PatientProgram.ProgramID != 0)
        {
            _deleteButton.Clicked += DeleteProgramClicked;
            _deleteButton.IsVisible = true;
        }
    }

    private void SelectProgram(long programID)
    {
        var selectedProgram = _programData.OrganizationPrograms.FirstOrDefault(x => x.OptionID == programID);
        if (selectedProgram != null && selectedProgram.OptionID > 0)
        {
            _programOption.SelectedValue = selectedProgram.OptionID;
            _programOption.IsEnabled = false;
        }
    }

    private async Task SyncAndNavigateAsync()
    {
        await _parentPage.SyncDataWithServerAsync(Pages.PatientProgramPage, _programData.IsActive, default).ConfigureAwait(true);
        if (_programData.RecordCount == -2)
        {
            await ShellMasterPage.CurrentShell.RenderPageAsync().ConfigureAwait(true);
        }
    }

    private void DisplayError(string key)
    {
        AppHelper.ShowBusyIndicator = false;
        _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(key));
    }

    private async Task<bool> SaveButtonClickedAsync()
    {
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;

            if (_parentPage.IsFormValid())
            {
                DateTimeOffset date = _dateControl.IsVisible ? _dateControl.GetSetDate.Value : DateTime.Now;
                _programData.PatientProgram = new PatientProgramModel
                {
                    ProgramID = _programOption.SelectedValue,
                    TrackerID = (short)_trackerTypes.SelectedValue,
                    PatientID = _programData.SelectedUserID,
                    EntryTypeID = (int)_endPointTypes.SelectedValue,
                    ProgramEntryPoint = _entryPoint.Value != null ? _entryPoint.Value : "0",
                    EntryDate = new DateTimeOffset(date.DateTime, TimeSpan.Zero),
                    PatientProgramID = _programData.PatientProgram.PatientProgramID,
                    IsActive = true,
                    IsSynced = false,
                    AddedON = GenericMethods.GetUtcDateTime,
                    LastModifiedON = GenericMethods.GetUtcDateTime
                };
                await (_parentPage.PageService as PatientProgramService).SaveAssignPatientProgramsAsync(_programData).ConfigureAwait(true);
                if (_programData.ErrCode == ErrorCode.OK)
                {
						await SyncAndNavigateAsync();
                    return true;
                }
                else
                {
                    DisplayError(_programData.ErrCode.ToString());
                }
            }
        }
        AppHelper.ShowBusyIndicator = false;
        return false;
    }

    private async void DeleteProgramClicked(object sender, EventArgs e)
    {
        await DeleteMessagePopupCallAsync().ConfigureAwait(true);
    }

    private async Task DeletePatientProgram()
    {
        ErrorCode result = await (_parentPage.PageService as PatientProgramService).DeletePatientProgramAsync(_programData).ConfigureAwait(true);
        if (result == ErrorCode.OK)
        {
            await _parentPage.SyncDataWithServerAsync(Pages.PatientProgramPage, false, App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
            if (MobileConstants.IsDevicePhone)
            {
                await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
            }
            else
            {
                OnSaveButtonClicked?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
                //todo: 
                //if (PopupNavigation.Instance.PopupStack?.Count > 0)
                //{
                //    //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
                //}
            }
        }
        else
        {
            _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(ErrorCode.ErrorWhileDeletingRecords.ToString()));
        }
    }

    private async void OnMessgeDeleteProgramActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                _parentPage.MessagePopup.PopCustomMessageControlAsync();
                _parentPage.MessagePopup.IsVisible = false;
                await DeletePatientProgram().ConfigureAwait(true);
                break;
            case 2:
                _parentPage.MessagePopup.PopCustomMessageControlAsync();
                _parentPage.MessagePopup.IsVisible = false;
                break;
            default:// to do
                break;
        }
    }

    private async void OnMessgeSaveProgramActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                _parentPage.MessagePopup.PopCustomMessageControlAsync();
                _parentPage.MessagePopup.IsVisible = false;
                await SaveButtonClickedAsync().ConfigureAwait(true);
                OnSaveButtonClicked?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
                //todo:await Navigation.PopAllPopupAsync();
                break;
            case 2:
                _parentPage.MessagePopup.PopCustomMessageControlAsync();
                _parentPage.MessagePopup.IsVisible = false;
                break;
            default:// to do
                break;
        }
    }

    private async Task DeleteMessagePopupCallAsync()
    {
        _deleteButton.Clicked -= DeleteProgramClicked;
        await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeDeleteProgramActionClicked, true, true, false).ConfigureAwait(true);
        _deleteButton.Clicked += DeleteProgramClicked;
    }

    protected override void OnDisappearing()
    {
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        base.OnDisappearing();
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        OnSaveButtonClicked?.Invoke(default, new EventArgs());
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        if(Convert.ToInt32(_patientprogramId) > 0)
        {
            await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_SAVE_POPUP_WARNING_KEY, OnMessgeSaveProgramActionClicked, true, true, false).ConfigureAwait(true);
        }
        else
        {
            if (await SaveButtonClickedAsync().ConfigureAwait(true))
            {
                OnSaveButtonClicked?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
                //todo:await Navigation.PopAllPopupAsync();
            }
        }
    }
}