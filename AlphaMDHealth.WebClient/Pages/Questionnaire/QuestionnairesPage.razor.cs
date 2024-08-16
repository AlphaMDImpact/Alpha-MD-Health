using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class QuestionnairesPage : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO { Questionnaire = new QuestionnaireModel() };
    private long _questionnaireID;
    private bool _isDashboardView;

    /// <summary>
    /// Questionnaire ID parameter
    /// </summary>
    [Parameter]
    public long QuestionnaireID
    {
        get { return _questionnaireID; }
        set
        {
            if (_questionnaireID != value)
            {
                if (_questionnaireData.RecordCount > 0 || _questionnaireID == 0)
                {
                    _questionnaireData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _questionnaireID = value;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
       await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        if (Parameters?.Count > 0)
        {
            _questionnaireData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(QuestionnaireDTO.RecordCount)));
        }
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnairesFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        _isDashboardView = _questionnaireData.RecordCount > 0;
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var columns = new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(QuestionnaireModel.QuestionnaireID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(QuestionnaireModel.CaptionText),DataHeader=ResourceConstants.R_QUESTIONNAIRE_TITLE_TEXT_KEY},
        };
        if (_questionnaireData.RecordCount == 0)
        {
            columns.Add(new TableDataStructureModel { DataField = nameof(QuestionnaireModel.CreatedOn), DataHeader = ResourceConstants.R_CREATED_ON_TEXT_KEY, Formatter = _questionnaireData.Questionnaire.CreatedOn });
            columns.Add(new TableDataStructureModel { DataField = nameof(QuestionnaireModel.NoOfQuestions), DataHeader = ResourceConstants.R_QUSETIONS_TEXT_KEY });
            columns.Add(new TableDataStructureModel { DataField = nameof(QuestionnaireModel.NoOfSubscales), DataHeader = ResourceConstants.R_SUBSCALES_TEXT_KEY });
        }
        columns.Add(new TableDataStructureModel { DataField = nameof(QuestionnaireModel.PublisheUnpublishText), DataHeader = ResourceConstants.R_STATUS_KEY, IsBadge = true, BadgeFieldType = nameof(QuestionnaireModel.StatusStyle) });
        return columns;
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.QuestionnairesView.ToString()).ConfigureAwait(true);
    }

    private async Task OnRowClickAsync(QuestionnaireModel questionData)
    {
        Success = Error = string.Empty;
        if (_questionnaireData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.QuestionnairesView.ToString(), (questionData == null ? 0 : questionData.QuestionnaireID).ToString()).ConfigureAwait(false);
        }
        else
        {
            ShowDetailPage = true;
            _questionnaireID = questionData == null ? 0 : questionData.QuestionnaireID;
        }
    }

    private async Task OnQuestionnaireClosedAsync(string result)
    {
        ShowDetailPage = false;
        _questionnaireID = 0;
        Success = Error = string.Empty;
        if (string.IsNullOrWhiteSpace(result) || result == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = result;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = result;
            StateHasChanged();
        }
    }
}