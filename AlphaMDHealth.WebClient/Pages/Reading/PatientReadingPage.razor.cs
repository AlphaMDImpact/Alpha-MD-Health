using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;
using System.Globalization;
using Wangkanai.Extensions;


namespace AlphaMDHealth.WebClient;

public partial class PatientReadingPage : BasePage
{
    private readonly PatientReadingDTO _readingData = new PatientReadingDTO { RecordCount = -1 };
    private readonly PatientReadingDTO _searchFoodData = new PatientReadingDTO { IsActive = false };
    private List<OptionModel> _foodEntryOptions = new List<OptionModel>();
    private ReadingService _readingService;
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private long _selectedParentReadingID;
    private string _sourceName;
    private double? _sourceQuantity;
    private bool _isEditable = false;
    private bool _isDeletable;
    private bool _isFoodSelected;
    private IList<ButtonActionModel> _actionButtons = null;

    /// <summary>
    /// Show category
    /// </summary>
    [Parameter]
    public bool ShowCategory { get; set; }

    /// <summary>
    /// Patient Reading ID parameter
    /// </summary>
    [Parameter]
    public Guid PatientReadingID
    {
        get { return _readingData.PatientReadingID; }
        set { _readingData.PatientReadingID = value; }
    }

    /// <summary>
    /// ReadingCategory ID
    /// </summary>
    [Parameter]
    public short? ReadingCategoryID
    {
        get { return _readingData.ReadingCategoryID; }
        set
        {
            _readingData.ReadingCategoryID = (short)value;
        }
    }

    /// <summary>
    /// Reading  ID
    /// </summary>
    [Parameter]
    public short? ReadingID
    {
        get { return _readingData.ReadingID; }
        set
        {
            if (_readingData.ReadingID != value && IsComingFromQuestionnaireTaskPage)
            {
                _readingData.ReadingID = value ?? default;
                GetDataAsync().ConfigureAwait(false);
            }
            else
            {
                _readingData.ReadingID = value ?? default;
            }
        }
    }

    /// <summary>
    /// Is Coming From QuestionnaireTaskPage
    /// </summary>
    [Parameter]
    public bool IsComingFromQuestionnaireTaskPage
    {
        get
        {
            return _readingData.IsCommingFromQuestionnaireTaskPage;
        }
        set
        {
            _readingData.IsCommingFromQuestionnaireTaskPage = value;
        }
    }

