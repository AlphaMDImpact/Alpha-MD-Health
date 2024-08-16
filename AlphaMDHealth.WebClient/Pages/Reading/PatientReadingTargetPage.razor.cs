using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class PatientReadingTargetPage : BasePage
{
    private readonly PatientReadingDTO _targetData = new PatientReadingDTO
    {
        PatientReadingTarget = new ReadingTargetModel(),
        ReadingRange = new ReadingRangeModel(),
        RecordCount = -3
    };
    private double? _targetMinValueString;
    private double? _targetMaxValueString;
    private string _showErrorMessageLabel;
    private string _min;
    private string _max;
    private string _minPlaceHolder;
    private string _maxPlaceHolder;
    private bool _isFirstCallForPlceholder;
    private int _decimalPrecision = 0;
    private ReadingService _service;

    /// <summary>
    /// Reading  ID parameter
    /// </summary>
    [Parameter]
    public short ReadingID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _service = new ReadingService(AppState.webEssentials);
        _targetData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        _targetData.ReadingID = ReadingID;
        await SendServiceRequestAsync(_service.GetPatientReadingsAsync(_targetData), _targetData).ConfigureAwait(true);
        if (_targetData.ErrCode == ErrorCode.OK)
        {
            _isFirstCallForPlceholder = true;
            if (_targetData.PatientReadingTargets.Count != 0)
            {
                var selectedData = _targetData.ReadingOptions.FirstOrDefault(x => x.IsSelected);
                _targetData.PatientReadingTarget = _targetData.PatientReadingTargets.FirstOrDefault();
                SetDecimalPrecision(selectedData.OptionID);
                AssignAbsoluteRanges(_targetData, _targetData.PatientReadingTarget.ReadingID);
                AssignValues();
            }
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_targetData.ErrCode.ToString());
        }
    }

    private void SetDecimalPrecision(long id)
    {
        _decimalPrecision = _targetData.ChartMetaData.FirstOrDefault(x => x.ReadingID == id)?.DigitsAfterDecimalPoint ?? 0;
    }

    private void AssignValues()
    {
        _targetMinValueString = _targetData.PatientReadingTarget.TargetMinValue;
        _targetMaxValueString = _targetData.PatientReadingTarget.TargetMaxValue;
    }

    private async Task OnSaveButtonClickedAsync()
    {
        _showErrorMessageLabel = string.Empty;
        if (IsValid())
        {
            _targetData.PatientReadingTarget = new ReadingTargetModel
            {
                ReadingID = (short)(_targetData.ReadingOptions.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0),
                TargetMinValue = Convert.ToSingle(_targetMinValueString, CultureInfo.InvariantCulture),
                TargetMaxValue = Convert.ToSingle(_targetMaxValueString, CultureInfo.InvariantCulture),
                UserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0),
                IsActive = true,
            };
            _targetData.PatientReadingTargets.Clear();
            _targetData.PatientReadingTargets.Add(_targetData.PatientReadingTarget);
            if (_targetData.PatientReadingTarget.TargetMinValue > _targetData.PatientReadingTarget.TargetMaxValue)
            {
                _showErrorMessageLabel = string.Format(CultureInfo.InvariantCulture
                    , LibResources.GetResourceValueByKey(_targetData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY)
                    , LibResources.GetResourceValueByKey(_targetData.Resources, ResourceConstants.R_READING_TARGET_MIN_VALUE_KEY)
                    , LibResources.GetResourceValueByKey(_targetData.Resources, ResourceConstants.R_READING_TARGET_MAX_VALUE_KEY));
                StateHasChanged();
            }
            else
            {
                await SendServiceRequestAsync(_service.SavePatientReadingsTargetAsync(_targetData, CancellationToken.None), _targetData).ConfigureAwait(true);
                if (_targetData.ErrCode == ErrorCode.OK)
                {
                    await OnClose.InvokeAsync(_targetData.ErrCode.ToString());
                }
                else
                {
                    Error = _targetData.ErrCode.ToString();
                }
            }
        }
    }

    public void AssignAbsoluteRanges(PatientReadingDTO targetData, int selectedValue)
    {
        if (_isFirstCallForPlceholder)
        {
            _minPlaceHolder = targetData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_READING_TARGET_MIN_VALUE_KEY)?.ResourceValue;
            _maxPlaceHolder = targetData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_READING_TARGET_MAX_VALUE_KEY)?.ResourceValue;
            _isFirstCallForPlceholder = false;
        }
        _service.AssignAbsoluteRanges(targetData, selectedValue, _minPlaceHolder, _maxPlaceHolder);
    }

    private async Task OnCanceledClickAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnReadingTypeChange(object optionId)
    {
        if (optionId != null)
        {
            if (!string.IsNullOrWhiteSpace(optionId.ToString()))
            {
                var readingTargetModel = _targetData.ReadingOptions.FirstOrDefault(x => x.IsSelected);
                _min = readingTargetModel.OptionText;
                _max = readingTargetModel.OptionText;
                _targetData.PatientReadingTarget = _targetData.PatientReadingTargets.FirstOrDefault(x => x.ReadingID == readingTargetModel.OptionID);
                SetDecimalPrecision(readingTargetModel.OptionID);
                AssignAbsoluteRanges(_targetData, (int)readingTargetModel.OptionID);
                AssignValues();
                _isDataFetched = true;
                StateHasChanged();
            }
        }
    }
}