using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Readings details view
/// </summary>
public class PatientReadingDetailView : ViewManager
{
    private readonly CustomReadingDetailsControl _readingControl;
    private PatientReadingDTO _readingDetailsData;
    private readonly CustomLabelControl _tabletHeader;
    private readonly CustomLabelControl _tabletAddReadingButton;
    private readonly CustomLabelControl _tabletSetTargetButton;
    private short _readingCategoryID;
    private bool _hasHealthPermission = true;

    /// <summary>
    /// Event invoked when list item selection is changed
    /// </summary>
    public Func<string, string, Task> HandleNavigation { get; set; }

    /// <summary>
    /// Get details view height
    /// </summary>
    public double GetViewHeight
    {
        get
        {
            if (_readingDetailsData.ListData?.Count > 0 || _readingDetailsData.GraphData?.Count > 0)
            {
                return 280
                    + (_readingDetailsData.ChartMetaData[0].ShowInData
                        ? (65 * _readingDetailsData.ListData.Count) + AppStyles.GetImageSize(AppImageSize.ImageSizeL)
                        : 0)
                    + (_readingDetailsData.ChartMetaData[0].ShowInGraph
                        ? 240
                        : 0);
            }
            return 0;
        }
    }

    /// <summary>
    /// Reading details view
    /// </summary>
    /// <param name="page">Instance of base page</param>
    /// <param name="parameters">View parameters</param>
    public PatientReadingDetailView(BasePage page, object parameters) : base(page, parameters)
    {
        var padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
        ParentPage.PageService = new ReadingService(App._essentials);
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 1, true);
        ParentPage.AddRowColumnDefinition(GridLength.Star, 1, true);
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 1, true);
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 2, false);
        _readingControl = new CustomReadingDetailsControl { HasHealthKitPermission = _hasHealthPermission };
        Padding = new Thickness(padding, padding, padding, 0);
        BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
        if (IsPatientPage())
        {
            _tabletHeader = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft);
            ParentPage.PageLayout.RowSpacing = padding;
            ParentPage.PageLayout.Add(_tabletHeader, 0, 0);

            _tabletAddReadingButton = new CustomLabelControl(LabelType.LinkLabelSmallLeft) { TextDecorations = TextDecorations.None, Padding = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(padding, 0, 0, 0) : new Thickness(0, 0, padding, 0), IsVisible = false };
            TapGestureRecognizer ReadingAddtapGestureRecognizer = new TapGestureRecognizer();
            ReadingAddtapGestureRecognizer.Tapped += AddReadingClicked;
            _tabletAddReadingButton.GestureRecognizers.Add(ReadingAddtapGestureRecognizer);
            ParentPage.PageLayout.Add(_tabletAddReadingButton, 1, 0);

            _tabletSetTargetButton = new CustomLabelControl(LabelType.LinkLabelSmallLeft) { TextDecorations = TextDecorations.None, Padding = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness((double)AppImageSize.ImageSizeD, 0, 0, 0) : new Thickness(0, 0, (double)AppImageSize.ImageSizeD, 0) };
            TapGestureRecognizer SetTargettapGestureRecognizer = new TapGestureRecognizer();
            SetTargettapGestureRecognizer.Tapped += SetTargetClicked;
            _tabletSetTargetButton.GestureRecognizers.Add(SetTargettapGestureRecognizer);
            ParentPage.PageLayout.Add(_tabletSetTargetButton, 2, 0);
        }
        else
        {
            Margin = new OnIdiom<Thickness>
            {
                Phone = new Thickness(0, -padding, 0, 0),
                Tablet = new Thickness(-padding, 0)
            };
        }
        ParentPage.PageLayout.Add(_readingControl, 0, 1);
        Grid.SetColumnSpan(_readingControl, 3);
        if (MobileConstants.IsTablet)
        {
            SetPageContent(ParentPage.PageLayout);
        }
    }

    /// <summary>
    /// Loads graph UI along with chart data
    /// </summary>
    /// <returns>Renders UI</returns>
    public async override Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            _readingControl.ChartData = null;
            _readingControl.SelectionChanged += ListSelectionChanged;
            _readingControl.RetrieveDataButtonClicked += RetrieveDataButtonClicked;
        }
        _readingControl.ReadingType = GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientReadingDTO.ReadingID)));
        _readingCategoryID = GenericMethods.MapValueType<short>(GetParameterValue(nameof(PatientReadingDTO.ReadingCategoryID)));
        _readingControl.OnDurationChange = GetReadingDataAsync;
        _readingControl.ShowConnectButton = !IsPatientPage();
        await _readingControl.LoadUIAsync(isRefreshRequest).ConfigureAwait(true);
        var metadata = _readingDetailsData.ChartMetaData[0];
        if (metadata.AllowHealthKitData && !isRefreshRequest)
        {
            await SyndAndDisplayHealthDataOptionsAsync();
        }
        if (IsPatientPage())
        {
            _tabletHeader.Text = metadata.ReadingParentID > 0 && !metadata.IsGroupValue ? metadata.Reading : metadata.ReadingParent;
            _tabletAddReadingButton.Text = String.Concat(ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY), Constants.STRING_SPACE, _readingDetailsData.ChartMetaData[0].ReadingParent);
            _tabletSetTargetButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_SET_TARGET_KEY);
            ShowHideAddReadingAction(true);
        }
    }

    private void ShowHideAddReadingAction(bool isProviderPage)
    {
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
        bool isAddAllowed = (ParentPage.PageService as ReadingService).IsAddAllowed(_readingDetailsData.ChartMetaData, (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker));
        if (isAddAllowed && isProviderPage)
        {
            _tabletAddReadingButton.IsVisible = true;
        }
        else if (!isAddAllowed && !isProviderPage)
        {
            ShellMasterPage.CurrentShell.CurrentPage.ShowHideLeftRightHeader(ParentPage is PatientReadingDetailsPage ? MenuLocation.Left : MenuLocation.Right, false);
        }
        else if(!isAddAllowed && isProviderPage)
        {
            _tabletAddReadingButton.IsVisible = false;
        }
    }

    private async void RetrieveDataButtonClicked(object sender, EventArgs e)
    {
        await SyncDataFromDevicesAsync(sender, _readingDetailsData.ChartMetaData[0].ReadingID.ToString(), _readingDetailsData).ConfigureAwait(false);
    }

    /// <summary>
    /// Sync data from bluetooth devices
    /// </summary>
    /// <param name="sender">Sender button</param>
    /// <param name="readingType">currently selected reading type</param>
    /// <param name="result">Return result returns RefreshMasterPage if page needs to be refreshed</param>
    /// <returns>Task representing the sync from device</returns>
    public async Task SyncDataFromDevicesAsync(object sender, string readingType, PatientReadingDTO result)
    {
        Button synchronizeButton = sender as Button;
        synchronizeButton.IsEnabled = false;
        AppHelper.ShowBusyIndicator = true;
        if (await ParentPage.RequestBluetoothPermissionAsync().ConfigureAwait(true))
        {
            BaseDTO returnResult = new BaseDTO();
            await new DeviceService(App._essentials).SyncReadingsFromDevicesAsync(returnResult, readingType).ConfigureAwait(true);
            if (returnResult.ErrCode == ErrorCode.OK)
            {
                if (returnResult.RecordCount > 0)
                {
                    await ParentPage.DisplayMessagePopupAsync(string.Format(CultureInfo.InvariantCulture, ParentPage.GetResourceValueByKey(ResourceConstants.R_RECORDS_UPDATED_MESSAGE_KEY), returnResult.RecordCount), false, true, true).ConfigureAwait(true);
                    if (MobileConstants.IsTablet)
                    {
                        result.ErrCode = ErrorCode.RefreshMasterPage;
                        InvokeListRefresh(result, new EventArgs());
                    }
                    else
                    {
                        await LoadUIAsync(true).ConfigureAwait(true);
                    }
                    await ParentPage.SyncDataWithServerAsync(Pages.PatientReadingPage, false, 0).ConfigureAwait(false);
                }
                else
                {
                    await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_NO_NEW_MEASUREMENT_TEXT_KEY, false, true, false).ConfigureAwait(true);
                }
            }
            else if (returnResult.ErrCode == ErrorCode.DeviceNotFound)
            {
                await ParentPage.DisplayMessagePopupAsync(ParentPage.GetResourceValueByKey(returnResult.ErrCode.ToString()), false, true, true).ConfigureAwait(true);
            }
            else
            {
                await ParentPage.DisplayMessagePopupAsync(ErrorCode.ErrorWhileRetrievingRecords.ToString(), false, true, false).ConfigureAwait(true);
            }
        }
        AppHelper.ShowBusyIndicator = false;
        synchronizeButton.IsEnabled = true;
    }

    /// <summary>
    /// Unloads UI and unregisters attached events
    /// </summary>
    /// <returns>Unloads UI and returns</returns>
    public override Task UnloadUIAsync()
    {
        _readingControl.SelectionChanged -= ListSelectionChanged;
        _readingControl.RetrieveDataButtonClicked -= RetrieveDataButtonClicked;
        return _readingControl.UnloadUIAsync();
    }

    private async void SetTargetClicked(object sender, EventArgs e)
    {
        PatientReadingTargetPage patientReadingTargetPopupPage = new PatientReadingTargetPage(_readingDetailsData.ReadingID);
        patientReadingTargetPopupPage.OnSaveButtonClicked += RefreshMainList;
        //todo:await Navigation.PushPopupAsync(patientReadingTargetPopupPage).ConfigureAwait(true);
    }

    private async void AddReadingClicked(object sender, EventArgs e)
    {
        await ShowReadingAddEditInPopupPageAsync(Guid.Empty.ToString(), _readingDetailsData.ReadingID).ConfigureAwait(true);
    }

    private void RefreshMainList(object sender, EventArgs e)
    {
        _readingDetailsData.ReadingCategoryID = _readingCategoryID;
        InvokeListRefresh(_readingDetailsData, e);
    }

    private async Task<PatientReadingDTO> GetReadingDataAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, string readingType)
    {
        _readingDetailsData = _readingDetailsData ?? new PatientReadingDTO
        {
            RecordCount = -2,
            ReadingID = Convert.ToInt16(readingType),
            SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID))),
            LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0)
        };
        _readingDetailsData.FromDate = startDate?.ToString(CultureInfo.InvariantCulture);
        _readingDetailsData.ToDate = endDate?.ToString(CultureInfo.InvariantCulture);
        await (ParentPage.PageService as ReadingService).GetPatientReadingsAsync(_readingDetailsData).ConfigureAwait(true);
        ParentPage.PageData.Resources = _readingDetailsData.Resources;
        ParentPage.PageData.Settings = _readingDetailsData.Settings;
        if (_readingDetailsData.ErrCode == ErrorCode.OK)
        {
            await ShowDeviceRetrieveButtonAsync().ConfigureAwait(true);
            ShowHideAddReadingAction(false);
            if (!IsPatientPage())
            {
                MenuView titleView = new MenuView(MenuLocation.Header, _readingDetailsData.ChartMetaData[0].ReadingParent, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
                if (ShellMasterPage.CurrentShell.CurrentPage is PatientReadingDetailsPage)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await ParentPage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
                    });
                }
                else
                {
                    if (MobileConstants.IsTablet)
                    {
                        await ShellMasterPage.CurrentShell.CurrentPage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
                    }
                }
            }
            else
            {
                _tabletSetTargetButton.IsVisible = _readingDetailsData.ShowSetTargetButton;
            }
            _readingControl.PageResources = ParentPage.PageData;
            return _readingDetailsData;
        }
        else
        {
            ParentPage.DisplayOperationStatus(_readingDetailsData.ErrCode.ToString());
            return new PatientReadingDTO { ErrCode = _readingDetailsData.ErrCode };
        }
    }

    private async Task ShowDeviceRetrieveButtonAsync()
    {
        if (!IsPatientPage())
        {
            int devicePairingStatus = await new DeviceService(App._essentials).IsDevicePairedForReadingsAsync(_readingControl.ReadingType).ConfigureAwait(true);
            if (devicePairingStatus == 1)
            {
                _readingControl.ShowDeviceButton = true;
            }
        }
    }

    private async void ListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentSelection = ((CollectionView)sender).SelectedItem;
        if (currentSelection == null)
        {
            return;
        }
        var vital = (PatientReadingUIModel)e.CurrentSelection[0];
        string vitalID = vital.PatientReadingID.ToString();
        string readingID = vital.ReadingID.ToString();
        if (MobileConstants.IsTablet)
        {
            if (IsPatientPage())
            {
                ((CollectionView)sender).SelectedItem = null;
                await ShowReadingAddEditInPopupPageAsync(vitalID, vital.ReadingID).ConfigureAwait(true);
                ((CollectionView)sender).SelectedItem = null;
            }
            else
            {
                await HandleNavigation(readingID, vitalID).ConfigureAwait(true);
            }
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientReadingPage),
                GenericMethods.GenerateParamsWithPlaceholder(Param.name, Param.type, Param.id),
                _readingCategoryID.ToString(), readingID, vitalID).ConfigureAwait(true);
        }
    }

    private async Task ShowReadingAddEditInPopupPageAsync(string vitalID, short readingID)
    {
        _readingControl.SelectionChanged -= ListSelectionChanged;
        ReadingPopupPage readingPopupPage = new ReadingPopupPage(_readingCategoryID.ToString(), readingID, vitalID, _readingDetailsData.SelectedUserID.ToString(CultureInfo.InvariantCulture));
        readingPopupPage.OnSaveButtonClicked += RefreshMainList;
        //todo:await Navigation.PushPopupAsync(readingPopupPage).ConfigureAwait(true);
        _readingControl.SelectionChanged += ListSelectionChanged;
    }

    private async Task SyndAndDisplayHealthDataOptionsAsync()
    {
        //todo:
        //BaseDTO result = await ParentPage.SyncDataWithServerAsync(Pages.PatientReadingDetailsPage, ServiceSyncGroups.RSSyncFromDeviceGroup, _readingDetailsData.ChartMetaData[0].Reading, string.Empty, 0).ConfigureAwait(true);
        //if (result.ErrCode == ErrorCode.OK)
        //{
        //    _hasHealthPermission = true;
        //    if (result.RecordCount > 0)
        //    {
        //        await ParentPage.SyncDataWithServerAsync(Pages.PatientReadingPage, false, 0).ConfigureAwait(true);
        //    }
        //}
        //else
        //{
        //    _hasHealthPermission = (Device.RuntimePlatform == Device.iOS && MobileConstants.IsTablet) || result.ErrCode != ErrorCode.Unauthorized;
        //}
        _readingControl.HasHealthKitPermission = _hasHealthPermission;
        await LoadUIAsync(true).ConfigureAwait(true);
    }
}