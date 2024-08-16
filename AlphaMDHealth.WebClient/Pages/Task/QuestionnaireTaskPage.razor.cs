using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class QuestionnaireTaskPage : BasePage
{
    private readonly QuestionnaireDTO _questionnaireData = new QuestionnaireDTO() { Questionnaire = new QuestionnaireModel() { QuestionnaireAction = QuestionnaireAction.StartQuestionnaire }, Question = new QuestionnaireQuestionModel(), QuestionnaireQuestionAnswer = new PatientQuestionnaireQuestionAnswersModel() };
    private List<AttachmentModel> _file = new List<AttachmentModel>();
    private long _previousQuestionID = 0;
    private bool _isStatusChanged;
    private PatientReadingPage _patientReadingPageRef;
    private Guid _patientReadingID;
    private bool _allowNext = true;
    private DateTimeOffset? _dateTime;
    private double? _numericValue;
    private string _numericEntryErrorLabel;

    /// <summary>
    /// Patient Task ID parameter
    /// </summary>
    [Parameter]
    public string PatientTaskID
    {
        get { return _questionnaireData.PatientTaskID; }
        set { _questionnaireData.PatientTaskID = value; }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetNextQuestionAsync();
        if (_questionnaireData.QuestionnaireQuestionAnswer != null && _questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID != Guid.Empty)
        {
            _previousQuestionID = _questionnaireData.QuestionnaireQuestionAnswer?.PreviousQuestionID ?? 0;
        }
    }

    private async Task GetNextQuestionAsync()
    {
        if (_questionnaireData.QuestionnaireQuestionAnswer == null)
        {
            _questionnaireData.QuestionnaireQuestionAnswer = new PatientQuestionnaireQuestionAnswersModel();
        }
        RemoveControlContainsKey(ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY);
        _numericEntryErrorLabel = string.Empty;
        _dateTime = null;
        await SendServiceRequestAsync(new QuestionnaireService(AppState.webEssentials).GetQuestionnaireAsync(_questionnaireData), _questionnaireData).ConfigureAwait(true);
        _patientReadingID = Guid.Empty;
        _numericValue = null;
        _file?.Clear();
        if (_questionnaireData.QuestionnaireQuestionAnswer?.PatientAnswerID != Guid.Empty)
        {
            MapAnswer();
        }
        _isDataFetched = true;
    }

    private async Task OnSaveAndNextButtonClick()
    {
        if (_questionnaireData.Question.QuestionTypeID == QuestionType.NumericQuestionKey && (_questionnaireData.Question.IsRequired && string.IsNullOrEmpty(_numericValue.ToString())))
        {
            GetErrorMessage();
        }

        if (IsValid())
        {
            _questionnaireData.QuestionnaireQuestionAnswer.QuestionID = _questionnaireData.Question.QuestionID;
            await MapQuestionAnswerValues();
            _questionnaireData.Questionnaire.QuestionnaireAction = QuestionnaireAction.Next;
            _questionnaireData.QuestionnaireQuestionAnswer.TaskType = 1;
            _questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID = _previousQuestionID;
            _questionnaireData.QuestionnaireQuestionAnswer.IsActive = true;
            _questionnaireData.QuestionnaireQuestionAnswer.LastModifiedON = GenericMethods.GetUtcDateTime;
            _previousQuestionID = _questionnaireData.Question.QuestionID;
            if (_allowNext)
            {
                await GetNextQuestionAsync();
            }
        }
    }

    private void GetErrorMessage()
    {
        if (_numericValue.HasValue)
        {
            _numericEntryErrorLabel = string.Empty;
        }
        else
        {
            _numericEntryErrorLabel = string.Format(CultureInfo.CurrentCulture,
                 LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY),
                 LibResources.GetResourceValueByKey(_questionnaireData.Resources, ResourceConstants.R_QUESTIONNAIRE_DYNAMIC_RESOURCE_KEY));
        }
    }

    private async Task OnPreviousButtonClick()
    {
        _questionnaireData.Questionnaire.QuestionnaireAction = QuestionnaireAction.PreviousAndNext;
        if (_questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID == Guid.Empty)
        {
            _questionnaireData.QuestionnaireQuestionAnswer.PreviousQuestionID = _previousQuestionID;
        }
        await GetNextQuestionAsync();
        _previousQuestionID = _questionnaireData.QuestionnaireQuestionAnswer?.PreviousQuestionID ?? 0;
    }

    private async Task OnCloseClickedAsync()
    {
        await OnClose.InvokeAsync(_isStatusChanged ? ErrorCode.OK.ToString() : string.Empty);
    }

    private async Task OnStartQuestionnaireClick()
    {
        _questionnaireData.Questionnaire.QuestionnaireAction = QuestionnaireAction.Next;
        await GetNextQuestionAsync();
        _isStatusChanged = true;
    }

    private async Task OnDoneQuestionnaireClick()
    {
        await OnClose.InvokeAsync(ErrorCode.OK.ToString());
    }

    private void MapAnswer()
    {
        switch (_questionnaireData.Question.QuestionTypeID)
        {
            case QuestionType.NumericQuestionKey:
            case QuestionType.VerticalSliderQuestionKey:
            case QuestionType.HorizontalSliderQuestionKey:
                if (!string.IsNullOrWhiteSpace(_questionnaireData.QuestionnaireQuestionAnswer?.AnswerValue))
                {
                    _numericValue = string.IsNullOrWhiteSpace(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue) ? null : Convert.ToDouble(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture);
                }
                break;
            case QuestionType.DateQuestionKey:
            case QuestionType.DateTimeQuestionKey:
            case QuestionType.TimeQuestionKey:
                if (!string.IsNullOrWhiteSpace(_questionnaireData.QuestionnaireQuestionAnswer?.AnswerValue))
                {
                    _dateTime = AppState.webEssentials.ConvertToLocalTime(Convert.ToDateTime(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, CultureInfo.InvariantCulture));
                }
                break;
            case QuestionType.FilesAndDocumentQuestionKey:
                if (GenericMethods.IsListNotEmpty(_questionnaireData.FileDocuments))
                {
                    var fileDocument = _questionnaireData.FileDocuments.First(x => x.IsActive);
                    if (!string.IsNullOrWhiteSpace(fileDocument.DocumentName))
                    {
                        _file = new List<AttachmentModel>
                        {
                            new AttachmentModel
                            {
                                FileValue = fileDocument.DocumentName,
                                FileDescription = fileDocument.DocumentDescription,
                                FileID = fileDocument.FileID,
                                IsActive = true
                            }
                        };
                    }
                }
                break;
            case QuestionType.MeasurementQuestionKey:
                if (!string.IsNullOrWhiteSpace(_questionnaireData.QuestionnaireQuestionAnswer?.AnswerValue))
                {
                    if (Guid.TryParse(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue, out Guid parsedGuid))
                    {
                        _patientReadingID = parsedGuid;
                    }
                    else
                    {
                        _patientReadingID = Guid.Empty;
                    }
                }
                break;
        }
    }

    private async Task MapQuestionAnswerValues()
    {
        switch (_questionnaireData.Question.QuestionTypeID)
        {
            case QuestionType.NumericQuestionKey:
            case QuestionType.HorizontalSliderQuestionKey:
            case QuestionType.VerticalSliderQuestionKey:
                _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = _numericValue?.ToString() ?? string.Empty;
                break;
            case QuestionType.DateQuestionKey:
                _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = _dateTime.HasValue ? GenericMethods.ConvertToIsoDatetimeOffset(_dateTime) : string.Empty;
                break;
            case QuestionType.DateTimeQuestionKey:
            case QuestionType.TimeQuestionKey:
                _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = _dateTime.HasValue
                                                                            ? _dateTime.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture)
                                                                            : string.Empty;
                break;
            case QuestionType.DropDownQuestionKey:
            case QuestionType.SingleSelectQuestionKey:
            case QuestionType.MultiSelectQuestionKey:
                if (_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue == Constants.MINUS_ONE)
                {
                    _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = string.Empty;
                }
                break;
            case QuestionType.FilesAndDocumentQuestionKey:
                if (GenericMethods.IsListNotEmpty(_questionnaireData.FileDocuments))
                {
                    var file = _questionnaireData.FileDocuments.FirstOrDefault(x => x.IsActive);
                    if(file != null)
                    {
                        new FileService(AppState.webEssentials).MapQuestionnaireFileData((long)_questionnaireData.Question.CategoryID, file);
                        _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = file.DocumentDescription;
                        if (_questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID != Guid.Empty)
                        {
                            file.DocumentSourceID = _questionnaireData.QuestionnaireQuestionAnswer.PatientAnswerID.ToString();
                        }
                    }
                    else
                    {
                        _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = string.Empty;
                    }
                }
                else
                {
                    _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = string.Empty;
                }
                break;
            case QuestionType.MeasurementQuestionKey:
                if (_patientReadingPageRef != null)
                {
                    Guid PatientReadingID = Guid.Empty;
                    var result = await _patientReadingPageRef.SavePatientReadingData();
                    PatientReadingID = result.Item2;
                    _allowNext = result.Item1;
                    if (PatientReadingID != Guid.Empty)
                    {
                        if (!(!string.IsNullOrEmpty(_questionnaireData.QuestionnaireQuestionAnswer.AnswerValue) &&
                              _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue != PatientReadingID.ToString()))
                        {
                            _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = PatientReadingID.ToString();
                        }
                    }
                    else
                    {
                        _questionnaireData.QuestionnaireQuestionAnswer.AnswerValue = string.Empty;
                    }
                }
                break;
        }
    }

    private void OnImageChnaged(AttachmentModel attachment)
    {
        var existingFile = attachment.FileID == Guid.Empty
                ? _questionnaireData.FileDocuments.FirstOrDefault(x => x.FileDocumentName == attachment.FileName)
                : _questionnaireData.FileDocuments.FirstOrDefault(x => x.FileID == attachment.FileID);

        if (_questionnaireData.FileDocuments.Contains(existingFile))
        {
            var index = _questionnaireData.FileDocuments.IndexOf(existingFile);
            _questionnaireData.FileDocuments[index].FileID = attachment.FileID;
            _questionnaireData.FileDocuments[index].DocumentDescription = attachment.FileDescription;
            _questionnaireData.FileDocuments[index].FileDocumentName = attachment.IsActive ? attachment.FileName : string.Empty;
            _questionnaireData.FileDocuments[index].DocumentName = attachment.IsActive ? attachment.FileValue : string.Empty;
            _questionnaireData.FileDocuments[index].IsActive = attachment.IsActive;
        }
        else
        {
            _questionnaireData.FileDocuments.Add(new FileDocumentModel
            {
                FileID = attachment.FileID,
                FileDocumentID = Guid.Empty,
                FileDocumentName = attachment.IsActive ? attachment.FileName : string.Empty,
                DocumentDescription = attachment.FileDescription,
                DocumentName = attachment.IsActive ? attachment.FileValue : string.Empty,
                IsActive = attachment.IsActive
            });
        }
    }
}
