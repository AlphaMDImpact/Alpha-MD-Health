using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class AssignEducationPage : BasePopupPage
{
    private readonly ContentPageDTO _educationData = new ContentPageDTO { RecordCount = -1 };
    private readonly CustomBindablePickerControl _categoryPicker;
    private readonly CustomBindablePickerControl _educationPicker;
    private readonly CustomDateTimeControl _fromDate;
    private readonly CustomDateTimeControl _toDate;
    private readonly Grid _buttonLayout;
    private readonly CustomButtonControl _deleteButton;
    private readonly CustomButtonControl _prevButton;
    private readonly double _padding;
    private readonly long _patientEducationID;

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    public AssignEducationPage(string patientEducationID) : base(new BasePage())
    {
        _padding = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        _patientEducationID = Convert.ToInt64(patientEducationID, CultureInfo.InvariantCulture);
        if (!string.IsNullOrWhiteSpace(patientEducationID))
        {
            _educationData.Page = new ContentPageModel
            {
                PageID = _patientEducationID,
            };
        }
        _fromDate = CreateDateControl(ResourceConstants.R_START_DATE_KEY);
        _toDate = CreateDateControl(ResourceConstants.R_END_DATE_KEY);
        _deleteButton = new CustomButtonControl(ButtonType.DeleteWithoutMargin);
        _prevButton = new CustomButtonControl(ButtonType.PrimaryWithoutMargin);
        _categoryPicker = CreatePickerControl(ResourceConstants.R_CATEGORY_KEY);
        _educationPicker = CreatePickerControl(ResourceConstants.R_SELECT_EDUCATION_LABEL_KEY);
        _educationPicker.IsVisible = false;
        Grid bodyGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
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
        _buttonLayout = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            BackgroundColor = Color.FromArgb("#FFFFFF"),//.White,
            ColumnSpacing = 1,
            Padding = new Thickness(-_padding, 0),
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition{ Height = GridLength.Auto },
                new RowDefinition{ Height = GridLength.Auto }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
               new ColumnDefinition { Width = GridLength.Star },
               new ColumnDefinition { Width = GridLength.Star }
            },
        };
        _buttonLayout.Add(_prevButton, 0, 1);
        _buttonLayout.Add(_deleteButton, 1, 1);
        bodyGrid.Add(_categoryPicker, 0, 0);
        bodyGrid.Add(_educationPicker, 0, 1);
        bodyGrid.Add(_fromDate, 0, 2);
        bodyGrid.Add(_toDate, 0, 3);
        bodyGrid.Add(_buttonLayout, 0, 8);
        ScrollView content = new ScrollView { Content = bodyGrid };
        Grid mainGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR],
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star) },
            }
        };
        mainGrid.Add(content, 0, 0);
        _prevButton.Clicked += PreviewButtonClicked;
        _parentPage.PageLayout.Add(mainGrid, 0, 0);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        base.OnAppearing();
        await LoadUIDataAsync().ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    private CustomDateTimeControl CreateDateControl(string resourceKey)
    {
        return new CustomDateTimeControl
        {
            ControlType = FieldTypes.DateControl,
            ControlResourceKey = resourceKey
        };
    }

    protected override void OnDisappearing()
    {
        _categoryPicker.SelectedValuesChanged -= CategoryPickerSelectedValuesChanged;
        OnLeftHeaderClickedEvent -= OnLeftHeaderClicked;
        OnRightHeaderClickedEvent -= OnRightHeaderClicked;
        base.OnDisappearing();
    }

    internal async Task LoadUIDataAsync()
    {
        await Task.WhenAll(
            new ContentPageService(App._essentials).SyncContentPageFromServerAsync(_educationData, GenericMethods.GetDefaultDateTime, CancellationToken.None),
            _parentPage.GetFeaturesAsync(AppPermissions.PatientEducationAddEdit.ToString()),
            _parentPage.GetResourcesAsync(GroupConstants.RS_MENU_PAGE_GROUP, GroupConstants.RS_COMMON_GROUP)
        ).ConfigureAwait(true);
        if (_educationData.ErrCode == ErrorCode.OK)
        {
            AssignValues();
            _prevButton.IsVisible = false;
            _deleteButton.IsVisible = false;
            await Task.Delay(Constants.DATE_RENDER_DELAY).ConfigureAwait(true);
            if (_educationData.Page.PageID > 0)
            {
                _categoryPicker.SelectedValue = _educationData.Educations.FirstOrDefault(x => x.IsSelected).ParentOptionID;
                _educationPicker.ItemSource = _educationData.Educations.Where(x => x.ParentOptionID == _categoryPicker.SelectedValue || x.OptionID == -1).ToList();
                _educationPicker.SelectedValue = _educationData.Educations.FirstOrDefault(x => x.IsSelected).OptionID;
                _deleteButton.Clicked += DeletePatientEducationClicked;
                _deleteButton.IsVisible = true;
                _prevButton.IsVisible = true;
                _fromDate.GetSetDate = _educationData.Page.FromDate.Value.Date;
                _toDate.GetSetDate = _educationData.Page.ToDate.Value.Date;
            }
            if (_educationData.Page.ProgramEducationID > 0)
            {
                _fromDate.IsEnabled = false;
                _toDate.IsEnabled = false;
                _deleteButton.IsVisible = false;
                _categoryPicker.IsEnabled = _educationPicker.IsEnabled = false;
            }
            OnLeftHeaderClickedEvent += OnLeftHeaderClicked;
            OnRightHeaderClickedEvent += OnRightHeaderClicked;
        }
        else
        {
            await _parentPage.DisplayMessagePopupAsync(_educationData.ErrCode.ToString(), OnErrorPopupActionClicked).ConfigureAwait(true);
        }
    }

    private async Task OnDeleteButtonClicked()
    {
        ContentPageDTO educationData = new ContentPageDTO
        {
            PatientEducation = new PatientEducationModel
            {
                PatientEducationID = _patientEducationID,
                PageID = _educationPicker.SelectedValue,
                UserID = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0),
                FromDate = _educationData.Page.FromDate.Value,
                ToDate = GenericMethods.GetUtcDateTime,
                Status = PatientEducationStatus.Open,
                AddedOn = GenericMethods.GetUtcDateTime,
                LastModifiedON = GenericMethods.GetUtcDateTime,
                IsActive = false,
            }
        };
        if (educationData.PatientEducation.FromDate > GenericMethods.GetUtcDateTime)
        {
            educationData.PatientEducation.IsActive = false;
        }
        else
        {
            educationData.PatientEducation.IsActive = true;
        }
        await SaveEducationData(educationData);
    }

    private double GetMinDate(string dateKey)
    {
        if (_educationData.Page.FromDate.Value.Date < DateTime.Now.Date)
        {
            return -(DateTime.Now.Date - _educationData.Page.FromDate.Value.Date).Days;
        }
        else
        {
            return _parentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == dateKey).MinLength;
        }
    }

    private void AssignValues()
    {
        _parentPage.PageData = _parentPage.PageService.PageData;
        _categoryPicker.PageResources = _parentPage.PageData;
        _educationPicker.PageResources = _parentPage.PageData;
        _educationPicker.PageResources = _parentPage.PageData;
        _educationPicker.PageResources = _parentPage.PageData;
        _categoryPicker.ItemSource = _educationData.EducationTypes;
        _educationPicker.ItemSource = _educationData.Educations;
        SetTitle(_parentPage.GetFeatureValueByCode(AppPermissions.PatientEducationAddEdit.ToString()));
        DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
        DisplayRightHeader(ResourceConstants.R_SAVE_ACTION_KEY);
        _prevButton.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_PREVIEW_KEY);
        _deleteButton.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY);
        _categoryPicker.SelectedValuesChanged += CategoryPickerSelectedValuesChanged;
        _educationPicker.SelectedValuesChanged += EducationPickerSelectedValuesChanged;
        if (_educationData.Page.PageID > 0)
        {
            _parentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_START_DATE_KEY).MinLength = GetMinDate(ResourceConstants.R_START_DATE_KEY);
            _parentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_END_DATE_KEY).MinLength = GetMinDate(ResourceConstants.R_END_DATE_KEY);
        }
        _toDate.PageResources = _parentPage.PageData;
        _fromDate.PageResources = _parentPage.PageData;
    }

    private async void DeletePatientEducationClicked(object sender, EventArgs e)
    {
        await DeleteMessagePopupCallAsync().ConfigureAwait(true);
    }

    private async Task DeleteMessagePopupCallAsync()
    {
        _deleteButton.Clicked -= DeletePatientEducationClicked;
        await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeDeletePatientEducationActionClicked, true, true, false).ConfigureAwait(true);
        _deleteButton.Clicked += DeletePatientEducationClicked;
    }

    private async void OnMessgeDeletePatientEducationActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                _parentPage.MessagePopup.PopCustomMessageControlAsync();
                _parentPage.MessagePopup.IsVisible = false;
                await OnDeleteButtonClicked().ConfigureAwait(true);
                break;
            case 2:
                _parentPage.MessagePopup.PopCustomMessageControlAsync();
                _parentPage.MessagePopup.IsVisible = false;
                break;
            default:// to do
                break;
        }
    }

    private void DisplayPreviewButton(bool showPreview)
    {
        OnBottomButtonClickedEvent -= PreviewButtonClicked;
        ShowHideBottomButton(showPreview);
        if (showPreview)
        {
            OnBottomButtonClickedEvent += PreviewButtonClicked;
        }
    }

    private async Task SavePatientEducationAsync()
    {
        if (CheckAndDisplayInternetError() && _parentPage.IsFormValid())
        {
            if (_fromDate.GetSetDate.Value > _toDate.GetSetDate.Value)
            {
                _parentPage.DisplayOperationStatus(string.Format(CultureInfo.InvariantCulture, _parentPage.GetResourceValueByKey(ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    _parentPage.GetResourceValueByKey(ResourceConstants.R_START_DATE_KEY), _parentPage.GetResourceValueByKey(ResourceConstants.R_END_DATE_KEY)));
            }
            else
            {
                DateTimeOffset from = _fromDate.GetSetDate.Value;
                DateTimeOffset to = _toDate.GetSetDate.Value;
                ContentPageDTO educationData = new ContentPageDTO
                {
                    PatientEducation = new PatientEducationModel
                    {
                        PatientEducationID = _patientEducationID,
                        PageID = _educationPicker.SelectedValue,
                        UserID = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0),
                        FromDate = new DateTimeOffset(from.DateTime, TimeSpan.Zero),
                        ToDate = new DateTimeOffset(to.DateTime, TimeSpan.Zero),
                        Status = PatientEducationStatus.Open,
                        AddedOn = GenericMethods.GetUtcDateTime,
                        LastModifiedON = GenericMethods.GetUtcDateTime,
                        IsActive = true,
                    }
                };
                AppHelper.ShowBusyIndicator = true;
                await SaveEducationData(educationData);
            }
        }
    }

    private async Task SaveEducationData(ContentPageDTO educationData)
    {
        await new ContentPageService(App._essentials).SyncPatientEducationToServerAsync(educationData, CancellationToken.None).ConfigureAwait(true);
        if (educationData.ErrCode == ErrorCode.OK)
        {
            await _parentPage.SyncDataWithServerAsync(Pages.PatientEducationsView, ServiceSyncGroups.RSSyncFromServerGroup
                , DataSyncFor.Educations, DataSyncFor.Educations.ToString(), default).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
            await _parentPage.DisplayMessagePopupAsync(educationData.ErrCode.ToString(), OnPopupActionClicked).ConfigureAwait(true);
        }
        else
        {
            AppHelper.ShowBusyIndicator = false;
            _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(educationData.ErrCode.ToString()));
        }
    }

    private async void OnErrorPopupActionClicked(object sender, int e)
    {
        _parentPage.MessagePopup.IsVisible = false;
        //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
    }

    private async void OnPopupActionClicked(object sender, int e)
    {
        _parentPage.MessagePopup.IsVisible = false;
        OnSaveButtonClicked?.Invoke(true, new EventArgs());
        //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
    }

    private void CategoryPickerSelectedValuesChanged(object sender, EventArgs e)
    {
        DisplayPreviewButton(false);
        _educationPicker.IsVisible = true;
        var selectedCategoryId = _categoryPicker.SelectedValue;
        _educationPicker.ItemSource = _educationData.Educations.Where(x => x.ParentOptionID == selectedCategoryId || x.OptionID == -1).ToList();
    }

    private void EducationPickerSelectedValuesChanged(object sender, EventArgs e)
    {
        DisplayPreviewButton(_educationPicker.SelectedValue > 0);
        _prevButton.IsVisible = true;
    }

    private CustomBindablePickerControl CreatePickerControl(string resourceKey)
    {
        return new CustomBindablePickerControl
        {
            ControlResourceKey = resourceKey,
            IsUnderLine = true
        };
    }

    private bool CheckAndDisplayInternetError()
    {
        if (!MobileConstants.CheckInternet)
        {
            _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(ResourceConstants.R_OFFLINE_OPERATION_KEY));
            return false;
        }
        return true;
    }

    private async void PreviewButtonClicked(object sender, EventArgs e)
    {
        var patientEducationPage = new EducationPreviewPopupPage(_educationPicker.SelectedValue);
        //todo:await Navigation.PushPopupAsync(patientEducationPage).ConfigureAwait(true);
    }

    private async void OnLeftHeaderClicked(object sender, EventArgs e)
    {
        OnSaveButtonClicked.Invoke(false, new EventArgs());
        //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
    }

    private async void OnRightHeaderClicked(object sender, EventArgs e)
    {
        await SavePatientEducationAsync().ConfigureAwait(false);
    }
}
