using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient reading view
/// </summary>
public class PatientReadingView : ViewManager, IDisposable
{
    internal PatientReadingDTO _readingData;
    private readonly Grid _subLayout;
    private readonly CustomFilterControl _categoryFilterView;
    private readonly CustomBindablePickerControl _readingParentOptionsPicker;
    private readonly CustomBindablePickerControl _bloodGroupType;
    private readonly Grid _valuesContainer;
    private readonly CustomInfoControl _infoView;
    private readonly CustomRadioTextList _foodAddOptionsList;
    private readonly CustomEntryControl _portionSize;
    private readonly CustomEntryControl _foodSource;
    private readonly CustomLabelControl _emptyLabel;
    private readonly CustomEntryControl _searchBox;
    private readonly FoodOptionsPopupPage _searchedFoodItemsPage;
    private readonly CustomDateTimeControl _readingDateTime;
    private readonly CustomLabelControl _addedByHeaderLabel;
    private readonly CustomLabelControl _addedByValueLabel;
    private readonly CustomMultiLineEntryControl _noteLabelValue;
    private readonly CustomButtonControl _deleteButton;
    private List<OptionModel> _readingParentOptions;
    private string _infoText;
    private bool _isViewOnly;
    private bool _isDeleteAllowed;
    private bool _isManualAddAllowed;
    private bool _isPatientLogin;
    private bool _isManualSelected;
    private bool _isCommingFromQuestionnaireView;
    private bool _isRequired;
    private bool _readingAddedByQuestionnaireView;
    private string _errorMessage;
    private PatientReadingDTO _foodData;

    /// <summary>
    /// Searched Food Data
    /// </summary>
    private void SetSearchedFoodList(List<OptionModel> searchedFoodList)
    {
        if (searchedFoodList.Count == 0)
        {
            if (!_isManualSelected)
            {
                _subLayout.Children.Remove(_emptyLabel);
                _subLayout.Add(_emptyLabel, 0, 4);
            }
            _portionSize.Value = string.Empty;
            UpdateNutrientsValue(null, true);
        }
        else
        {
            _subLayout.Children.Remove(_emptyLabel);
            _searchedFoodItemsPage.SearchedFoodList = searchedFoodList;
            _searchedFoodItemsPage.SearchedText = _searchBox.Value;
            //todo:Navigation.PushPopupAsync(_searchedFoodItemsPage).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// IsViewOnly
    /// </summary>
    public bool IsViewOnly
    {
        get => _isViewOnly;
        set
        {
            _isViewOnly = value;
            _subLayout.IsEnabled = !value;
        }
    }

    /// <summary>
    /// Patient reading add/edit view
    /// </summary>
    /// <param name="page">Page instance</param>
    /// <param name="parameters">Parameter values</param>
    public PatientReadingView(BasePage page, object parameters) : base(page, parameters)
    {
        //, bool IsCommingFromQuestionnaireView = false, bool IsNotRequired = false
        _readingData = new PatientReadingDTO
        {
            RecordCount = -1,
            LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0)
        };
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        _isPatientLogin = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        ParentPage.PageService = new ReadingService(App._essentials);

        _categoryFilterView = new CustomFilterControl(false) { Margin = new Thickness(0, 0) };
        _categoryFilterView.SelectedItemChanged += OnCategorySelectedItemChanged;

        _subLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            VerticalOptions = LayoutOptions.StartAndExpand,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
                new RowDefinition { Height = GridLength.Auto},
            },
            ColumnDefinitions =
            {
               new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
        };

        _readingParentOptionsPicker = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_MEASUREMENT_TYPE_TEXT_KEY
        };
        _bloodGroupType = new CustomBindablePickerControl
        {
            ControlResourceKey = ResourceConstants.R_BLOOD_GROUP_KEY,
        };
        _readingParentOptionsPicker.SelectedValuesChanged += OnReadingTypePickerSelectionChange;
        _subLayout.Add(_readingParentOptionsPicker, 0, 0);

