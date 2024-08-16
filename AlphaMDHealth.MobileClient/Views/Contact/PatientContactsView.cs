using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient contacts view
/// </summary>
public class PatientContactsView : BaseLibCollectionView
{
    private readonly ContactDTO _contactData;
    private PatientContactView _contactView;
    private readonly Grid _mainLayout;
    private readonly CustomMessageControl _emptyMessageView;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientContactsView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new ContactsService(App._essentials);
        _contactData = new ContactDTO { Contact = new ContactModel(), Contacts = new List<ContactModel>() };
        MapParameters();
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellHeader = nameof(ContactModel.ContactType),
            CellDescription = nameof(ContactModel.ContactValue),
            ArrangeHorizontal = false
        };
        IsTabletListHeaderDisplay = IsPatientPage() && !IsDashboardView(_contactData.RecordCount);
        _mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = !IsDashboardView(_contactData.RecordCount) && DeviceInfo.Idiom == DeviceIdiom.Tablet
                ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture)
                : Constants.ZERO_PADDING,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            ColumnDefinitions = CreateTabletViewColumn(IsDashboardView(_contactData.RecordCount) || DeviceInfo.Idiom == DeviceIdiom.Phone)
        };
        if (IsTabletListHeaderDisplay)
        {
            AddCollectionViewWithTabletHeader(_mainLayout, customCellModel);
        }
        else
        {
            AddCollectionView(_mainLayout, customCellModel, 0, 1);
            if (!IsDashboardView(_contactData.RecordCount))
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
        MapProperties(isRefreshRequest);
        OnListItemSelection(Contacts_SelectionChanged, !IsPatientOverview(_contactData.RecordCount));
        await (ParentPage.PageService as ContactsService).GetContactsAsync(_contactData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await LoadUIForPatientAsync(isRefreshRequest).ConfigureAwait(true);
        });
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        OnListItemSelection(Contacts_SelectionChanged, false);
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        if (_contactView != null)
        {
            await _contactView.UnloadUIAsync().ConfigureAwait(true);
        }
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Refresh the list
    /// </summary>
    internal async Task RefreshListAsync(Guid contactID)
    {
        await (ParentPage.PageService as ContactsService).GetContactsAsync(_contactData).ConfigureAwait(true);
        if (_contactData.ErrCode == ErrorCode.OK)
        {
            CollectionViewField.ItemsSource = _contactData.Contacts;
            if (!(ShellMasterPage.CurrentShell.CurrentPage is PatientsPage))
            {
                CollectionViewField.SelectedItem = _contactData.Contacts.FirstOrDefault(x => x.ContactID == contactID);
                CollectionViewField.ScrollTo(CollectionViewField.SelectedItem);
            }
        }
    }

    /// <summary>
    /// Save Contacts Data
    /// </summary>
    internal async Task<bool> SaveContactsAsync()
    {
        return _contactView != null && _mainLayout.Children.Contains(_contactView) && await _contactView.OnSaveActionClicked().ConfigureAwait(true);
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_contactData.Contacts))
        {
            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _contactData.Contacts.Count + 60;
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }

    internal async Task ContactClickAsync(Guid contactID, bool isAddButtonClicked)
    {
        if (isAddButtonClicked)
        {
            CollectionViewField.SelectedItem = null;
        }
        if (MobileConstants.IsTablet)
        {
            if (HasAddPermission())
            {
                if (ShellMasterPage.CurrentShell.CurrentPage is ContactsPage)
                {
                    //await ParentPage.SetRightHeaderItemsAsync(nameof(ContactPage)).ConfigureAwait(true);
                    if (_contactView != null && _mainLayout.Children.Contains(_contactView))
                    {
                        _mainLayout.Children.Remove(_contactView);
                    }
                    if (_mainLayout.Children.Contains(_emptyMessageView))
                    {
                        _mainLayout.Children.Remove(_emptyMessageView);
                    }
                    _contactView = new PatientContactView(ParentPage, ParentPage.AddParameters(
                        ParentPage.CreateParameter(nameof(ContactModel.ContactID), Convert.ToString(contactID, CultureInfo.InvariantCulture))
                    ));
                    _contactView.OnListRefresh += ContactView_OnListRefresh;
                    _mainLayout.Add(_contactView, 2, 0);
                    Grid.SetRowSpan(_contactView, 2);
                    await _contactView.LoadUIAsync(false).ConfigureAwait(true);
                }
                else if (IsPatientPage())
                {
                    if (!ContactPopupPage.IsPopupClick)
                    {
                        var contactAddEditPage = new ContactPopupPage(contactID.ToString());
                        contactAddEditPage._contactView.OnListRefresh += ContactView_OnListRefresh;
                        //todo:await Navigation.PushPopupAsync(contactAddEditPage).ConfigureAwait(true);
                        CollectionViewField.SelectedItem = null;
                    }
                }
                else
                {
                    await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.ContactsPage.ToString()
                        , GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id), "0", contactID.ToString()).ConfigureAwait(true);
                }
            }
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.ContactPage.ToString()
                , GenericMethods.GenerateParamsWithPlaceholder(Param.id), contactID.ToString()).ConfigureAwait(true);
        }
    }

    private async Task LoadUIForPatientAsync(bool isRefreshRequest)
    {
        if (!IsDashboardView(_contactData.RecordCount))
        {
            SearchField.Value = string.Empty;
        }
        _emptyListView.PageResources = ParentPage.PageData;
        _emptyMessageView.PageResources = ParentPage.PageData;
        if (!IsDashboardView(_contactData.RecordCount))
        {
            SearchField.IsVisible = true;
            SearchField.PageResources = ParentPage.PageData;
            SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !IsPatientPage())
            {
                Separator.IsVisible = true;
            }
        }
        if (_contactData?.ErrCode == ErrorCode.OK)
        {
            await RenderContactDataAsync(isRefreshRequest).ConfigureAwait(true);
        }
        else
        {
            RenderErrorView(_mainLayout, _contactData?.ErrCode.ToString() ?? ErrorCode.ErrorWhileRetrievingRecords.ToString()
                , IsDashboardView(_contactData.RecordCount), 0, false, true);
        }
    }

    private async Task RenderContactDataAsync(bool isRefreshRequest)
    {
        if (IsPatientPage() && !IsDashboardView(_contactData.RecordCount))
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
            if (!IsDashboardView(_contactData.RecordCount))
            {
                TabletHeader.Text = GetHeaderText(_contactData.Contacts.Count);
            }
        }
        CollectionViewField.ItemsSource = new List<ContactModel>();
        if (GenericMethods.IsListNotEmpty(_contactData.Contacts))
        {
            await RenderViewsAsync().ConfigureAwait(true);
        }
        else
        {
            bool isDashboard = IsDashboardView(_contactData.RecordCount);
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
        CollectionViewField.ItemsSource = _contactData.Contacts;
        if (!IsDashboardView(_contactData.RecordCount))
        {
            await RenderDetailViewAsync().ConfigureAwait(true);
        }
        else
        {
            _mainLayout.HeightRequest = MobileConstants.IsDevicePhone && DeviceInfo.Platform == DevicePlatform.iOS
                ? CellRowHeight * _contactData.Contacts.Count
                : (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _contactData.Contacts.Count + new OnIdiom<int> { Phone = 5, Tablet = 0 };
        }
    }

    private bool HasAddPermission()
    {
        return ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientContactAddEdit.ToString());
    }

    private string GetHeaderText(int count)
    {
        return $"{ParentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientContactsView.ToString())} ({count})";
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY))
        {
            var contactAddEditPage = new ContactPopupPage(Guid.Empty.ToString());
            contactAddEditPage._contactView.OnListRefresh += ContactView_OnListRefresh;
            //todo:await Navigation.PushPopupAsync(contactAddEditPage).ConfigureAwait(true);
        }
    }

    private async Task RenderDetailViewAsync()
    {
        if (MobileConstants.IsTablet && !IsPatientPage())
        {
            if (_contactData.Contact?.ContactID != Guid.Empty)
            {
                await ContactClickAsync(_contactData.Contact.ContactID, false).ConfigureAwait(true);
                CollectionViewField.SelectedItem = _contactData.Contacts.FirstOrDefault(x => x.ContactID == _contactData.Contact.ContactID);
            }
            else
            {
                CollectionViewField.SelectedItem = null;
                //ParentPage.ClearRightHeaderItems();
                if (_contactView != null && _mainLayout.Children.Contains(_contactView))
                {
                    _mainLayout.Children.Remove(_contactView);
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
            CollectionViewField.ItemsSource = _contactData.Contacts;
            if (IsPatientPage())
            {
                TabletHeader.Text = GetHeaderText(_contactData.Contacts.Count);
            }
        }
        else
        {
            var filteredContacts = _contactData.Contacts.FindAll(y =>
            {
                return y.ContactType.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });
            CollectionViewField.ItemsSource = filteredContacts;
            if (IsPatientPage())
            {
                TabletHeader.Text = GetHeaderText(filteredContacts.Count);
            }
            if (filteredContacts.Count <= 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
    }

    private async void Contacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (isPatientData)
        {
            await ContactSelectedAsync(sender).ConfigureAwait(true);
        }
        else
        {
            if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY))
            {
                await ContactSelectedAsync(sender).ConfigureAwait(true);
            }
        }
    }

    private async Task ContactSelectedAsync(object sender)
    {
        var item = sender as CollectionView;
        if (item.SelectedItem != null)
        {
            await ContactClickAsync((item.SelectedItem as ContactModel).ContactID, false).ConfigureAwait(true);
        }
    }

    private async void ContactView_OnListRefresh(object sender, EventArgs e)
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

    private void MapProperties(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            MapParameters();
        }
    }

    private void MapParameters()
    {
        _contactData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        var contactID = GenericMethods.MapValueType<string>(GetParameterValue(nameof(ContactModel.ContactID)));
        _contactData.Contact = new ContactModel
        {
            ContactID = !string.IsNullOrWhiteSpace(contactID)
                ? new Guid(contactID)
                : Guid.Empty
        };
    }
}