using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ProgramReadingPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { ProgramReading = new ReadingModel(), RecordCount = -1 };
    private List<ButtonActionModel> _actionData;
    private List<OptionModel> _selectedReadings = new List<OptionModel>();
    private ReadingService _readingService;
    private double? _sequenceNo;
    private bool _hideConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// IsSynced
    /// </summary>
    [Parameter]
    public bool IsSynced { get; set; }

    /// <summary>
    /// Program ID
    /// </summary>
    [Parameter]
    public long ProgramID
    {
        get;
        set;
    }

    /// <summary>
    /// selected reading data
    /// </summary>
    [Parameter]
    public long ProgramReadingID
    {
        get { return _programData.ProgramReading.ProgramReadingID; }
        set { _programData.ProgramReading.ProgramReadingID = value; }
    }

    protected override async Task OnInitializedAsync()
    {
        _readingService = new ReadingService(AppState.webEssentials);
        _programData.ProgramReading.ProgramID = ProgramID;
        await SendServiceRequestAsync(_readingService.SyncProgramReadingsFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            if (ProgramReadingID != 0)
            {
                _programData.ProgramReading.ReadingCategoryID = Convert.ToInt16(_programData.OperationTypes.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0);
                _sequenceNo = _programData.ProgramReading.SequenceNo;
            }
            FilterReadings();
            _isEditable = IsSynced;
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(null);
        }
    }

    private void OnCategoryChange(object? e)
    {
        Int16.TryParse(e as string, out var optionId);
        _programData.ProgramReading.ReadingCategoryID = optionId;
        FilterReadings();
    }

    private void FilterReadings()
    {
        _selectedReadings = _programData.Items.Where(x => x.ParentOptionID == _programData.ProgramReading.ReadingCategoryID || x.OptionID == -1).ToList();
    }

    private List<OptionModel> GetOptions()
    {
        return new List<OptionModel> {new OptionModel {
            OptionID = 1,
            OptionText = LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_ADD_TO_MEDICAL_HISTORY),
            IsSelected = _programData.ProgramReading.IsCritical
        }};
    }

    private void OnCancelClickedAsync()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task DeletePopUpCallbackAsync(object value)
    {
        Success = Error = string.Empty;
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _programData.ProgramReading.IsActive = false;
                await SaveProgramReadingAsync();
            }
        }
    }

    private void OnSelectReadingsChange(object? e)
    {
        if (Int16.TryParse(e as string, out var optionId))
        {
            _programData.ProgramReading.ReadingID = optionId;
        }
    }

    private async void OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            var programReading = _programData.Items?.FirstOrDefault(x => x.OptionID == _programData.ProgramReading.ReadingID);
            _programData.ProgramReading.ProgramID = ProgramID;
            _programData.ProgramReading.SequenceNo = Convert.ToByte(_sequenceNo, CultureInfo.InvariantCulture);
            _programData.ProgramReading.ReadingCategory = _programData.OperationTypes.FirstOrDefault(x => x.IsSelected == true).OptionText;
            _programData.ProgramReading.Reading = programReading.OptionText;
            _programData.ProgramReading.IsActive = true;
            await SaveProgramReadingAsync();
        }
    }

    private async Task SaveProgramReadingAsync()
    {
        _programData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync(_readingService.SyncProgramReadingToServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString());
        }
        else
        {
            Error = _programData.ErrCode.ToString();
            StateHasChanged();
        }
    }
}