        _foodAddOptionsList = new CustomRadioTextList
        {
            IsHorizontal = true,
            RadioWidth = Device.RuntimePlatform == Device.iOS ? (double)AppImageSize.ImageSizeXXL : 190,
            RadioButtonStyle = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_RADIO_BUTTON_KEY],
            Margin = new Thickness(0, 10, 0, 5)
        };
        _searchBox = new CustomEntryControl
        {
            ControlResourceKey = ResourceConstants.R_SEARCH_FOOD_TEXT_KEY,
            ControlIcon = ImageConstants.I_SEARCH_ICON_PNG,
            ShowHeader = true,
            IsUnderLine = true,
        };
        _searchBox.OnAdvanceEntryUnfocused += OnFoodItemSerched;
        _emptyLabel = new CustomLabelControl(LabelType.ClientErrorLabel) { Margin = new Thickness(0, -10, 0, 10) };

        _foodSource = new CustomEntryControl
        {
            ControlType = FieldTypes.TextEntryControl,
            ControlResourceKey = ResourceConstants.R_ENTER_FOOD_TEXT_KEY
        };

        _portionSize = new CustomEntryControl
        {
            ControlType = FieldTypes.DecimalEntryControl,
            ControlResourceKey = ResourceConstants.R_PORTION_SIZE_TEXT_KEY
        };
        _portionSize.OnAdvanceEntryUnfocused += OnPotionSizeChanged;

        _searchedFoodItemsPage = new FoodOptionsPopupPage();
        _searchedFoodItemsPage.OnSelectFoodItem += OnFoodItemSelect;

        _valuesContainer = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            VerticalOptions = LayoutOptions.StartAndExpand,
        };
        _subLayout.Add(_valuesContainer, 0, 6);

        _readingDateTime = new CustomDateTimeControl
        {
            ApplyDateWidthPicker = true,
            ApplyTimeWidthPicker = true,
            ControlResourceKey = ResourceConstants.R_DATE_TIME_TEXT_KEY,
            ControlType = FieldTypes.DateTimeControl
        };
        _subLayout.Add(_readingDateTime, 0, 7);

        _noteLabelValue = new CustomMultiLineEntryControl
        {
            ControlResourceKey = ResourceConstants.R_NOTE_TEXT_KEY,
            EditorHeightRequest = EditorHeight.Default
        };
        _subLayout.Add(_noteLabelValue, 0, 10);

        _addedByHeaderLabel = new CustomLabelControl(LabelType.SecondrySmallLeft);
        _addedByValueLabel = new CustomLabelControl(LabelType.PrimarySmallRight)
        {
            LineBreakMode = LineBreakMode.TailTruncation
        };

        ParentPage.AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 1, true);
        ParentPage.AddRowColumnDefinition(new GridLength(1, GridUnitType.Star), 1, true);
        ParentPage.AddRowColumnDefinition(new GridLength(1, GridUnitType.Auto), 3, true);
        ParentPage.PageLayout.Add(new ScrollView
        {
            Padding = new Thickness(0, (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING]),
            Content = _subLayout
        }, 0, 1);

        _infoView = new CustomInfoControl(false) { IsVisible = false };
        ParentPage.PageLayout.Add(_infoView, 0, 2);

        _deleteButton = new CustomButtonControl(ButtonType.DeleteWithMargin)
        {
            IsVisible = false,
            VerticalOptions = LayoutOptions.End
        };
        _deleteButton.Clicked += OnDeleteReadingClick;
        ParentPage.PageLayout.Add(_deleteButton, 0, 3);
        if (Parameters?.Count > 0)
        {
            _isCommingFromQuestionnaireView = GenericMethods.MapValueType<bool>(GetParameterValue(Constants.IS_COMMING_FROM_QUESTIONNAIRE_VIEW));
        }
        if (MobileConstants.IsDevicePhone)
        {
            SetPageContent(ParentPage.PageLayout);
        }
        else
        {
            if (!IsPatientPage())
            {
                Content = ParentPage.PageLayout;
            }
            else if (_isCommingFromQuestionnaireView)
            {
                SetPageContent(ParentPage.PageLayout);
            }
        }
        SetBackGroundTransparent(!IsPatientPage());
    }

    /// <summary>
    /// Load patient reading add/edit UI
    /// </summary>
    /// <param name="isRefreshRequest"></param>
    /// <returns></returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            _readingData.ReadingCategoryID = GenericMethods.MapValueType<short>(GetParameterValue(nameof(PatientReadingDTO.ReadingCategoryID)));
            _readingData.PatientTaskID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(PatientReadingUIModel.PatientTaskID)));
            _readingData.Title = GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientReadingUIModel.Reading)));
            _readingData.ReadingID = GenericMethods.MapValueType<short>(GetParameterValue(nameof(PatientReadingUIModel.ReadingID)));
            _readingData.PatientReadingID = new Guid(GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientReadingUIModel.PatientReadingID))));
            _readingData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
            if (_readingData.SelectedUserID == 0)
            {
                _readingData.SelectedUserID = (ParentPage.PageService as ReadingService).GetUserID();
            }
            _isCommingFromQuestionnaireView = GenericMethods.MapValueType<bool>(GetParameterValue(Constants.IS_COMMING_FROM_QUESTIONNAIRE_VIEW));
            _isRequired = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(QuestionnaireQuestionModel.IsRequired))); ;
        }
        await GetPageDataAsync().ConfigureAwait(true);
        if (_readingData.PatientTaskID > 0)
        {
            await ParentPage.SyncDataWithServerAsync(Pages.PatientReadingPage, false, _isPatientLogin ? default : _readingData.SelectedUserID).ConfigureAwait(false);
        }
    }

    private async Task GetPageDataAsync()
    {
        _readingData.ErrCode = ErrorCode.OK;
        await (ParentPage.PageService as ReadingService).GetPatientReadingsAsync(_readingData).ConfigureAwait(true);
        //todo: (ParentPage.PageService as ReadingService).MakeValidationResourceChangesForQuestionnaireQuestion(_readingData, _isCommingFromQuestionnaireView, _isRequired);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (!_isCommingFromQuestionnaireView && _readingData.PatientReadingID != Guid.Empty)
        {
            _readingAddedByQuestionnaireView = await new QuestionnaireService(App._essentials).CheckIfAnsValueIsReadingIDAsync(_readingData.PatientReadingID.ToString());
        }
        ClearFields();
        if (_readingData.ErrCode == ErrorCode.OK
            && GenericMethods.IsListNotEmpty(_readingData.ChartMetaData)
            && GenericMethods.IsListNotEmpty(_readingData.ReadingParentOptions))
        {
            if (_readingData.SelectedUserID == App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0) && !_isCommingFromQuestionnaireView)
            {
                await SetTitleViewAsync().ConfigureAwait(true);
            }
            SetReadingCategoryFilters();
            SeReadingParentOptions(_readingData.ReadingParentOptions);
            ApplyResourceValue();
            if (_readingData.PatientTaskID > 0)
            {
                _infoText = ParentPage.GetResourceByKey(ResourceConstants.R_READING_QUESTION_KEY)?.InfoValue;
            }
            if (GenericMethods.IsListNotEmpty(_readingData.ListData))
            {
                SetExistingData();
            }
        }
        else
        {
            _readingData.ErrCode = _readingData.ErrCode == ErrorCode.OK
                ? ErrorCode.ErrorWhileRetrievingRecords
                : _readingData.ErrCode;
            await NaviagatetoListAsync(_readingData).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Unload patient reading add/edit UI
    /// </summary>
    /// <returns></returns>
    public override async Task UnloadUIAsync()
    {
        ClearFields();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get title for popup page
    /// </summary>
    /// <returns>Title for popup page</returns>
    public string GetPageTitle()
    {
        string title = string.Empty;
        if (_readingData.ReadingID > 0)
        {
            title = string.Format(CultureInfo.InvariantCulture,
               ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_READING_TYPE_TITLE_KEY),
               (ParentPage.PageService as ReadingService).GetChildReadings(_readingData, _readingData.ReadingID)?.FirstOrDefault()?.ReadingParent
           );
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            title = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_READING_TITLE_KEY);
        }
        return title;
    }

    private async void OnCategorySelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
    {
        _categoryFilterView.SelectedItemChanged -= OnCategorySelectedItemChanged;
        AppHelper.ShowBusyIndicator = true;
        _readingData.ReadingCategoryID = Convert.ToInt16((e.SelectedItem as OptionModel).OptionID);
        await GetPageDataAsync().ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
        _categoryFilterView.SelectedItemChanged += OnCategorySelectedItemChanged;
    }

    private void ClearFields()
    {
        ClearNutritionValues();
        ClearValueFields();
    }

    private void OnReadingTypePickerSelectionChange(object sender, EventArgs e)
    {
        _readingParentOptionsPicker.SelectedValuesChanged -= OnReadingTypePickerSelectionChange;
        ClearValueFields();
        CustomBindablePicker typePicker = (CustomBindablePicker)sender;
        var readingParentID = _readingParentOptions[typePicker.SelectedIndex].OptionID;
        if (readingParentID == ResourceConstants.R_NUTRITION_KEY_ID)
        {
            AddNutritionFieldsInLayout();
        }
        else
        {
            ClearNutritionValues();
        }
        RenderValueFieldBasedonType(readingParentID);
        _readingParentOptionsPicker.SelectedValuesChanged += OnReadingTypePickerSelectionChange;
    }

    private async void OnFoodItemSerched(object sender, EventArgs e)
    {
        _searchBox.OnAdvanceEntryUnfocused -= OnFoodItemSerched;
        if (!string.IsNullOrWhiteSpace(_searchBox.Value))
        {
            if (MobileConstants.CheckInternet)
            {
                AppHelper.ShowBusyIndicator = true;
                _foodData = new PatientReadingDTO { ErrorDescription = _searchBox.Value, ChartMetaData = _readingData.ChartMetaData };
                await (ParentPage.PageService as ReadingService).SearchFoodAsync(_foodData).ConfigureAwait(true);
                CheckAndDisplayNutritionvalue(_foodData);
                AppHelper.ShowBusyIndicator = false;
            }
            else
            {
                ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(ResourceConstants.R_OFFLINE_OPERATION_KEY));
            }
        }
        _searchBox.OnAdvanceEntryUnfocused += OnFoodItemSerched;
    }

    private void OnAddFoodOptionsSelectionChanged(object sender, EventArgs e)
    {
        _foodAddOptionsList.OnSelectionChanged -= OnAddFoodOptionsSelectionChanged;
        _subLayout.Children.Remove(_foodSource);
        _subLayout.Children.Remove(_searchBox);
        if ((sender as CustomRadioTextList).SelectedIndex == 1)
        {
            _subLayout.Add(_searchBox, 0, 3);
            _isManualSelected = false;
        }
        else
        {
            _subLayout.Children.Remove(_emptyLabel);
            _foodSource.Value = string.IsNullOrWhiteSpace(_searchBox.Value)
                ? _foodSource.Value
                : _searchBox.Value;
            _searchBox.Value = string.Empty;
            _subLayout.Add(_foodSource, 0, 2);
            _isManualSelected = true;
        }
        _foodAddOptionsList.OnSelectionChanged += OnAddFoodOptionsSelectionChanged;
    }

    private void OnPotionSizeChanged(object sender, EventArgs e)
    {
        _portionSize.OnAdvanceEntryUnfocused -= OnPotionSizeChanged;
        if (!_isManualSelected)
        {
            UpdateNutrientsValue();
        }
        _portionSize.OnAdvanceEntryUnfocused += OnPotionSizeChanged;
    }

    private async void OnFoodItemSelect(object sender, EventArgs e)
    {
        _searchedFoodItemsPage.OnSelectFoodItem -= OnFoodItemSelect;
        OptionModel selectedFood = sender as OptionModel;
        _searchBox.Value = selectedFood.OptionText;
        if (MobileConstants.CheckInternet)
        {
            AppHelper.ShowBusyIndicator = true;
            _foodData = new PatientReadingDTO
            {
                AddedBy = Convert.ToString(selectedFood.GroupName, CultureInfo.InvariantCulture),
                ChartMetaData = _readingData.ChartMetaData
            };
            await (ParentPage.PageService as ReadingService).GetFoodNutritionDataAsync(_foodData).ConfigureAwait(true);
            CheckAndDisplayNutritionvalue(_foodData);
            AppHelper.ShowBusyIndicator = false;
        }
        else
        {
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(ResourceConstants.R_OFFLINE_OPERATION_KEY));
        }
        _searchedFoodItemsPage.OnSelectFoodItem += OnFoodItemSelect;
    }

    private async void OnDeleteReadingClick(object sender, EventArgs e)
    {
        _deleteButton.Clicked -= OnDeleteReadingClick;
        await ParentPage.DisplayMessagePopupAsync(ParentPage.GetResourceByKey(ResourceConstants.R_DELETE_ACTION_KEY).InfoValue,
            OnMessgeDeleteFilesActionClicked, true, true, true).ConfigureAwait(true);
        _deleteButton.Clicked += OnDeleteReadingClick;
    }

    private async void OnMessgeDeleteFilesActionClicked(object sender, int e)
    {
        ParentPage.MessagePopup.PopCustomMessageControlAsync();
        ParentPage.MessagePopup.IsVisible = false;
        if (e == 1)
        {
            await SaveReadingsAsync(MapRecordsToDelete()).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Save reading in db
    /// </summary>
    public async Task<(bool, Guid, string)> OnSaveButtonClickedAsync()
    {
        if (ParentPage.IsFormControlValid(_subLayout) && IsDateTimeValid())
        {
            MapReadingsToSave();
            //if (_isCommingFromQuestionnaireView && _readingData.ListData.Any(x => x.ReadingValue == null))
            //{
            //    return (true, Guid.Empty, _errorMessage);
            //}
            if (_readingData.ListData.Any(x => x.IsActive))
            {
                PatientReadingDTO readingDataToSave = new PatientReadingDTO
                {
                    ListData = _readingData.ListData,
                    ReadingID = _readingData.ReadingID,
                    ReadingCategoryID = _readingData.ReadingCategoryID,
                    Title = _readingData.ListData[0].Reading,
                    PatientTaskID = _readingData.PatientTaskID,
                    ChartMetaData = _readingData.ChartMetaData,
                    SelectedUserID = _readingData.SelectedUserID,
                    RecordCount = -1
                };
                //for questionnaireTask page value added by other(patient/provider) can not be added by any other (patient/provider)
                if (_isCommingFromQuestionnaireView && !string.IsNullOrWhiteSpace(_addedByValueLabel.Text)
                    && _addedByValueLabel.Text != LibResources.GetResourceValueByKey(ParentPage.PageService.PageData?.Resources, ResourceConstants.R_SELF_PERFORMER_KEY))
                {
                    return (true, readingDataToSave.ListData.FirstOrDefault().PatientReadingID, string.Empty);
                }
                await SaveReadingsAsync(readingDataToSave).ConfigureAwait(true);
                return (true, readingDataToSave.ListData.FirstOrDefault().PatientReadingID, string.Empty);
            }
            else
            {
                DisplayErrorStatusBanner(ResourceConstants.R_MANDATORY_ERROR_KEY);
            }
        }
        return (false, Guid.Empty, _errorMessage);
    }

    private async Task SaveReadingsAsync(PatientReadingDTO readingDataToSave)
    {
        readingDataToSave.ChartMetaData = _readingData.ChartMetaData;
        await (ParentPage.PageService as ReadingService).SavePatientReadingAsync(readingDataToSave).ConfigureAwait(true);
        _readingData.ErrCode = readingDataToSave.ErrCode;
        _readingData.ReadingID = readingDataToSave.ReadingID;
        if (readingDataToSave.ErrCode == ErrorCode.OK)
        {
            await ParentPage.SyncDataWithServerAsync(Pages.PatientReadingPage, false, _isPatientLogin ? default : _readingData.SelectedUserID).ConfigureAwait(false);
            if (!_isCommingFromQuestionnaireView)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await NaviagatetoListAsync(_readingData).ConfigureAwait(true);
                });
            }
        }
        else
        {
            if (MobileConstants.IsTablet)
            {
                if (_readingData.PatientTaskID > 0)
                {
                    InvokeListRefresh(_readingData.ErrCode, null);
                }
                else
                {
                    InvokeListRefresh(_readingData, null);
                }
            }
            else
            {
                ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(_readingData.ErrCode.ToString()));
            }
        }
    }

    private void MapReadingsToSave()
    {
        if (GenericMethods.IsListNotEmpty(_readingData.ListData))
        {
            MarkRecordsInactive();
        }
        _readingData.ListData = _readingData.ListData ?? new List<PatientReadingUIModel>();
        OptionModel selectedReadingOption = _readingParentOptions.FirstOrDefault(x => x.OptionID == _readingParentOptionsPicker.SelectedValue);
        var childReadings = (ParentPage.PageService as ReadingService).GetChildReadings(_readingData, selectedReadingOption.OptionID);
        if (GenericMethods.IsListNotEmpty(childReadings))
        {
            for (int index = 0; index < childReadings.Count; index++)
            {
                var patientReading = _readingData.ListData.FirstOrDefault(x => x.ReadingID == childReadings[index].ReadingID);
                if (patientReading == null)
                {
                    patientReading = new PatientReadingUIModel
                    {
                        Unit = childReadings[index].Unit,
                        UnitIdentifier = childReadings[index].UnitIdentifier,
                        ReadingID = childReadings[index].ReadingID,
                    };
                }
                else
                {
                    _readingData.ListData.Remove(patientReading);
                }
                if (string.IsNullOrWhiteSpace(patientReading.PatientReadingID.ToString()) || patientReading.PatientReadingID == Guid.Empty)
                {

                    patientReading.PatientReadingID = GenericMethods.GenerateGuid();
                    patientReading.AddedByID = Convert.ToString(
                        App._essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0),
                        CultureInfo.InvariantCulture);

                }
                if (selectedReadingOption.OptionID == ResourceConstants.R_NUTRITION_KEY_ID)
                {
                    patientReading.SourceQuantity = GetPortionSize();
                    patientReading.SourceName = _isManualSelected ? _foodSource.Value : _searchBox.Value;
                }
                patientReading.ReadingFrequency = childReadings[index].ReadingFrequency;
                patientReading.ReadingDateTime = _readingDateTime.GetSetDate.Value;
                patientReading.ReadingValue = GetValueFromReadingField(childReadings[index]);
                patientReading.ReadingSourceType = (_isPatientLogin
                ? ReadingSource.Manual
                : ReadingSource.ProviderManual).ToString();
                patientReading.ReadingNotes = _noteLabelValue.Value?.Trim() ?? string.Empty;
                patientReading.PatientTaskID = _readingData.PatientTaskID;
                patientReading.IsActive = true;
                if (string.IsNullOrWhiteSpace(patientReading.UnitIdentifier))
                {
                    patientReading.UnitIdentifier = childReadings[index].UnitIdentifier;
                }
                _readingData.ListData.Add(patientReading);
            }
        }
    }

    private void MarkRecordsInactive()
    {
        _readingData.ListData.ForEach(x =>
        {
            x.IsActive = false;
            x.IsSynced = false;
        });
    }

    private PatientReadingDTO MapRecordsToDelete()
    {
        MarkRecordsInactive();
        return new PatientReadingDTO
        {
            ListData = _readingData.ListData,
            ReadingID = _readingData.ReadingID,
            SelectedUserID = _readingData.SelectedUserID,
            ChartMetaData = _readingData.ChartMetaData,
            RecordCount = -1
        };
    }

    private bool IsDateTimeValid()
    {
        if (_readingDateTime.GetSetDate.Value > DateTimeOffset.Now)
        {
            _errorMessage = string.Format(CultureInfo.InvariantCulture,
               ParentPage.GetResourceValueByKey(ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
               ParentPage.GetResourceValueByKey(ResourceConstants.R_DATE_TIME_TEXT_KEY),
               DateTimeOffset.Now.DateTime);
            DisplayMessage(_errorMessage);
            return false;
        }
        return true;
    }

    private void DisplayMessage(string errorMessage)
    {
        if (MobileConstants.IsTablet && !IsPatientPage())
        {
            InvokeListRefresh(errorMessage, null);
        }
        else
        {
            ParentPage.DisplayOperationStatus(errorMessage);
        }
    }

    private void CheckAndDisplayNutritionvalue(PatientReadingDTO foodData)
    {
        if (foodData.ErrCode == ErrorCode.OK)
        {
            //MapReadingsToSave(false);
            if (GenericMethods.IsListNotEmpty(foodData.ListData))
            {
                foreach (var item in foodData.ListData)
                {
                    var decimalPrecision = _readingData.ChartMetaData.FirstOrDefault(x => x.ReadingID == item.ReadingID)?.DigitsAfterDecimalPoint ?? 0;
                    item.ReadingValue = (double)Math.Round(
                        Convert.ToDecimal(item.ReadingValue, CultureInfo.InvariantCulture),
                        decimalPrecision
                    );
                }
                if (string.IsNullOrWhiteSpace(_portionSize.Value))
                {
                    _portionSize.Value = "1";
                }
                UpdateNutrientsValue(foodData.ListData);
            }
            else
            {
                SetSearchedFoodList(foodData.SummaryDataOptions ?? new List<OptionModel>());
            }
        }
        else
        {
            DisplayErrorStatusBanner(foodData.ErrCode.ToString());
        }
    }

    private void DisplayErrorStatusBanner(string errorKey)
    {
        string errorMessage = ParentPage.GetResourceValueByKey(errorKey);
        if (MobileConstants.IsTablet && !IsPatientPage())
        {
            InvokeListRefresh(errorMessage, null);
        }
        else
        {
            ParentPage.DisplayOperationStatus(errorMessage);
        }
    }

    private View GetMatchingValueField(short readingID)
    {
        return (View)(_valuesContainer?.Children?.FirstOrDefault());//todo:_valuesContainer?.Children?.FirstOrDefault(x => x.StyleId == readingID.ToString());
    }

    private void RenderValueFieldBasedonType(long selectedTypeID)
    {
        if (selectedTypeID > 0)
        {
            _valuesContainer.Children.Clear();
            var childReadings = (ParentPage.PageService as ReadingService).GetChildReadings(_readingData, selectedTypeID);
            var index = 0;
            if (GenericMethods.IsListNotEmpty(childReadings))
            {
                foreach (var metadata in childReadings)
                {
                    var resourecKey = ParentPage.GetResourceByKeyID(metadata.ReadingID).ResourceKey;
                    double? existingValue = _readingData.ListData?.FirstOrDefault(x => x.ReadingID == metadata.ReadingID)?.ReadingValue;
                    if (metadata.IsActive || existingValue.HasValue)
                    {
                        var groupDesc = ParentPage.GetResourceGroupDescriptionByGroupID(metadata.ReadingValueTypeID);
                        switch (groupDesc)
                        {
                            case GroupConstants.RS_COUNTER_READING_VALUE_TYPE_GROUP:
                            case GroupConstants.RS_DAILY_COUNTER_READING_VALUE_TYPE_GROUP:
                                CustomEntryControl counterControl = CreateEntryControl(FieldTypes.CounterEntryControl, resourecKey, existingValue, metadata);
                                index = AddPageControl(index, metadata.ReadingID, counterControl);
                                break;
                            case GroupConstants.RS_SINGLE_SELECT_READING_VALUE_TYPE_GROUP:
                                CustomRadioList singleSelectRadioList = CreateCustomRadioTextList(resourecKey, existingValue, metadata);
                                index = AddPageControl(index, metadata.ReadingID, singleSelectRadioList);
                                break;
                            case GroupConstants.RS_DROPDOWN_READING_VALUE_TYPE_GROUP:
                                CustomBindablePickerControl customDropDownMultiSelect = CreateCustomDropDownList(resourecKey, existingValue, metadata);
                                index = AddPageControl(index, metadata.ReadingID, customDropDownMultiSelect);
                                break;
                            default: //GroupConstants.RS_NUMERIC_READING_VALUE_TYPE_GROUP:
                                var control = metadata.DigitsAfterDecimalPoint > 0 ? FieldTypes.DecimalEntryControl : FieldTypes.NumericEntryControl;
                                CustomEntryControl numerinControl = CreateEntryControl(control, resourecKey, existingValue, metadata);
                                index = AddPageControl(index, metadata.ReadingID, numerinControl);
                                break;
                        }
                    }
                }
            }
        }
    }

    private double? GetValueFromReadingField(ReadingMetadataUIModel metadata)
    {
        var valueField = GetMatchingValueField(metadata.ReadingID);
        if (valueField != null)
        {
            var groupDesc = ParentPage.GetResourceGroupDescriptionByGroupID(metadata.ReadingValueTypeID);
            switch (groupDesc)
            {
                case GroupConstants.RS_COUNTER_READING_VALUE_TYPE_GROUP:
                case GroupConstants.RS_DAILY_COUNTER_READING_VALUE_TYPE_GROUP:
                    if (!string.IsNullOrWhiteSpace((valueField as CustomEntryControl).Value))
                    {
                        return Convert.ToDouble((valueField as CustomEntryControl).Value, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return 0;
                    }
                case GroupConstants.RS_SINGLE_SELECT_READING_VALUE_TYPE_GROUP:
                    return (valueField as CustomRadioList).SelectedValue;
                case GroupConstants.RS_DROPDOWN_READING_VALUE_TYPE_GROUP:
                    return (valueField as CustomBindablePickerControl).SelectedValue;
                default: // GroupConstants.RS_NUMERIC_READING_VALUE_TYPE_GROUP:
                    if (!string.IsNullOrWhiteSpace((valueField as CustomEntryControl).Value))
                    {
                        return Convert.ToDouble((valueField as CustomEntryControl).Value, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return 0;
                    }
            }
        }
        return 0;
    }

    private void UpdateNutrientsValue(List<PatientReadingUIModel> listData = null, bool shouldClear = false)
    {
        double? portionSize = GetPortionSize();
        var childReadings = (ParentPage.PageService as ReadingService).GetChildReadings(_readingData, _readingParentOptionsPicker.SelectedValue);
        bool isSearched = !shouldClear && listData?.Count > 0;
        if (GenericMethods.IsListNotEmpty(childReadings))
        {
            foreach (var metadata in childReadings)
            {
                var valueField = GetMatchingValueField(metadata.ReadingID);
                if (valueField != null)
                {
                    var value = isSearched ? listData.FirstOrDefault(x => x.ReadingID == metadata.ReadingID)?.ReadingValue : default;
                    var groupDesc = ParentPage.GetResourceGroupDescriptionByGroupID(metadata.ReadingValueTypeID);
                    switch (groupDesc)
                    {
                        case GroupConstants.RS_NUMERIC_READING_VALUE_TYPE_GROUP:
                        case GroupConstants.RS_COUNTER_READING_VALUE_TYPE_GROUP:
                        case GroupConstants.RS_DAILY_COUNTER_READING_VALUE_TYPE_GROUP:
                            value = UpdateNumericEntryValue(shouldClear, portionSize, isSearched, metadata, valueField, value);
                            break;
                    }
                }
            }
        }
    }

    private double? UpdateNumericEntryValue(bool shouldClear, double? portionSize, bool isSearched, ReadingMetadataUIModel metadata, View valueField, double? value)
    {
        value = isSearched || shouldClear
                ? value
                : Convert.ToDouble((valueField as CustomEntryControl).Value, CultureInfo.InvariantCulture);

        (valueField as CustomEntryControl).Value = value.HasValue && value.Value > 0.0
            ? GetValueBasedcOnPortionSize(portionSize, metadata, value.Value)
            : default;

        return value;
    }

    private double? GetPortionSize()
    {
        if (string.IsNullOrWhiteSpace(_portionSize.Value))
            return null;
        else
            return Convert.ToDouble(_portionSize.Value, CultureInfo.InvariantCulture);
    }

    private string GetValueBasedcOnPortionSize(double? portionSize, ReadingMetadataUIModel metadata, double value)
    {
        return Math.Round(
            Convert.ToDecimal(value * portionSize, CultureInfo.InvariantCulture), metadata.DigitsAfterDecimalPoint
        ).ToString(CultureInfo.InvariantCulture);
    }

    private CustomEntryControl CreateEntryControl(FieldTypes controlType, string key, double? existingValue, ReadingMetadataUIModel metadata)
    {
        var entryControl = new CustomEntryControl
        {
            ControlType = controlType,
            DecimalPrecision = metadata.DigitsAfterDecimalPoint,
            ControlResourceKey = key,
            PageResources = ParentPage.PageData,
        };
        entryControl.IsBackGroundTransparent = true;
        if (existingValue.HasValue)
        {
            if (existingValue.Value != 0)
            {
                entryControl.Value = existingValue.Value.ToString(CultureInfo.InvariantCulture);
            }
        }
        if (!metadata.IsActive)
        {
            entryControl.IsControlEnabled = metadata.IsActive;
        }
        return entryControl;
    }

    private CustomRadioList CreateCustomRadioTextList(string resourecKey, double? existingValue, ReadingMetadataUIModel metadata)
    {
        CustomRadioList singleSelectRadioList = new CustomRadioList
        {
            ControlResourceKey = resourecKey,
            PageResources = ParentPage.PageData,
            SelectedValue = existingValue.HasValue ? Convert.ToInt64(existingValue.Value, CultureInfo.InvariantCulture) : -1
        };
        singleSelectRadioList.IsBackGroundTransparent = true;
        singleSelectRadioList.ItemSource = (ParentPage.PageService as ReadingService).MapResourcesIntoOptionsByGroupID(
            metadata.ReadingValueTypeID, string.Empty, false, singleSelectRadioList.SelectedValue
        );
        if (!metadata.IsActive)
        {
            singleSelectRadioList.IsControlEnabled = metadata.IsActive;
        }
        return singleSelectRadioList;
    }

    private CustomBindablePickerControl CreateCustomDropDownList(string resourecKey, double? existingValue, ReadingMetadataUIModel metadata)
    {
        CustomBindablePickerControl pickerControl = new CustomBindablePickerControl
        {
            ControlResourceKey = resourecKey,
            PageResources = ParentPage.PageData,
            SelectedValue = existingValue.HasValue ? Convert.ToInt64(existingValue.Value, CultureInfo.InvariantCulture) : -1
        };
        pickerControl.ItemSource = (ParentPage.PageService as ReadingService).MapResourcesIntoOptionsByGroupID(
           metadata.ReadingValueTypeID, string.Empty, false, pickerControl.SelectedValue
        );
        pickerControl.SelectedValue = existingValue.HasValue ? (long)existingValue.Value : -1;
        if (!metadata.IsActive)
        {
            pickerControl.IsControlEnabled = metadata.IsActive;
        }
        return pickerControl;
    }

    private int AddPageControl(int index, short readingID, View control)
    {
        control.StyleId = readingID.ToString();
        _valuesContainer.Add(control, 0, index);
        index++;
        return index;
    }

    private void ClearValueFields()
    {
        if (_valuesContainer.Children?.Count > 0)
        {
            int count = _valuesContainer.Children.Count;
            for (int index = count; index > 0; index--)
            {
                _valuesContainer.Children.RemoveAt(index - 1);
            }
            _valuesContainer.Children.Clear();
        }
    }

    private void AddNutritionFieldsInLayout()
    {
        _foodAddOptionsList.OnSelectionChanged -= OnAddFoodOptionsSelectionChanged;
        _subLayout.Add(_foodAddOptionsList, 0, 1);
        _subLayout.Add(_foodSource, 0, 2);
        _subLayout.Add(_portionSize, 0, 5);
        _foodAddOptionsList.OnSelectionChanged += OnAddFoodOptionsSelectionChanged;
    }

    private void ClearNutritionValues()
    {
        _foodAddOptionsList.OnSelectionChanged -= OnAddFoodOptionsSelectionChanged;
        _foodSource.Value = string.Empty;
        _portionSize.Value = string.Empty;
        _searchBox.Value = string.Empty;
        UpdateNutrientsValue(null, true);
        _subLayout.Children.Remove(_foodAddOptionsList);
        _subLayout.Children.Remove(_foodSource);
        _subLayout.Children.Remove(_searchBox);
        _subLayout.Children.Remove(_emptyLabel);
        _subLayout.Children.Remove(_portionSize);
    }

    private void SetExistingData()
    {
        if (_readingData.PatientReadingID != Guid.Empty)
        {
            ReadingService readingService = ParentPage.PageService as ReadingService;
            _addedByHeaderLabel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADDED_BY_KEY);
            _addedByValueLabel.Text = _readingData.ListData[0].AddedByText;
            _isManualAddAllowed = readingService.IsManualAddAllowed(_readingData, _isPatientLogin);
            _isDeleteAllowed = readingService.IsDeleteAllowed(_readingData, IsPatientPage());
            IsViewOnly = readingService.IsAddEditPageViewOnly(_readingData, IsPatientPage());
            ShowHideLeftRightHeader();
        }
        AssignData(_readingData.ListData);
    }

    private async void AssignData(List<PatientReadingUIModel> newValue)
    {
        _readingParentOptionsPicker.SelectedValue = _readingParentOptions.FirstOrDefault(x => x.OptionID == newValue[0].ReadingID)?.OptionID ?? -1;
        if (newValue[0].PatientReadingID != Guid.Empty)
        {
            await Task.Delay(10).ConfigureAwait(true);
            _readingDateTime.GetSetDate = newValue[0].ReadingDateTime.Value.Date;
            _readingDateTime.SetTime = newValue[0].ReadingDateTime.Value.TimeOfDay;
            _noteLabelValue.Value = newValue[0].ReadingNotes;
            _deleteButton.IsVisible = _isDeleteAllowed;
            _subLayout.IsEnabled = _isManualAddAllowed && !_isViewOnly;
        }
        else
        {
            await Task.Delay(10).ConfigureAwait(true);
            _readingDateTime.GetSetDate = DateTime.Now.Date;
            _readingDateTime.SetTime = DateTime.Now.TimeOfDay;
        }
        if (_readingParentOptionsPicker.SelectedValue == ResourceConstants.R_NUTRITION_KEY_ID)
        {
            _foodSource.Value = string.IsNullOrWhiteSpace(newValue[0].SourceName) ? string.Empty : newValue[0].SourceName;
            if (newValue[0].SourceQuantity != null)
            {
                _portionSize.Value = (newValue[0].SourceQuantity.Value).ToString(CultureInfo.InvariantCulture);
            }
        }
        if (!string.IsNullOrWhiteSpace(_addedByValueLabel.Text))
        {
            _subLayout.Add(new StackLayout
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_END_TO_END_LAYOUT_KEY],
                Orientation = StackOrientation.Horizontal,
                Spacing = 1,
                Children =
                {
                    _addedByHeaderLabel,
                    _addedByValueLabel
                }
            }, 0, 8);
            _subLayout.Add(new BoxView
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_LIGHT_BACKGROUND_SEPARATOR_STYLE],
                Margin = new Thickness(0, 10)
            }, 0, 9);
        }
    }

    private void ShowHideLeftRightHeader()
    {
        if (!IsPatientPage())
        {
            ShellMasterPage.CurrentShell.CurrentPage.ShowHideLeftRightHeader(
                ParentPage is PatientReadingPage ? MenuLocation.Left : MenuLocation.Right,
                _isManualAddAllowed && !IsViewOnly);
        }
    }

    private async Task SetTitleViewAsync()
    {
        if (MobileConstants.IsTablet)
        {
            if (_readingData.PatientTaskID < 1 && !IsPatientPage())
            {
                await ShellMasterPage.CurrentShell.CurrentPage.OverrideTitleViewAsync(CreateMenuWithPageTitle());
            }
        }
        else
        {
            await ParentPage.OverrideTitleViewAsync(CreateMenuWithPageTitle()).ConfigureAwait(true);
        }
    }

    private MenuView CreateMenuWithPageTitle()
    {
        return new MenuView(MenuLocation.Header, GetPageTitle(), ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
    }

    private async Task NaviagatetoListAsync(PatientReadingDTO result)
    {
        if (MobileConstants.IsTablet)
        {
            if (_readingData.PatientTaskID > 0)
            {
                InvokeListRefresh(result.ErrCode.ToString(), new EventArgs());
            }
            else
            {
                InvokeListRefresh(result, new EventArgs());
            }
            //todo:
            //if (PopupNavigation.Instance.PopupStack?.Count > 0)
            //{
            //    await Navigation.PopPopupAsync().ConfigureAwait(true);
            //}
        }
        else
        {
            await ParentPage.DisplayMessagePopupAsync(result.ErrCode.ToString(), false, true, false).ConfigureAwait(true);
            await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
        }
    }

    private void ApplyResourceValue()
    {
        _readingParentOptionsPicker.PageResources = _readingDateTime.PageResources = _noteLabelValue.PageResources
            = _searchBox.PageResources = _portionSize.PageResources = _foodSource.PageResources = ParentPage.PageData;
        _emptyLabel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_NO_DATA_FOUND_KEY);
        var options = (ParentPage.PageService as ReadingService).GetFoodEntryOptions();
        _foodAddOptionsList.ItemsSource = options.Select(x => x.OptionText);
        _foodAddOptionsList.SelectedIndex = -1;
        _foodAddOptionsList.SelectedIndex = options.IndexOf(options.FirstOrDefault(x => x.IsSelected));
        if (_foodAddOptionsList.SelectedIndex == 0)
        {
            _isManualSelected = true;
        }
        if (!_isCommingFromQuestionnaireView && !_readingAddedByQuestionnaireView)
        {
            _deleteButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY);
        }
        if (!string.IsNullOrWhiteSpace(_infoText))
        {
            _infoView.IsVisible = true;
            _infoView.SetInfoValue(_infoText);
        }
    }

    private void SetReadingCategoryFilters()
    {
        if (_readingData.ReadingID < 1 && _readingData.PatientTaskID < 1)
        {
            ParentPage.PageLayout.Add(_categoryFilterView, 0, 0);
            _categoryFilterView.ItemSource = _readingData.FilterOptions;
        }
    }

    private void SeReadingParentOptions(IList<OptionModel> value)
    {
        _readingParentOptions = value.ToList();
        _readingParentOptionsPicker.ItemSource = value;
        ClearValueFields();
        ClearNutritionValues();
        _readingParentOptionsPicker.SelectedValue = _readingData.ReadingID > 0
            ? _readingParentOptions.FirstOrDefault(x => x.OptionID == _readingData.ReadingID)?.OptionID ?? 0
            : 0;
    }

    private void SetBackGroundTransparent(bool value)
    {
        _readingParentOptionsPicker.IsBackGroundTransparent = value;
        _foodSource.IsBackGroundTransparent = value;
        _searchBox.IsBackGroundTransparent = value;
        _portionSize.IsBackGroundTransparent = value;
        if (value)
        {
            //todo:_valuesContainer.BackgroundColor = Color.Transparent;
        }
        _readingDateTime.IsBackGroundTransparent = value;
        _noteLabelValue.IsBackGroundTransparent = value;
    }

    /// <summary>
    /// Dispose method
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose method
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        // Cleanup
    }
}