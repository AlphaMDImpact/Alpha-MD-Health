using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientTrackerAddEdit : ViewManager
{
    private readonly TrackerDTO _patientTrackerDTO = new TrackerDTO { PatientTracker = new PatientTrackersModel() };
    private readonly CustomBindablePickerControl _trackerName;
    private readonly CustomDateTimeControl _FromDatePicker;
    private readonly CustomDateTimeControl _ToDatePicker;
    private readonly CustomButtonControl _deleteButton;
    private readonly bool _isNotPatientPage;
    private readonly CustomButtonControl _viewButton;
    private readonly Grid _buttonLayout;

    /// <summary>
    /// Is Read Only property
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// DisplayStatus
    /// </summary>
    public event EventHandler<EventArgs> OnSaveSuccess;
    public event EventHandler<EventArgs> OnCloseSuccess;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientTrackerAddEdit(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new PatientTrackerService(App._essentials);
        _isNotPatientPage = !IsPatientPage();
        var padding = (double)App.Current.Resources[StyleConstants.ST_APP_PADDING];
        _deleteButton = new CustomButtonControl(ButtonType.TransparentWithMargin)
        {
            TextColor = Color.FromArgb(StyleConstants.ERROR_COLOR),
            IsVisible = false,
            VerticalOptions = LayoutOptions.End
        };
        _trackerName = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_TRACKER_NAME_TEXT_KEY,
        };
        _FromDatePicker = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            IsApplyHeightToError = false,
            ControlResourceKey = ResourceConstants.R_START_DATE_KEY,
            IsBackGroundTransparent = _isNotPatientPage,
        };
        _ToDatePicker = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            IsApplyHeightToError = false,
            ControlResourceKey = ResourceConstants.R_END_DATE_KEY,
            IsBackGroundTransparent = _isNotPatientPage,
        };
        _viewButton = new CustomButtonControl(ButtonType.TransparentWithMargin)
        {
            TextColor = Color.FromArgb(StyleConstants.ERROR_COLOR),
            IsVisible = false,
            VerticalOptions = LayoutOptions.End
        };
        _buttonLayout = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            //todo:BackgroundColor = Color.White,
            ColumnSpacing = 1,
            Padding = new Thickness(padding, 0),
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Auto }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
               new ColumnDefinition { Width = GridLength.Star },
               new ColumnDefinition { Width = GridLength.Star }
            },
        };
        Grid bodyGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };
        bodyGrid.Add(_trackerName, 0, 0);
        bodyGrid.Add(_FromDatePicker, 0, 1);
        bodyGrid.Add(_ToDatePicker, 0, 2);
        _buttonLayout.Add(_viewButton, 0, 1);
        _buttonLayout.Add(_deleteButton, 1, 1);
        bodyGrid.Add(_buttonLayout, 0, 8);
        ScrollView content = new ScrollView { Content = bodyGrid };
        ParentPage.PageLayout.Add(content, 0, 0);
    }

    private async void _viewButton_Clicked(object sender, EventArgs e)
    {
        var patientEducationPage = new PatientTrackerDetailPopupPage(_patientTrackerDTO.PatientTracker.PatientTrackerID);
        //todo:await Navigation.PushPopupAsync(patientEducationPage).ConfigureAwait(true);
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public async override Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            string patientTrackerId = GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientTrackersModel.PatientTrackerID)));
            _patientTrackerDTO.PatientTracker.PatientTrackerID = string.IsNullOrWhiteSpace(patientTrackerId) ? Guid.Empty : new Guid(patientTrackerId);
            _patientTrackerDTO.RecordCount = -1;
        }
        await Task.WhenAll((ParentPage.PageService as PatientTrackerService).GetPatientTrackersAsync(_patientTrackerDTO)).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_patientTrackerDTO.ErrCode == ErrorCode.OK)
        {
            AssignResources();
            _viewButton.Clicked += _viewButton_Clicked;
            if (_patientTrackerDTO?.PatientTracker?.PatientTrackerID != Guid.Empty)
            {
                SelectTracker(_patientTrackerDTO.PatientTracker.TrackerID);
                _deleteButton.Clicked += DeleteTrackerClicked;
                _deleteButton.IsVisible = true;
                _viewButton.IsVisible = true;
                _trackerName.SelectedValue = _patientTrackerDTO.TrackerTypes?.FirstOrDefault(x => x.OptionID == _patientTrackerDTO.PatientTracker.TrackerID)?.OptionID ?? -1;
                await Task.Delay(Constants.DATE_RENDER_DELAY).ConfigureAwait(true);
                _FromDatePicker.GetSetDate = _patientTrackerDTO.PatientTracker.FromDate.Value.Date;
                _ToDatePicker.GetSetDate = _patientTrackerDTO.PatientTracker.ToDate.Value.Date;
            }
            if (_patientTrackerDTO.PatientTracker.ProgramTrackerID > 0 || !_patientTrackerDTO.PatientTracker.IsEdit)
            {
                _deleteButton.IsVisible = false;
                _trackerName.IsEnabled = false;
                _FromDatePicker.IsEnabled = false;
                _ToDatePicker.IsEnabled = false;
            }
            if (_patientTrackerDTO.PatientTracker.ProgramTrackerID > 0)
            {
                IsReadOnly = true;
            }
        }
        else
        {
            OnSaveSuccess?.Invoke(_patientTrackerDTO.ErrCode.ToString(), new EventArgs());
            OnCloseSuccess?.Invoke(default, new EventArgs());
            //todo:await Navigation.PopAllPopupAsync();
        }
    }

    private void AssignResources()
    {
        if (_patientTrackerDTO?.PatientTracker?.PatientTrackerID != Guid.Empty)
        {
            ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_START_DATE_KEY).MinLength = GetMinDate(ResourceConstants.R_START_DATE_KEY);
            ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_END_DATE_KEY).MinLength = GetMinDate(ResourceConstants.R_END_DATE_KEY);
        }
        _FromDatePicker.PageResources = ParentPage.PageData;
        _ToDatePicker.PageResources = ParentPage.PageData;
        _trackerName.PageResources = ParentPage.PageData;
        _trackerName.ItemSource = _patientTrackerDTO.TrackerTypes;
        _deleteButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY);
        _viewButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_VIEW_BUTTON_KEY);
    }

    private void SelectTracker(long trackerID)
    {
        var selectedTracker = _patientTrackerDTO.TrackerTypes.FirstOrDefault(x => x.OptionID == trackerID);
        if (selectedTracker != null && selectedTracker.OptionID > 0)
        {
            _trackerName.SelectedValue = selectedTracker.OptionID;
            _trackerName.IsEnabled = false;
        }
    }

    private async void DeleteTrackerClicked(object sender, EventArgs e)
    {
        await DeleteMessagePopupCallAsync().ConfigureAwait(true);
    }

    private async Task DeleteMessagePopupCallAsync()
    {
        _deleteButton.Clicked -= DeleteTrackerClicked;
        await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeDeleteTrackerActionClicked, true, true, false).ConfigureAwait(true);
        _deleteButton.Clicked += DeleteTrackerClicked;
    }

    private async void OnMessgeDeleteTrackerActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                ParentPage.MessagePopup.PopCustomMessageControlAsync();
                ParentPage.MessagePopup.IsVisible = false;
                await DeletePatientTracker().ConfigureAwait(true);
                break;
            case 2:
                ParentPage.MessagePopup.PopCustomMessageControlAsync();
                ParentPage.MessagePopup.IsVisible = false;
                break;
            default:
                // to do
                break;
        }
    }

    private async Task DeletePatientTracker()
    {
        ErrorCode result = await (ParentPage.PageService as PatientTrackerService).DeletePatientTrackerAsync(_patientTrackerDTO).ConfigureAwait(true);
        if (result == ErrorCode.OK)
        {
            _ = ParentPage.SyncDataWithServerAsync(Pages.PatientTrackersView, false,
                App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
            if (MobileConstants.IsDevicePhone)
            {
                await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
            }
            else
            {
                OnSaveSuccess?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
                //todo:if (PopupNavigation.Instance.PopupStack?.Count > 0)
                {
                    //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
                }
            }
        }
        else
        {
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(ErrorCode.ErrorWhileDeletingRecords.ToString()));
        }
    }

    public async Task InvokeAndClosePopupAsync()
    {
        AppHelper.ShowBusyIndicator = false;
        OnCloseSuccess?.Invoke(default, new EventArgs());
        //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Save Patient Tracker
    /// </summary>
    /// <returns>Operation Status</returns>
    public async Task<bool> SaveTrackerAsync()
    {
        if (ParentPage.IsFormValid())
        {
            AppHelper.ShowBusyIndicator = true;
            if (_FromDatePicker.GetSetDate.Value > _ToDatePicker.GetSetDate.Value)
            {
                AppHelper.ShowBusyIndicator = false;
                DisplayErrorMessage(string.Format(CultureInfo.InvariantCulture,
                    ParentPage.GetResourceValueByKey(ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    ParentPage.GetResourceValueByKey(ResourceConstants.R_START_DATE_KEY),
                    ParentPage.GetResourceValueByKey(ResourceConstants.R_END_DATE_KEY)), false);
                return false;
            }
            _patientTrackerDTO.PatientTracker = new PatientTrackersModel
            {
                TrackerID = (short)_trackerName.SelectedValue,
                PatientID = _patientTrackerDTO.SelectedUserID,
                UserID = _patientTrackerDTO.SelectedUserID,
                FromDate = GenericMethods.GetFullDateTimeFromDate(_FromDatePicker.GetSetDate.Value, Util.Essentials).ToUniversalTime(),
                ToDate = GenericMethods.GetFullDateTimeFromDate(_ToDatePicker.GetSetDate.Value, Util.Essentials).ToUniversalTime(),
                IsActive = true,
                IsSynced = false,
                LastModifiedON = GenericMethods.GetUtcDateTime,
                PatientTrackerID = _patientTrackerDTO.PatientTracker.PatientTrackerID
            };
            _patientTrackerDTO.ErrCode = ErrorCode.OK;
            await (ParentPage.PageService as PatientTrackerService).SavePatientTrackersAsync(_patientTrackerDTO).ConfigureAwait(true);
            if (_patientTrackerDTO.ErrCode == ErrorCode.OK)
            {
                _ = ParentPage.SyncDataWithServerAsync(Pages.PatientTrackersView, _patientTrackerDTO.IsActive,
                    App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)
                ).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
                OnSaveSuccess?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
                //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
                return true;
            }
            else
            {
                ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(_patientTrackerDTO.ErrCode.ToString()));
                AppHelper.ShowBusyIndicator = false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    private double GetMinDate(string dateKey)
    {
        DateTime fromDate = App._essentials.ConvertToLocalTime(_patientTrackerDTO.PatientTracker.FromDate.Value).Date;
        if (fromDate < DateTime.Now.Date)
        {
            return -(DateTime.Now.Date - fromDate).Days;
        }
        else
        {
            return ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == dateKey).MinLength;
        }
    }

    private void DisplayErrorMessage(string message, bool isSuccess)
    {
        if (MobileConstants.IsDevicePhone)
        {
            ParentPage.DisplayOperationStatus(message, isSuccess);
        }
        else
        {
            OnSaveSuccess?.Invoke(message, isSuccess ? new EventArgs() : EventArgs.Empty);
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public async override Task UnloadUIAsync()
    {
        await Task.CompletedTask;
    }
}