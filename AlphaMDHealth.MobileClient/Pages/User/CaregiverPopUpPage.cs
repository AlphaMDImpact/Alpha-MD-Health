using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class CaregiverPopUpPage : BasePopupPage
{
    private readonly CaregiverDTO _caregiverData = new CaregiverDTO { Caregiver = new CaregiverModel(), RecordCount = -11 };
    private readonly CustomBindablePickerControl _organisationPicker;
    private readonly CustomBindablePickerControl _branchPicker;
    private readonly CustomBindablePickerControl _departmentPicker;
    private readonly CustomBindablePickerControl _caregiverPicker;
    private readonly CustomLabelControl _program;
    private readonly CustomLabelControl _programValue;
    private readonly CustomDateTimeControl _fromDate;
    private readonly CustomDateTimeControl _endDate;
    private bool _isReadonly = false;

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    public CaregiverPopUpPage(string patientCaregiverID) : base(new BasePage())
    {
        _parentPage.PageService = new UserService(App._essentials);
        if (!string.IsNullOrWhiteSpace(patientCaregiverID))
        {
            _caregiverData.Caregiver.PatientCareGiverID = Convert.ToInt64(patientCaregiverID, CultureInfo.InvariantCulture);
        }
        _program = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _programValue = new CustomLabelControl(LabelType.PrimarySmallLeft) { Margin = new Thickness(0, 10, 0, Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture)) };
        _fromDate = CreateDateControl(ResourceConstants.R_START_DATE_KEY);
        _endDate = CreateDateControl(ResourceConstants.R_END_DATE_KEY);
        _organisationPicker = CreatePickerControl(ResourceConstants.R_ORGANISATION_KEY);
        _branchPicker = CreatePickerControl(ResourceConstants.R_BRANCH_KEY);
        _departmentPicker = CreatePickerControl(ResourceConstants.R_DEPARTMENT_KEY);
        _caregiverPicker = CreatePickerControl(ResourceConstants.R_CAREGIVER_KEY);

        Grid bodyGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowSpacing = 0,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };
        bodyGrid.Add(_organisationPicker, 0, 0);
        bodyGrid.Add(_branchPicker, 0, 1);
        bodyGrid.Add(_departmentPicker, 0, 2);
        bodyGrid.Add(_caregiverPicker, 0, 3);
        bodyGrid.Add(_program, 0, 4);
        bodyGrid.Add(_programValue, 0, 5);
        bodyGrid.Add(_fromDate, 0, 6);
        bodyGrid.Add(_endDate, 0, 7);
        ScrollView content = new ScrollView { Content = bodyGrid };
        _parentPage.PageLayout.Add(content, 0, 0);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        base.OnAppearing();
        await LoadUIDataAsync().ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override void OnDisappearing()
    {
        _branchPicker.SelectedValuesChanged -= BranchPickerSelectedValuesChanged;
        _departmentPicker.SelectedValuesChanged -= DepartmentPickerSelectedValuesChanged;
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        OnBottomButtonClickedEvent -= DeleteButtonClicked;
        base.OnDisappearing();
    }

    internal async Task LoadUIDataAsync()
    {
        await (_parentPage.PageService as UserService).GetPatientCareGiversAsync(_caregiverData).ConfigureAwait(true);
        _parentPage.PageData = _parentPage.PageService.PageData;
        if (_caregiverData.ErrCode == ErrorCode.OK)
        {
            _isReadonly = _caregiverData.Caregiver.PatientCareGiverID > 0
                && (_caregiverData.Caregiver.ProgramID > 0
                || GenericMethods.GetUtcDateTime >= _caregiverData.Caregiver.ToDate);
            AssignControlResources();
            await Task.Delay(Constants.DATE_RENDER_DELAY).ConfigureAwait(true);
            if (_caregiverData.Caregiver.PatientCareGiverID > 0)
            {
                _fromDate.GetSetDate = _caregiverData.Caregiver.FromDate.Value.Date;
                _endDate.GetSetDate = _caregiverData.Caregiver.ToDate.Value.Date;
                if (!_isReadonly && _parentPage.CheckFeaturePermissionByCode(AppPermissions.CaregiverDelete.ToString()))
                {
                    DisplayBottomButton(ResourceConstants.R_DELETE_ACTION_KEY, FieldTypes.DeleteTransparentExButtonControl);
                    OnBottomButtonClickedEvent += DeleteButtonClicked;
                }
            }
            else
            {
                _fromDate.GetSetDate = DateTime.Now.Date;
                _endDate.GetSetDate = DateTime.Now.Date;
            }
            DisplayProgramName();
        }
        else
        {
            OnSaveButtonClicked.Invoke(_caregiverData.ErrCode.ToString(), new EventArgs());
            //todo:await Navigation.PopAllPopupAsync();
        }
    }

    private void DisplayProgramName()
    {
        if (_caregiverData.Caregiver?.ProgramID > 0)
        {
            _program.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_PROGRAM_TITLE_KEY);
            _programValue.Text = _caregiverData.Caregiver.ProgramName.ToString();
            _program.IsVisible = _programValue.IsVisible = true;
        }
        else
        {
            _program.IsVisible = _programValue.IsVisible = false;
        }
    }

    private double GetMinDate(string dateKey)
    {
        if (_caregiverData.Caregiver.FromDate.Value.Date < DateTime.Now.Date)
        {
            return -(DateTime.Now.Date - _caregiverData.Caregiver.FromDate.Value.Date).Days;
        }
        else
        {
            return _parentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == dateKey).MinLength;
        }
    }

    private void AssignControlResources()
    {
        if (!_isReadonly && _caregiverData.Caregiver.PatientCareGiverID > 0)
        {
            _parentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_START_DATE_KEY).MinLength = GetMinDate(ResourceConstants.R_START_DATE_KEY);
            _parentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_END_DATE_KEY).MinLength = GetMinDate(ResourceConstants.R_END_DATE_KEY);
        }
        _organisationPicker.PageResources = _parentPage.PageData;
        _branchPicker.PageResources = _parentPage.PageData;
        _departmentPicker.PageResources = _parentPage.PageData;
        _caregiverPicker.PageResources = _parentPage.PageData;
        _fromDate.PageResources = _parentPage.PageData;
        _endDate.PageResources = _parentPage.PageData;
        _organisationPicker.ItemSource = _caregiverData.Organisations;
        _branchPicker.ItemSource = _caregiverData.Branches;
        _departmentPicker.ItemSource = _caregiverData.DepartmentList;
        _caregiverPicker.ItemSource = _caregiverData.CaregiverList;
        _organisationPicker.SelectedValue = _caregiverData.Organisations.FirstOrDefault().OptionID;
        if (_caregiverData.Caregiver.PatientCareGiverID > 0)
        {
            if (_caregiverData.Branches.Any(x => x.IsSelected))
            {
                _branchPicker.SelectedValue = _caregiverData.Branches.FirstOrDefault(x => x.IsSelected).OptionID;
            }
            if (_caregiverData.Departments.Any(x => x.IsSelected))
            {
                _departmentPicker.SelectedValue = _caregiverData.Departments.FirstOrDefault(x => x.IsSelected).OptionID;
            }
            if (_caregiverData.CaregiverOptions.Any(x => x.IsSelected))
            {
                _caregiverPicker.SelectedValue = _caregiverData.CaregiverOptions.FirstOrDefault(x => x.IsSelected).OptionID;
            }
        }
        _organisationPicker.IsEnabled = _branchPicker.IsEnabled = _departmentPicker.IsEnabled =
            _caregiverPicker.IsEnabled = _fromDate.IsEnabled = _endDate.IsEnabled = !_isReadonly;
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.CaregiverAddEdit.ToString()));
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
        if (!_isReadonly)
        {
            DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
            _branchPicker.SelectedValuesChanged += BranchPickerSelectedValuesChanged;
            _departmentPicker.SelectedValuesChanged += DepartmentPickerSelectedValuesChanged;
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
    }

    private void BranchPickerSelectedValuesChanged(object sender, EventArgs e)
    {
        (_parentPage.PageService as UserService).AssignPickerValues(_caregiverData, 1, _caregiverData.Branches[((sender) as CustomBindablePicker).SelectedIndex].OptionID);
        _departmentPicker.ItemSource = _caregiverData.DepartmentList;
        _caregiverPicker.ItemSource = _caregiverData.CaregiverList;
    }

    private void DepartmentPickerSelectedValuesChanged(object sender, EventArgs e)
    {
        var selectedDept = _caregiverData.DepartmentList?[((sender) as CustomBindablePicker).SelectedIndex];
        if (selectedDept != null)
        {
            (_parentPage.PageService as UserService).AssignPickerValues(_caregiverData, 2, _caregiverData.DepartmentList[((sender) as CustomBindablePicker).SelectedIndex].OptionID);
            _caregiverPicker.SelectedValue = _caregiverData.CaregiverList.FirstOrDefault(x => x.IsSelected).OptionID;
            _caregiverPicker.ItemSource = _caregiverData.CaregiverList;
        }
    }

    private async Task SaveCaregiverAsync(bool isActive)
    {
        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            if (isActive)
            {
                if (_parentPage.IsFormValid())
                {
                    if (_fromDate.GetSetDate.Value > _endDate.GetSetDate.Value)
                    {
                        _parentPage.DisplayOperationStatus(string.Format(CultureInfo.InvariantCulture,
                            _parentPage.GetResourceValueByKey(ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                            _parentPage.GetResourceValueByKey(ResourceConstants.R_START_DATE_KEY),
                            _parentPage.GetResourceValueByKey(ResourceConstants.R_END_DATE_KEY)));
                        return;
                    }
                    await MapAndSaveCaregiverDataAsync(isActive).ConfigureAwait(true);
                }
            }
            else
            {
                await MapAndSaveCaregiverDataAsync(isActive).ConfigureAwait(true);
            }
        }
    }

    private async Task MapAndSaveCaregiverDataAsync(bool isActive)
    {
        AppHelper.ShowBusyIndicator = true;
        CaregiverDTO caregiverData = MapCaregiverData(isActive);
        await (_parentPage.PageService as UserService).SyncPatientCaregiverToServerAsync(caregiverData, CancellationToken.None).ConfigureAwait(true);
        if (caregiverData.ErrCode == ErrorCode.OK)
        {
            if (caregiverData.IsActive)
            {
                await _parentPage.SyncDataWithServerAsync(Pages.CaregiverPage, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.Users, DataSyncFor.Users.ToString(), App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
            }
            await InvokeAndClosePopupAsync().ConfigureAwait(true);
        }
        else
        {
            AppHelper.ShowBusyIndicator = false;
            _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(caregiverData.ErrCode.ToString()));
        }
    }

    private async Task InvokeAndClosePopupAsync()
    {
        AppHelper.ShowBusyIndicator = false;
        OnSaveButtonClicked?.Invoke(ErrorCode.OK.ToString(), new EventArgs());
        //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
    }

    private CaregiverDTO MapCaregiverData(bool isActive)
    {
        CaregiverDTO caregiverData = new CaregiverDTO
        {
            Caregiver = _caregiverData.Caregiver,
        };
        if (isActive)
        {
            if (_departmentPicker.SelectedValue > 0)
            {
                _caregiverData.Caregiver.OrganisationID = _departmentPicker.SelectedValue;
            }
            else if (_branchPicker.SelectedValue > 0)
            {
                _caregiverData.Caregiver.OrganisationID = _branchPicker.SelectedValue;
            }
            else
            {
                _caregiverData.Caregiver.OrganisationID = _organisationPicker.SelectedValue;
            }
            _caregiverData.Caregiver.CareGiverID = _caregiverPicker.SelectedValue;
            _caregiverData.Caregiver.FromDate = _fromDate.GetSetDate.Value;
            _caregiverData.Caregiver.ToDate = _endDate.GetSetDate.Value;
            caregiverData.IsActive = true;
        }
        else
        {
            _caregiverData.Caregiver.ToDate = GenericMethods.GetUtcDateTime;
            if (_caregiverData.Caregiver.FromDate > GenericMethods.GetUtcDateTime)
            {
                caregiverData.IsActive = false;
            }
            else
            {
                caregiverData.IsActive = true;
            }
        }
        return caregiverData;
    }

    private async void DeleteButtonClicked(object sender, EventArgs e)
    {
        await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeViewActionClicked, true, true, false).ConfigureAwait(true);
    }

    private async void OnMessgeViewActionClicked(object sender, int e)
    {
        OnBottomButtonClickedEvent -= DeleteButtonClicked;
        _parentPage.OnClosePupupAction(sender, e);
        if (e == 1)
        {
            await SaveCaregiverAsync(false).ConfigureAwait(true);
        }
        OnBottomButtonClickedEvent += DeleteButtonClicked;
    }

    private CustomBindablePickerControl CreatePickerControl(string resourceKey)
    {
        return new CustomBindablePickerControl
        {
            ControlResourceKey = resourceKey,
            IsUnderLine = true
        };
    }

    private CustomDateTimeControl CreateDateControl(string resourceKey)
    {
        return new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = resourceKey
        };
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        OnSaveButtonClicked.Invoke(default, new EventArgs());
        //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        await SaveCaregiverAsync(true).ConfigureAwait(false);
    }
}