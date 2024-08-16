using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;
public partial class SubscalesScoresPage : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO { QuestionnaireSubscaleData = new QuestionnaireSubscaleModel(), RecordCount = 0 };
    private long _subscaleRange;
    private bool _showAddEditPage;

    /// <summary>
    /// Id of the questionnaire
    /// </summary>
    [Parameter]
    public long QuestionnaireID { get; set; }

    /// <summary>
    /// Id of the subscale
    /// </summary>
    [Parameter]
    public long SubscaleID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _questionnaireData.Questionnaire = new QuestionnaireModel { QuestionnaireID = QuestionnaireID };
        _questionnaireData.Resources = AppState.MasterData.Resources;
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnaireSubScaleFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(QuestionnaireSubscaleRangesModel.SubScaleRangeID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false},
            new TableDataStructureModel{DataField=nameof(QuestionnaireSubscaleRangesModel.MinValue), DataHeader=ResourceConstants.R_MIN_TEXT_KEY },
            new TableDataStructureModel{DataField=nameof(QuestionnaireSubscaleRangesModel.MaxValue), DataHeader=ResourceConstants.R_MAX_TEXT_KEY },
            new TableDataStructureModel{DataField=nameof(QuestionnaireSubscaleRangesModel.PageHeading), DataHeader=ResourceConstants.R_RECOMMENDATION_TEXT_KEY },
        };
    }

    private void OnAddEditClick(QuestionnaireSubscaleRangesModel subscaleRange)
    {
        Success = Error = string.Empty;
        _subscaleRange = subscaleRange == null ? 0 : subscaleRange.SubScaleRangeID;
        ShowDetailPage = _showAddEditPage = true;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = _showAddEditPage = false;
        Success = Error = string.Empty;
        _subscaleRange = 0;
        if (errorMessage == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = errorMessage;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = errorMessage;
        }
    }
}