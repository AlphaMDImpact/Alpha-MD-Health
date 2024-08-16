using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientTasksProviderView : BaseLibCollectionView
{
    private readonly ProgramDTO _tasksData = new ProgramDTO();
    private readonly Grid _mainLayout;

    public PatientTasksProviderView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new PatientTaskService(App._essentials);
        MapParameters();
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellHeader = nameof(TaskModel.SelectedItemName),
            BandColor = nameof(TaskModel.ProgramColor),
            CellDescription = nameof(TaskModel.Name),
            CellLeftDefaultIcon = nameof(TaskModel.ImageName),
            CellRightContentHeader = nameof(TaskModel.FromDateString),
            CellRightContentDescription = nameof(TaskModel.StatusValue),
            CellDescriptionColor = nameof(TaskModel.StatusColor),
            IconSize = AppImageSize.ImageSizeM,
        };
        IsTabletListHeaderDisplay = IsPatientPage() && _tasksData.RecordCount < 1;
        _mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnSpacing = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            ColumnDefinitions = CreateTabletViewColumn(false),
        };
        AddCollectionViewWithTabletHeader(_mainLayout, customCellModel);
        SetPageContent(_mainLayout);
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        MapParameters();
        await RefreshPageUIAsync(isRefreshRequest).ConfigureAwait(true);
        if (_tasksData.ErrCode == ErrorCode.OK)
        {
            _emptyListView.PageResources = ParentPage.PageData;
            if (_tasksData.RecordCount == 0)
            {
                SearchField.PageResources = ParentPage.PageData;
                SearchField.OnSearchTextChanged += SearchField_OnSearchTextChanged;
            }
            else
            {
                SearchField.IsVisible = false;
            }
            if (ParentPage.CheckFeaturePermissionByCode(AppPermissions.PatientAssignTaskView.ToString()))
            {
                TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                OnActionButtonClicked += OnAddButtonClicked;
            }
            else
            {
                HideAddButton(_mainLayout, true);
            }
            OnListItemSelection(CollectionViewField_SelectionChanged, ParentPage.CheckFeaturePermissionByCode(AppPermissions.PatientTaskView.ToString()) && !IsPatientOverview(_tasksData.RecordCount));
        }
    }

    private void MapParameters()
    {
        if (Parameters?.Count > 0)
        {
            _tasksData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            _tasksData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
        }
    }

    private async Task RefreshPageUIAsync(bool isRefreshRequest)
    {
        await (ParentPage.PageService as PatientTaskService).GetTasksAsync(_tasksData, string.Empty).ConfigureAwait(true);
        if (!isRefreshRequest)
        {
            ParentPage.PageData = ParentPage.PageService.PageData;
        }
        if (_tasksData.ErrCode == ErrorCode.OK)
        {
            if (_tasksData.RecordCount == 0)
            {
                SetTableHeader(GenericMethods.IsListNotEmpty(_tasksData.Tasks) ? _tasksData.Tasks.Count : 0);
            }
            if (GenericMethods.IsListNotEmpty(_tasksData.Tasks))
            {
                CollectionViewField.ItemsSource = _tasksData.Tasks;
            }
            else
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, _tasksData.RecordCount > 0, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
            }
        }
        else
        {
            RenderErrorView(_mainLayout, _tasksData.ErrCode.ToString(), _tasksData.RecordCount > 0, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
    }

    private void SetTableHeader(int count)
    {
        TabletHeader.Text = string.Concat(ParentPage.GetFeatureValueByCode(AppPermissions.PatientTasksView.ToString()), " ", $"({count})");
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        OnActionButtonClicked -= OnAddButtonClicked;
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true) && ParentPage.IsFormValid())
        {
            var patientTaskPage = new AssignTaskPopupPage(_tasksData.SelectedUserID);
            patientTaskPage.OnSaveButtonClicked += RefreshTasksList;
            //todo:await Navigation.PushPopupAsync(patientTaskPage).ConfigureAwait(true);
        }
        OnActionButtonClicked += OnAddButtonClicked;
    }
    private async void RefreshTasksList(object sender, EventArgs e)
    {
        CollectionViewField.SelectedItem = null;
        string errorCode = sender.ToString();
        ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(sender.ToString()), errorCode == ErrorCode.OK.ToString());
        if (errorCode == ErrorCode.OK.ToString())
        {
            await RefreshPageUIAsync(true).ConfigureAwait(true);
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
            }
        }
    }

    private void SearchField_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        CollectionViewField.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            CollectionViewField.ItemsSource = _tasksData.Tasks;
            SetTableHeader(GenericMethods.IsListNotEmpty(_tasksData.Tasks) ? _tasksData.Tasks.Count : 0);
        }
        else
        {
            var searchedUsers = _tasksData.Tasks.FindAll(y =>
            {
                return y.Name.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant()) ||
                    y.SelectedItemName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });
            CollectionViewField.ItemsSource = searchedUsers;
            SetTableHeader(GenericMethods.IsListNotEmpty(searchedUsers) ? searchedUsers.Count : 0);
            if (searchedUsers.Count <= 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
    }

    public override Task UnloadUIAsync()
    {
        OnListItemSelection(CollectionViewField_SelectionChanged, false);
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= SearchField_OnSearchTextChanged;
        }
        return base.UnloadUIAsync();
    }

    internal async Task PatientTaskClickedAsync(TaskModel selectedTask)
    {
        if (selectedTask.PatientTaskID == 0)
        {
            CollectionViewField.SelectedItem = null;
        }
        var patientTaskPage = new PatientTaskProviderPopupPage(GetTaskType(selectedTask), selectedTask.PatientTaskID, selectedTask.ItemID, selectedTask.ProgramID, selectedTask.Status, 0);
        patientTaskPage.OnSaveButtonClicked += RefreshTasksList;
        //todo:await Navigation.PushPopupAsync(patientTaskPage).ConfigureAwait(true);
    }

    private TaskType GetTaskType(TaskModel selectedTask)
    {
        return selectedTask.Status == ResourceConstants.R_COMPLETED_STATUS_KEY || selectedTask.Status == ResourceConstants.R_MISSED_STATUS_KEY || selectedTask.TaskRespondent == ReadingAddedBy.PatientKey.ToString() ? TaskType.Default : selectedTask.TaskType.ToEnum<TaskType>();
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