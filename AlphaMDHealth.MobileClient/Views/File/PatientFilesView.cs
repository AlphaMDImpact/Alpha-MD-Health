using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientFilesView : BaseLibCollectionView
{
    private readonly FileDTO _documentsData = new FileDTO { Files = new List<FileModel>() };
    private PatientFileView _fileView;
    private readonly Grid _mainLayout;
    private readonly CustomMessageControl _emptyMessageView;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientFilesView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new FileService(App._essentials);
        _documentsData.File = new FileModel
        {
            FileID = GenericMethods.MapValueType<Guid>(GetParameterValue(nameof(FileModel.FileID))),
        };
        if (Parameters?.Count > 0)
        {
            _documentsData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        }
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellHeader = nameof(FileModel.FileName),
            CellDescription = nameof(FileModel.FormattedDate),
            CellLeftDefaultIcon = nameof(FileModel.FileImage),
            //todo:CellLeftSourceIcon = nameof(FileModel.ImageSource),
            CellRightContentHeader = nameof(FileModel.FormattedNumberOfFiles),
            CellRightContentDescription = nameof(FileModel.NumberOfFiles),
            RightDesciptionStyle = StyleConstants.ST_BADGE_STYLE,
            IsUnreadHeader = nameof(FileModel.IsUnreadHeader),
            IconSize = AppImageSize.ImageSizeM,
            ArrangeHorizontal = false
        };
        IsTabletListHeaderDisplay = IsPatientPage() && !IsDashboardView(_documentsData.RecordCount);
        _mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = !IsDashboardView(_documentsData.RecordCount) && DeviceInfo.Idiom == DeviceIdiom.Tablet
                ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture)
                : Constants.ZERO_PADDING,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            ColumnDefinitions = CreateTabletViewColumn(IsDashboardView(_documentsData.RecordCount) || DeviceInfo.Idiom == DeviceIdiom.Phone)
        };
        if (IsTabletListHeaderDisplay)
        {
            AddCollectionViewWithTabletHeader(_mainLayout, customCellModel);
        }
        else
        {
            AddCollectionView(_mainLayout, customCellModel, 0, 1);
            if (!IsDashboardView(_documentsData.RecordCount))
            {
                AddSearchView(_mainLayout, false);
                SearchField.IsVisible = false;
                if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
                {
                    AddSeparatorView(_mainLayout, 1, 0);
                    Separator.IsVisible = false;
                    Grid.SetRowSpan(Separator, 2);
                }
            }

        }
        SetPageContent(_mainLayout);
        _emptyMessageView = new CustomMessageControl(false);
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            _documentsData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            var fileId = GenericMethods.MapValueType<string>(GetParameterValue(nameof(FileModel.FileID)));
            _documentsData.File = new FileModel
            {
                FileID = !string.IsNullOrWhiteSpace(fileId)
                    ? new Guid(fileId)
                    : Guid.Empty
            };
            _documentsData.AccountID = App._essentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, (long)0);
            _documentsData.LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
        }
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            OnListItemSelection(Documents_SelectionChanged, !IsPatientOverview(_documentsData.RecordCount));
            await (ParentPage.PageService as FileService).GetFilesAsync(_documentsData).ConfigureAwait(true);
            ParentPage.PageData = ParentPage.PageService.PageData;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadUIForPatientAsync(isRefreshRequest).ConfigureAwait(true);
            });
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        OnListItemSelection(Documents_SelectionChanged, false);
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        if (_fileView != null)
        {
            await _fileView.UnloadUIAsync().ConfigureAwait(true);
        }
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Refresh the list
    /// </summary>
    public async Task RefreshListAsync(Guid fileId)
    {
        await (ParentPage.PageService as FileService).GetFilesAsync(_documentsData).ConfigureAwait(true);
        if (_documentsData.ErrCode == ErrorCode.OK)
        {
            CollectionViewField.ItemsSource = _documentsData.Files;
            if (!(ShellMasterPage.CurrentShell.CurrentPage is PatientsPage))
            {
                CollectionViewField.SelectedItem = _documentsData.Files.FirstOrDefault(x => x.FileID == fileId);
                CollectionViewField.ScrollTo(CollectionViewField.SelectedItem);
            }
        }
    }

    /// <summary>
    /// Save Documents Data
    /// </summary>
    public async Task<bool> SaveDocumentAsync()
    {
        return _fileView != null && _mainLayout.Children.Contains(_fileView) && await _fileView.SaveDocumentAsync().ConfigureAwait(true);
    }

    private async Task LoadUIForPatientAsync(bool isRefreshRequest)
    {
        if (!IsDashboardView(_documentsData.RecordCount))
        {
            SearchField.Value = string.Empty;
        }
        _emptyListView.PageResources = ParentPage.PageData;
        _emptyMessageView.PageResources = ParentPage.PageData;
        if (!IsDashboardView(_documentsData.RecordCount))
        {
            SearchField.IsVisible = true;
            SearchField.PageResources = ParentPage.PageData;
            SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !IsPatientPage())
            {
                Separator.IsVisible = true;
            }
        }
        if (_documentsData?.ErrCode == ErrorCode.OK)
        {
            await RenderFileDataAsync(isRefreshRequest).ConfigureAwait(true);
        }
        else
        {
            RenderErrorView(_mainLayout, _documentsData?.ErrCode.ToString() ?? ErrorCode.ErrorWhileRetrievingRecords.ToString()
                , IsDashboardView(_documentsData.RecordCount), 0, false, true);
        }
    }

    private async Task RenderFileDataAsync(bool isRefreshRequest)
    {
        if (IsPatientPage() && !IsDashboardView(_documentsData.RecordCount))
        {
            SearchField.IsVisible = true;
            if (HasAddPermission())
            {
                TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                if (!isRefreshRequest)
                {
                    OnActionButtonClicked += OnAddButtonClicked;
                }
            }
            else
            {
                HideAddButton(_mainLayout, true);
            }
            if (!IsDashboardView(_documentsData.RecordCount))
            {
                TabletHeader.Text = GetHeaderText(_documentsData.Files.Count);
            }
        }
        CollectionViewField.ItemsSource = new List<FileModel>();
        if (GenericMethods.IsListNotEmpty(_documentsData.Files))
        {
            await RenderViewsAsync().ConfigureAwait(true);
        }
        else
        {
            bool isDashboard = IsDashboardView(_documentsData.RecordCount);
            RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, isDashboard
                , (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
            if (!isDashboard)
            {
                await RenderDetailViewAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task RenderViewsAsync()
    {
        CollectionViewField.ItemsSource = _documentsData.Files;
        if (!IsDashboardView(_documentsData.RecordCount))
        {
            await RenderDetailViewAsync().ConfigureAwait(true);
        }
        else
        {
            _mainLayout.HeightRequest = MobileConstants.IsDevicePhone && DeviceInfo.Platform == DevicePlatform.iOS
                ? CellRowHeight * _documentsData.Files.Count
                : (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _documentsData.Files.Count + new OnIdiom<int> { Phone = 5, Tablet = 0 };
        }
    }

    private bool HasAddPermission()
    {
        return ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientFileAddEdit.ToString());
    }

    private string GetHeaderText(int count)
    {
        return $"{ParentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientFilesView.ToString())} ({count})";
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY))
        {
            var fileAddEditPage = new PatientFilePopupPage(Guid.Empty.ToString());
            fileAddEditPage._fileView.OnListRefresh += FileView_OnListRefresh;
            //todo:await Navigation.PushPopupAsync(fileAddEditPage).ConfigureAwait(true);
        }
    }

    private async Task RenderDetailViewAsync()
    {
        if (MobileConstants.IsTablet && !IsPatientPage())
        {
            if (_documentsData.File?.FileID != Guid.Empty)
            {
                await DocumentClickAsync(_documentsData.File.FileID, false).ConfigureAwait(true);
                CollectionViewField.SelectedItem = _documentsData.Files.FirstOrDefault(x => x.FileID == _documentsData.File.FileID);
            }
            else
            {
                if (_fileView != null && _mainLayout.Children.Contains(_fileView))
                {
                    _mainLayout.Children.Remove(_fileView);
                }
                _emptyMessageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
                _mainLayout.Add(_emptyMessageView, 2, 0);
                Grid.SetRowSpan(_emptyMessageView, 2);
            }
        }
    }

    private void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        CollectionViewField.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            CollectionViewField.ItemsSource = _documentsData.Files;
            if (IsPatientPage())
            {
                TabletHeader.Text = GetHeaderText(_documentsData.Files.Count);
            }
        }
        else
        {
            var searchedUsers = _documentsData.Files.FindAll(y =>
            {
                return y.FileName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });
            CollectionViewField.ItemsSource = searchedUsers;
            if (IsPatientPage())
            {
                TabletHeader.Text = GetHeaderText(searchedUsers.Count);
            }
            if (searchedUsers.Count <= 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
    }

    private async void Documents_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (isPatientData)
        {
            await DocumentSelectedAsync(sender).ConfigureAwait(true);
        }
        else
        {
            if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY))
            {
                await DocumentSelectedAsync(sender).ConfigureAwait(true);
            }
        }
    }

    private async Task DocumentSelectedAsync(object sender)
    {
        var item = sender as CollectionView;
        if (item.SelectedItem != null)
        {
            await DocumentClickAsync((item.SelectedItem as FileModel).FileID, false).ConfigureAwait(true);
        }
    }

    internal async Task DocumentClickAsync(Guid documentID, bool isAddButtonClicked)
    {
        if (isAddButtonClicked)
        {
            CollectionViewField.SelectedItem = null;
        }
        if (MobileConstants.IsTablet)
        {
            if (HasAddPermission())
            {
                if (ShellMasterPage.CurrentShell.CurrentPage is PatientFilesPage)
                {
                    //await ParentPage.SetRightHeaderItemsAsync(nameof(PatientFilePage)).ConfigureAwait(true);
                    if (_fileView != null && _mainLayout.Children.Contains(_fileView))
                    {
                        _mainLayout.Children.Remove(_fileView);
                    }
                    if (_mainLayout.Children.Contains(_emptyMessageView))
                    {
                        _mainLayout.Children.Remove(_emptyMessageView);
                    }
                    _fileView = new PatientFileView(ParentPage, ParentPage.AddParameters(
                        ParentPage.CreateParameter(nameof(FileModel.FileID), Convert.ToString(documentID, CultureInfo.InvariantCulture)),
                        ParentPage.CreateParameter(nameof(BaseDTO.IsActive), Convert.ToString(false, CultureInfo.InvariantCulture)))
                    );
                    _fileView.OnListRefresh += FileView_OnListRefresh;
                    _mainLayout.Add(_fileView, 2, 0);
                    Grid.SetRowSpan(_fileView, 2);
                    await _fileView.LoadUIAsync(false).ConfigureAwait(true);
                }
                else if (IsPatientPage())
                {
                    if (!PatientFilePopupPage.IsPopupClick)
                    {
                        var fileAddEditPage = new PatientFilePopupPage(documentID.ToString());
                        fileAddEditPage._fileView.OnListRefresh += FileView_OnListRefresh;
                        //todo:await Navigation.PushPopupAsync(fileAddEditPage).ConfigureAwait(true);
                    }
                }
                else
                {
                    await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientFilesPage.ToString()
                        , GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id), "0", documentID.ToString()).ConfigureAwait(true);
                }
            }
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientFilePage.ToString()
                , GenericMethods.GenerateParamsWithPlaceholder(Param.id), documentID.ToString()).ConfigureAwait(true);
        }
    }

    private async void FileView_OnListRefresh(object sender, EventArgs e)
    {
        var senderItem = sender?.ToString();
        bool isValid = Guid.TryParse(senderItem, out Guid guidOutput);
        if (isValid)
        {
            if (guidOutput == Guid.Empty)
            {
                ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(ErrorCode.OK.ToString()), true);
                await LoadUIAsync(true).ConfigureAwait(true);
            }
            else
            {
                await RefreshListAsync(guidOutput).ConfigureAwait(true);
            }
        }
        else
        {
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey((string)sender));
            await LoadUIAsync(true).ConfigureAwait(true);
        }
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_documentsData.Files))
        {
            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _documentsData.Files.Count + 60;
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }
}