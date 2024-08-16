using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientTasksView : BaseLibCollectionView
{
    private readonly ProgramDTO _tasksData = new ProgramDTO { Task = new TaskModel() };
    private readonly Grid _mainLayout;
    private CustomTabsControl _customTabs;
    private string _selectedTabKey;
    private ViewManager _detailView;
    private readonly bool _isDashboard;
    private long _selectTaskID;
    private readonly CustomCellModel _customCellData;
    private readonly bool _isPatientPage;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientTasksView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new PatientTaskService(App._essentials);
        if (Parameters?.Count > 0)
        {
            _tasksData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        }
        _isDashboard = IsDashboardView(_tasksData.RecordCount);
        _isPatientPage = IsPatientPage();
        _customCellData = new CustomCellModel
        {
            CellHeader = nameof(TaskModel.SelectedItemName),
            IconSize = AppImageSize.ImageSizeM,
            BandColor = nameof(TaskModel.ProgramColor)
        };
        CreateCellViewData();
        IsTabletListHeaderDisplay = _isPatientPage && _tasksData.RecordCount < 1;
        _mainLayout = new Grid
        {
            Style = _isPatientPage && !_isDashboard ? (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE] : (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = !_isDashboard && DeviceInfo.Idiom == DeviceIdiom.Tablet
                ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture)
                : Constants.ZERO_PADDING,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions = CreateTabletViewColumn((_isDashboard || DeviceInfo.Idiom == DeviceIdiom.Phone))
        };
        AssignCollectionView();
        SetPageContent(_mainLayout);
    }

    private void AssignCollectionView()
    {
        if (_isDashboard)
        {
            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            AddCollectionView(_mainLayout, _customCellData, 0, 0);
        }
        else if (_isPatientPage)
        {
            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            AddCollectionViewWithTabletHeader(_mainLayout, _customCellData);
            SearchField.IsVisible = false;
        }
        else
        {
            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            AddSearchView(_mainLayout, false);
            _customTabs = new CustomTabsControl
            {
                Margin = new Thickness(0, (double)App.Current.Resources[StyleConstants.ST_APP_PADDING], 0, 0),
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            _mainLayout.Add(_customTabs, 0, 1);
            AddCollectionView(_mainLayout, _customCellData, 0, 2);
            SearchField.IsVisible = false;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                AddSeparatorView(_mainLayout, 1, 0);
                Grid.SetRowSpan(Separator, 3);
            }
        }
    }
    private void CreateCellViewData()
    {
        if (_isPatientPage)
        {
            _customCellData.CellDescription = nameof(TaskModel.Name);
            _customCellData.CellLeftDefaultIcon = nameof(TaskModel.ImageName);
            _customCellData.CellRightContentHeader = nameof(TaskModel.FromDateString);
            _customCellData.CellRightContentDescription = nameof(TaskModel.StatusValue);
            _customCellData.CellDescriptionColor = nameof(TaskModel.StatusColor);
        }
        else
        {
            _customCellData.CellDescription = nameof(TaskModel.FromDateString);
            _customCellData.CellLeftDefaultIcon = nameof(TaskModel.LeftDefaultIcon);
            _customCellData.CellRightContentDescription = nameof(TaskModel.ToDateString);
            _customCellData.CellRightContentDescriptionInPancakeView = false;
            _customCellData.RowHeight = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] / 2;
        }
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        _selectTaskID = _tasksData.Task.PatientTaskID;
        if (!isRefreshRequest)
        {
            MapParameters();
        }
        if ((CollectionViewField.SelectedItem as TaskModel)?.PatientTaskID != _selectTaskID || _selectTaskID == 0)
        {
            _selectedTabKey = isRefreshRequest ? _selectedTabKey : ResourceConstants.R_OPEN_TASK_KEY;
            await (ParentPage.PageService as PatientTaskService).GetTasksAsync(_tasksData, _isPatientPage ? string.Empty : _selectedTabKey).ConfigureAwait(true);
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                ParentPage.PageData = ParentPage.PageService.PageData;
                _emptyListView.PageResources = ParentPage.PageData;
                if (_tasksData.IsActive)
                {
                    if (_tasksData.ErrCode == ErrorCode.OK && !GenericMethods.IsListNotEmpty(_tasksData.Tasks))
                    {
                        await ShellMasterPage.CurrentShell.PopMainPageAsync().ConfigureAwait(true);
                        await ShellMasterPage.CurrentShell.RenderPageAsync().ConfigureAwait(true);
                        return;
                    }
                    _customTabs.IsVisible = false;
                    _customTabs.Margin = 0;
                }
                else
                {
                    GenerateTabs(isRefreshRequest);
                }
                await RenderUIDataAsync().ConfigureAwait(true);
            });
        }
    }

    private void MapParameters()
    {
        _tasksData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        _tasksData.Task.PatientTaskID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(TaskModel.PatientTaskID)));
        _tasksData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
        /// Flag to indicate whether list page is called after first time login 
        _tasksData.IsActive = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(BaseDTO.IsActive)));
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        App._essentials.SetPreferenceValue(StorageConstants.PR_REMOVE_SCROLL_VIEW_KEY, false);
        if (SearchField != null && SearchField.Value != null)
        {
            SearchField.Value = string.Empty;
            SearchField.OnSearchTextChanged -= OnTaskSearch;
        }
        OnActionButtonClicked -= OnAddButtonClicked;
        OnListItemSelection(OnListItemClick, false);
        if (ParentPage is BasePage)
        {
            ParentPage.TabClicked -= OnTaskTabClicked;
        }
        _tasksData.Task.PatientTaskID = 0;
        await base.UnloadUIAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Action to perform on click of Top menus
    /// </summary>
    /// <param name="menuAction">Menus action needs to perform</param>
    /// <param name="isDetailView">Is Action perform from detail view</param>
    public async Task OnActionClickedAsync(MenuAction menuAction, bool isDetailView)
    {
        if (menuAction == MenuAction.MenuActionSaveKey && MobileConstants.IsTablet
            && _detailView is PatientReadingView)
        {
            await (_detailView as PatientReadingView).OnSaveButtonClickedAsync();
        }
    }

    private async void OnTaskTabClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        _customTabs.TabClicked -= OnTaskTabClicked;
        await SetRightPageHeaderAsync(string.Empty, false).ConfigureAwait(true);
        _selectedTabKey = (string)sender;
        SearchField.Value = string.Empty;
        _tasksData.Task = new TaskModel();
        await LoadUIAsync(true).ConfigureAwait(true);
        _customTabs.TabClicked += OnTaskTabClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    private void OnTaskSearch(object sender, EventArgs e)
    {
        if (GenericMethods.IsListNotEmpty(_tasksData.Tasks))
        {
            CollectionViewField.Footer = null;
            var serchBar = sender as CustomSearchBar;
            if (string.IsNullOrWhiteSpace(serchBar.Text))
            {
                CollectionViewField.ItemsSource = new List<TaskModel>();
                CollectionViewField.ItemsSource = _tasksData.Tasks;
                SetTableHeader(_tasksData.Tasks.Count);
                SetMainGridSize();
            }
            else
            {
                var searchedUsers = _tasksData.Tasks.FindAll(y =>
                {
                    return (!string.IsNullOrWhiteSpace(y.SelectedItemName) && y.SelectedItemName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())) ||
                                                (!string.IsNullOrWhiteSpace(y.FromDateString) && y.FromDateString.ToLowerInvariant().Contains(serchBar.Text.ToLowerInvariant().Trim())) ||
                                                (!string.IsNullOrWhiteSpace(y.ToDateString) && y.ToDateString.ToLowerInvariant().Contains(serchBar.Text.ToLowerInvariant().Trim()));
                });
                if (searchedUsers.Count > 0)
                {
                    CollectionViewField.ItemsSource = searchedUsers;
                    _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * searchedUsers.Count + 60;
                }
                else
                {
                    CollectionViewField.ItemsSource = new List<TaskModel>();
                    RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
                    _mainLayout.HeightRequest = 500;
                }
                SetTableHeader(searchedUsers.Count);
            }
        }
    }

    private async void OnRefreshCall(object sender, EventArgs e)
    {
        // when e == null, display operation status for detail page
        // when errorCode is success, display success status and refresh list 
        // when errorcode is not success, display error status and view default detail view of list page
        if (!(e is AlphaMDHealth.Model.CustomEventArgs))
        {
            var message = ParentPage.GetResourceValueByKey((string)sender);
            ParentPage.DisplayOperationStatus(string.IsNullOrWhiteSpace(message) ? (string)sender : message, (string)sender == ErrorCode.OK.ToString());
        }
        if (e != null)
        {
            CollectionViewField.SelectedItem = null;
            await SetRightPageHeaderAsync(string.Empty, true).ConfigureAwait(true);
            if ((string)sender == ErrorCode.OK.ToString())
            {
                AppHelper.ShowBusyIndicator = true;
                _tasksData.Task = new TaskModel();
                await LoadUIAsync(true).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
            else
            {
                await NoRecordFoundViewAsync().ConfigureAwait(true);
            }
        }
    }

    private async void CollectionViewField_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) && ParentPage.IsFormValid())
        {
            var item = sender as CollectionView;
            if (item.SelectedItem != null)
            {
                await PatientTaskClickedAsync(item.SelectedItem as TaskModel).ConfigureAwait(true);
                item.SelectedItem = null;
            }
        }
    }

    internal async Task PatientTaskClickedAsync(TaskModel selectedTask)
    {
        if (selectedTask.PatientTaskID == 0)
        {
            CollectionViewField.SelectedItem = null;
        }
        var patientTaskPage = new PatientTaskProviderPopupPage(GetTaskType(selectedTask), selectedTask.PatientTaskID, selectedTask.ItemID, selectedTask.ProgramID, selectedTask.Status, _tasksData.SelectedUserID);
        patientTaskPage.OnSaveButtonClicked += RefreshTasksList;
        //todo:await Navigation.PushPopupAsync(patientTaskPage).ConfigureAwait(true);
    }

    private TaskType GetTaskType(TaskModel selectedTask)
    {
        return selectedTask.Status == ResourceConstants.R_COMPLETED_STATUS_KEY
            || selectedTask.Status == ResourceConstants.R_MISSED_STATUS_KEY
            || selectedTask.TaskRespondent.ToEnum<ReadingAddedBy>() == ReadingAddedBy.PatientKey
                ? TaskType.Default
                : selectedTask.TaskType.ToEnum<TaskType>();
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {

        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) && ParentPage.IsFormValid())
        {
            OnActionButtonClicked -= OnAddButtonClicked;
            var patientTaskPage = new AssignTaskPopupPage(_tasksData.SelectedUserID);
            patientTaskPage.OnSaveButtonClicked += RefreshTasksList;
            //todo:await Navigation.PushPopupAsync(patientTaskPage).ConfigureAwait(true);
            OnActionButtonClicked += OnAddButtonClicked;
        }

    }

    private async void RefreshTasksList(object sender, EventArgs e)
    {
        CollectionViewField.SelectedItem = null;
        if (sender != default)
        {
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey((string)sender), (string)sender == ErrorCode.OK.ToString());
            if ((string)sender == ErrorCode.OK.ToString())
            {
                AppHelper.ShowBusyIndicator = true;
                await LoadUIAsync(true).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
        }
        /*CollectionViewField.SelectedItem = null;
        string errorCode = sender.ToString();
        ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(sender.ToString()), errorCode == ErrorCode.OK.ToString());
        if (errorCode == ErrorCode.OK.ToString())
        {
            await LoadUIAsync(true).ConfigureAwait(true);
        }*/
    }

    private async void OnListItemClick(object sender, SelectionChangedEventArgs e)
    {
        var item = sender as CollectionView;
        if (item.SelectedItem is TaskModel taskData && taskData.PatientTaskID != _selectTaskID)
        {
            if (taskData.TaskRespondent.ToEnum<ReadingAddedBy>() != ReadingAddedBy.ProviderKey)
            {
                AppHelper.ShowBusyIndicator = true;
                _selectTaskID = _tasksData.Task.PatientTaskID = taskData.PatientTaskID;
                await SetRightPageHeaderAsync(null, true).ConfigureAwait(true);
                TaskType taskType = _selectedTabKey == ResourceConstants.R_OPEN_TASK_KEY ? taskData.TaskType.ToEnum<TaskType>() : TaskType.Default;
                if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
                {
                    if (ShellMasterPage.CurrentShell.CurrentPage is DashboardPage)
                    {
                        await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientTasksPage.ToString(),
                            GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id, Param.type),
                            Constants.CONSTANT_ZERO, taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture), taskData.TaskType).ConfigureAwait(true);
                    }
                    else
                    {
                        await InitializeDetailViewAsync(taskData, taskType).ConfigureAwait(true);
                        _detailView.OnListRefresh += OnRefreshCall;
                        await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                        if (taskType == TaskType.InstructionKey)
                        {
                            await SetRightPageHeaderAsync(ParentPage.GetResourceValueByKey(taskType.ToString()), true).ConfigureAwait(true);
                        }
                        else
                        {
                            await SetRightPageHeaderAsync(taskData.SelectedItemName, true).ConfigureAwait(true);
                        }
                        AppHelper.ShowBusyIndicator = false;
                    }
                }
                else
                {
                    await NavigateOnDetailPageAsync(taskData, taskType).ConfigureAwait(true);
                }
            }
            else
            {
                var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
                bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
                string messageToDisplay = string.Format(CultureInfo.InvariantCulture
                    , ParentPage.GetResourceValueByKey(ResourceConstants.R_CANNOT_CONTINUE_TASK_MESSAGE_KEY)
                    , ParentPage.GetResourceValueByKey(
                        isPatientData
                            ? ResourceConstants.R_PROVIDER_KEY
                            : ResourceConstants.R_PATIENT_KEY));
                await ParentPage.DisplayMessagePopupAsync(messageToDisplay, OnClosePopoUp, false, true, true).ConfigureAwait(false);
                // CollectionViewField.SelectedItem = null;
                await SetRightPageHeaderAsync(string.Empty, true).ConfigureAwait(true);
                await NoRecordFoundViewAsync().ConfigureAwait(true);
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3168:\"async\" methods should not return \"void\"", Justification = "To use this method as action, return type must be void")]
    private async void OnClosePopoUp(object sender, int e)
    {
        //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
    }

    private async Task RenderUIDataAsync()
    {
        if (_tasksData.ErrCode == ErrorCode.OK)
        {
            bool isPatientOverview = IsPatientOverview(_tasksData.RecordCount);
            // Display Search box when view is loaded as a page (i.e. do not display search box in dashboard page)
            if (!_isDashboard && !isPatientOverview)
            {
                SearchField.IsVisible = true;
                SearchField.PageResources = ParentPage.PageData;
                SearchField.OnSearchTextChanged += OnTaskSearch;
            }
            // Clear right side view when no task is selected and list is refreshing (Ex: after completing task from detail page)
            // register list item click event when method is not being called from list refresh flow
            if (_isPatientPage && !_isDashboard)
            {
                ProviderPatientUIData(isPatientOverview);
            }
            else
            {
                OnListItemSelection(OnListItemClick, !isPatientOverview);
            }
            CollectionViewField.ItemsSource = new List<TaskModel>();
            CollectionViewField.ItemsSource = _tasksData.Tasks;
            (ShellMasterPage.CurrentShell as ShellMasterPage).UpdateBadgeCount(nameof(PatientTasksPage), _tasksData.BadgeCount);
            if (GenericMethods.IsListNotEmpty(_tasksData.Tasks))
            {
                // In case of Dashboard, Do not Allow scroll in list, and apply height of view based on record count
                if (_isDashboard)
                {
                    _mainLayout.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _tasksData.Tasks.Count + new OnIdiom<int> { Phone = 5, Tablet = 0 };
                }
                else
                {
                    if (!_isPatientPage)
                    {
                        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
                        {
                            await PatientTabletUIDataAsync();
                        }
                        else
                        {
                            CollectionViewField.SelectedItem = _tasksData.Tasks.FirstOrDefault(task => task.PatientTaskID == _tasksData.Task.PatientTaskID);
                        }
                    }
                }
                return;
            }
        }
        //Display ErrorMessage or No data found view when not returned from above cases
        await DisplayMessageViewAsync().ConfigureAwait(true);
    }

    private async Task PatientTabletUIDataAsync()
    {
        if (_tasksData.Task.PatientTaskID > 0)
        {
            var selectedItem = _tasksData.Tasks.FirstOrDefault(x => x.PatientTaskID == _tasksData.Task.PatientTaskID);
            CollectionViewField.SelectedItem = selectedItem;
            _tasksData.Task.ItemID = selectedItem?.ItemID ?? 0;
        }
        else
        {
            await NoRecordFoundViewAsync().ConfigureAwait(true);
        }
    }

    private void ProviderPatientUIData(bool isPatientOverview)
    {
        if (_tasksData.RecordCount == 0)
        {
            SetTableHeader(GenericMethods.IsListNotEmpty(_tasksData.Tasks) ? _tasksData.Tasks.Count : 0);
        }
        if (ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientAssignTaskView.ToString()) && !isPatientOverview)
        {
            if (IsTabletListHeaderDisplay)
            {
                TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
            }
            if (!IsPatientOverview(_tasksData.RecordCount))
            {
                OnActionButtonClicked -= OnAddButtonClicked;
                OnActionButtonClicked += OnAddButtonClicked;
            }
        }
        else
        {
            HideAddButton(_mainLayout, true);
        }
        OnListItemSelection(CollectionViewField_SelectionChanged, ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientTaskView.ToString()) && !isPatientOverview);
    }

    private void SetTableHeader(int count)
    {
        if (TabletHeader != null)
        {
            TabletHeader.Text = string.Concat(ParentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientTasksView.ToString()), " ", $"({count})");
        }
    }

    private void GenerateTabs(bool isRefreshRequest)
    {
        if (!_isDashboard && !isRefreshRequest && !IsPatientOverview(_tasksData.RecordCount) && !_isPatientPage)
        {
            _customTabs.LoadUIData((from task in ParentPage.PageData.Resources
                                    where task.ResourceKey == ResourceConstants.R_OPEN_TASK_KEY
                                        || task.ResourceKey == ResourceConstants.R_HISTORY_TASK_KEY
                                    select new OptionModel
                                    {
                                        OptionID = task.ResourceID,
                                        OptionText = task.ResourceValue,
                                        GroupName = task.ResourceKey
                                    }).ToList(), true);
            _customTabs.TabClicked += OnTaskTabClicked;
        }
    }

    private async Task InitializeDetailViewAsync(TaskModel taskData, TaskType taskType)
    {
        if (_detailView != null)
        {
            _detailView.OnListRefresh -= OnRefreshCall;
            _mainLayout.Children.Remove(_detailView);
        }
        switch (taskType)
        {
            case TaskType.MeasurementKey:
                _detailView = new PatientReadingView(new BasePage(), ParentPage.AddParameters(
                    ParentPage.CreateParameter(nameof(PatientReadingUIModel.ReadingID), taskData.ItemID.ToString(CultureInfo.InvariantCulture)),
                    ParentPage.CreateParameter(nameof(PatientReadingModel.PatientTaskID), taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture)),
                    ParentPage.CreateParameter(nameof(PatientReadingUIModel.PatientReadingID), Guid.Empty.ToString())
                    ))
                {
                    Margin = new Thickness(0, -(double)Application.Current.Resources[StyleConstants.ST_APP_PADDING] + 5, 0, 0)
                };
                break;
            case TaskType.EducationKey:
                App._essentials.SetPreferenceValue(StorageConstants.PR_REMOVE_SCROLL_VIEW_KEY, true);
                _detailView = new StaticMessageView(new BasePage(), ParentPage.AddParameters
                (
                    ParentPage.CreateParameter(nameof(StaticMessageView.Key), taskData.ItemID.ToString(CultureInfo.InvariantCulture)),
                    ParentPage.CreateParameter(nameof(StaticMessageView.MessageType), PageType.ContentPage.ToString()),
                    ParentPage.CreateParameter(nameof(StaticMessageView.TargetPageParams)
                        , await ShellMasterPage.GenerateParamAsync(taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture), taskData.Status)),
                    ParentPage.CreateParameter(nameof(StaticMessageView.GroupName), GroupConstants.RS_MENU_PAGE_GROUP))
                );
                break;
            case TaskType.QuestionnaireKey:
                _detailView = new QuestionnaireTaskView(new BasePage(), ParentPage.AddParameters(
                    ParentPage.CreateParameter(nameof(TaskModel.PatientTaskID), taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture)),
                    ParentPage.CreateParameter(nameof(TaskModel.ItemID), taskData.ItemID.ToString(CultureInfo.InvariantCulture)))
                );
                break;
            case TaskType.InstructionKey:
                _detailView = new PatientInstructionView(new BasePage(), ParentPage.AddParameters(
                    ParentPage.CreateParameter(nameof(TaskModel.PatientTaskID), taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture)),
                    ParentPage.CreateParameter(nameof(TaskModel.ItemID), taskData.ItemID.ToString(CultureInfo.InvariantCulture)))
                );
                break;
            case TaskType.Default:
                _detailView = new PatientTaskView(new BasePage(), ParentPage.AddParameters(
                    ParentPage.CreateParameter(nameof(TaskModel.PatientTaskID), taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture))))
                {
                    Margin = new Thickness(0)
                };
                break;
            default:
                _detailView = new DefaultContentView(ParentPage, ParentPage.AddParameters(
                    ParentPage.CreateParameter(nameof(MessageViewType), ((int)MessageViewType.StaticMessageView).ToString(CultureInfo.InvariantCulture)),
                    ParentPage.CreateParameter(nameof(ResourceModel.ResourceKey), ResourceConstants.R_NO_DATA_FOUND_KEY))
                );
                break;
        }
        _mainLayout.Add(_detailView, 2, 0);
        Grid.SetRowSpan(_detailView, 3);
        if (_detailView is QuestionnaireTaskView)
        {
            //todo: _mainLayout.LowerChild(_detailView);
        }
    }

    private async Task NavigateOnDetailPageAsync(TaskModel taskData, TaskType taskType)
    {
        switch (taskType)
        {
            case TaskType.MeasurementKey:
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientReadingPage.ToString()
                    , GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.type)
                    , taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture)
                    , taskData.ItemID.ToString(CultureInfo.InvariantCulture)
                    , taskData.IsActive.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
                break;
            case TaskType.EducationKey:
                App._essentials.SetPreferenceValue(StorageConstants.PR_REMOVE_SCROLL_VIEW_KEY, true);
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Constants.STATIC_MESSAGE_PAGE_IDENTIFIER
                    , GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.type, Param.identifier, Param.isAdd)
                    , taskData.ItemID.ToString(CultureInfo.InvariantCulture), PageType.ContentPage.ToString()
                    , await ShellMasterPage.GenerateParamAsync(taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture), taskData.Status)
                    , GroupConstants.RS_MENU_PAGE_GROUP).ConfigureAwait(true);
                break;
            case TaskType.QuestionnaireKey:
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.QuestionnaireTaskPage.ToString()
                    , GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.identifier)
                    , taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture)
                    , taskData.ItemID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
                break;
            case TaskType.InstructionKey:
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientInstructionPage.ToString()
                    , GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.identifier)
                    , taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture)
                    , taskData.ItemID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
                break;
            default:
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientTaskPage.ToString()
                    , GenericMethods.GenerateParamsWithPlaceholder(Param.id)
                    , taskData.PatientTaskID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
                break;
        }
    }

    private async Task DisplayMessageViewAsync()
    {
        CollectionViewField.ItemsSource = new List<TaskModel>();
        await NoRecordFoundViewAsync().ConfigureAwait(true);
        RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, _isDashboard
            , (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
    }

    private async Task SetRightPageHeaderAsync(string title, bool shouldAutoOverrideRightHeader)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !_isDashboard)
        {
            //if (shouldAutoOverrideRightHeader)
            //{
                //if (title == null)
                //{
                    //await ParentPage.SetRightHeaderItemsAsync(nameof(PatientTaskPage)).ConfigureAwait(true);
                //}
                //else
                //{
                    //if (_detailView is PatientReadingView)
                    //{
                        //await ParentPage.SetRightHeaderItemsAsync(nameof(PatientReadingPage)).ConfigureAwait(true);
                    //}
                //    await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, title, true)).ConfigureAwait(true);
                //}
            //}
            //else
            //{
                if (!(_detailView is DefaultContentView))
                {
                    await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, title, true)).ConfigureAwait(true);
                }
            //}
        }
    }

    private async Task NoRecordFoundViewAsync()
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !_isDashboard && !IsPatientPage())
        {
            await InitializeDetailViewAsync(null, TaskType.SMSKey).ConfigureAwait(true);
            await _detailView.LoadUIAsync(false).ConfigureAwait(true);
        }
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_tasksData.Tasks))
        {
            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _tasksData.Tasks.Count + 60;
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }
}