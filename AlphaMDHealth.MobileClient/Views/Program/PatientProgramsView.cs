using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient
{
    public class PatientProgramsView : BaseLibCollectionView
    {
        private readonly PatientProgramDTO _patientProgramData = new PatientProgramDTO { Programs = new List<ProgramModel>() , PatientProgram = new PatientProgramModel() };
        private readonly Grid _mainLayout;

        /// <summary>
        /// Parameterized constructor containing page instance and Parameters
        /// </summary>
        /// <param name="page">page instance on which view is rendering</param>
        /// <param name="parameters">Featue parameters to render view</param>
        public PatientProgramsView(BasePage page, object parameters) : base(page, parameters)
        {
            ParentPage.PageService = new PatientProgramService(App._essentials);
            if (Parameters?.Count > 0)
            {
                _patientProgramData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            }
            _patientProgramData.PatientProgram = new PatientProgramModel
            {
                ProgramID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(PatientProgramModel.ProgramID))),
            };
            IsTabletListHeaderDisplay = IsPatientPage() && !IsDashboardView(_patientProgramData.RecordCount);
            CustomCellModel customCellModel = new CustomCellModel
            {
                CellID = nameof(ProgramModel.ProgramID),
                CellHeader = nameof(ProgramModel.Name),
                CellLeftDefaultIcon = nameof(ProgramModel.ProgramImage),
                CellDescription = nameof(ProgramModel.AddedOnString),
                LeftTintColor = nameof(ProgramModel.ProgramGroupIdentifier),
                IconSize = AppImageSize.ImageSizeM,
                ArrangeHorizontal = false
            };
            _mainLayout = new Grid
            {
                Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                ColumnSpacing = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                },
                ColumnDefinitions = CreateTabletViewColumn(true)
            };
            if (IsDashboardView(_patientProgramData.RecordCount))
            {
                AddCollectionView(_mainLayout, customCellModel, 0, 1);
            }
            else
            {
                AddCollectionViewWithTabletHeader(_mainLayout, customCellModel);
            }
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
                _patientProgramData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
                _patientProgramData.SelectedUserID = IsPatientPage()
                    ? App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)
                    : App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0);
            }
            _patientProgramData.PatientProgram.PatientProgramID = Convert.ToInt64(GenericMethods.MapValueType<long>(GetParameterValue(nameof(PatientProgramModel.PatientProgramID))));
            _patientProgramData.PatientProgram = new PatientProgramModel
            {
                ProgramID = !string.IsNullOrWhiteSpace(_patientProgramData.PatientProgram.PatientProgramID.ToString())
                       ? _patientProgramData.PatientProgram.PatientProgramID
                       : Convert.ToInt64(String.Empty)
            };
            await (ParentPage.PageService as PatientProgramService).GetPatientProgramsAsync(_patientProgramData).ConfigureAwait(true);
            ParentPage.PageData = ParentPage.PageService.PageData;
            _emptyListView.PageResources = ParentPage.PageData;
            if (_patientProgramData.ErrCode == ErrorCode.OK)
            {
                if (!isRefreshRequest)
                {
                    OnListItemSelection(ProgramSelectionChanged, ParentPage.CheckFeaturePermissionByCode(AppPermissions.PatientProgramAddEdit.ToString()) && !IsPatientOverview(_patientProgramData.RecordCount));
                }
                UpdateHeaderView(isRefreshRequest);
                if (GenericMethods.IsListNotEmpty(_patientProgramData.Programs))
                {
                    CollectionViewField.ItemsSource = _patientProgramData.Programs;
                    if (IsDashboardView(_patientProgramData.RecordCount))
                    {
                        _mainLayout.HeightRequest = ((double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _patientProgramData.Programs.Count) - new OnIdiom<double> { Phone = 0.0, Tablet = ((_margin - 5) * (_patientProgramData.Programs.Count - 1)) };
                    }
                }
                else
                {
                    CollectionViewField.ItemsSource = null;
                    RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, IsDashboardView(_patientProgramData.RecordCount), (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
                }
            }
            else
            {
                RenderErrorView(_mainLayout, _patientProgramData.ErrCode.ToString(), IsDashboardView(_patientProgramData.RecordCount), (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
            }
        }

        /// <summary>
        /// unregister event of views
        /// </summary>
        public override async Task UnloadUIAsync()
        {
            if (!IsDashboardView(_patientProgramData.RecordCount) && IsPatientPage())
            {
                SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
                OnActionButtonClicked -= OnAddButtonClicked;
            }
            OnListItemSelection(ProgramSelectionChanged, false);
            await base.UnloadUIAsync().ConfigureAwait(true);
        }

        private void UpdateHeaderView(bool isRefreshRequest)
        {
            if (!IsDashboardView(_patientProgramData.RecordCount) && IsPatientPage())
            {
                SearchField.PageResources = ParentPage.PageData;
                SearchField.Value = string.Empty;
                if (ParentPage.CheckFeaturePermissionByCode(AppPermissions.PatientProgramsView.ToString()))
                {
                    TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                    if (!isRefreshRequest && !IsPatientOverview(_patientProgramData.RecordCount))
                    {
                        OnActionButtonClicked += OnAddButtonClicked;
                    }
                }
                else
                {
                    HideAddButton(_mainLayout, true);
                }
                TabletHeader.Text = GetHeaderText(_patientProgramData.Programs.Count);
                SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
            }
        }

        private string GetHeaderText(int count)
        {
            return $"{ParentPage.GetFeatureValueByCode(AppPermissions.PatientProgramsView.ToString())} ({count})";
        }

        private async void ProgramSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsPatientPage())
            {
                if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, default).ConfigureAwait(true))
                {
                    CollectionViewField.SelectionChanged -= ProgramSelectionChanged;
                    var item = sender as CollectionView;
                    if (item.SelectedItem != null)
                    {
                        await NavigateToProgramEditAsync((item.SelectedItem as ProgramModel).PatientProgramID.ToString(), false).ConfigureAwait(true);
                    }
                    CollectionViewField.SelectionChanged += ProgramSelectionChanged;
                }
            }
            else
            {
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientProgramPage), GenericMethods.GenerateParamsWithPlaceholder(Param.type), false.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
            }
        }

        private async Task NavigateToProgramEditAsync(string patientprogramID, bool isAddButtonClicked)
        {
            var programPage = new PatientProgramPopupPage(patientprogramID);
            programPage.OnSaveButtonClicked += RefreshPatientProgramsList;
            //todo:await Navigation.PushPopupAsync(programPage).ConfigureAwait(false);
        }

        private void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
        {
            var searchText = sender as CustomSearchBar;
            CollectionViewField.Footer = null;
            if (string.IsNullOrWhiteSpace(searchText.Text))
            {
                CollectionViewField.ItemsSource = new List<ProgramModel>();
                CollectionViewField.ItemsSource = _patientProgramData.Programs;
                TabletHeader.Text = GetHeaderText(_patientProgramData.Programs.Count);
            }
            else
            {
                var searchedUsers = _patientProgramData.Programs.FindAll(y =>
                {
                    return !string.IsNullOrWhiteSpace(y.Name) && y.Name.ToLowerInvariant().Contains(searchText.Text.ToLowerInvariant().Trim());
                });
                CollectionViewField.ItemsSource = searchedUsers;
                TabletHeader.Text = GetHeaderText(searchedUsers.Count);
                if (searchedUsers.Count <= 0)
                {
                    RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
                }
            }
        }

        private async void OnAddButtonClicked(object sender, EventArgs e)
        {
            if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
            {
                var programPage = new PatientProgramPopupPage(Constants.CONSTANT_ZERO);
                programPage.OnSaveButtonClicked += RefreshPatientProgramsList; ;
                //todo:await Navigation.PushPopupAsync(programPage).ConfigureAwait(true);
            }
        }

        private async void RefreshPatientProgramsList(object sender, EventArgs e)
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
        }

        internal void SetMainGridSize()
        {
            if (GenericMethods.IsListNotEmpty(_patientProgramData.Programs))
            {
                _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _patientProgramData.Programs.Count + 60;
            }
            else
            {
                _mainLayout.HeightRequest = 500;
            }
        }
    }
}