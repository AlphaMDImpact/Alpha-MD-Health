using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using Wangkanai.Extensions;

namespace AlphaMDHealth.WebClient;

public partial class PatientProviderNotePage : BasePage
{
    private readonly PatientProviderNoteDTO _patientProviderNoteData = new() { PatientProviderNote = new PatientProviderNoteModel(), RecordCount = -1 };
    private List<QuestionnaireQuestionModel> _existingQuestions;
    private List<AttachmentModel> _file;
    private List<ButtonActionModel> _actionData;
    private QuestionnaireService _questionnaireService;
    private bool _hideDeleteConfirmationPopup = true;
    private bool _isEditable;
    private long _selectedQuestionnaireID;
    private double? _numericValue;
    private bool _isFirstTime;
    private bool _isNoteFirstTime;

    /// <summary>
    /// Provider Note ID
    /// </summary>
    [Parameter]
    public Guid ProviderNoteID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _questionnaireService = new QuestionnaireService(AppState.webEssentials);
        _patientProviderNoteData.PatientProviderNote.ProviderNoteID = ProviderNoteID;
        _patientProviderNoteData.QuestionnaireQuestionAnswers = new List<PatientQuestionnaireQuestionAnswersModel>();
        await SendServiceRequestAsync(_questionnaireService.GetProviderNoteAsync(_patientProviderNoteData), _patientProviderNoteData).ConfigureAwait(true);
        if (_patientProviderNoteData.ErrCode == ErrorCode.OK)
        {
            _file ??= new List<AttachmentModel>();
            _patientProviderNoteData.Providers ??= new List<OptionModel>();
            _isEditable = LibPermissions.HasPermission(_patientProviderNoteData.FeaturePermissions, AppPermissions.PatientProviderNoteAddEdit.ToString());
            if (ProviderNoteID != Guid.Empty)
            {
                if (GenericMethods.IsListNotEmpty(_patientProviderNoteData.PatientPrograms) || GenericMethods.IsListNotEmpty(_patientProviderNoteData.ProgramNotes))
                {
                    if (_patientProviderNoteData.PatientPrograms.Any(x => x.OptionID == _patientProviderNoteData.PatientProviderNote.ProgramID && x.IsDefault)
                        || !_patientProviderNoteData.ProgramNotes.Any(x => x.OptionID == _patientProviderNoteData.PatientProviderNote.ProgramNoteID && x.IsDefault))
                    {
                        _isEditable = false;
                    }
                }
                _selectedQuestionnaireID = _patientProviderNoteData.ProgramNotes?.FirstOrDefault(x => x.IsSelected == true)?.ParentOptionID ?? 0;

                var attachment = _patientProviderNoteData.FileDocuments?.LastOrDefault(x => x.DocumentName != string.Empty);
                if (attachment != null)
                {
                    _file.Add(new AttachmentModel()
                    {
                        FileValue = attachment.DocumentDescription,
                        FileDescription = attachment.DocumentName,
                        IsActive = true,
                    });
                }
                if (GenericMethods.IsListNotEmpty(_patientProviderNoteData.ProviderQuestions))
                {
                    for (int index = 0; index < _patientProviderNoteData.ProviderQuestions.Count; index++)
                    {
                        var question = _patientProviderNoteData.ProviderQuestions[index];
                        var key = GetKey(question);
                        var resource = _patientProviderNoteData.Resources?.FirstOrDefault(x => x.ResourceKey == key);
                        if (resource != null)
                        {
                            var answer = _patientProviderNoteData.QuestionnaireQuestionAnswers?.FirstOrDefault(x => x.QuestionID == question.QuestionID);
                            switch (question.QuestionTypeID)
                            {
                                case QuestionType.FilesAndDocumentQuestionKey:
                                    var fileDocument = _patientProviderNoteData.FileDocuments?.FirstOrDefault(x => x.DocumentSourceID == answer?.PatientAnswerID.ToString());
                                    if (fileDocument != null)
                                    {
                                        resource.IsRequired = false;
                                        fileDocument.UniqueKey = string.Concat(key, index);
                                        question.AnswerPlaceHolder = (fileDocument.DocumentName);
                                        _patientProviderNoteData.ProviderQuestions[index].AnswerPlaceHolder = (fileDocument.DocumentName);
                                    }
                                  
                                    break;
                                case QuestionType.DateTimeQuestionKey:
                                case QuestionType.DateQuestionKey:
                                case QuestionType.TimeQuestionKey:
                                    question.AnswerPlaceHolder = string.IsNullOrWhiteSpace(answer?.AnswerValue)
                                        ? string.Empty
                                        : AppState.webEssentials.ConvertToLocalTime(Convert.ToDateTime(answer.AnswerValue, CultureInfo.InvariantCulture)).ToString();
                                    break;
                                case QuestionType.NumericQuestionKey:
                                    _numericValue = Convert.ToDouble(answer?.AnswerValue);
                                    break;
                                case QuestionType.TextQuestionKey:
                                case QuestionType.VerticalSliderQuestionKey:
                                case QuestionType.HorizontalSliderQuestionKey:
                                case QuestionType.MultilineTextQuestionKey:
                                default:
                                    question.AnswerPlaceHolder = answer?.AnswerValue;
                                    break;

                            }
                        }
                    }
                }
            }
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_patientProviderNoteData.ErrCode.ToString());
        }
    }

    private async Task OnProgramChangedAsync(object e)
    {
        Success = Error = string.Empty;
        if (e != null && !string.IsNullOrWhiteSpace((string)e))
        {
            _existingQuestions = _patientProviderNoteData.ProviderQuestions;
            PatientProviderNoteDTO patientProviderNoteData = new()
            {
                RecordCount = -2,
                PatientProviderNote = new PatientProviderNoteModel
                {
                    ProgramID = Convert.ToInt64(e),
                },
            };
            await SendServiceRequestAsync(_questionnaireService.GetProviderNoteAsync(patientProviderNoteData), patientProviderNoteData).ConfigureAwait(true);
            if (patientProviderNoteData.ErrCode == ErrorCode.OK)
            {
                if (ProviderNoteID == Guid.Empty || _isFirstTime)
                {
                    _patientProviderNoteData.Providers = patientProviderNoteData.Providers?.Any(x => !x.IsSelected) ?? false
                        ? patientProviderNoteData.Providers
                        : _patientProviderNoteData.Providers;
                }
                var noteID = _patientProviderNoteData.ProgramNotes?.FirstOrDefault(x => x.IsSelected)?.OptionID;
                _patientProviderNoteData.ProgramNotes = GenericMethods.IsListNotEmpty(patientProviderNoteData.ProgramNotes)
                                ? patientProviderNoteData.ProgramNotes
                                : new List<OptionModel>();
                if (noteID != null)
                {
                    patientProviderNoteData.ProgramNotes.FirstOrDefault(x => x.OptionID == noteID).IsSelected = true;
                }

                _isFirstTime = true;
                var selectedQuestionnaireID = _patientProviderNoteData.ProgramNotes?.FirstOrDefault(x => x.IsSelected)?.ParentOptionID;
                if (_existingQuestions != null && (selectedQuestionnaireID != null && selectedQuestionnaireID.Value != _selectedQuestionnaireID))
                {
                    RemoveExistingControls(_existingQuestions);
                }
                _selectedQuestionnaireID = selectedQuestionnaireID ?? _selectedQuestionnaireID;
            }
            else
            {
                Error = patientProviderNoteData.ErrCode.ToString();
            }
        }
        else
        {
            await OnProviderNoteChangeAsync(null);
            _patientProviderNoteData.Providers.Clear();
        }
    }

    private async Task OnProviderNoteChangeAsync(object e)
    {
        if (e != null && !string.IsNullOrWhiteSpace((string)e))
        {
            Success = Error = string.Empty;
            _existingQuestions = _patientProviderNoteData.ProviderQuestions;
            var selectedQuestionnaireID = _patientProviderNoteData.ProgramNotes?.FirstOrDefault(x => x.IsSelected)?.ParentOptionID;
            if (selectedQuestionnaireID != null)
            {
                if (_selectedQuestionnaireID != selectedQuestionnaireID.Value)
                {
                    RemoveExistingControls(_existingQuestions);
                }
                _selectedQuestionnaireID = selectedQuestionnaireID.Value;
                PatientProviderNoteDTO patientProviderNoteData = new PatientProviderNoteDTO
                {
                    RecordCount = -3,
                    PatientProviderNote = new PatientProviderNoteModel
                    {
                        QuestionnaireID = _selectedQuestionnaireID
                    }
                };
                await SendServiceRequestAsync(_questionnaireService.GetProviderNoteAsync(patientProviderNoteData), patientProviderNoteData).ConfigureAwait(true);
                if (patientProviderNoteData.ErrCode == ErrorCode.OK)
                {
                    _patientProviderNoteData.QuestionConditions = patientProviderNoteData.QuestionConditions;
                    _patientProviderNoteData.QuestionnaireQuestions = patientProviderNoteData.QuestionnaireQuestions;
                    _patientProviderNoteData.QuestionnaireQuestionOptions = patientProviderNoteData.QuestionnaireQuestionOptions;
                    if(_isNoteFirstTime || ProviderNoteID == Guid.Empty)
                    {
                        _patientProviderNoteData.ProviderQuestions = patientProviderNoteData.ProviderQuestions;
                        _patientProviderNoteData.FileDocuments?.Clear();
                        _patientProviderNoteData.Files?.Clear();
                        _file?.Clear();
                    }
                    else
                    {
                        _isNoteFirstTime = !_isNoteFirstTime;
                    }
                    _patientProviderNoteData.Resources = patientProviderNoteData.Resources;
                    _patientProviderNoteData.Files = patientProviderNoteData.Files;
                    _patientProviderNoteData.QuestionnaireQuestionAnswers.ForEach(x => x.AnswerValue=null);

                }
                else
                {
                    Error = patientProviderNoteData.ErrCode.ToString();
                }
            }
        }
        else
        {
            if (e == null)
            {
                _patientProviderNoteData.ProgramNotes?.Clear();
            }
            else if (Convert.ToString(e) == string.Empty)
            {
                _existingQuestions = _patientProviderNoteData.ProviderQuestions;
                RemoveExistingControls(_existingQuestions);
            }
            _patientProviderNoteData.ProviderQuestions?.Clear();
            _numericValue = null;
            _file?.Clear();
        }
        StateHasChanged();
    }

    private void OnCareGiverChanged(object e)
    {
        if (e != null && !string.IsNullOrWhiteSpace((string)e))
        {
            _patientProviderNoteData.PatientProviderNote.CareGiverID = Convert.ToInt64(e);
        }
    }

    private void OnAttachmentClick(object fileModel, string uniqueKey)
    {
        var file = fileModel as AttachmentModel;
        if (file.IsActive)
        {
            if (_patientProviderNoteData.FileDocuments != null)
            {
                var existingFile = _patientProviderNoteData.FileDocuments.FirstOrDefault(x => x.UniqueKey == uniqueKey);
                if (existingFile != null)
                {
                    _patientProviderNoteData.FileDocuments.Remove(existingFile);
                }
                _patientProviderNoteData.FileDocuments.Add(new FileDocumentModel
                {
                    FileDocumentID = Guid.Empty,
                    AddedOn = GenericMethods.GetUtcDateTime,
                    DocumentName = file.FileValue,
                    DocumentDescription = file.FileDescription,
                    DocumentStatus = file.FileName,
                    FileDocumentName = file.FileName,
                    IsActive = true,
                    IsUnreadHeader = true,
                    ShowRemoveButton = true,
                    ShowUnreadBadge = false,
                    UniqueKey = uniqueKey
                });
            }
            else
            {
                _patientProviderNoteData.FileDocuments = new List<FileDocumentModel>
                {
                    new FileDocumentModel
                    {
                        FileDocumentID = Guid.Empty,
                        AddedOn = GenericMethods.GetUtcDateTime,
                        DocumentName = file.FileValue,
                        DocumentDescription = file.FileDescription,
                        DocumentStatus = file.FileName,
                        FileDocumentName = file.FileName,
                        IsActive = true,
                        IsUnreadHeader = true,
                        ShowRemoveButton = true,
                        ShowUnreadBadge = false,
                        UniqueKey = uniqueKey
                    }
                };
            }
        }
        else
        {
            var matchedDocument = _patientProviderNoteData.FileDocuments.FirstOrDefault(x => x.UniqueKey == uniqueKey);

            if (matchedDocument != null)
            {
                _patientProviderNoteData.FileDocuments.Remove(matchedDocument);
            }
        }
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
        };
        _hideDeleteConfirmationPopup = false;
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnSaveButtonClickedAsync()
    {
        if (IsValid())
        {
            _patientProviderNoteData.PatientProviderNote.CareGiverID = _patientProviderNoteData.Providers?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? _patientProviderNoteData.PatientProviderNote.CareGiverID;
            _patientProviderNoteData.PatientProviderNote.ProgramNoteID = _patientProviderNoteData.ProgramNotes?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? _patientProviderNoteData.PatientProviderNote.ProgramNoteID;
            PatientProviderNoteDTO PatientProviderNoteData = new()
            {
                PatientProviderNote = new PatientProviderNoteModel { ProviderNoteID = ProviderNoteID },
                ProviderQuestions = _patientProviderNoteData.ProviderQuestions,
                FileDocuments = new List<FileDocumentModel>()
            };
            PatientProviderNoteData.PatientProviderNote.CareGiverID = _patientProviderNoteData.PatientProviderNote.CareGiverID;
            PatientProviderNoteData.PatientProviderNote.PatientID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
            PatientProviderNoteData.PatientProviderNote.ProgramNoteID = _patientProviderNoteData.PatientProviderNote.ProgramNoteID;
            PatientProviderNoteData.PatientProviderNote.NoteDateTime = _patientProviderNoteData.PatientProviderNote.NoteDateTime?.ToUniversalTime();
            PatientProviderNoteData.Providers = _patientProviderNoteData.Providers;
            //PatientProviderNoteData.QuestionnaireQuestionAnswers = GenericMethods.IsListNotEmpty(_patientProviderNoteData.QuestionnaireQuestionAnswers)
            //    ? _patientProviderNoteData.QuestionnaireQuestionAnswers
            //    : new List<PatientQuestionnaireQuestionAnswersModel>();

            foreach (var question in _patientProviderNoteData.ProviderQuestions.ToList().Select((value, i) => new { i, value }))
            {
                var key = string.Concat(question.value.QuestionTypeID, question.value.QuestionID.ToString());
                var index = question.i;
                switch (question.value.QuestionTypeID)
                {
                    case QuestionType.FilesAndDocumentQuestionKey:
                        FileDocumentModel doc = _patientProviderNoteData.FileDocuments?.FirstOrDefault(x => x.UniqueKey == string.Concat(key, index));
                        AddAnswerToQuestionnaireQuestionAnswers(doc?.DocumentDescription ?? string.Empty, question.value.QuestionID);
                        if (doc != null)
                        {
                            var categoryID = (long)_patientProviderNoteData.QuestionnaireQuestions.FirstOrDefault(x => x.QuestionID == question.value.QuestionID).CategoryID;
                            if (_patientProviderNoteData.FileDocuments[0].DocumentDescription.IsNotNullOrEmpty())
                            {
                                doc.DocumentDescription = _patientProviderNoteData.FileDocuments[0].DocumentDescription;
                            }
                            else
                            {
                                doc.DocumentDescription = ResourceConstants.R_DESCRIPTION_TEXT_KEY;
                            }
                            new FileService(AppState.webEssentials).MapQuestionnaireFileData(categoryID, doc);
                            PatientProviderNoteData.FileDocuments.Add(doc);
                        }
                        break;
                    case QuestionType.NumericQuestionKey:
                        AddAnswerToQuestionnaireQuestionAnswers(_numericValue.ToString(), question.value.QuestionID);
                        break;
                    case QuestionType.TextQuestionKey:
                    case QuestionType.MultilineTextQuestionKey:
                    case QuestionType.HorizontalSliderQuestionKey:
                    case QuestionType.VerticalSliderQuestionKey:
                    case QuestionType.SingleSelectQuestionKey:
                    case QuestionType.DropDownQuestionKey:
                    case QuestionType.MultiSelectQuestionKey:
                    case QuestionType.DateQuestionKey:
                    case QuestionType.DateTimeQuestionKey:
                    case QuestionType.TimeQuestionKey:
                    default:
                        AddAnswerToQuestionnaireQuestionAnswers(question.value.AnswerPlaceHolder, question.value.QuestionID);
                        break;
                }
            }
            PatientProviderNoteData.QuestionnaireQuestionAnswers = _patientProviderNoteData.QuestionnaireQuestionAnswers.Where(x => x.IsActive)?.ToList();
            PatientProviderNoteData.PatientProviderNote.IsActive = true;
            await SaveProviderNoteAsync(PatientProviderNoteData).ConfigureAwait(true);
        }
    }

    private void AddAnswerToQuestionnaireQuestionAnswers(string answerValue, long questionID)
    {
        var questAnswer = _patientProviderNoteData.QuestionnaireQuestionAnswers?.FirstOrDefault(x => x.QuestionID == questionID);
        if (questAnswer != null)
        {
            questAnswer.IsActive = true;
            questAnswer.AnswerValue = answerValue;
            questAnswer.NextQuestionID = _patientProviderNoteData.QuestionConditions?.FirstOrDefault(x => x.QuestionID == questionID)?.TargetQuestionID;
        }
        else
        {
            _patientProviderNoteData.QuestionnaireQuestionAnswers.Add(new PatientQuestionnaireQuestionAnswersModel
            {
                QuestionID = questionID,
                IsActive = true,
                TaskType = 2,
                AnswerValue = answerValue,
                PreviousQuestionID = 0,
                NextQuestionID = _patientProviderNoteData.QuestionConditions?.FirstOrDefault(x => x.QuestionID == questionID)?.TargetQuestionID
            });
        }
    }

    private async Task SaveProviderNoteAsync(PatientProviderNoteDTO patientProviderNoteData)
    {
        await SendServiceRequestAsync(_questionnaireService.SavePatientProviderAsync(patientProviderNoteData), patientProviderNoteData).ConfigureAwait(true);
        if (patientProviderNoteData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(patientProviderNoteData.ErrCode.ToString());
        }
        else
        {
            Error = patientProviderNoteData.ErrCode.ToString();
        }
    }

    private async Task DeletePopUpCallbackAsync(object sequenceNo)
    {
        _hideDeleteConfirmationPopup = false;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _patientProviderNoteData.PatientProviderNote.IsActive = false;
                PatientProviderNoteDTO patientProviderNoteData = new()
                {
                    PatientProviderNote = _patientProviderNoteData.PatientProviderNote
                };

                await SaveProviderNoteAsync(patientProviderNoteData);
            }
            else
            {
                _hideDeleteConfirmationPopup = true;
            }
        }
    }

    private string GetKey(QuestionnaireQuestionModel question)
    {
        return string.Concat(question.QuestionTypeID, question.QuestionID.ToString());
    }

    private void RemoveExistingControls(List<QuestionnaireQuestionModel> existingQuestions)
    {
        if (GenericMethods.IsListNotEmpty(existingQuestions))
        {
            foreach (var question in existingQuestions)
            {
                var key = GetKey(question);
                RemoveControlContainsKey(key);
            }
        }
    }

    private List<OptionModel> CreateOptionList(QuestionnaireQuestionModel question)
    {
        var answers = question.AnswerPlaceHolder?.Split(Constants.PIPE_SEPERATOR);
        List<OptionModel> optionList = (from questionOption in _patientProviderNoteData.QuestionnaireQuestionOptions
                                        where questionOption.QuestionID == question.QuestionID
                                        select new OptionModel
                                        {
                                            OptionID = questionOption.QuestionOptionID,
                                            ParentOptionID = questionOption.QuestionID,
                                            OptionText = questionOption.CaptionText,
                                            IsSelected = answers?.Contains(questionOption.QuestionOptionID.ToString(CultureInfo.InvariantCulture)) ?? false
                                        }).ToList();
        return optionList;
    }
}