    /// <summary>
    /// Is Require Or Not
    /// </summary>
    [Parameter]
    public bool IsRequiredQuestion
    {
        get
        {
            return _readingData.IsRequiredQuestion;
        }
        set
        {
            _readingData.IsRequiredQuestion = value;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync();
    }

    private async Task GetDataAsync()
    {
        _readingService = new ReadingService(AppState.webEssentials);
        _readingData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await SendServiceRequestAsync(_readingService.GetPatientReadingsAsync(_readingData), _readingData).ConfigureAwait(true);
        if (IsPatientMobileView)
        {
            _actionButtons ??= new List<ButtonActionModel>();
            _actionButtons.Add(new ButtonActionModel
            {
                ButtonResourceKey = ResourceConstants.R_SAVE_ACTION_KEY,
                ButtonAction = () => { SavePatientReadingData(); },
                Icon = ImageConstants.I_SAVE_ICON_RESPONSIVE
            });
            _actionButtons.Add(new ButtonActionModel
            {
                ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                ButtonAction = () => { OnCancelClickedAsync(); },
                Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
            });
        }
        if (_readingData.ErrCode == ErrorCode.OK)
        {
            if (GenericMethods.IsListNotEmpty(_readingData.FilterOptions)
                || GenericMethods.IsListNotEmpty(_readingData.ReadingParentOptions))
            {
                _isEditable = _readingService.IsEditAllowed(_readingData, AppState.IsPatient);
                _isDeletable = _readingData.ReadingID != 0 && _readingService.IsDeleteAllowed(_readingData,AppState.IsPatient);
                OnReadingTypeChanged();
                _isDataFetched = true;
                return;
            }
            else
            {
                _readingData.ErrCode = ErrorCode.NoDataFoundKey;
            }
        }
       
        await OnClose.InvokeAsync(_readingData.ErrCode.ToString());
    }

    private List<OptionModel> GetOptionsBasedOnGroupName(long groupId, string selectedID)
    {
        return _readingService.GetOptions(Convert.ToInt16(groupId), string.IsNullOrWhiteSpace(selectedID)
            ? default
            : Convert.ToInt16(selectedID));
    }

    private void OnReadingCategoryClicked(object value)
    {
        _isDataFetched = false;
        _controls.Clear();
        _readingData.ReadingCategoryID = (short)(_readingData.FilterOptions.FirstOrDefault(x => x.IsSelected)?.OptionID ?? default);
        _readingService.MapReadingParentOptions(_readingData, true);
        OnReadingTypeChanged();
        _isDataFetched = true;
        StateHasChanged();
    }

    private void OnReadingTypeChanged()
    {
        _controls.Clear();
        var optionID = _readingData.ReadingParentOptions?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? -1;
        if (_selectedParentReadingID != optionID)
        {
            _readingData.ReadingOptions = null;
            _selectedParentReadingID = optionID;
            _foodEntryOptions?.Clear();
            if (_selectedParentReadingID > 0)
            {
                _isDataFetched = false;
                IsNutritionAddEdit();
                var childReadings = GetChildReadings(_selectedParentReadingID);
                _readingService.MapChildReadingOptions(_readingData, childReadings);
                _isDataFetched = true;
                StateHasChanged();
            }
        }
    }

    private void OnNumericEntryChange(object? value, long readingID)
    {
        string? readingValue = value?.ToString();
        SetOptionText(readingID, readingValue);
    }

    private void OnNutritionEntryTypeChanged(object obj)
    {
        Error = string.Empty;
        RemoveControlContainsKey(ResourceConstants.R_ENTER_FOOD_TEXT_KEY);
        RemoveControlContainsKey(ResourceConstants.R_SEARCH_FOOD_TEXT_KEY);
        if (_foodEntryOptions.FirstOrDefault(x => x.IsSelected)?.GroupName == ResourceConstants.R_SEARCH_FOOD_TEXT_KEY)
        {
            _sourceName = string.Empty;
        }
        StateHasChanged();
    }

    private async void OnSearchValueChanged()
    {
        if (string.IsNullOrWhiteSpace(_sourceName))
        {
            _isFoodSelected = false;
            EmptyNutrientValues(null);
        }
        else
        {
            if (_sourceName.Length > 2 && !_isFoodSelected)
            {
                _searchFoodData.IsActive = false;
                _searchFoodData.ErrCode = ErrorCode.OK;
                _searchFoodData.ErrorDescription = _sourceName;
                _searchFoodData.ChartMetaData = _readingData.ChartMetaData;
                _searchFoodData.ReadingUnits = _readingData.ReadingUnits;
                await SendServiceRequestAsync(_readingService.SearchFoodAsync(_searchFoodData), _searchFoodData).ConfigureAwait(true);
                if (_searchFoodData.ErrCode == ErrorCode.OK)
                {
                    if (GenericMethods.IsListNotEmpty(_searchFoodData.SummaryDataOptions))
                    {
                        if (_searchFoodData.SummaryDataOptions.Count == 1)
                        {
                            UpdateNutritionValues(_searchFoodData, _searchFoodData.SummaryDataOptions[0].OptionText);
                        }
                        else if (_searchFoodData.SummaryDataOptions.Count > 0)
                        {
                            _searchFoodData.Resources = _readingData.Resources;
                            Error = string.Empty;
                            _searchFoodData.IsActive = true;
                            StateHasChanged();
                        }
                    }
                    else
                    {
                        //// Display no data found label below search box
                        EmptyNutrientValues(null);
                        Error = LibResources.GetResourceValueByKey(_readingData.Resources, ResourceConstants.R_NO_DATA_FOUND_KEY);
                    }
                }
                else
                {
                    //// Display Error in status banner
                    Error = _searchFoodData.ErrCode.ToString();
                }
            }
        }
    }

    private async Task FoodOptionsPopUpClosedEventCallbackAsync(PatientReadingDTO selectedFoodData)
    {
        _searchFoodData.IsActive = false;
        _isFoodSelected = true;
        if (selectedFoodData != null)
        {
            await SendServiceRequestAsync(_readingService.GetFoodNutritionDataAsync(selectedFoodData), selectedFoodData).ConfigureAwait(true);
            if (selectedFoodData.ErrCode == ErrorCode.OK)
            {
                UpdateNutritionValues(selectedFoodData, selectedFoodData.Title);
            }
            else
            {
                //// Display Error in status banner
                Error = selectedFoodData.ErrCode.ToString();
            }
        }
    }

    private void SetOptionText(long readingID, string? value)
    {
        _readingData.ReadingOptions.FirstOrDefault(x => x.OptionID == readingID).OptionText = value;
    }

    private List<ReadingMetadataUIModel> GetChildReadings(long readingID)
    {
        return _readingData.ChartMetaData?.Where(x => x.ReadingParentID == readingID || x.ReadingID == readingID)?.OrderBy(x => x.SequenceNo)?.ToList();
    }

    private void IsNutritionAddEdit()
    {
        if (IsNutritionType())
        {
            _foodEntryOptions = _readingService.GetFoodEntryOptions();
            var reading = _readingData.ListData.FirstOrDefault();
            _sourceName = Convert.ToString(reading.SourceName ?? string.Empty, CultureInfo.InvariantCulture);
            _sourceQuantity = reading.SourceQuantity != null ? (long)reading.SourceQuantity : null;
        }
    }

    private void UpdateNutritionValues(PatientReadingDTO nutrientData, string sourceName)
    {
        EmptyNutrientValues(_sourceQuantity == null ? 1 : Convert.ToInt64(_sourceQuantity));
        if (GenericMethods.IsListNotEmpty(nutrientData.ListData))
        {
            //// Update Food nutrition values 
            _sourceName = sourceName;
            foreach (var nutrient in nutrientData.ListData)
            {
                var readingOption = _readingData.ReadingOptions.FirstOrDefault(x => x.OptionID == nutrient.ReadingID);
                if (readingOption != null)
                {
                    readingOption.OptionText = nutrient.ReadingValue?.ToString() ?? nutrient.ReadingValue2;
                    //saving initial value in Parent Option Text
                    readingOption.ParentOptionText = nutrient.ReadingValue?.ToString() ?? nutrient.ReadingValue2;
                }
            }
            AssignNutrientValues(_sourceQuantity);
        }
        else
        {
            //// Display no data found label below search box
            Error = LibResources.GetResourceValueByKey(_readingData.Resources, ResourceConstants.R_NO_DATA_FOUND_KEY);
        }
    }

    private bool IsNutritionType()
    {
        return _selectedParentReadingID == ResourceConstants.R_NUTRITION_KEY_ID;
    }

    private void OnPortionValueChanged()
    {
        if (_foodEntryOptions.Any(x => x.IsSelected
            && x.GroupName == ResourceConstants.R_SEARCH_FOOD_TEXT_KEY)
            && GenericMethods.IsListNotEmpty(_searchFoodData.ListData))
        {
            AssignNutrientValues(_sourceQuantity);
        }
    }

    private void AssignNutrientValues(double? value)
    {
        if (value.HasValue)
        {
            foreach (var nutrient in _readingData.ReadingOptions)
            {
                nutrient.OptionText = Math.Round(value.Value * (string.IsNullOrWhiteSpace(nutrient.ParentOptionText)
                    ? 0
                    : Convert.ToDouble(nutrient.ParentOptionText)), (int)nutrient.SequenceNo).ToString();
            }
        }      
    }

    private void EmptyNutrientValues(long? portion)
    {
        _sourceQuantity = portion;
        _searchFoodData.ListData ??= new List<PatientReadingUIModel>();
    }

    public async Task<(bool, Guid)> SavePatientReadingData()
    {
        return await OnSaveClick().ConfigureAwait(false);
    }

    private async Task<(bool, Guid)> OnSaveClick()
    {
        Error = Success = string.Empty;
        if (IsValid())
        {
            if (_selectedParentReadingID == ResourceConstants.R_NUTRITION_KEY_ID && string.IsNullOrWhiteSpace(_sourceName) && !_readingData.IsCommingFromQuestionnaireTaskPage)
            {
                Error = string.Format(
                    CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_readingData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_readingData.Resources, ResourceConstants.R_ENTER_FOOD_TEXT_KEY)
                );
                StateHasChanged();
            }
            _readingData.ListData.FirstOrDefault().IsActive = true;
            MapPatientReadingDataToSave();
            if (_readingData.PatientReadings.Any(x => x.IsActive))
            {
                return (true, await SaveReadingDataAsync().ConfigureAwait(true));
            }
            else
            {
                Error = ResourceConstants.R_MANDATORY_ERROR_KEY;
            }
        }
        StateHasChanged();
        return (false, Guid.Empty);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnActionClickAsync(object buttonID)
    {
        _hideDeletedConfirmationPopup = true;
        Error = string.Empty;
        if (buttonID?.ToString() == Constants.NUMBER_ONE && _readingData.ListData != null)
        {
            _readingData.ListData.FirstOrDefault().IsActive = false;
            if (_readingData.ListData.Count > 0)
            {
                _readingData.ListData.Where(x => (x.PatientReadingID != Guid.Empty || x.PatientReadingID != default))?.ToList()
                    .ForEach(s => s.IsActive = false);
            }
            MapPatientReadingDataToSave();
            await SaveReadingDataAsync().ConfigureAwait(true);
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private async Task<Guid> SaveReadingDataAsync()
    {
        await SendServiceRequestAsync(_readingService.SavePatientReadingAsync(_readingData), _readingData).ConfigureAwait(true);
        if (_readingData.ErrCode == ErrorCode.OK)
        {
            if (IsComingFromQuestionnaireTaskPage)
            {
                return _readingData.PatientReadings.FirstOrDefault().PatientReadingID;
            }
            string result = string.Join(Constants.PIPE_SEPERATOR, _readingData.ErrCode, _readingData.ChartMetaData.FirstOrDefault().ReadingParentID == 0 ? _readingData.PatientReadings.FirstOrDefault().ReadingID : _readingData.ChartMetaData.FirstOrDefault().ReadingParentID, _readingData.ReadingCategoryID);
            await OnClose.InvokeAsync(result);
        }
        else
        {
            Error = _readingData.ErrCode.ToString();
        }
        if (!IsComingFromQuestionnaireTaskPage)
        {
            StateHasChanged();
        }
        return Guid.Empty;
    }

    private void MapPatientReadingDataToSave()
    {
        _readingData.PatientReadings = new List<PatientReadingModel>();
        var firstData = _readingData.ListData[0];
        var isAdd = firstData.PatientReadingID == Guid.Empty;
        var addedByID = isAdd
            ? AppState.webEssentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, Constants.CONSTANT_ZERO)
            : firstData.AddedByID;
        var addedON = GenericMethods.GetUtcDateTime;
        var lastModifiedOn = GenericMethods.GetUtcDateTime;
        if (_readingData.ReadingOptions != null)
        {
            for (int index = 0; index < _readingData.ReadingOptions.Count; index++)
            {
                var rIndex = _readingData.ListData.FindIndex(x => x.ReadingID == _readingData.ReadingOptions[index].OptionID);
                var currentData = _readingData.ListData[rIndex];
                GetValueFromReadingField(_readingData.ReadingOptions[index], currentData);

                var metadata = _readingData.ChartMetaData.FirstOrDefault(x => x.ReadingID == currentData.ReadingID);

                var reading = new PatientReadingModel
                {
                    PatientReadingID = currentData.PatientReadingID == Guid.Empty
                        ? GenericMethods.GenerateGuid()
                        : currentData.PatientReadingID,
                    ReadingID = currentData.ReadingID,
                    ReadingSourceType = firstData.PatientReadingID == Guid.Empty
                        ? ReadingSource.ProviderManual.ToString()
                        : firstData.ReadingSourceType,
                    UserID = _readingData.SelectedUserID,
                    ReadingDateTime =  firstData.ReadingDateTime.Value.ToUniversalTime(),
                    AddedON = addedON,
                    LastModifiedON = lastModifiedOn,
                    AddedByID = addedByID,
                    IsSynced = false,
                    IsActive = firstData.IsActive,
                    UnitIdentifier = metadata.UnitIdentifier,
                };
                if (reading.IsActive)
                {
                    reading.PatientTaskID = Convert.ToInt64(firstData.PatientTaskID, CultureInfo.InvariantCulture);
                    reading.ReadingValue = (currentData != null && currentData.ReadingValue != null) ? currentData.ReadingValue.Value : null;
                    reading.ReadingValue2 = (currentData != null && currentData.ReadingValue2 != null) ? currentData.ReadingValue2 : null;
                    reading.ReadingDateTime = firstData.ReadingDateTime.Value.ToUniversalTime();
                    reading.ReadingFrequency = metadata.ReadingFrequency;
                    reading.ReadingNotes = firstData.ReadingNotes;
                    reading.ReadingSourceID = firstData.ReadingSourceID == default ? Guid.Empty : firstData.ReadingSourceID;
                    reading.SourceName = _sourceName;
                    reading.SourceQuantity = _sourceQuantity == null
                        ? null
                        : Convert.ToDouble(_sourceQuantity, CultureInfo.InvariantCulture);
                }
                _readingData.PatientReadings.Add(reading);            }
        }
    }

    private void GetValueFromReadingField(OptionModel metadata, PatientReadingUIModel reading)
    {
        var key = _readingData?.Resources.FirstOrDefault(x => x.ResourceKeyID == metadata.OptionID).ResourceKey;
        var valueField = _controls.FirstOrDefault(x => x.Key == key).Value;
        if (valueField != null)
        {
            var groupDesc = _readingData.Resources.FirstOrDefault(r => r.GroupID == metadata.ParentOptionID)?.GroupDesc;
            reading.ReadingValue = null;
            reading.ReadingValue2 = null;
            switch (groupDesc)
            {
                case GroupConstants.RS_COUNTER_READING_VALUE_TYPE_GROUP:
                case GroupConstants.RS_DAILY_COUNTER_READING_VALUE_TYPE_GROUP:
                    reading.ReadingValue = Convert.ToDouble((valueField as AmhNumericEntryControl).Value, CultureInfo.InvariantCulture);
                    break;
                case GroupConstants.RS_SINGLE_SELECT_READING_VALUE_TYPE_GROUP:
                    reading.ReadingValue = Convert.ToDouble((valueField as AmhRadioButtonListControl).Value, CultureInfo.InvariantCulture);
                    break;
                case GroupConstants.RS_DROPDOWN_READING_VALUE_TYPE_GROUP:
                    reading.ReadingValue = Convert.ToDouble((valueField as AmhDropdownControl).Value, CultureInfo.InvariantCulture);
                    break;
                case GroupConstants.RS_TEXT_READING_VALUE_TYPE_GROUP:
                    reading.ReadingValue2 = (valueField as AmhEntryControl).Value;
                    break;
                default:
                    if ((valueField as AmhNumericEntryControl).Value.HasValue)
                    {
                        reading.ReadingValue = Convert.ToDouble((valueField as AmhNumericEntryControl).Value, CultureInfo.InvariantCulture);
                    }
                    break;
            }
        }
    }
}