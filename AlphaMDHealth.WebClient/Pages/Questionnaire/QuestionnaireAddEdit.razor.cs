using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class QuestionnaireAddEdit : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO { RecordCount = -1 };
    private List<ButtonActionModel> _popupActions;
    private List<OptionModel> _getQuestionnaireOptions;
    private bool _hidePublishUnpublishConfirmation = true;
    private long _questionnaireID;
    private string _selectedStep;

    /// <summary>
    /// Questionnaire ID parameter
    /// </summary>
    [Parameter]
    public long QuestionnaireID
    {
        get
        {
            return _questionnaireID;
        }
        set
        {
            if (_questionnaireID != value)
            {
                _questionnaireID = value;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _questionnaireData.Questionnaire = new QuestionnaireModel { QuestionnaireID = QuestionnaireID };
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnairesFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            SetOptions();
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
        }
    }

    private void OnPublishButtonClick()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel {  ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel {  ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hidePublishUnpublishConfirmation = false;
    }

    private async Task OnPublishUnpublishActionClick(object id)
    {
        if (id != null)
        {
            _hidePublishUnpublishConfirmation = true;
            Success = Error = string.Empty;
            if (Convert.ToInt64(id) == 1)
            {
                await SaveQuestionnaireStatusAsync();
                StateHasChanged();
            }
        }
    }

    private async Task SaveQuestionnaireStatusAsync()
    {
        _questionnaireData.Questionnaire.IsPublished = !_questionnaireData.Questionnaire.IsPublished;
        _questionnaireData.Questionnaire.QuestionnaireID = _questionnaireID;
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncPublishQuestionnaireToServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            Success = _questionnaireData.ErrCode.ToString();
        }
        else
        {
            _questionnaireData.Questionnaire.IsPublished = !_questionnaireData.Questionnaire.IsPublished;
            Error = _questionnaireData.ErrCode.ToString();
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnAccordianOptionChnged(object selectedQuestionnaire)
    {
        AppState.Loader.ShowLoader(true);
        if (selectedQuestionnaire is string selectedMenu)
        {
            var selectedVal = _selectedStep;
            if (_getQuestionnaireOptions.Any(x => x.OptionID.ToString() == selectedMenu && !x.IsDisabled))
            {
                _selectedStep = selectedMenu;
            }
            else
            {
                _selectedStep = string.Empty;
                _selectedStep = selectedVal;
            }
        }
        AppState.Loader.ShowLoader(false);
    }

    private void OnSaveAndNextInvoked(Tuple<bool, long, bool> result, long currentStep)
    {
        if (currentStep == 1)
        {
            _questionnaireID = result.Item2;
        }

        var nextStep = currentStep + 1;
        var nextStepOption = _getQuestionnaireOptions.FirstOrDefault(x => x.OptionID == nextStep);
        if (!result.Item3 && ((result.Item1 && nextStepOption.IsDisabled && result.Item2 > 0) || !result.Item1))
        {
            _selectedStep = nextStep.ToString();
        }
        nextStepOption.IsDisabled = result.Item2 < 1;
    }

    public void SetOptions()
    {
        _getQuestionnaireOptions = new List<OptionModel>()
        {
            CreateOption(1, AppPermissions.QuestionnaireAddEdit, false),
            CreateOption(2, AppPermissions.QuestionsView, true),
            CreateOption(3, AppPermissions.QuestionnaireQuestionsView, true),
            CreateOption(4, AppPermissions.SubscalesView, true),
            CreateOption(5, AppPermissions.QuestionScoresView, true),
            CreateOption(6, AppPermissions.SubscalesScoreView, true),
        };
        _selectedStep = _getQuestionnaireOptions.FirstOrDefault().OptionID.ToString();
    }

    private OptionModel CreateOption(long id, AppPermissions featureCode, bool isDisabled)
    {
        string featureText = LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, featureCode.ToString());
        return new OptionModel { OptionID = id, OptionText = $"{id}. {featureText}", GroupName = featureCode.ToString(), IsDisabled = isDisabled };
    }

    private bool IsPublishUnPublishAllowed()
    {
        return _questionnaireID > 0
            && LibPermissions.HasPermission(_questionnaireData.FeaturePermissions, AppPermissions.QuestionnairePublish.ToString())
            && _getQuestionnaireOptions.Where(x => x.OptionID < 5).All(x => !x.IsDisabled);
    }
}