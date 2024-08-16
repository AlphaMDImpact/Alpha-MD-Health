using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ProgramReadingMetadataPage : BasePage
{
    private readonly ReadingMasterDataDTO _readingMasterData = new ReadingMasterDataDTO();

    /// <summary>
    /// Program Reading ID
    /// </summary>
    [Parameter]
    public long ProgramReadingID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _readingMasterData.ReadingMetadata = new ReadingModel { ProgramReadingID = ProgramReadingID };
        await SendServiceRequestAsync(new ReadingService(AppState.webEssentials).SyncReadingMetadataFromServerAsync(_readingMasterData, CancellationToken.None), _readingMasterData).ConfigureAwait(true);
        if (_readingMasterData.ErrCode == ErrorCode.OK && _readingMasterData.ReadingMetadata != null)
        {
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_readingMasterData.ErrCode.ToString());
        }
    }

    public void OnSummaryRecordValueChanged(short recordCount)
    {
        _readingMasterData.ErrCode = ErrorCode.OK;
        _readingMasterData.ReadingMetadata.SummaryRecordCount = recordCount;
    }

    public void OnDigitsAfterDecimalPointValueChanged(byte digitsPointValue)
    {
        _readingMasterData.ErrCode = ErrorCode.OK;
        _readingMasterData.ReadingMetadata.DigitsAfterDecimalPoint = digitsPointValue;
    }

    public void OnReadHistoryDataValueChanged(short value)
    {
        _readingMasterData.ErrCode = ErrorCode.OK;
        _readingMasterData.ReadingMetadata.DaysOfPastRecordsToSync = value;
    }

    private async Task OnSaveButtonClickedAsync()
    {
        if (IsValid())
        {
            _readingMasterData.ReadingMetadata.ProgramReadingID = Convert.ToInt64(ProgramReadingID);
            _readingMasterData.ReadingMetadata.ReadingFrequency = (short)_readingMasterData.FrequencyType?.FirstOrDefault(x => x.IsSelected)?.OptionID;
            _readingMasterData.ReadingMetadata.ChartType = (short)_readingMasterData.ChartType?.FirstOrDefault(x => x.IsSelected)?.OptionID;
            _readingMasterData.ReadingMetadata.ValueAddedBy = (short)_readingMasterData.ValueAddedByType?.FirstOrDefault(x => x.IsSelected)?.OptionID;
            _readingMasterData.ReadingMetadata.AllowDelete = IsSelected(_readingMasterData.CanBeDeletedType?.FirstOrDefault(x => x.IsSelected)?.OptionID);
            _readingMasterData.ReadingMetadata.AllowDeviceData = IsSelected(_readingMasterData.DeviceDataType?.FirstOrDefault(x => x.IsSelected)?.OptionID);
            _readingMasterData.ReadingMetadata.AllowHealthKitData = IsSelected(_readingMasterData.HealthKitDataType?.FirstOrDefault(x => x.IsSelected)?.OptionID);
            _readingMasterData.ReadingMetadata.AllowManualAdd = IsSelected(_readingMasterData.ManualReadingType?.FirstOrDefault(x => x.IsSelected)?.OptionID);
            _readingMasterData.ReadingMetadata.ShowInData = IsSelected(_readingMasterData.ShowInDataType?.FirstOrDefault(x => x.IsSelected)?.OptionID);
            _readingMasterData.ReadingMetadata.ShowInDifferentLines = IsSelected(_readingMasterData.ShowInDifferentLinesType?.FirstOrDefault(x => x.IsSelected)?.OptionID);
            _readingMasterData.ReadingMetadata.ShowInGraph = IsSelected(_readingMasterData.ShowInGraphType?.FirstOrDefault(x => x.IsSelected)?.OptionID);
            _readingMasterData.ReadingMetadatas = new List<ReadingModel> { _readingMasterData.ReadingMetadata };
            _readingMasterData.RecordCount = -1;
            await SendServiceRequestAsync(new ReadingService(AppState.webEssentials).SyncReadingMetadataToServerAsync(_readingMasterData, new CancellationToken()), _readingMasterData).ConfigureAwait(true);
            if (_readingMasterData.ErrCode == ErrorCode.OK)
            {
                await OnClose.InvokeAsync(_readingMasterData.ErrCode.ToString());
            }
            else
            {
                Error = _readingMasterData.ErrCode.ToString();
            }
        }
    }

    private static bool IsSelected(long? value)
    {
        return value == 1;
    }

    private async void OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    public void SetBooleanValue(string input, Action<bool> setAction)
    {
        setAction(!string.IsNullOrWhiteSpace(input) && Convert.ToInt32(input) == 1);
    }

}