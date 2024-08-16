using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class QuestionsPage : BasePage
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
    /// Callback for save button click
    /// </summary>
    [Parameter]
    public EventCallback<Tuple<bool, long, bool>> SaveNextClicked { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _questionnaireData.Question = new QuestionnaireQuestionModel { QuestionnaireID = QuestionnaireID };
        await GetDataAsync(false).ConfigureAwait(true);
    }

    private async Task GetDataAsync(bool isRefreshRequest)
    {
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnaireQuestionsFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            await SaveNextClicked.InvokeAsync(new Tuple<bool, long, bool>(true, _questionnaireData.Questions?.Count ?? 0, isRefreshRequest));
        }
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(QuestionnaireQuestionModel.QuestionID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(QuestionnaireQuestionModel.IsStartingQuestionValue),DataHeader=ResourceConstants.R_QUESTION_KEY,IsHtmlTag=true },
            new TableDataStructureModel{DataField=nameof(QuestionnaireQuestionModel.CaptionText),DataHeader=ResourceConstants.R_QUESTION_TYPE_KEY },
            new TableDataStructureModel{DataField=nameof(QuestionnaireQuestionModel.InstructionsText),DataHeader=ResourceConstants.R_IS_REQUIRED_KEY},
            new TableDataStructureModel{DataField=nameof(QuestionnaireQuestionModel.AddToMedicalHistory),DataHeader=ResourceConstants.R_ADD_TO_MEDICAL_HISTORY_KEY },
        };
    }

    private void OnAddEditClick(QuestionnaireQuestionModel question)
    {
        Success = Error = string.Empty;
        _questionID = question == null ? 0 : question.QuestionID;
        ShowDetailPage = _showAddEditPage = true;
    }

    private async Task OnAddEditClosedAsync(string message)
    {
        ShowDetailPage = _showAddEditPage = false;
        _questionID = 0;
        Success = Error = string.Empty;
        if (message == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = message;
            await GetDataAsync(true).ConfigureAwait(true);
        }
        else
        {
            Error = message;
        }
    }

    private async Task OnSaveAndNextButtonClicked()
    {
        Error = Success = string.Empty;
        if (_questionnaireData.Questions.Count > 0)
        {
            await SaveNextClicked.InvokeAsync(new Tuple<bool, long, bool>(false, _questionnaireData.Questions.Count, false));
        }
        else
        {
            Error = ErrorCode.InvalidData.ToString();
        }
    }
}