using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ProgramReadingRangePage : BasePage
{
    private readonly ReadingMasterDataDTO _rangeData = new() { RecordCount = -1 };
    private List<ButtonActionModel> _popupActions;
    private bool _showDeletedConfirmationPopup = true;
    private int _decimalPrecision;
    private double? _absoluteMaxValueString;
    private double? _absoluteMinValueString;
    private double? _normalMaxValueString;
    private double? _normalMinValueString;
    private double? _fromAgeString;
    private double? _toAgeString;
    private string? ageValue;
    private string? genderValue;
    private bool _isEditable = false;

    /// <summary>
    /// ID of the reading in this program
    /// </summary>
    [Parameter]
    public long ProgramReadingID { get; set; }

    /// <summary>
    /// ID of reading range
    /// </summary>
    [Parameter]
    public long ReadingRangeID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetReadingRangeDataAsync().ConfigureAwait(true);
    }

    private void OnCancelClickedAsync()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void onAgeGroupChanged()
    {
        StateHasChanged();
    }

    private void OnRemoveClicked()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _showDeletedConfirmationPopup = false;
    }

    private async Task OnDeletePopupActionClickAsync(object sequenceNo)
    {
        _showDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _rangeData.ReadingRange.IsActive = false;
                await SaveReadingRangeDataAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        if (IsValid())
        {
            _rangeData.ReadingRange.ProgramReadingID = ProgramReadingID;
            _rangeData.ReadingRange.AbsoluteMinValue = (double)_absoluteMinValueString;
            _rangeData.ReadingRange.AbsoluteMaxValue = (double)_absoluteMaxValueString;
            _rangeData.ReadingRange.NormalMinValue = (double)_normalMinValueString;
            _rangeData.ReadingRange.NormalMaxValue = (double)_normalMaxValueString;
            _rangeData.ReadingRange.ForAgeGroup = short.Parse(ageValue);
            _rangeData.ReadingRange.ForGender = short.Parse(genderValue);

            if (IsAgeInvalid())
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_FROM_AGE_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_TO_AGE_KEY));
            }
            else if (_rangeData.ReadingRange.AbsoluteMinValue > _rangeData.ReadingRange.AbsoluteMaxValue)
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_ABSOLUTE_MIN_VALUE_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_ABSOLUTE_MAX_VALUE_KEY));
            }
            else if (_rangeData.ReadingRange.NormalMinValue > _rangeData.ReadingRange.NormalMaxValue)
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_IDEAL_MIN_VALUE_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_IDEAL_MAX_VALUE_KEY));
            }
            else if (_rangeData.ReadingRange.AbsoluteMinValue > _rangeData.ReadingRange.NormalMinValue)
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_ABSOLUTE_MIN_VALUE_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_IDEAL_MIN_VALUE_KEY));
            }
            else if (_rangeData.ReadingRange.NormalMaxValue > _rangeData.ReadingRange.AbsoluteMaxValue)
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_IDEAL_MAX_VALUE_KEY),
                    LibResources.GetResourceValueByKey(_rangeData.Resources, ResourceConstants.R_ABSOLUTE_MAX_VALUE_KEY));
            }
            else
            {
                _rangeData.ReadingRange.IsActive = true;
                await SaveReadingRangeDataAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task GetReadingRangeDataAsync()
    {
        _rangeData.ReadingRange = new ReadingRangeModel
        {
            ReadingRangeID = ReadingRangeID,
            ProgramReadingID = ProgramReadingID
        };
        await SendServiceRequestAsync(new ReadingService(AppState.webEssentials).SyncProgramReadingRangesFromServerAsync(_rangeData, CancellationToken.None), _rangeData).ConfigureAwait(true);
        if (_rangeData.ErrCode == ErrorCode.OK)
        {
            AssignValues();
            _isEditable = LibPermissions.HasPermission(_rangeData.FeaturePermissions, AppPermissions.ProgramReadingRangeAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_rangeData.ErrCode.ToString());
        }
    }

    private void AssignValues()
    {
        genderValue = _rangeData.GenderOptions?.Find(x => x.IsSelected)?.OptionID.ToString();
        ageValue = _rangeData.AgeOptions?.Find(x => x.IsSelected)?.OptionID.ToString();
        _decimalPrecision = Convert.ToInt32(LibResources.GetMaxLengthValueByKey(_rangeData.Resources, ResourceConstants.R_DIGITS_AFTER_DECIMAL_POINT) ?? 0, CultureInfo.InvariantCulture);
        _absoluteMaxValueString = (double)_rangeData.ReadingRange.AbsoluteMaxValue;
        _absoluteMinValueString = (double)_rangeData.ReadingRange.AbsoluteMinValue;
        _normalMaxValueString = (double)_rangeData.ReadingRange.NormalMaxValue;
        _normalMinValueString = (double)_rangeData.ReadingRange.NormalMinValue;
        _fromAgeString = _rangeData.ReadingRange.FromAge;
        _toAgeString = _rangeData.ReadingRange.ToAge;
    }

    private async Task SaveReadingRangeDataAsync()
    {
        await SendServiceRequestAsync(new ReadingService(AppState.webEssentials).SyncReadingRangeToServerAsync(_rangeData, new CancellationToken()), _rangeData).ConfigureAwait(true);
        if (_rangeData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_rangeData.ErrCode.ToString());
        }
        else
        {
            Error = _rangeData.ErrCode.ToString();
        }
    }

    private bool IsAgeInvalid()
    {
        if (_rangeData.ReadingRange.ForAgeGroup == ResourceConstants.R_AGE_TYPE_AGE_RANGE_KEY_ID)
        {
            _rangeData.ReadingRange.FromAge = Convert.ToInt16(_fromAgeString, CultureInfo.InvariantCulture);
            _rangeData.ReadingRange.ToAge = Convert.ToInt16(_toAgeString, CultureInfo.InvariantCulture);
            if (_rangeData.ReadingRange.FromAge > _rangeData.ReadingRange.ToAge)
            {
                return true;
            }
        }
        else
        {
            _rangeData.ReadingRange.FromAge = null;
            _rangeData.ReadingRange.ToAge = null;
        }
        return false;
    }
}