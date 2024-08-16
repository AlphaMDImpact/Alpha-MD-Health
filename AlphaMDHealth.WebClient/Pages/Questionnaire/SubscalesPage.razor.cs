using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class SubscalesPage : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO { Questionnaire = new QuestionnaireModel(), RecordCount = -2 };
    private bool _isEditable;

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
        _questionnaireData.Questionnaire.QuestionnaireID = Convert.ToInt64(QuestionnaireID);
        if (_questionnaireData.Questionnaire.QuestionnaireID > 0)
        {
            await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnaireSubScaleFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
            if (_questionnaireData.ErrCode == ErrorCode.OK)
            {
                var selected = _questionnaireData.DropDownOptions?.FirstOrDefault(x => x.IsSelected);
                if (selected != null)
                {
                    await SaveNextClicked.InvokeAsync(new Tuple<bool, long, bool>(true, selected.ParentOptionID, false));
                }
                _isEditable = LibPermissions.HasPermission(_questionnaireData.FeaturePermissions, AppPermissions.SubscaleAddEdit.ToString());
                _isDataFetched = true;
                await base.OnInitializedAsync();
                return;
            }
        }
        else
        {
            _questionnaireData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
    }

    /// <summary>
    /// Asynchronously handles the event when the save button is clicked, validates the data, and initiates the process of saving and synchronizing questionnaire subscale data to the server.
    /// </summary>
    /// <returns>An asynchronous task representing the completion of the operation.</returns>
    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            var subscale = _questionnaireData.DropDownOptions?.FirstOrDefault(x => x.IsSelected);

            QuestionnaireDTO questionnaireData = new QuestionnaireDTO
            {
                QuestionnaireSubscaleData = new QuestionnaireSubscaleModel
                {
                    QuestionnaireID = _questionnaireData.Questionnaire.QuestionnaireID,
                    SubscaleID = subscale?.ParentOptionID ?? 0,
                    ScoreTypeID = subscale.GroupName?.ToEnum<QuestionnaireSubscaleScoreType>() ?? default,
                },
            };
            await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnaireSubscaleToServerAsync(questionnaireData, CancellationToken.None), questionnaireData).ConfigureAwait(true);
            if (questionnaireData.ErrCode == ErrorCode.OK)
            {
                Success = questionnaireData.ErrCode.ToString();
                await SaveNextClicked.InvokeAsync(new Tuple<bool, long, bool>(false, questionnaireData.QuestionnaireSubscaleData.SubscaleID, false));
            }
            else
            {
                Error = questionnaireData.ErrCode.ToString();
            }
        }
    }
}