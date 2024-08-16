using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class QuestionnairePage : BasePage
{
    private QuestionnaireDTO _questionnaireData = new QuestionnaireDTO { RecordCount = -1 };
    private bool _isEditable;
    private List<TabDataStructureModel> _dataFormatter;

    /// <summary>
    /// QuestionnaireID parameter
    /// </summary>
    [Parameter]
    public long QuestionnaireID { get; set; }

    /// <summary>
    /// Save And Next Event Callback
    /// </summary>
    [Parameter]
    public EventCallback<Tuple<bool, long, bool>> SaveNextClicked { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _questionnaireData.Questionnaire = new QuestionnaireModel { QuestionnaireID = QuestionnaireID };
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnairesFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            await SaveNextClicked.InvokeAsync(new Tuple<bool, long, bool>(true, _questionnaireData.Questionnaire?.QuestionnaireID ?? 0, false));

            _isEditable = LibPermissions.HasPermission(_questionnaireData.FeaturePermissions, AppPermissions.QuestionnaireAddEdit.ToString());
            _isDataFetched = true;
            _dataFormatter = GetLanguageTabData();
        }
        else
        {
            await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
        }
    }

    private List<TabDataStructureModel> GetLanguageTabData()
    {
        return new List<TabDataStructureModel> {
               new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageHeading), ResourceKey=ResourceConstants.R_QUESTIONNAIRE_TITLE_TEXT_KEY},
               new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageData), ResourceKey=ResourceConstants.R_INSTRUCTIONS_TEXT_KEY }
        };
    } 

    private async Task OnSaveButtonClickedAsync()
    {
        _questionnaireData.ErrCode = ErrorCode.OK;
        Success = Error = string.Empty;
        if (IsValid())
        {
            _questionnaireData.Questionnaire.QuestionnaireTypeID = (short)(_questionnaireData.DropDownOptions?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0);
            _questionnaireData.Questionnaire.DefaultRespondentID = (short)(_questionnaireData.DefaultRespondants?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0);
            _questionnaireData.IsActive = true;
            await SaveQuestionnaireDataAsync();
        }
    }

    private async Task SaveQuestionnaireDataAsync()
    {
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnaireToServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            Success = _questionnaireData.ErrCode.ToString();
            await SaveNextClicked.InvokeAsync(new Tuple<bool, long, bool>(false, _questionnaireData.Questionnaire.QuestionnaireID, false));
        }
        else
        {
            Error = _questionnaireData.ErrCode.ToString();
        }
    }
}