using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class QuestionScorePage : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO { RecordCount = -1 };
    public List<QuestionScoreModel> _numericConditions = new List<QuestionScoreModel>();
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private bool _isValueChanged;
    private double? _selectedIfBlankAnswer = 0;
    private double? _selectedElseAnswer = 0 ;
    public List<int> _scoreIndex = new List<int>();
    private bool _isEditable = false;

    /// <summary>
    /// Id of the questionnaire
    /// </summary>
    [Parameter]
    public long QuestionnaireID { get; set; }

    /// <summary>
    /// Id of the question
    /// </summary>
    [Parameter]
    public long QuestionID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _questionnaireData.Question = new QuestionnaireQuestionModel { QuestionnaireID = QuestionnaireID, QuestionID = QuestionID };

        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionScoreFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            _questionnaireData.Questionnaire = new QuestionnaireModel { QuestionnaireID = QuestionnaireID };
            _isEditable = LibPermissions.HasPermission(_questionnaireData.FeaturePermissions, AppPermissions.QuestionScoreAddEdit.ToString());
            _isDataFetched = true;
            if (_questionnaireData.QuestionScores.Count > 2)
            {
                _numericConditions = _questionnaireData.QuestionScores.Where(x => x.Value1 != 0 && x.Value1 != -1).ToList();
                GetValueForIfElse();
            }
            else if (_questionnaireData.QuestionScores.Count == 2)
            {
                GetValueForIfElse();
            }
            if (_questionnaireData.Question.QuestionID == 0)
            {
                _questionnaireData.Question.QuestionnaireID = QuestionnaireID;
            }
        }
        else
        {
            await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
        }
    }
    private void GetValueForIfElse()
    {
        var question = _questionnaireData.Question;
        var questionID = question.QuestionID;
        var questionTypeID = question.QuestionTypeID;

        // Define a set of question types to check against
        var targetQuestionTypes = new HashSet<QuestionType>{
            QuestionType.NumericQuestionKey,
            QuestionType.DateQuestionKey,
            QuestionType.DateTimeQuestionKey,
            QuestionType.TimeQuestionKey,
            QuestionType.HorizontalSliderQuestionKey,
            QuestionType.VerticalSliderQuestionKey
        };

        if (questionID > 0 && targetQuestionTypes.Contains(questionTypeID))
        {
            var questionScores = _questionnaireData.QuestionScores;
            _selectedIfBlankAnswer = GetScoreValue(questionScores, 0);
            _selectedElseAnswer = GetScoreValue(questionScores, -1);
        }
    }
    private double GetScoreValue(IEnumerable<QuestionScoreModel> questionScores, int value1)
    {
        return Convert.ToDouble(questionScores.FirstOrDefault(x => x.Value1 == value1)?.ScoreValue ?? 0);
    }

    private void GetValueChanged(bool isAddClicked)
    {
        if (isAddClicked)
        {
            return;
        }
        _scoreIndex.Clear();
        var data = _questionnaireData.QuestionScores.Where(x => x.ScoreValue > 0).ToList();
        foreach (var item in data)
        {
            _scoreIndex.Add(_questionnaireData.QuestionScores.IndexOf(item));
        }
    }

    private async Task OnQuestionTypeChanged(object optionID)
    {
        bool isAddClicked = _questionnaireData.Question.QuestionID == 0;
        _questionnaireData.Question.QuestionID = Convert.ToInt64(optionID);
        _questionnaireData.Question.QuestionnaireID = QuestionnaireID;
        RemoveExistingControls();
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionScoreFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            GetValueChanged(isAddClicked);
            _numericConditions.Clear();
            if (_questionnaireData.QuestionScores.Count > 2)
            {
                _numericConditions = _questionnaireData.QuestionScores.Where(x => x.Value1 != 0 && x.Value1 != -1).ToList();
            }
            if (_questionnaireData.QuestionScores.Count >= 2)
            {
                GetValueForIfElse();
            }
        }
    }

    private void RemoveExistingControls()
    {
        switch (_questionnaireData.Question.QuestionTypeID)
        {
            case QuestionType.NumericQuestionKey:
                foreach (var conditionData in _numericConditions.Select((value, i) => new { i, value }))
                {
                    var index = conditionData.i;
                    RemoveControlByKey(string.Concat(Constants.ADD_BETWEEN_CONDITION, index));
                }
                break;
            case QuestionType.TextQuestionKey:
            case QuestionType.MultilineTextQuestionKey:
            case QuestionType.DropDownQuestionKey:
            case QuestionType.MultiSelectQuestionKey:
            case QuestionType.SingleSelectQuestionKey:
            case QuestionType.RichTextQuestionKey:
            case QuestionType.FilesAndDocumentQuestionKey:
            case QuestionType.MeasurementQuestionKey:
                foreach (var optionData in _questionnaireData.QuestionScores)
                {
                    RemoveControlByKey(ResourceConstants.R_VALUE_KEY);
                }
                break;
            default:
                break;
        }
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            bool isValid = true;
            switch (_questionnaireData.Question.QuestionTypeID)
            {
                case QuestionType.DropDownQuestionKey:
                case QuestionType.SingleSelectQuestionKey:
                case QuestionType.MultiSelectQuestionKey:
                case QuestionType.TextQuestionKey:
                case QuestionType.MultilineTextQuestionKey:
                case QuestionType.RichTextQuestionKey:
                case QuestionType.FilesAndDocumentQuestionKey:
                case QuestionType.MeasurementQuestionKey:
                    foreach (var optionData in _questionnaireData.QuestionScores)
                    {
                        var control = _controls.FirstOrDefault(x => x.Key == string.Concat(AppPermissions.QuestionScoreAddEdit.ToString(), Constants.TXT_SCORE, optionData.OptionID.ToString().Replace('-', '_'))).Value;
                        if (control != null && control is AmhEntryControl)
                        {
                            string value = (control as AmhEntryControl).Value;
                            UpdateOrAddCondition(optionData.OptionID.ToString(), Convert.ToDouble(value));
                        }
                    }
                    break;
                case QuestionType.NumericQuestionKey:
                case QuestionType.DateQuestionKey:
                case QuestionType.DateTimeQuestionKey:
                case QuestionType.TimeQuestionKey:
                case QuestionType.VerticalSliderQuestionKey:
                case QuestionType.HorizontalSliderQuestionKey:
                    if (IsDuplicateRange() || IsRangeOverlapping())
                    {
                        isValid = false;
                    }
                    else
                    {
                        _questionnaireData.QuestionScores = new List<QuestionScoreModel>();
                        _questionnaireData.QuestionScores.AddRange(_numericConditions);
                        UpdateOrAddCondition(Constants.ZERO, Convert.ToDouble(_selectedIfBlankAnswer));
                        UpdateOrAddCondition(Constants.MINUS_ONE, Convert.ToDouble(_selectedElseAnswer));
                    }
                    break;
                default:
                    break;
            }
            if (isValid)
            {
                await SaveDataAsync();
            }
        }
    }

    private void UpdateOrAddCondition(string optionID, double targetID)
    {
        double optionIdAsDouble = Convert.ToDouble(optionID);
        double target = targetID != Convert.ToDouble(Constants.MINUS_ONE) ? targetID : 0;
        var questionScore = _questionnaireData.QuestionScores.FirstOrDefault(x => x.Value1 == optionIdAsDouble);
        if (questionScore != null)
        {
            questionScore.ScoreValue = target;
        }
        else
        {
            _questionnaireData.QuestionScores.Add(new QuestionScoreModel
            {
                Value1 = optionIdAsDouble,
                Value2 = 0,
                ScoreValue = target,
            });
        }
    }

    private bool IsDuplicateRange()
    {
        List<QuestionScoreModel> newList = new List<QuestionScoreModel>(_numericConditions);
        foreach (var condition in _numericConditions)
        {
            newList.Remove(condition);
            if (newList.Any(range => range.Value1.Equals(condition.Value1) && range.Value2.Equals(condition.Value2)))
            {
                Error = ResourceConstants.R_DUPLICATE_DATA_KEY;
                return true;
            }
        }
        return false;
    }

    private bool IsRangeOverlapping()
    {
        List<QuestionScoreModel> newList = new List<QuestionScoreModel>(_numericConditions);
        foreach (var condition in _numericConditions)
        {
            newList.Remove(condition);
            if (newList.Any(range => Math.Max(Convert.ToInt64(range.Value1), Convert.ToInt64(condition.Value1)) - Math.Min(Convert.ToInt64(range.Value2), Convert.ToInt64(condition.Value2)) <= 0))
            {
                Error = ResourceConstants.R_OVERLAP_RANGE_KEY;
                return true;
            }
        }
        return false;
    }

    private async Task SaveDataAsync()
    {
        _questionnaireData.Question.IsActive = true;
        await SaveQuestionAsync();
    }

    private async Task SaveQuestionAsync()
    {
        QuestionnaireDTO questionnaire = new QuestionnaireDTO
        {
            Question = _questionnaireData.Question,
            QuestionScores = _questionnaireData.QuestionScores
        };
        questionnaire.Question.QuestionnaireID = QuestionnaireID;
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnaireQuestionScoresToServerAsync(questionnaire), questionnaire).ConfigureAwait(true);
        if (questionnaire.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(questionnaire.ErrCode.ToString());
        }
        else
        {
            Error = questionnaire.ErrCode == ErrorCode.DuplicateData ? ResourceConstants.R_SEQUENCE_NO_ERROR_KEY : questionnaire.ErrCode.ToString();
        }
    }

    private void OnDeleteButtonClicked()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel {  ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel {  ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private void OnConditionDelete(int optionID)
    {
        foreach (var _control in _numericConditions.Select((value, i) => new { i, value }))
        {
            string uniqueID = GetUniqeID(_control.i);
            RemoveControlContainsKey(uniqueID);
        }
        _numericConditions.RemoveAt(optionID);
        _isValueChanged = true;
    }

    private string GetUniqeID(int optionID)
    {
        return string.Concat(AppPermissions.QuestionnaireQuestionAddEdit.ToString(), Constants.ADD_BETWEEN_CONDITION, optionID);
    }

    private void OnAddClicked()
    {
        if (IsValid())
        {
            _numericConditions.Add(new QuestionScoreModel
            {
                Value1 = 0,
                Value2 = 0,
                ScoreValue = 0,
            });
        }
    }

    private async Task DeleteQuestionPopUpCallbackAsync(object id)
    {
        if (id != null)
        {
            _hideConfirmationPopup = true;
            Success = Error = string.Empty;
            if (Convert.ToInt64(id) == 1)
            {
                _questionnaireData.Question.IsActive = false;
                await SaveQuestionAsync();
            }
        }
    }

    private List<OptionModel> CreateOptionList(string targetQuestionID)
    {
        List<OptionModel> optionList = new List<OptionModel>();
        foreach (var resource in _questionnaireData.DropDownOptions)
        {
            optionList.Add(new OptionModel
            {
                OptionID = resource.OptionID,
                OptionText = resource.OptionText,
                IsSelected = false
            });
        }
        if (GenericMethods.IsListNotEmpty(_questionnaireData.QuestionConditions))
        {
            if (optionList.FirstOrDefault(x => x.OptionID.ToString() == targetQuestionID) != null)
            {
                optionList.FirstOrDefault(x => x.OptionID.ToString() == targetQuestionID).IsSelected = true;
            }
        }
        return optionList;
    }

    private void UpdateScoreValue(double value, QuestionScoreModel model, bool isCheck)
    {
        if (isCheck)
            model.Value1 = value;
        else
            model.Value2 = value;
    }
}