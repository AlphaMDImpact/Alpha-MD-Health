using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Runtime.Serialization;

namespace AlphaMDHealth.WebClient;

public partial class AnswerPage : BasePage
{
    private QuestionnaireQuestionOptionModel _selectedAnswer = new QuestionnaireQuestionOptionModel { IsActive = true };
    private List<QuestionnaireQuestionOptionModel> _pageOptionDetails = new List<QuestionnaireQuestionOptionModel>();
    private List<ButtonActionModel> _actionData;
    private double? _sequenceNo;
    private int _alreadyGivenSequence;
    private bool _hideConfirmationPopup = true;
    private bool _isAddCase;

    /// <summary>
    /// Questionnaire data
    /// </summary>
    [Parameter]
    public QuestionnaireDTO QuestionnaireData { get; set; }

    /// <summary>
    /// Id of the selected answer
    /// </summary>
    [Parameter]
    public long SelectedAnswerID { get; set; }

    /// <summary>
    /// Language data for tab
    /// </summary>
    [Parameter]
    [DataMember]
    public List<LanguageModel> Languages { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (SelectedAnswerID == 0)
        {
            _isAddCase = true;
            _selectedAnswer.QuestionOptionID = GenericMethods.RandomNegativeNumber;
            foreach (var item in Languages)
            {
                _pageOptionDetails.Add(new QuestionnaireQuestionOptionModel
                {
                    LanguageID = Convert.ToByte(item.LanguageID, CultureInfo.InvariantCulture),
                    LanguageName = item.LanguageName,
                    QuestionOptionID = _selectedAnswer.QuestionOptionID
                });
            }
        }
        else
        {
            _selectedAnswer = QuestionnaireData.QuestionOptions.FirstOrDefault(x => x.QuestionOptionID == SelectedAnswerID);
            _pageOptionDetails = QuestionnaireData.QuestionnaireQuestionOptionDetails.Where(x => x.QuestionOptionID == SelectedAnswerID).ToList();
            _sequenceNo = _selectedAnswer.SequenceNo;
        }
        _alreadyGivenSequence = _selectedAnswer.SequenceNo;
        _isDataFetched = true;
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField = nameof(QuestionnaireQuestionOptionModel.CaptionText),
            ResourceKey = ResourceConstants.R_ANSWER_KEY,
            IsRequired = true
        },
    };

    /// <summary>
    /// Asynchronous callback method for handling pop-up interactions.
    /// </summary>
    /// <param name="id">The identifier associated with the pop-up action.</param>
    private async Task PopUpCallbackAsync(object id)
    {
        if (id != null)
        {
            _hideConfirmationPopup = true;
            Success = Error = string.Empty;
            if (Convert.ToInt64(id) == 1)
            {
                _selectedAnswer.IsActive = false;
                SaveData();
                await OnClose.InvokeAsync(ResourceConstants.R_DELETE_ACTION_KEY);
            }
        }
    }

    /// <summary>
    /// Handles the click event for the removal action.
    /// </summary>
    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel {  ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel {  ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task OnCancelAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    /// <summary>
    /// Handles the asynchronous click event for the save button.
    /// </summary>
    private async Task OnSaveButtonClickedAsync()
    {
        if (IsValid())
        {
            var seqNo = Convert.ToInt32(_sequenceNo, CultureInfo.InvariantCulture);
            if (QuestionnaireData.QuestionOptions.Any(x => x.SequenceNo == seqNo && x.IsActive && _alreadyGivenSequence != seqNo))
            {
                Error = ResourceConstants.R_SEQUENCE_NO_ERROR_KEY;
            }
            else
            {
                SaveData();
                await OnClose.InvokeAsync(ErrorCode.OK.ToString());
            }
        }
    }

    /// <summary>
    /// Saves data related to the selected answer.
    /// </summary>
    private void SaveData()
    {
        _selectedAnswer.SequenceNo = Convert.ToInt32(_sequenceNo, CultureInfo.InvariantCulture);
        _selectedAnswer.CaptionText = _pageOptionDetails.FirstOrDefault(x => x.LanguageID == AppState.SelectedLanguageID).CaptionText;
        if (SelectedAnswerID != 0)
        {
            QuestionnaireData.QuestionOptions.Remove(QuestionnaireData.QuestionOptions.Find(x => x.QuestionOptionID == _selectedAnswer.QuestionOptionID));
            QuestionnaireData.QuestionnaireQuestionOptionDetails.RemoveAll(x => x.QuestionOptionID == _selectedAnswer.QuestionOptionID);
        }
        QuestionnaireData.QuestionOptions.AddRange(new List<QuestionnaireQuestionOptionModel> { _selectedAnswer });
        QuestionnaireData.QuestionnaireQuestionOptionDetails.AddRange(_pageOptionDetails);
    }
}