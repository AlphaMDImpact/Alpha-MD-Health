using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ProgramReadingRangesPage : BasePage
{
    private readonly ReadingMasterDataDTO _rangeData = new() { ReadingRanges = new List<ReadingRangeModel>() };
    private List<ButtonActionModel> _actionButtons;
    private long _readingRangeID;
    private bool _showAddEditPage = false;

    /// <summary>
    /// ID of the reading in this program
    /// </summary>
    [Parameter]
    public long ProgramReadingID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetProgramReadingRangesAsync().ConfigureAwait(true);
    }

    private async Task GetProgramReadingRangesAsync()
    {
        _rangeData.ReadingRange = new ReadingRangeModel { ProgramReadingID = ProgramReadingID };
        await SendServiceRequestAsync(new ReadingService(AppState.webEssentials).SyncProgramReadingRangesFromServerAsync(_rangeData, CancellationToken.None), _rangeData).ConfigureAwait(true);
        if (_rangeData.ErrCode == ErrorCode.OK)
        {
            _actionButtons = new()
            {
                new()
                {
                    ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                    ButtonAction = async () => { await OnCancelClickAsync(); }
                }
            };
        }
        _isDataFetched = true;
    }

    private async Task OnCancelClickAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private static List<TableDataStructureModel> GenerateTableStructure() => new()
    {
        new TableDataStructureModel{ DataField= nameof(ReadingRangeModel.ReadingRangeID), IsKey= true, IsSearchable= false, IsHidden= true, IsSortable= false},
        new TableDataStructureModel{ DataField= nameof(ReadingRangeModel.Reading), DataHeader= ResourceConstants.R_READING_TYPE_KEY},
        new TableDataStructureModel{ DataField= nameof(ReadingRangeModel.GenderString), DataHeader= ResourceConstants.R_GENDER_KEY},
        new TableDataStructureModel{ DataField= nameof(ReadingRangeModel.AgeGroupString), DataHeader= ResourceConstants.R_AGE_GROUPS_KEY},
        new TableDataStructureModel{ DataField= nameof(ReadingRangeModel.AbsoluteMinValue), DataHeader= ResourceConstants.R_ABSOLUTE_MIN_VALUE_KEY},
        new TableDataStructureModel{ DataField= nameof(ReadingRangeModel.AbsoluteMaxValue), DataHeader= ResourceConstants.R_ABSOLUTE_MAX_VALUE_KEY},
        new TableDataStructureModel{ DataField= nameof(ReadingRangeModel.NormalMinValue), DataHeader= ResourceConstants.R_IDEAL_MIN_VALUE_KEY},
        new TableDataStructureModel{ DataField= nameof(ReadingRangeModel.NormalMaxValue), DataHeader= ResourceConstants.R_IDEAL_MAX_VALUE_KEY},
    };

    private void OnAddEditClick(ReadingRangeModel rangeData)
    {
        Success = Error = string.Empty;
        _readingRangeID = rangeData == null ? 0 : rangeData.ReadingRangeID;
        _showAddEditPage = true;
    }

    private async Task OnAddEditClosedAsync(string message)
    {
        Success = Error = string.Empty;
        _showAddEditPage = false;
        _readingRangeID = 0;
        if (message == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = message;
            await GetProgramReadingRangesAsync().ConfigureAwait(true);
        }
        else
        {
            Error = message;
        }
    }
}