using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientTrackerDetailView : ViewManager
{
    private readonly TrackerDTO _patientTrackerDTO = new TrackerDTO { PatientTracker = new PatientTrackersModel(), PatientTrackerValue = new PatientTrackersValuesModel(), RecordCount = -3 };
    private readonly CustomDateTimeControl _FromDatePicker;
    private readonly CustomImageControl _TrackerImage;
    private readonly CustomWebView _browser;
    public event EventHandler<EventArgs> OnSaveSuccess;
    public readonly CustomLabelControl _headerLabel;
    public readonly CustomLabelControl _CaptionLabel;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientTrackerDetailView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new PatientTrackerService(App._essentials);
        _headerLabel = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft);
        _CaptionLabel = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft);
        _FromDatePicker = new CustomDateTimeControl
        {
            IsApplyHeightToError=false,
            IsBackGroundTransparent = true,
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = ResourceConstants.R_PATIENT_PROGRAM_DATE_KEY,
        };
        _TrackerImage = new CustomImageControl(AppImageSize.ImageSizeXXXL, AppImageSize.ImageSizeXXXL, string.Empty, _patientTrackerDTO.PatientTracker.ImageName, false)
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
        };

        _browser = new CustomWebView
        {
            HeightRequest = 1,
            IsAutoIncreaseHeight = true,
            ShowBusyIndicator = true
        };
        _browser.IsEnabled = false;
        Grid bodyGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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
        bodyGrid.Add(_FromDatePicker, 0, 0);
        bodyGrid.Add(_TrackerImage, 0, 1);
        bodyGrid.Add(_CaptionLabel, 0, 2);
        bodyGrid.Add(_browser, 0, 3);
        ScrollView content = new ScrollView { Content = bodyGrid };
        ParentPage.PageLayout.Add(content, 0, 0);

        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (isPatientData)
        {
            SetPageContent(ParentPage.PageLayout);
        }
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
        }
        await (ParentPage.PageService as PatientTrackerService).GetPatientTrackersAsync(_patientTrackerDTO).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_patientTrackerDTO.ErrCode == ErrorCode.OK)
        {
            AssignResources();
            if (_patientTrackerDTO?.PatientTrackerValue == null || _patientTrackerDTO?.PatientTracker == null)
            {
                //AssignResources();
                if (_patientTrackerDTO.PatientTracker?.ProgramTrackerID > 0 || _patientTrackerDTO.PatientTracker.PatientTrackerID != Guid.Empty)
                {
                    _FromDatePicker.IsEnabled = _patientTrackerDTO.PatientTracker.IsEdit;
                }
            }
            else
            {
                _headerLabel.Text = _patientTrackerDTO.PatientTracker.TrackerName;
                if (DeviceInfo.Idiom == DeviceIdiom.Phone)
                {
                    await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, _headerLabel.Text, true)).ConfigureAwait(true);
                }
                await Task.Delay(Constants.DATE_RENDER_DELAY).ConfigureAwait(true);
                _FromDatePicker.GetSetDate = _patientTrackerDTO.PatientTracker.CurrentValueInDate.Value.LocalDateTime;
                if (_patientTrackerDTO.PatientTracker?.ProgramTrackerID > 0 || _patientTrackerDTO.PatientTracker?.PatientTrackerID != Guid.Empty)
                {
                    _FromDatePicker.IsEnabled = _patientTrackerDTO.PatientTracker.IsEdit;
                }
                _TrackerImage.ImagePathSource = _patientTrackerDTO.PatientTracker.ImageName;
                _CaptionLabel.Text = _patientTrackerDTO.PatientTracker.CaptionText;
                HtmlWebViewSource _trackerDetailView = new HtmlWebViewSource
                {
                    Html = ParentPage.GetSettingsValueByKey(SettingsConstants.S_HTML_WRAPPER_KEY).Replace(Constants.STRING_FROMAT, _patientTrackerDTO.PatientTracker.InstructionsText)
                };
                _browser.Source = _trackerDetailView;
            }
        }
    }

    private void AssignResources()
    {
        _FromDatePicker.PageResources = ParentPage.PageData;
    }

    /// <summary>
    /// Save Trackers
    /// </summary>
    /// <returns>Operation Status</returns>
    public async Task<bool> SaveTrackerAsync()
    {
        if (ParentPage.IsFormValid())
        {
            AppHelper.ShowBusyIndicator = true;
            if (_FromDatePicker.GetSetDate == null)
            {
                AppHelper.ShowBusyIndicator = false;
                return false;
            }
            _patientTrackerDTO.PatientTrackerValue = new PatientTrackersValuesModel
            {
                PatientTrackerID = _patientTrackerDTO.PatientTracker.PatientTrackerID,
                //todo:CurrentValue = GenericMethods.ConvertDatetimeOffsetToIsoDateString(_FromDatePicker.GetSetDate.Value.Date),
                IsActive = true,
                IsSynced = false,
                LastModifiedON = GenericMethods.GetUtcDateTime,
            };
            _patientTrackerDTO.ErrCode = ErrorCode.OK;
            if (await (ParentPage.PageService as PatientTrackerService).ValidatePatientTrackerValue(_patientTrackerDTO))
            {
                await (ParentPage.PageService as PatientTrackerService).SavePatientTrackerValueAsync(_patientTrackerDTO).ConfigureAwait(true);
                if (_patientTrackerDTO.ErrCode == ErrorCode.OK)
                {
                    _ = ParentPage.SyncDataWithServerAsync(Pages.PatientTrackersView, false,
                        App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
                    AppHelper.ShowBusyIndicator = false;
                    if (MobileConstants.IsDevicePhone)
                    {
                        await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
                    }
                    else
                    {
                        InvokeListRefresh(Guid.Empty, new EventArgs());
                    }
                    return true;
                }
                else
                {
                    AppHelper.ShowBusyIndicator = false;
                    DisplayErrorMessage(ParentPage.GetResourceValueByKey(_patientTrackerDTO.ErrCode.ToString()), false);
                    return false;
                }
            }
            else
            {
                AppHelper.ShowBusyIndicator = false;
                string Error = ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY;
                string ErrorMessage = string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(ParentPage.PageData.Resources, Error), _patientTrackerDTO.PatientTracker.FromDateDisplayFormatString, _patientTrackerDTO.PatientTracker.ToDateDisplayFormatString);
                if (MobileConstants.IsDevicePhone)
                {
                    DisplayErrorMessage((ErrorMessage.ToString()), false);
                }
                {
                    var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
                    bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
                    if (MobileConstants.IsTablet && isPatientData)
                    {
                        await ParentPage.DisplaySystemPopupAsync(ErrorMessage, true, true, true).ConfigureAwait(true);
                        return false;
                    }
                    else
                    {
                        ParentPage.DisplayOperationStatus(ErrorMessage.ToString());
                        return false;
                    }
                }
/*                    return false;
*/                }

        }
        else
        {
            return false;
        }
        return true;
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