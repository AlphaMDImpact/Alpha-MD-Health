using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientProviderNotesView : BaseLibCollectionView
{
    private readonly PatientProviderNoteDTO _providerNoteDTO = new PatientProviderNoteDTO { PatientProviderNotes = new List<PatientProviderNoteModel>(), PatientProviderNote = new PatientProviderNoteModel() };
    private readonly Grid _mainLayout;
    private CustomListView _notesCustomListView;
    public readonly bool _isDashboard;
    public bool _isPatientLogin;
    public Dictionary<string, double> GetCellHieght { get; } = new Dictionary<string, double>();

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Feature parameters to render view</param>
    public PatientProviderNotesView(BasePage page, object parameters) : base(page, parameters)
    {
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        _isPatientLogin = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        ParentPage.PageService = new QuestionnaireService(App._essentials);
        if (Parameters?.Count > 0)
        {
            _providerNoteDTO.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        }
        _providerNoteDTO.PatientProviderNote = new PatientProviderNoteModel
        {
            ProviderNoteID = GenericMethods.MapValueType<Guid>(GetParameterValue(nameof(PatientProviderNoteModel.ProviderNoteID))),
        };
        _isDashboard = IsDashboardView(_providerNoteDTO.RecordCount);
        IsTabletListHeaderDisplay = IsPatientPage() && !IsDashboardView(_providerNoteDTO.RecordCount);
        _notesCustomListView = new CustomListView(Device.RuntimePlatform == Device.iOS ? ListViewCachingStrategy.RetainElement : ListViewCachingStrategy.RecycleElement)
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_LIST_KEY],
            Margin = new Thickness(0, IsTabletListHeaderDisplay ? _margin : 0, 0, 0),
            ItemTemplate = new DataTemplate(() => { return new NotesViewCell(this); })
        };
        _mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[IsTabletListHeaderDisplay ? StyleConstants.ST_END_TO_END_GRID_STYLE : StyleConstants.ST_DEFAULT_GRID_STYLE],
            Margin = new Thickness(0, _margin, 0, 0),
            ColumnSpacing = !_isDashboard && MobileConstants.IsTablet ? Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture) : Constants.ZERO_PADDING,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            ColumnDefinitions = CreateTabletViewColumn(_isDashboard || DeviceInfo.Idiom == DeviceIdiom.Phone)
        };
        if (IsTabletListHeaderDisplay)
        {
            AddCollectionViewWithTabletHeader(_mainLayout, null);
        }
        _mainLayout.Add(_notesCustomListView, 0, 1);
        Grid.SetColumnSpan(_notesCustomListView, 3);
        SetPageContent(_mainLayout);
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns view with loaded data</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            _providerNoteDTO.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            _providerNoteDTO.PatientProviderNote.ProviderNoteID = GenericMethods.MapValueType<Guid>(GetParameterValue(nameof(PatientProviderNoteModel.ProviderNoteID)));
        }
        await (ParentPage.PageService as QuestionnaireService).GetProviderNotesAsync(_providerNoteDTO).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        _emptyListView.PageResources = ParentPage.PageData;
        if (_providerNoteDTO.ErrCode == ErrorCode.OK)
        {
            if (IsTabletListHeaderDisplay)
            {
                SearchField.IsVisible = false;
                //SearchField.PageResources = ParentPage.PageData;
                //SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
                if (ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientProviderNoteAddEdit.ToString()) && !isRefreshRequest)
                {
                    TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                    OnActionButtonClicked += OnAddButtonClick;
                }
                ApplyTableHeaderText(_providerNoteDTO.Providers?.Count ?? 0);
            }

            _notesCustomListView.ItemsSource = null;
            if (GenericMethods.IsListNotEmpty(_providerNoteDTO.Providers))
            {
                _notesCustomListView.Footer = null;
                _notesCustomListView.ItemsSource = _providerNoteDTO.Providers;

                if (_isDashboard)
                {
                    _notesCustomListView.IsEnabled = false;
                }
            }
            else
            {
                ShowEmptyView(ResourceConstants.R_NO_DATA_FOUND_KEY);
            }
        }
        else
        {
            ShowEmptyView(_providerNoteDTO.ErrCode.ToString());
        }
    }

    public void NotesViewCell_ListCompleted()
    {
        if (GenericMethods.IsListNotEmpty(_providerNoteDTO.Providers))
        {
            double totalHieght = 0;
            GetCellHieght.ToList().ForEach(x =>
            {
                totalHieght += x.Value;
            });
            _mainLayout.HeightRequest = totalHieght + new OnIdiom<int> { Phone = 5, Tablet = 10 };
        }
    }

    /// <summary>
    /// unregister event of views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        if (!IsDashboardView(_providerNoteDTO.RecordCount) && IsPatientPage())
        {
            //SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        OnActionButtonClicked -= OnAddButtonClick;
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    public async void OnNoteClicked(OptionModel item)
    {
        await NavigateToAddEditPage(item.ParentOptionText);
    }

    private async void OnAddButtonClick(object sender, EventArgs e)
    {
        await NavigateToAddEditPage(Guid.Empty.ToString());
    }

    private async Task NavigateToAddEditPage(string id)
    {
        var notePage = new PatientProviderNotePopupPage(id);
        notePage.OnSaveButtonClicked += RefreshProviderNotesAsync;
        //todo:await Navigation.PushPopupAsync(notePage).ConfigureAwait(true);
    }

    private async void RefreshProviderNotesAsync(object sender, EventArgs e)
    {
        _notesCustomListView.SelectedItem = null;
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
    }

    private void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        _notesCustomListView.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            _notesCustomListView.ItemsSource = null;
            _notesCustomListView.ItemsSource = _providerNoteDTO.Providers;
        }
        else
        {
            var searchedUsers = _providerNoteDTO.Providers.FindAll(y =>
            {
                return y.OptionText.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });
            _notesCustomListView.ItemsSource = null;
            _notesCustomListView.ItemsSource = searchedUsers;
            if (searchedUsers.Count <= 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
        if (TabletHeader != null)
        {
            ApplyTableHeaderText(_notesCustomListView.ItemsSource?.Cast<object>().Count() ?? 0);
        }
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_providerNoteDTO.Providers))
        {
            _mainLayout.HeightRequest = (100 + (2 * _margin)) * _notesCustomListView.Height + 60;
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }

    private void ApplyTableHeaderText(int count)
    {
        TabletHeader.Text = $"{ParentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientProviderNotesView.ToString())} ({count})";
    }

    private void ShowEmptyView(string errorKey)
    {
        if (_isDashboard)
        {
            _mainLayout.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT];
            _notesCustomListView.HeightRequest = _mainLayout.HeightRequest;
            _notesCustomListView.Footer = new CustomLabelControl(LabelType.SecondrySmallCenter)
            {
                VerticalTextAlignment = TextAlignment.Center,
                HeightRequest = _mainLayout.HeightRequest,
                Text = ParentPage.GetResourceValueByKey(errorKey),
            };
            _notesCustomListView.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
        }
        else
        {
            _emptyListView.ControlResourceKey = errorKey;
            _emptyListView.VerticalOptions = LayoutOptions.Center;
            _emptyListView.HeightRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, (double)0) * 0.7;
            _mainLayout.HeightRequest = _emptyListView.HeightRequest;
            _notesCustomListView.ItemsSource = null;
            _notesCustomListView.HeightRequest = _emptyListView.HeightRequest;
            _notesCustomListView.Footer = _emptyListView;
        }
    }
}