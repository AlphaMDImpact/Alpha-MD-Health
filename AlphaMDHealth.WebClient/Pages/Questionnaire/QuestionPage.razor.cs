using System.Globalization;
using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Primitives;

namespace AlphaMDHealth.WebClient;

public partial class QuestionPage : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO { RecordCount = -1 };
    private readonly List<LanguageModel> _languages = new List<LanguageModel>();
    private IList<TabDataStructureModel> _dataFormatter;
    private bool _shouldShowAnswersList;
    private bool _shouldShowAddEditAnswer;
    private double? _maxValueString;
    private double? _minValueString;
    private double? _sliderValueString;
    private bool _hideConfirmationPopup = true;
    private bool _showConfirmationPopup = true;
    private List<ButtonActionModel> _actionData;
    private long _selectedAnswerID;
    private bool _isEditable = false;
    private bool _isDashboardView;
    private string OptionSelected;

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
        await GetDataAsync().ConfigureAwait(true);
        if (_questionnaireData.ErrCode == ErrorCode.OK)
        {
            OnQuestionTypeChanged(-2);
            _questionnaireData.Questionnaire = new QuestionnaireModel { QuestionnaireID = QuestionnaireID };
            _isEditable = LibPermissions.HasPermission(_questionnaireData.FeaturePermissions, AppPermissions.QuestionAddEdit.ToString());
            if (_questionnaireData.Question.QuestionID == 0)
            {
                _questionnaireData.Question.QuestionnaireID = QuestionnaireID;
            }
            else
            {
                _maxValueString = Convert.ToDouble(_questionnaireData.Question.MaxValue.ToString());
                _minValueString = Convert.ToDouble(_questionnaireData.Question.MinValue.ToString());
                _sliderValueString = Convert.ToDouble(_questionnaireData.Question.SliderSteps.ToString());
                if (_questionnaireData.Question.QuestionTypeID == QuestionType.SingleSelectQuestionKey || _questionnaireData.Question.QuestionTypeID == QuestionType.MultiSelectQuestionKey)
                {
                    _shouldShowAnswersList = true;
                }
            }
            #region LanguageTab Source
            foreach (var item in _questionnaireData.QuestionDetails)
            {
                _languages.Add(new LanguageModel
                {
                    LanguageID = item.LanguageID,
                    LanguageName = item.LanguageName
                });
            }
            #endregion
            _questionnaireData.QuestionOptions ??= new List<QuestionnaireQuestionOptionModel>();
            _questionnaireData.QuestionnaireQuestionOptionDetails ??= new List<QuestionnaireQuestionOptionModel>();
            _dataFormatter = GetDataFormatterForPageDetails(_questionnaireData.Question.QuestionTypeID == QuestionType.RichTextQuestionKey);
        }
        else
        {
            await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
        }

    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionnaireQuestionsFromServerAsync(_questionnaireData, CancellationToken.None), _questionnaireData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    /// <summary>
    /// Handles the event when the question type is changed, updating the display and resetting controls if needed.
    /// </summary>
    /// <param name="optionID">The ID of the selected option.</param>
    private void OnQuestionTypeChanged(object optionID)
    {
        _shouldShowAnswersList = false;
        _shouldShowAddEditAnswer = false;

        if (optionID != null && !string.IsNullOrWhiteSpace(optionID.ToString()))
        {
            OptionSelected = GetSelected(Convert.ToInt64(optionID));
            _questionnaireData.Question.QuestionTypeID = OptionSelected.ToEnum<QuestionType>();

            if (Convert.ToInt64(optionID) != -2)
            {
                _dataFormatter = GetDataFormatterForPageDetails(_questionnaireData.Question.QuestionTypeID == QuestionType.RichTextQuestionKey);

                if (_questionnaireData.Question.QuestionTypeID == QuestionType.RichTextQuestionKey)
                {
                    ResetPageDetails(ResourceConstants.R_QUESTION_KEY);
                    ResetPageDetails(ResourceConstants.R_ANSWER_PLACEHOLDER_TEXT_KEY);
                    ResetPageDetails(ResourceConstants.R_QUESTION_INSTRUCTIONS_TEXT_KEY);
                    ClearControls();
                }
                else
                {
                    ResetPageDetails(ResourceConstants.R_READ_ONLY_QUESTION_CONTENT_KEY);
                }
            }

            if (_questionnaireData.Question.QuestionTypeID == QuestionType.SingleSelectQuestionKey
                || _questionnaireData.Question.QuestionTypeID == QuestionType.MultiSelectQuestionKey)
            {
                _shouldShowAnswersList = true;
            }
        }
        RemoveExistingControls();
    }


    /// <summary>
    /// Removes existing controls based on the current question type and answer options requirement.
    /// </summary>
    private void RemoveExistingControls()
    {
        if (_questionnaireData.Question.QuestionTypeID != default)
        {
            if (IsAnswerOptionsNeeded())
            {
                RemoveControlByKey(ResourceConstants.R_MIN_TEXT_KEY);
                RemoveControlByKey(ResourceConstants.R_MAX_TEXT_KEY);
                RemoveControlByKey(ResourceConstants.R_SLIDER_STEPS_KEY);
                if (_questionnaireData.Question.QuestionTypeID == QuestionType.RichTextQuestionKey
                    || _questionnaireData.Question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey
                    || _questionnaireData.Question.QuestionTypeID == QuestionType.MeasurementQuestionKey)
                {
                    _shouldShowAnswersList = false;
                }
                else
                {
                    _shouldShowAnswersList = true;
                }
            }
            else
            {
                if (!ShouldShowSlider())
                    RemoveControlByKey(ResourceConstants.R_SLIDER_STEPS_KEY);
            }
        }
    }

    private bool ShouldShowSlider()
    {
        return _questionnaireData.Question.QuestionTypeID == QuestionType.HorizontalSliderQuestionKey
            || _questionnaireData.Question.QuestionTypeID == QuestionType.VerticalSliderQuestionKey;
    }

    /// <summary>
    /// Retrieves the selected resource key based on the provided optionID.
    /// </summary>
    /// <param name="optionID">The ID of the selected option.</param>
    private string GetSelected(long optionID)
    {
        return _questionnaireData.Resources.FirstOrDefault(x => x.ResourceKeyID == optionID)?.ResourceKey ?? string.Empty;
    }

    /// <summary>
    /// Handles the event when the answer pop-up is closed.
    /// </summary>
    /// <param name="isDataUpdated">A string indicating whether the data was updated.</param>
    private void AnswerPopUpClosedEventCallback(string isDataUpdated)
    {
        _shouldShowAddEditAnswer = false;
        _shouldShowAnswersList = true;
        _selectedAnswerID = 0;
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    /// <summary>
    /// Handles the asynchronous event when the save button is clicked.
    /// </summary>
    private async Task OnSaveButtonClickedAsync()
    {
        Error = string.Empty;
        if (IsValid())
        {
            bool isValid = true;
            if (IsAnswerOptionsNeeded() && _questionnaireData.Question.QuestionTypeID != QuestionType.RichTextQuestionKey
                                      && _questionnaireData.Question.QuestionTypeID != QuestionType.FilesAndDocumentQuestionKey
                                      && _questionnaireData.Question.QuestionTypeID != QuestionType.MeasurementQuestionKey
                                      && ((_questionnaireData.Question.QuestionTypeID == QuestionType.SingleSelectQuestionKey
                                      || _questionnaireData.Question.QuestionTypeID == QuestionType.MultiSelectQuestionKey
                                      || _questionnaireData.Question.QuestionTypeID == QuestionType.DropDownQuestionKey)
                                      && _questionnaireData.QuestionOptions.Count(x => x.IsActive) < 2))
            {
                Error = ErrorCode.InvalidData.ToString();
                isValid = false;
            }
            else
            {
                if (_questionnaireData.Question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey)
                {
                    _questionnaireData.Question.CategoryID = (short)(_questionnaireData.DefaultRespondants?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0);
                }
                else if (_questionnaireData.Question.QuestionTypeID == QuestionType.MeasurementQuestionKey)
                {
                    _questionnaireData.Question.CategoryID = (short)(_questionnaireData.ReadingsOptions?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0);

                }
                else if ((_questionnaireData.Question.QuestionTypeID == QuestionType.HorizontalSliderQuestionKey) || (_questionnaireData.Question.QuestionTypeID == QuestionType.VerticalSliderQuestionKey) || (_questionnaireData.Question.QuestionTypeID == QuestionType.TextQuestionKey) ||
                    (_questionnaireData.Question.QuestionTypeID == QuestionType.MultilineTextQuestionKey) ||
                    (_questionnaireData.Question.QuestionTypeID == QuestionType.DateQuestionKey) ||
                    (_questionnaireData.Question.QuestionTypeID == QuestionType.DateTimeQuestionKey) ||
                    (_questionnaireData.Question.QuestionTypeID == QuestionType.TimeQuestionKey) ||
                    (_questionnaireData.Question.QuestionTypeID == QuestionType.NumericQuestionKey))
                {
                    _questionnaireData.Question.MinValue = (float)_minValueString;
                    _questionnaireData.Question.MaxValue = (float)_maxValueString;
                }
                else
                {
                    _questionnaireData.Question.CategoryID = null;
                }
                if ((_questionnaireData.Question.QuestionTypeID == QuestionType.HorizontalSliderQuestionKey) || (_questionnaireData.Question.QuestionTypeID == QuestionType.VerticalSliderQuestionKey))
                {
                    _questionnaireData.Question.SliderSteps = (float)_sliderValueString;
                }
                if (_questionnaireData.Question.MinValue > _questionnaireData.Question.MaxValue)
                {
                    isValid = false;
                    Error = string.Format(CultureInfo.InvariantCulture
                        , LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY)
                        , LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_MIN_TEXT_KEY)
                        , LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_MAX_TEXT_KEY));
                }
            }
            if (isValid)
            {
                await SaveDataAsync();
            }
        }
    }

    private async Task SaveDataAsync()
    {
        _questionnaireData.Question.IsActive = true;
        if (IsAnswerOptionsNeeded())
        {
            _questionnaireData.Question.MinValue = 0;
            _questionnaireData.Question.MaxValue = 0;
            _questionnaireData.Question.SliderSteps = 0;
            if (_questionnaireData.Question.QuestionTypeID == QuestionType.RichTextQuestionKey)
            {
                _questionnaireData.Question.IsRequired = false;
                _questionnaireData.QuestionOptions?.ForEach(x => x.IsActive = false);
            }
        }
        else
        {
            _questionnaireData.QuestionOptions?.ForEach(x => x.IsActive = false);
        }
        GetSelectedOptions();
        await SaveQuestionAsync().ConfigureAwait(true);
    }

    private string GetValueForString(string valueToGet)
    {
        return string.IsNullOrWhiteSpace(valueToGet) ? Constants.CONSTANT_ZERO : valueToGet;
    }

    private void OnDeleteButtonClicked()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel {  ButtonID = Constants.NUMBER_ONE,  ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel {  ButtonID = Constants.NUMBER_TWO,  ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task SaveQuestionAsync()
    {
        Success = Error = string.Empty;
        _questionnaireData.ErrCode = ErrorCode.OK;
        _questionnaireData.QuestionDetails.ForEach(x => x.CaptionText = string.IsNullOrWhiteSpace(x.CaptionText) ? string.Empty : x.CaptionText);
        QuestionnaireDTO questionnaire = new QuestionnaireDTO
        {
            Questionnaire = _questionnaireData.Questionnaire,
            Question = _questionnaireData.Question,
            QuestionDetails = _questionnaireData.QuestionDetails,
            QuestionOptions = _questionnaireData.QuestionOptions,
            QuestionnaireQuestionOptionDetails = _questionnaireData.QuestionnaireQuestionOptionDetails
        };
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).SyncQuestionaireQuestionToServerAsync(questionnaire), questionnaire).ConfigureAwait(true);
        if (questionnaire.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_questionnaireData.ErrCode.ToString());
        }
        else
        {
            Error = questionnaire.ErrCode == ErrorCode.DuplicateData ? ResourceConstants.R_SEQUENCE_NO_ERROR_KEY : questionnaire.ErrCode.ToString();
        }
    }

    private void GetSelectedOptions()
    {
        foreach (var item in _questionnaireData.QuestionOptions)
        {
            item.QuestionID = QuestionID;
            if (!item.IsActive)
            {
                _questionnaireData.QuestionnaireQuestionOptionDetails.RemoveAll(x => x.QuestionOptionID == item.QuestionOptionID);
            }
        }
        _questionnaireData.QuestionOptions.RemoveAll(x => !x.IsActive);
    }

    private void OnAddEditClick(QuestionnaireQuestionOptionModel answer)
    {
        _selectedAnswerID = answer == null ? 0 : answer.QuestionOptionID;
        _shouldShowAnswersList = false;
        _shouldShowAddEditAnswer = true;
        Success = Error = string.Empty;

    }

    private List<TabDataStructureModel> GetDataFormatterForPageDetails(bool isRichTextBoxControl)
    {
        return isRichTextBoxControl
            ? new List<TabDataStructureModel>
            {
                new TabDataStructureModel{ DataField = nameof(QuestionnaireQuestionDetailsModel.CaptionText),FieldType = FieldTypes.RichTextControl, ResourceKey = ResourceConstants.R_QUESTION_KEY, Height = Constants.SMALL_TEXT_BOX_HEIGHT, AllowImage = false, IsRequired = LibResources.IsRequired(_questionnaireData.Resources,ResourceConstants.R_QUESTION_KEY) },
                new TabDataStructureModel{ DataField = nameof(QuestionnaireQuestionDetailsModel.InstructionsText), FieldType = FieldTypes.RichTextControl, ResourceKey = ResourceConstants.R_READ_ONLY_QUESTION_CONTENT_KEY,
                    MaxFileDimensionSize = LibSettings.GetSettingValueByKey(_questionnaireData.Settings, SettingsConstants.S_SMALL_IMAGE_RESOLUTION_KEY),
                    SupportedImageTypes = LibSettings.GetSettingValueByKey(_questionnaireData.Settings, SettingsConstants.S_UPLOAD_IMAGE_TYPE_KEY),
                    MaxFileUploadSize = LibSettings.GetSettingValueByKey(_questionnaireData.Settings, SettingsConstants.S_MAX_FILE_UPLOAD_KEYS),
                    IsRequired = LibResources.IsRequired(_questionnaireData.Resources,ResourceConstants.R_READ_ONLY_QUESTION_CONTENT_KEY) }
            }
            : new List<TabDataStructureModel>
            {
                new TabDataStructureModel{ DataField = nameof(QuestionnaireQuestionDetailsModel.CaptionText),FieldType = FieldTypes.RichTextControl, ResourceKey = ResourceConstants.R_QUESTION_KEY, Height = Constants.SMALL_TEXT_BOX_HEIGHT, AllowImage = false, IsRequired = LibResources.IsRequired(_questionnaireData.Resources,ResourceConstants.R_QUESTION_KEY) },
                new TabDataStructureModel{ DataField = nameof(QuestionnaireQuestionDetailsModel.AnswerPlaceHolder),FieldType = FieldTypes.TextEntryControl,  ResourceKey = ResourceConstants.R_ANSWER_PLACEHOLDER_TEXT_KEY, IsRequired = LibResources.IsRequired(_questionnaireData.Resources,ResourceConstants.R_ANSWER_PLACEHOLDER_TEXT_KEY) },
                new TabDataStructureModel{ DataField = nameof(QuestionnaireQuestionDetailsModel.InstructionsText),FieldType = FieldTypes.RichTextControl, ResourceKey = ResourceConstants.R_QUESTION_INSTRUCTIONS_TEXT_KEY, Height = Constants.SMALL_TEXT_BOX_HEIGHT, AllowImage = false, IsRequired = LibResources.IsRequired(_questionnaireData.Resources,ResourceConstants.R_QUESTION_INSTRUCTIONS_TEXT_KEY) }
            };
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField = nameof(QuestionnaireQuestionOptionModel.QuestionOptionID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false},
            new TableDataStructureModel{DataField = nameof(QuestionnaireQuestionOptionModel.CaptionText),DataHeader=ResourceConstants.R_ANSWER_KEY},
            new TableDataStructureModel{DataField = nameof(QuestionnaireQuestionOptionModel.SequenceNo),DataHeader=ResourceConstants.R_SEQUENCE_NO_KEY},
        };
    }

    private void ResetPageDetails(string resourceKey)
    {
        foreach (var questionDetail in _questionnaireData.QuestionDetails)
        {
            RemoveControlByKey(resourceKey);
        }
    }

    private async Task OnDeleteButtonClickAsync(object sequenceNo)
    {
        if (sequenceNo != null)
        {
            _hideConfirmationPopup = true;
            Success = Error = string.Empty;
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _questionnaireData.Question.IsActive = false;
                await SaveQuestionAsync();
            }
        }
    }

    private bool IsAnswerOptionsNeeded()
    {
        return _questionnaireData.Question.QuestionTypeID == QuestionType.SingleSelectQuestionKey
            || _questionnaireData.Question.QuestionTypeID == QuestionType.MultiSelectQuestionKey
            || _questionnaireData.Question.QuestionTypeID == QuestionType.DropDownQuestionKey
            || _questionnaireData.Question.QuestionTypeID == QuestionType.RichTextQuestionKey
            || _questionnaireData.Question.QuestionTypeID == QuestionType.FilesAndDocumentQuestionKey
            || _questionnaireData.Question.QuestionTypeID == QuestionType.MeasurementQuestionKey
            || _questionnaireData.Question.QuestionTypeID == 0;
    }

    private List<OptionModel> CreateOptions(long id, string key, bool value)
    {
        return new List<OptionModel> {
            new OptionModel {
                OptionID = id,
                OptionText = LibResources.GetResourceValueByKey(_questionnaireData.Resources, key),
                IsSelected = value
            },
        };
    }
}