using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class QuestionScoresPage : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO();
    private long _questionID;
    private bool _showAddEditPage;

    /// <summary>
    /// Id of the questionnaire
    /// </summary>
    [Parameter]
    public long QuestionnaireID { get; set; }

    /// <summary>
    /// Callback for save click
    /// </summary>
    [Parameter]
    public EventCallback<Tuple<bool, long, bool>> SaveNextClicked { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _questionnaireData.Question = new QuestionnaireQuestionModel { QuestionnaireID = QuestionnaireID };
        await GetDataAsync(false);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
        }
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(QuestionScoreModel.QuestionID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(QuestionScoreModel.OptionText),DataHeader=ResourceConstants.R_QUESTION_KEY,IsHtmlTag=true },
            new TableDataStructureModel{DataField=nameof(QuestionScoreModel.InstructionText),DataHeader = ResourceConstants.R_QUESTION_TYPE_KEY },
            new TableDataStructureModel{DataField=nameof(QuestionScoreModel.Value1),DataHeader=ResourceConstants.R_NUMBER_OF_SCORES_KEY },
        };
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.QuestionScoreAddEdit.ToString()).ConfigureAwait(true);
    }

    private void OnAddEditClick(QuestionScoreModel question)
    {
        Success = Error = string.Empty;
        _questionID = question == null ? 0 : question.QuestionID;
        ShowDetailPage = _showAddEditPage = true;
    }

    private async Task OnSaveAndNextButtonClicked()
    {
        Error = string.Empty;
        if (_questionnaireData.QuestionnaireQuestionScores.Count > 0)
        {
            await SaveNextClicked.InvokeAsync(new Tuple<bool, long, bool>(false, _questionnaireData.QuestionnaireQuestionScores.Count, false));
        }
        else
        {
            Error = ErrorCode.InvalidData.ToString();
        }
    }

    private async Task OnAddEditClosedAsync(string isDataUpdated)
    {
        ShowDetailPage = _showAddEditPage = false;
        _questionID = 0;
        Success = Error = string.Empty;
        if (isDataUpdated == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = isDataUpdated;
            await GetDataAsync(true);
            _isDataFetched = true;
        }
        else
        {
            Error = isDataUpdated;
        }
    }

    private async Task GetDataAsync(bool isRefreshRequest)
    {
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionScoreFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            await SaveNextClicked.InvokeAsync(new Tuple<bool, long, bool>(true, _questionnaireData.QuestionnaireQuestionScores?.Count ?? 0, isRefreshRequest));
        }
    }
}