using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class AssignTaskPopupPage : BasePopupPage
{
    private readonly CustomBindablePickerControl _taskTypePicker;
    private readonly CustomBindablePickerControl _itemPicker;
    private readonly CustomDateTimeControl _fromDate;
    private readonly CustomDateTimeControl _toDate;
    private readonly ProgramDTO _taskData = new ProgramDTO { Task = new TaskModel(), RecordCount = -1 };

    /// <summary>
    /// on click event of save Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    public AssignTaskPopupPage(long selectedUserID) : base(new BasePage())
    {
        _taskData.SelectedUserID = selectedUserID;
        _parentPage.PageService = new PatientTaskService(App._essentials);
        _taskTypePicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_TASK_TYPE_KEY,
        };
        _itemPicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_ITEM_KEY,
        };
        _fromDate = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = ResourceConstants.R_START_DATE_KEY,
        };
        _toDate = new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = ResourceConstants.R_END_DATE_KEY,
        };
        _parentPage.PageLayout.Padding = new Thickness((double)App.Current.Resources[StyleConstants.ST_APP_PADDING], 0);
        _parentPage.AddRowColumnDefinition(GridLength.Auto, 4, true);
        _parentPage.PageLayout.Add(_taskTypePicker, 0, 0);
        _parentPage.PageLayout.Add(_itemPicker, 0, 1);
        _parentPage.PageLayout.Add(_fromDate, 0, 2);
        _parentPage.PageLayout.Add(_toDate, 0, 3);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        base.OnAppearing();
        await Task.WhenAll(
           _parentPage.GetFeaturesAsync(AppPermissions.PatientAssignTaskView.ToString()),
           (_parentPage.PageService as PatientTaskService).GetPatientTaskAsync(_taskData)
        ).ConfigureAwait(true);
        _parentPage.PageService.PageData.FeaturePermissions = _parentPage.PageData.FeaturePermissions;
        _parentPage.PageData = _parentPage.PageService.PageData;
        if (_taskData.ErrCode == ErrorCode.OK)
        {
            AssignControlResources();
            await Task.Delay(Constants.DATE_RENDER_DELAY).ConfigureAwait(true);
            _fromDate.GetSetDate = DateTime.Now.Date;
            _toDate.GetSetDate = DateTime.Now.Date;
        }
        else
        {
            OnSaveButtonClicked?.Invoke(_taskData.ErrCode.ToString(), new EventArgs());
            await ClosePopupAsync().ConfigureAwait(true);
        }
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        _taskTypePicker.SelectedValuesChanged -= OnTaskTypeChanged;
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        base.OnDisappearing();
    }

    private void AssignControlResources()
    {
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.PatientAssignTaskView.ToString()));
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
        _taskTypePicker.PageResources = _parentPage.PageData;
        _itemPicker.PageResources = _parentPage.PageData;
        _fromDate.PageResources = _parentPage.PageData;
        _toDate.PageResources = _parentPage.PageData;
        _taskTypePicker.ItemSource = _taskData.TaskTypes;
        _itemPicker.ItemSource = _taskData.Items;
        _taskTypePicker.SelectedValuesChanged += OnTaskTypeChanged;
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
    }

    private async void OnTaskTypeChanged(object sender, EventArgs e)
    {
        _taskTypePicker.SelectedValuesChanged -= OnTaskTypeChanged;
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            ProgramDTO taskData = new ProgramDTO
            {
                Program = new ProgramModel { ProgramID = -1, Name = _taskData.TaskTypes.FirstOrDefault(x => x.OptionID == _taskTypePicker.SelectedValue).GroupName },
                SelectedUserID = _taskData.SelectedUserID
            };
            var service = new ProgramService(App._essentials)
            {
                PageData = _parentPage.PageData
            };
            await service.SyncTaskItemsFromServerAsync(taskData, CancellationToken.None).ConfigureAwait(true);
            if (taskData.ErrCode == ErrorCode.OK)
            {
                _itemPicker.ItemSource = taskData.Items;
            }
            else
            {
                _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(taskData.ErrCode.ToString()));
            }
            AppHelper.ShowBusyIndicator = false;
        }
        _taskTypePicker.SelectedValuesChanged += OnTaskTypeChanged;
    }

    protected async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        await ClosePopupAsync().ConfigureAwait(true);
    }

    protected async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) && _parentPage.IsFormValid())
        {
            if (_fromDate.GetSetDate > _toDate.GetSetDate)
            {
                _parentPage.DisplayOperationStatus(string.Format(CultureInfo.InvariantCulture, _parentPage.GetResourceValueByKey(ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    _parentPage.GetResourceValueByKey(ResourceConstants.R_START_DATE_KEY), _parentPage.GetResourceValueByKey(ResourceConstants.R_END_DATE_KEY)));
            }
            else
            {
                AppHelper.ShowBusyIndicator = true;
                DateTimeOffset from = _fromDate.GetSetDate.Value;
                DateTimeOffset to = _toDate.GetSetDate.Value;
                ProgramDTO taskData = new ProgramDTO
                {
                    Task = new TaskModel
                    {
                        TaskType = _taskData.TaskTypes.FirstOrDefault(x => x.OptionID == _taskTypePicker.SelectedValue).GroupName,
                        ItemID = _itemPicker.SelectedValue,
                        SelectedItemName = _itemPicker.SelectedText,
                        FromDate = new DateTimeOffset(from.DateTime, TimeSpan.Zero),
                        ToDate = new DateTimeOffset(to.DateTime, TimeSpan.Zero),
                        Status = ResourceConstants.R_NEW_STATUS_KEY,
                        UserID = _taskData.SelectedUserID,
                        CompletionDate = GenericMethods.GetUtcDateTime.Date,
                        IsActive = true
                    }
                };
                taskData.SelectedUserID = _taskData.SelectedUserID;
                await (_parentPage.PageService as PatientTaskService).SyncPatientTaskToServerAsync(taskData, CancellationToken.None).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
                if (taskData.ErrCode == ErrorCode.OK)
                {
                    await _parentPage.SyncDataWithServerAsync(Pages.PatientTasksView, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.PatientTasks, DataSyncFor.PatientTasks.ToString(),
             App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
                    OnSaveButtonClicked?.Invoke(taskData.ErrCode.ToString(), new EventArgs());
                    await ClosePopupAsync().ConfigureAwait(true);
                }
                else
                {
                    _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(taskData.ErrCode.ToString()));
                }
            }
        }
        OnRightHeaderClickedEvent += OnRightHeaderClicked;
    }
}