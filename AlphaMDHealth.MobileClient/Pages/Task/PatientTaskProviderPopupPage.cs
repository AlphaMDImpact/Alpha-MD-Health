using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientTaskProviderPopupPage : BasePopupPage
{
    private ViewManager _detailView;
    private readonly TaskType _taskType;
    private readonly long _programID;
    private readonly string _status;
    private readonly long _selectedUserID;

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    public PatientTaskProviderPopupPage(TaskType taskType, long patientTaskID, long itemID, long programID, string status, long selectedUserID) : base(new BasePage(PageLayoutType.MastersContentPageLayout, false))
    {
        _taskType = taskType;
        _programID = programID;
        _status = status;
        _selectedUserID = selectedUserID;
        SetTaskView(taskType, patientTaskID, itemID);
    }

    protected override async void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            AppHelper.ShowBusyIndicator = true;
            base.OnAppearing();
            await _detailView.LoadUIAsync(false);
            _parentPage.PageData = _detailView.ParentPage.PageData;
            GetPageTitle();
            DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
            OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
            SetRightHeader();
            if (_taskType == TaskType.Default && _parentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientTaskDelete.ToString()) && _programID == 0 && _status == ResourceConstants.R_NEW_STATUS_KEY)
            {
                DisplayBottomButton(ResourceConstants.R_DELETE_ACTION_KEY, FieldTypes.DeleteTransparentExButtonControl);
                OnBottomButtonClickedEvent += DeleteButtonClicked;
            }
            AppHelper.ShowBusyIndicator = false;
        }
    }

    private void GetPageTitle()
    {
        switch (_taskType)
        {
            case TaskType.Default:
                SetTitle(_parentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientTaskView.ToString()));
                break;
            case TaskType.MeasurementKey:
                SetTitle((_detailView as PatientReadingView).GetPageTitle());
                break;
            case TaskType.QuestionnaireKey:
                SetTitle((_detailView as QuestionnaireTaskView).GetPageTitle());
                break;
            default:
                // For future implementation
                break;
        }
    }

    private void SetRightHeader()
    {
        if (_taskType == TaskType.MeasurementKey)
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        else
        {
            if (_taskType == TaskType.Default)
            {
                DisplayRightHeader(ResourceConstants.R_OK_ACTION_KEY);
                OnRightHeaderClickedEvent += OnRightHeaderClicked;
            }
        }
    }

    private void SetTaskView(TaskType taskType, long patientTaskID, long itemID)
    {
        switch (taskType)
        {
            case TaskType.MeasurementKey:
                _detailView = new PatientReadingView(_parentPage, _parentPage.AddParameters(
                    _parentPage.CreateParameter(nameof(PatientReadingUIModel.ReadingID), itemID.ToString(CultureInfo.InvariantCulture)),
                    _parentPage.CreateParameter(nameof(PatientReadingModel.PatientTaskID), patientTaskID.ToString(CultureInfo.InvariantCulture)),
                    _parentPage.CreateParameter(nameof(PatientReadingUIModel.PatientReadingID), Guid.Empty.ToString()),
                    _parentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _selectedUserID.ToString(CultureInfo.InvariantCulture))));
                _parentPage.PageLayout.Padding = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], -(double)Application.Current.Resources[StyleConstants.ST_APP_PADDING] + 10, (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0);
                break;
            case TaskType.QuestionnaireKey:
                _detailView = new QuestionnaireTaskView(_parentPage, _parentPage.AddParameters(
                     _parentPage.CreateParameter(nameof(TaskModel.PatientTaskID), patientTaskID.ToString(CultureInfo.InvariantCulture)),
                     _parentPage.CreateParameter(nameof(TaskModel.ItemID), itemID.ToString(CultureInfo.InvariantCulture))));
                _detailView.OnListRefresh += OnQuetionnaireRefresh;
                break;
            default:
                _detailView = new PatientTaskView(_parentPage, _parentPage.AddParameters(
                    _parentPage.CreateParameter(nameof(TaskModel.PatientTaskID), patientTaskID.ToString(CultureInfo.InvariantCulture))));
                break;
        }
    }

    private async void OnQuetionnaireRefresh(object sender, EventArgs e)
    {
        string error = sender as string;
        if (error == ErrorCode.OK.ToString())
        {
            OnSaveButtonClicked?.Invoke(error, new EventArgs());
            await ClosePopupAsync().ConfigureAwait(true);
        }
        else
        {
            _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(error));
        }
    }

    private async void DeleteButtonClicked(object sender, EventArgs e)
    {
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, default).ConfigureAwait(true))
        {
            OnBottomButtonClickedEvent -= DeleteButtonClicked;
            await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeViewActionClicked, true, true, false).ConfigureAwait(true);
        }
    }

    private async void OnMessgeViewActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 0: break;
            case 1:
                _parentPage.MessagePopup.PopCustomMessageControlAsync();
                await SavePatientTaskAsync().ConfigureAwait(true);
                break;
            case 2:
                _parentPage.MessagePopup.PopCustomMessageControlAsync();
                break;
            default:
                // to do
                break;
        }
        OnBottomButtonClickedEvent += DeleteButtonClicked;
    }

    private async Task SavePatientTaskAsync()
    {
        ProgramDTO programData = new ProgramDTO
        {
            Task = (_detailView as PatientTaskView)._programDTO.Task,
        };
        if (programData.Task.FromDate > GenericMethods.GetUtcDateTime)
        {
            programData.Task.IsActive = false;
        }
        else
        {
            programData.Task.IsActive = true;
        }
        programData.Task.FromDateString = null;
        programData.Task.ToDateString = null;
        programData.Task.CompletionDateString = null;
        programData.Task.ToDate = GenericMethods.GetUtcDateTime;
        programData.Task.UserID = (_detailView as PatientTaskView)._programDTO.SelectedUserID;
        programData.Task.Status = programData.Task.Status == ResourceConstants.R_COMPLETED_STATUS_KEY ? ResourceConstants.R_COMPLETED_STATUS_KEY : ResourceConstants.R_MISSED_STATUS_KEY;
        programData.SelectedUserID = (_detailView as PatientTaskView)._programDTO.SelectedUserID;
        AppHelper.ShowBusyIndicator = true;
        await new PatientTaskService(App._essentials).SyncPatientTaskToServerAsync(programData, CancellationToken.None);
        AppHelper.ShowBusyIndicator = false;
        (_detailView as PatientTaskView)._programDTO.ErrCode = programData.ErrCode;
        if ((_detailView as PatientTaskView)._programDTO.ErrCode == ErrorCode.OK)
        {
            await _parentPage.SyncDataWithServerAsync(Pages.PatientTasksView, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.PatientTasks, DataSyncFor.PatientTasks.ToString(),
              App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
            OnSaveButtonClicked?.Invoke((_detailView as PatientTaskView)._programDTO.ErrCode.ToString(), new EventArgs());
            await ClosePopupAsync().ConfigureAwait(true);
        }
        else
        {
            _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey((_detailView as PatientTaskView)._programDTO.ErrCode.ToString()));
        }
    }

    protected override void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            OnBottomButtonClickedEvent -= DeleteButtonClicked;
            OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
            OnRightHeaderClickedEvent -= OnRightHeaderClicked;
            if (_detailView is QuestionnaireTaskView)
            {
                _detailView.OnListRefresh -= OnQuetionnaireRefresh;
            }
            base.OnDisappearing();
        }
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        if (_detailView is PatientReadingView)
        {
            var result = await (_detailView as PatientReadingView).OnSaveButtonClickedAsync();
            if (result.Item1)
            {
                OnSaveButtonClicked?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
            }
        }
        else
        {
            await ClosePopupAsync().ConfigureAwait(true);
        }
    }
}