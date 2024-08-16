using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientHeaderView : ViewManager
{
    private readonly UserDTO _patientData = new UserDTO { User = new UserModel() };
    private long _previousPatientID;
    private readonly Grid _mainGrid;
    private readonly CustomImageControl _profileImage;
    private readonly CustomImageControl _heightIcon;
    private readonly CustomImageControl _weightIcon;
    private readonly CustomImageControl _bloodGroupIcon;
    private readonly CustomLabelControl _userName;
    private readonly CustomLabelControl _email;
    private readonly CustomLabelControl _userEmail;
    private readonly CustomLabelControl _phoneNumber;
    private readonly CustomLabelControl _userPhoneNumber;
    private readonly CustomLabelControl _hospitalIdentifier;
    private readonly CustomLabelControl _userHospitalIdentifier;
    private readonly CustomLabelControl _height;
    private readonly CustomLabelControl _userHeight;
    private readonly CustomLabelControl _weight;
    private readonly CustomLabelControl _userWeight;
    private readonly CustomLabelControl _bloodGroup;
    private readonly CustomLabelControl _userBloodGroup;
    private readonly CustomTabsControl _tabs;
    private readonly ScrollView _scrollView;
    private ViewManager _detailView;


    /// <summary>
    /// on patient list refresh
    /// </summary>
    public event EventHandler<EventArgs> OnPatientListRefreshCallBack;


    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientHeaderView(BasePage page, object parameters) : base(page, parameters)
    {
        double layoutPadding = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture);
        _profileImage = new CustomImageControl(AppImageSize.ImageSizeXXL, AppImageSize.ImageSizeXXL, string.Empty, string.Empty, false);
        _heightIcon = new CustomImageControl(AppImageSize.ImageSizeD, AppImageSize.ImageSizeD, string.Empty, ImageConstants.I_HEIGHT_ICON_PNG, false)
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
        };
        _weightIcon = new CustomImageControl(AppImageSize.ImageSizeD, AppImageSize.ImageSizeD, string.Empty, ImageConstants.I_WEIGHT_GREEN_PNG, false)
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
        };
        _bloodGroupIcon = new CustomImageControl(AppImageSize.ImageSizeD, AppImageSize.ImageSizeD, string.Empty, ImageConstants.I_BLOOD_TYPE_PNG, false)
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
        };
        _email = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _phoneNumber = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _hospitalIdentifier = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _userName = new CustomLabelControl(LabelType.PrimaryMediumLeft);
        _userEmail = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _userPhoneNumber = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _userHospitalIdentifier = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _height = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _weight = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _bloodGroup = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _userHeight = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _userWeight = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _userBloodGroup = new CustomLabelControl(LabelType.PrimarySmallLeft);
        _tabs = new CustomTabsControl();
        _mainGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ////Used to add extra padding as per UI
            Padding = new Thickness(layoutPadding * 2, layoutPadding * 2, layoutPadding * 2, 0),
            ColumnSpacing = layoutPadding,
            RowSpacing = 10,
            RowDefinitions =
            {
                new RowDefinition { Height =  new GridLength(1, GridUnitType.Absolute) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height =  new GridLength(1, GridUnitType.Absolute) },
                new RowDefinition { Height =  new GridLength(1, GridUnitType.Absolute) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Absolute) },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Absolute) },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };
        _mainGrid.Add(_profileImage, 0, 0);
        Grid.SetRowSpan(_profileImage, 4);
        _mainGrid.Add(_userName, 1, 1);
        Grid.SetColumnSpan(_userName, 8);
        _mainGrid.Add(_email, 1, 2);
        _mainGrid.Add(_userEmail, 2, 2);
        _mainGrid.Add(_phoneNumber, 4, 2);
        _mainGrid.Add(_userPhoneNumber, 5, 2);
        _mainGrid.Add(_hospitalIdentifier, 7, 2);
        _mainGrid.Add(_userHospitalIdentifier, 8, 2);
        ParentPage.CreateSepratorView(_mainGrid, 0, 6, 9);
        _mainGrid.Add(_tabs, 0, 8);
        Grid.SetColumnSpan(_tabs, 9);
        LoadDefaultView();
        CreateDetailSection(layoutPadding);
        _scrollView = new ScrollView { Content = _mainGrid };
        Content = _scrollView;
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        _patientData.SelectedUserID = _patientData.User.UserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
        _patientData.User.RoleID = (int)RoleName.Patient;
        var userService = new UserService(App._essentials);
        //await userService.GetUserAsync(_patientData, true, false).ConfigureAwait(true);//todo:
        ParentPage.PageData = userService.PageData;
        await SyncPatientDataAsync(_patientData.User.UserID).ConfigureAwait(true);
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (_patientData.ErrCode == ErrorCode.OK && _patientData.User != null)
            {
                await LoadDataInControlsAsync().ConfigureAwait(true);
                await MapReadingsDataAsync().ConfigureAwait(true);
                await GenerateFeatureTabsAsync(isRefreshRequest).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
            else
            {
                if (_patientData.ErrCode != ErrorCode.HandledRedirection)
                {
                    if (isRefreshRequest && _patientData.ErrCode == ErrorCode.NoInternetConnection)
                    {

                        AppHelper.ShowBusyIndicator = false;
                    }
                    else
                    {

                        await ParentPage.DisplayMessagePopupAsync(_patientData.ErrCode.ToString(), OnErrorPupupActionClicked).ConfigureAwait(true);
                    }
                }
            }
        });
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        await Task.CompletedTask;
    }

    private async void OnErrorPupupActionClicked(object sender, int e)
    {
        ParentPage.OnPupupActionClicked(sender, e);
        await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientsPage)
            , GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id), Constants.NUMBER_ZERO
            , _previousPatientID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
    }

    private async Task SyncPatientDataAsync(long patientID)
    {
        if (_patientData.ErrCode == ErrorCode.OK)
        {
            _patientData.ErrCode = (await ParentPage.SyncDataWithServerAsync(Pages.PatientDetailView, false, patientID).ConfigureAwait(false)).ErrCode;
        }
    }

    private async Task GenerateFeatureTabsAsync(bool isRefreshRequest)
    {
        if (isRefreshRequest)
        {
            _tabs.InvokeTabRefresh(string.Empty);
        }
        else
        {
            if (GenericMethods.IsListNotEmpty(_patientData.FeaturePermissions))
            {
                _tabs.LoadUIData((from feature in _patientData.FeaturePermissions
                                  select new OptionModel
                                  {
                                      OptionID = feature.FeatureID,
                                      OptionText = feature.FeatureText,
                                      GroupName = feature.FeatureCode
                                  }).ToList(), true);
                _tabs.HorizontalOptions = LayoutOptions.Fill;
                _tabs.Margin = new Thickness(0);
                _tabs.TabClicked += OnTabChanged;
                await InitializeDetailViewAsync(_patientData.FeaturePermissions[0].FeatureCode).ConfigureAwait(true);
            }
        }
    }

    private async void OnTabChanged(object sender, EventArgs e)
    {
        _tabs.TabClicked -= OnTabChanged;
        await _scrollView.ScrollToAsync(_mainGrid, ScrollToPosition.Start, false);
        if (sender != null && sender.ToString() != Constants.TABS_MORE_OPTION_CONSTANT && !(sender is CustomButtonControl))
        {

            AppHelper.ShowBusyIndicator = true;
            await InitializeDetailViewAsync(sender.ToString()).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
        _tabs.TabClicked += OnTabChanged;
    }

    /// <summary>
    /// Loads Target view as content
    /// </summary>
    /// <param name="targetView">Name of Target View</param>
    public async Task LoadTragetViewAsync(string targetView)
    {
        if (!_tabs.InvokeTabRefresh(targetView))
        {
            await InitializeDetailViewAsync(targetView).ConfigureAwait(true);
        }
        ////_tabs.InvokeTabRefresh();
    }

    /// <summary>
    /// Loads Target view as content
    /// </summary>
    /// <param name="targetView">Name of Target View</param>
    private async Task InitializeDetailViewAsync(string targetView)
    {
        var parameters = ParentPage.AddParameters(
            ParentPage.CreateParameter(nameof(BaseDTO.RecordCount), 0.ToString(CultureInfo.InvariantCulture)),
            ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _patientData.User.UserID.ToString(CultureInfo.InvariantCulture))
        );
        if (_detailView != null)
        {
            _mainGrid.Children.Remove(_detailView);
            await _detailView.UnloadUIAsync().ConfigureAwait(true);
        }
        switch (targetView.ToEnum<Pages>())
        {
            case Pages.PatientView:
                _detailView = new PatientDemographicsView(ParentPage, parameters);

                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                break;
            case Pages.PatientProgramsView:
                _detailView = new PatientProgramsView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as PatientProgramsView).SetMainGridSize();
                break;
            case Pages.CaregiversView:
                _detailView = new CaregiversView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as CaregiversView).SetMainGridSize();
                break;
            case Pages.PatientReadingsView:
                //parameters.Add(ParentPage.CreateParameter(nameof(PatientReadingDTO.ReadingTypeIdentifier), string.Empty));
                parameters.Add(ParentPage.CreateParameter(nameof(PatientReadingDTO.ReadingCategoryID), Constants.ZERO));
                parameters.Add(ParentPage.CreateParameter(nameof(PatientReadingsPage.IsAdd), false.ToString(CultureInfo.InvariantCulture)));
                parameters.Add(ParentPage.CreateParameter(Constants.VIEW_TYPE_STRING, ListStyleType.SeperatorView.ToString()));
                parameters.Add(ParentPage.CreateParameter(Constants.DISPLAY_CATEGORY_FILTER_STRING, true.ToString(CultureInfo.InvariantCulture)));
                _detailView = new PatientReadingsView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                break;
            case Pages.PatientEducationsView:
            case Pages.EducationsView:
            case Pages.MyEducationsView:
                parameters.Add(ParentPage.CreateParameter(nameof(ContentPageModel.EducationCategoryID), 0.ToString(CultureInfo.InvariantCulture)));
                parameters.Add(ParentPage.CreateParameter(nameof(ContentPageModel.IsPatientPage), true.ToString(CultureInfo.InvariantCulture)));
                _detailView = new EducationsView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as EducationsView).SetMainGridSize();
                break;
            case Pages.PatientFilesView:
                parameters.Add(ParentPage.CreateParameter(nameof(FileModel.FileID), ""));
                _detailView = new PatientFilesView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as PatientFilesView).SetMainGridSize();
                break;
            case Pages.ChatView:
            case Pages.ChatsView:
                parameters.Add(ParentPage.CreateParameter(nameof(BaseDTO.IsActive), true.ToString(CultureInfo.InvariantCulture)));
                _detailView = new ChatView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                await (_detailView as ChatView).SetMainGridSizeAsync().ConfigureAwait(true);
                break;
            case Pages.AppointmentsView:
            case Pages.PatientAppointmentsView:
                parameters.Add(ParentPage.CreateParameter(nameof(AppointmentModel.AppointmentID), 0.ToString(CultureInfo.InvariantCulture)));
                _detailView = new AppointmentsView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as AppointmentsView).SetMainGridSize();
                break;
            case Pages.PatientTasksView:
                _detailView = new PatientTasksView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as PatientTasksView).SetMainGridSize();
                break;
            case Pages.PatientDetailView:
                _detailView = new DashboardView(new BasePage(), parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                break;
            case Pages.PatientMedicationsView:
                _detailView = new PatientMedicationsView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as PatientMedicationsView).SetMainGridSize();
                break;
            case Pages.PatientContactsView:
                _detailView = new PatientContactsView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as PatientContactsView).SetMainGridSize();
                break;
            case Pages.TrackersView:
            case Pages.PatientTrackersView:
                _detailView = new PatientTrackersView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as PatientTrackersView).SetMainGridSize();
                break;
            case Pages.PatientBillsView:
                _detailView = new PatientBillingsView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as PatientBillingsView).SetMainGridSize();
                break;
            case Pages.PatientProviderNotesView:
                _detailView = new PatientProviderNotesView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                (_detailView as PatientProviderNotesView).SetMainGridSize();
                break;
            case Pages.LinkedUsersView:
                _detailView = new MultipleUserView(ParentPage, parameters);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                _detailView.OnPatientListRefresh += RefreshPatientListView;
                (_detailView as MultipleUserView).SetMainGridSize();
                break;
            default:
                LoadDefaultView();
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                return;
        }
        _mainGrid.Add(_detailView, 0, 9);
        Grid.SetColumnSpan(_detailView, 9);
    }

    private void RefreshPatientListView(object sender, EventArgs e)
    {
        OnPatientListRefreshCallBack?.Invoke(sender, e);
    }

    private void LoadDefaultView()
    {
        _detailView = new DefaultContentView(ParentPage, ParentPage.AddParameters(
            ParentPage.CreateParameter(nameof(MessageViewType), ((int)MessageViewType.StaticMessageView).ToString(CultureInfo.InvariantCulture)),
            ParentPage.CreateParameter(nameof(ResourceModel.ResourceKey), ResourceConstants.R_NO_DATA_FOUND_KEY))
        );
    }

    private async Task LoadDataInControlsAsync()
    {
        _previousPatientID = _patientData.User.UserID;
        //todo: if (_patientData.User.ImageSource == null)
        //{
            _profileImage.Source = null;
            _profileImage.DefaultValue = _patientData.User.ImageName;
        //}
        //todo:
        //else
        //{
        //    _profileImage.Source = _patientData.User.ImageSource;
        //}
        await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, _patientData.User.FirstName, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage)).ConfigureAwait(true);
        //todo:_userName.Text = $"{_patientData.User.FirstName} {Constants.SYMBOL_PIPE_SEPERATOR} {_patientData.User.Age}{Constants.COMMA_SEPARATOR} {ParentPage.GetResourceByKey(_patientData.User.GenderID)?.ResourceValue}";
        _userName.Text = $"{_patientData.User.FirstName} {Constants.SYMBOL_PIPE_SEPERATOR} {ParentPage.GetResourceByKey(_patientData.User.GenderID)?.ResourceValue}";
        _email.Text = $"{ParentPage.GetResourceValueByKey(ResourceConstants.R_EMAIL_ADDRESS_KEY)}{Constants.COLON_KEY}";
        _userEmail.Text = _patientData.User.EmailId;
        _phoneNumber.Text = $"{ParentPage.GetResourceValueByKey(ResourceConstants.R_MOBILE_NUMBER_KEY)}{Constants.COLON_KEY}";
        _userPhoneNumber.Text = _patientData.User.PhoneNo;
        _hospitalIdentifier.Text = $"{ParentPage.GetResourceValueByKey(ResourceConstants.R_INTERNAL_CODE_KEY)}{Constants.COLON_KEY}";
        _userHospitalIdentifier.Text = _patientData.User.HospitalIdenfier;
        _bloodGroup.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_BLOOD_GROUP_KEY);
        _userBloodGroup.Text = string.IsNullOrEmpty(_patientData.User.LastName) ? Constants.SYMBOL_DOUBLE_HYPHEN : _patientData.User.LastName;
        _weight.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_WEIGHT_KEY);
        _height.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_HEIGHT_KEY);
    }

    private async Task MapReadingsDataAsync()
    {
        _userWeight.Text = Constants.SYMBOL_DOUBLE_HYPHEN;
        _userHeight.Text = Constants.SYMBOL_DOUBLE_HYPHEN;
        PatientReadingDTO readingsData = new PatientReadingDTO
        {
            LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0),
            SelectedUserID = _patientData.SelectedUserID,
            ErrCode = ErrorCode.OK,
        };
        //await new ReadingService(App._essentials).GetPatientOverviewReading(readingsData).ConfigureAwait(true);//todo:
        if (GenericMethods.IsListNotEmpty(readingsData?.PatientReadings))
        {
            foreach (var reading in readingsData.PatientReadings)
            {
                if (reading.ReadingID == 654) //Weight
                {
                    _userWeight.Text = $"{GenericMethods.ConvertToLocalNumber(Math.Round(reading.ReadingValue.Value, Constants.DIGITS_AFTER_DECIMAL), CultureInfo.CurrentCulture)}";
                }
                else if (reading.ReadingID == 655) //Height
                {
                    _userHeight.Text = $"{GenericMethods.ConvertToLocalNumber(Math.Round(reading.ReadingValue.Value, Constants.DIGITS_AFTER_DECIMAL), CultureInfo.CurrentCulture)}";
                }
            }
        }
    }

    private void CreateDetailSection(double layoutPadding)
    {
        Grid detailsGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnSpacing = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = new GridLength(layoutPadding, GridUnitType.Absolute) },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = new GridLength(layoutPadding, GridUnitType.Absolute) },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = GridLength.Auto },
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition{Width = GridLength.Auto },
            }
        };
        detailsGrid.Add(_heightIcon, 0, 0);
        Grid.SetRowSpan(_heightIcon, 2);
        detailsGrid.Add(_height, 1, 0);
        detailsGrid.Add(_userHeight, 1, 1);
        detailsGrid.Add(_bloodGroupIcon, 3, 0);
        Grid.SetRowSpan(_bloodGroupIcon, 2);
        detailsGrid.Add(_bloodGroup, 4, 0);
        detailsGrid.Add(_userBloodGroup, 4, 1);
        detailsGrid.Add(_weightIcon, 6, 0);
        Grid.SetRowSpan(_weightIcon, 2);
        detailsGrid.Add(_weight, 7, 0);
        detailsGrid.Add(_userWeight, 7, 1);
        _mainGrid.Add(detailsGrid, 0, 5);
        Grid.SetColumnSpan(detailsGrid, 9);
    }
}