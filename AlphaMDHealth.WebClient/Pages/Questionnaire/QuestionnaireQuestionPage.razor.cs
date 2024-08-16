using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class QuestionnaireQuestionPage : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO { RecordCount = -1 };
    public List<QuestionConditionModel> _numericConditions = new List<QuestionConditionModel>();
    private QuestionnaireService _questionnaireService;
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private bool _isComponentValid = true;
    private bool _isStartingQuestion;
    private bool _isEditable;

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
        _questionnaireService = new QuestionnaireService(AppState.webEssentials);
        _questionnaireData.Question = new QuestionnaireQuestionModel { QuestionnaireID = QuestionnaireID, QuestionID = QuestionID };
        await SendServiceRequestAsync(_questionnaireService.SyncQuestionConditionsFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            _questionnaireData.Questionnaire = new QuestionnaireModel { QuestionnaireID = QuestionnaireID };
            if (_questionnaireData.QuestionConditions.Count >= 2)
            {
                _numericConditions = _questionnaireData.QuestionConditions.Where(x => x.Value1.ToString() != Constants.TENTHOUSAND && x.Value1.ToString() != Constants.FIFTEENTHOUSAND).ToList();
            }
            _isStartingQuestion = _questionnaireData.Question.IsStartingQuestion;
            _isEditable = LibPermissions.HasPermission(_questionnaireData.FeaturePermissions, AppPermissions.QuestionnaireQuestionAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
        }
    }

    private async Task OnQuestionTypeChanged(object optionID)
    {
        _questionnaireData.Question.QuestionID = Convert.ToInt64(optionID);
        _questionnaireData.Question.QuestionnaireID = QuestionnaireID;
        UnregisterExistingControls();
        await SendServiceRequestAsync(_questionnaireService.SyncQuestionConditionsFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            if (_questionnaireData.QuestionConditions.Count == 2)
            {
                _numericConditions = _questionnaireData.QuestionConditions.Where(x => x.Value1.ToString() != Constants.TENTHOUSAND && x.Value1.ToString() != Constants.FIFTEENTHOUSAND).ToList();
            }
        }
    }

    private void OnQuestionChanged(object value, QuestionConditionModel condition)
    {
        if (value != null && condition != null)
        {
            condition.TargetQuestionID = string.IsNullOrEmpty(value.ToString()) ? (long?)null : Convert.ToInt64(value);
        }
    }
    private string getIfControlValue()
    {
        var result = _controls.FirstOrDefault(x => x.Key == string.Concat(Constants.IF_BLANK, ResourceConstants.R_ACTION_KEY));
        if (!result.Equals(default(KeyValuePair<string, object>)))
        {
            return (result.Value as AmhDropdownControl).Value;
        }
        return string.Empty;
    }

    private string getElseControlValue()
    {
        var result = _controls.FirstOrDefault(x => x.Key == string.Concat(Constants.ELSE, ResourceConstants.R_ACTION_KEY));
        if (!result.Equals(default(KeyValuePair<string, object>)))
        {
            return (result.Value as AmhDropdownControl).Value;
        }
        return string.Empty;
    }

    private List<OptionModel> CreateOptionList(string targetQuestionID, string selectedValue)
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
            if (string.IsNullOrWhiteSpace(selectedValue))
            {
                SetSelectedOption(targetQuestionID, optionList);
            }
            else
            {
                SetSelectedOption(selectedValue, optionList);
            }
        }
        return optionList;
    }

    private void SetSelectedOption(string targetQuestionID, List<OptionModel> optionList)
    {
        var selectedOption = optionList.FirstOrDefault(x => x.OptionID.ToString() == targetQuestionID);
        if (selectedOption != null)
        {
            selectedOption.IsSelected = true;
        }
    }

    private void UnregisterExistingControls()
    {
        RemoveControlContainsKey(ResourceConstants.R_JUMP_TO_QUESTION_KEY);
        RemoveControlContainsKey(ResourceConstants.R_VALUE_BLANK_CONDITION_KEY);
        RemoveControlContainsKey(ResourceConstants.R_ADD_MORE_CONDITION_KEY);
        RemoveControlContainsKey(ResourceConstants.R_CONDITION_VALUE_1_KEY);
        RemoveControlContainsKey(ResourceConstants.R_ACTION_KEY);
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Error = string.Empty;
        if (IsValid())
        {
            bool isValid = true;
            switch (_questionnaireData.Question.QuestionTypeID)
            {
                case QuestionType.NumericQuestionKey:
                case QuestionType.DateQuestionKey:
                case QuestionType.DateTimeQuestionKey:
                case QuestionType.TimeQuestionKey:
                case QuestionType.HorizontalSliderQuestionKey:
                case QuestionType.VerticalSliderQuestionKey:
                    if (IsDuplicateRange() || IsRangeOverlapping())
                    {
                        isValid = false;
                    }
                    else
                    {
                        _questionnaireData.QuestionConditions = new List<QuestionConditionModel>();
                        _questionnaireData.QuestionConditions.AddRange(_numericConditions);
                        long ifCondition = string.IsNullOrEmpty((_controls.FirstOrDefault(x => x.Key == string.Concat(Constants.IF_BLANK, ResourceConstants.R_ACTION_KEY)).Value as AmhDropdownControl).Value) ? -1 : Convert.ToInt64((_controls.FirstOrDefault(x => x.Key == string.Concat(Constants.IF_BLANK, ResourceConstants.R_ACTION_KEY)).Value as AmhDropdownControl).Value);
                        long elseCondition = string.IsNullOrEmpty((_controls.FirstOrDefault(x => x.Key == string.Concat(Constants.ELSE, ResourceConstants.R_ACTION_KEY)).Value as AmhDropdownControl).Value) ? -1 : Convert.ToInt64((_controls.FirstOrDefault(x => x.Key == string.Concat(Constants.ELSE, ResourceConstants.R_ACTION_KEY)).Value as AmhDropdownControl).Value);
                        UpdateOrAddCondition(Constants.TENTHOUSAND, ifCondition);
                        UpdateOrAddCondition(Constants.FIFTEENTHOUSAND, elseCondition);
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

    private void UpdateOrAddCondition(string optionID, long targetID)
    {
        long? target = targetID == -1 ? (long?)null : targetID;

        var condition = _questionnaireData.QuestionConditions.FirstOrDefault(x => x.Value1.ToString() == optionID);
        if (condition != null)
        {
            condition.TargetQuestionID = target;
        }
        else
        {
            _questionnaireData.QuestionConditions.Add(new QuestionConditionModel
            {
                Value1 = Convert.ToDouble(optionID),
                Value2 = 0,
                TargetQuestionID = target,
            });
        }
    }
    private bool IsDuplicateRange()
    {
        var seenConditions = new HashSet<(double Value1, double Value2)>();
        foreach (var condition in _numericConditions)
        {
            var key = (condition.Value1, condition.Value2);
            if (seenConditions.Contains(key))
            {
                // Duplicate found
                Error = ResourceConstants.R_DUPLICATE_DATA_KEY;
                return true;
            }
            seenConditions.Add(key);
        }
        return false;
    }

    private bool IsRangeOverlapping()
    {
        for (int i = 0; i < _numericConditions.Count; i++)
        {
            var condition1 = _numericConditions[i];
            long start1 = Convert.ToInt64(condition1.Value1);
            long end1 = Convert.ToInt64(condition1.Value2);

            for (int j = i + 1; j < _numericConditions.Count; j++)
            {
                var condition2 = _numericConditions[j];
                long start2 = Convert.ToInt64(condition2.Value1);
                long end2 = Convert.ToInt64(condition2.Value2);

                // Check for overlap
                if (!(end1 < start2 || start1 > end2))
                {
                    Error = ResourceConstants.R_OVERLAP_RANGE_KEY;
                    return true;
                }
            }
        }
        return false;
    }

    private async Task SaveDataAsync()
    {
        _questionnaireData.Question.IsActive = true;
        _questionnaireData.Question.IsStartingQuestion = _isStartingQuestion;
        await SaveQuestionAsync();
    }

    private async Task SaveQuestionAsync()
    {
        QuestionnaireDTO questionnaire = new QuestionnaireDTO
        {
            Question = _questionnaireData.Question,
            QuestionConditions = _questionnaireData.QuestionConditions
        };
        questionnaire.Question.QuestionnaireID = QuestionnaireID;
        await SendServiceRequestAsync(_questionnaireService.SyncQuestionnaireConditionsToServerAsync(questionnaire), questionnaire).ConfigureAwait(true);
        if (questionnaire.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(questionnaire.ErrCode.ToString());
        }
        else
        {
            Error = questionnaire.ErrCode.ToString();
        }
    }

    private void OnDeleteButtonClicked()
    {
        _actionData = new List<ButtonActionModel>
        {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
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
    }

    private string GetUniqeID(int optionID)
    {
        return string.Concat(AppPermissions.QuestionnaireQuestionAddEdit.ToString(), Constants.ADD_BETWEEN_CONDITION, optionID);
    }

    private void OnAddClicked()
    {
        if (IsValid())
        {
            _numericConditions.Add(new QuestionConditionModel
            {
                Value1 = 0,
                Value2 = 0,
                TargetQuestionID = 0,
                OptionID = _numericConditions.Count
            });
        }
    }

    private async Task OnDeleteConfirmationPopUpClickedAsync(object value)
    {
        _hideConfirmationPopup = true;
        ShowDetailPage = false;
        if (value != null)
        {
            if (Convert.ToInt64(value) == 1)
            {
                _questionnaireData.Question.IsActive = false;
                await SaveQuestionAsync();
            }
        }
    }

    private List<OptionModel> FirstQuestionOption()
    {
        return new List<OptionModel> {
           new OptionModel { OptionID = 1,
               OptionText = LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_IS_STARTING_QUESTION_KEY),
               IsSelected = _isStartingQuestion
           }
        };
    }
}