using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer;

public class PatientTaskServiceBussinessLayer : BaseServiceBusinessLayer
{
    /// <summary>
    /// Task service
    /// </summary>
    /// <param name="httpContext">Instance of HttpContext</param>
    public PatientTaskServiceBussinessLayer(HttpContext httpContext) : base(httpContext)
    {
    }

    /// <summary>
    /// check and update task status 
    /// </summary>
    /// <returns>Operation status code</returns>
    public async Task<ErrorCode> UpdateMissedTasksAsync()
    {
        try
        {
            return await new PatientTaskServiceDataLayer().UpdateMissedTasksAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            return ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Get patient tasks
    /// </summary>
    /// <param name="languageID">User's language ID</param>
    /// <param name="organisationID">Organisation ID</param>
    /// <param name="selectedUserID">Selected user's ID for whoe data needs to be retrived</param>
    /// <param name="permissionAtLevelID">User's permission level</param>
    /// <param name="recordCount">Record count to decide how much data to retrive</param>
    /// <param name="patientTaskID">Patient task ID to retrive specific task data</param>
    /// <returns>List of patient tasks with operation status</returns>
    public async Task<ProgramDTO> GetPatientTasksAsync(byte languageID, long organisationID, long selectedUserID, long permissionAtLevelID, long recordCount, long patientTaskID)
    {
        ProgramDTO taskData = new ProgramDTO();
        try
        {
            if (permissionAtLevelID < 1 || languageID < 1 || selectedUserID < 1)
            {
                taskData.ErrCode = ErrorCode.InvalidData;
                return taskData;
            }
            if (AccountID < 1)
            {
                taskData.ErrCode = ErrorCode.Unauthorized;
                return taskData;
            }
            if (await GetPatientTasksResourceAndSettingsAsync(taskData, organisationID, languageID).ConfigureAwait(false))
            {
                taskData.SelectedUserID = selectedUserID;
                taskData.RecordCount = recordCount;
                taskData.PermissionAtLevelID = permissionAtLevelID;
                taskData.Task = new TaskModel { PatientTaskID = patientTaskID };
                taskData.FeatureFor = FeatureFor;
                await new PatientTaskServiceDataLayer().GetPatientTasksAsync(taskData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            taskData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
        return taskData;
    }

    internal async Task<bool> GetPatientTasksResourceAndSettingsAsync(ProgramDTO taskData, long organisationID, byte languageID)
    {
        taskData.AccountID = AccountID;
        taskData.OrganisationID = organisationID;
        taskData.LanguageID = languageID;
        if (await GetSettingsResourcesAsync(taskData, true, string.Empty,
            $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_TASK_GROUP},{GroupConstants.RS_TASK_STATUS_GROUP},{GroupConstants.RS_TASK_TYPE_GROUP},{GroupConstants.RS_PROGRAMS_GROUP}"
        ).ConfigureAwait(false))
        {
            taskData.ErrCode = ErrorCode.OK;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Save patient task
    /// </summary>
    /// <param name="taskData">Task data to be saved</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <returns>Operatin status</returns>
    public async Task<ProgramDTO> SavePatientTaskAsync(byte languageID, long organisationID, ProgramDTO taskData, long permissionAtLevelID)
    {
        try
        {
            if (AccountID < 1 || permissionAtLevelID < 1 || taskData.Task == null || taskData.SelectedUserID < 1 || string.IsNullOrWhiteSpace(taskData.Task.Status))
            {
                taskData.ErrCode = ErrorCode.InvalidData;
                return taskData;
            }
            if (taskData.IsActive)
            {
                taskData.LanguageID = 1;
                if (await GetSettingsResourcesAsync(taskData, false, string.Empty,
                    $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_TASK_GROUP},{GroupConstants.RS_TASK_STATUS_GROUP},{GroupConstants.RS_TASK_TYPE_GROUP},{GroupConstants.RS_PROGRAMS_GROUP}"
                ).ConfigureAwait(false))
                {
                    if (!await ValidateDataAsync(taskData.Task, taskData.Resources))
                    {
                        taskData.ErrCode = ErrorCode.InvalidData;
                        return taskData;
                    }
                }
                else
                {
                    return taskData;
                }
            }
            taskData.AccountID = AccountID;
            taskData.PermissionAtLevelID = permissionAtLevelID;
            taskData.FeatureFor = FeatureFor;
            await new PatientTaskServiceDataLayer().SavePatientTaskAsync(taskData).ConfigureAwait(false);
            if (taskData.ErrCode == ErrorCode.OK)
            {
                TemplateDTO communicationDto = new TemplateDTO();
                communicationDto.AccountID = AccountID;
                communicationDto.OrganisationID = organisationID;
                communicationDto.SelectedUserID = taskData.SelectedUserID;
                communicationDto.LanguageID = languageID;
                communicationDto.Response = taskData.Task.PatientTaskID.ToString(CultureInfo.InvariantCulture);
                communicationDto.NotificationTags = $"{Constants.USER_TAG_PREFIX}{taskData.SelectedUserID}";
                communicationDto.AddedON = taskData.Task.FromDate;
                communicationDto.TemplateData = new TemplateModel
                {
                    TemplateName = TemplateName.ENewTask,
                    IsExternal = false,
                };
                await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            taskData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return taskData;
    }

    /// <summary>
    /// Update patient task status
    /// </summary>
    /// <param name="taskData">Onject containing Task status data that to be updated</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <returns>Operatin status</returns>
    public async Task<ProgramDTO> UpdatePatientTaskStatusAsync(ProgramDTO taskData, long permissionAtLevelID)
    {
        try
        {
            if (permissionAtLevelID < 1 || ValidateTaskStatus(taskData))
            {
                taskData.ErrCode = ErrorCode.InvalidData;
                return taskData;
            }
            if (AccountID < 1)
            {
                taskData.ErrCode = ErrorCode.Unauthorized;
                return taskData;
            }
            taskData.AccountID = AccountID;
            taskData.PermissionAtLevelID = permissionAtLevelID;
            taskData.FeatureFor = FeatureFor;
            await new PatientTaskServiceDataLayer().UpdatePatientTaskStatusAsync(taskData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            taskData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return taskData;
    }

    /// <summary>
    /// Get next/previous question
    /// </summary>
    /// <param name="languageID">languageID</param>
    /// <param name="permissionAtLevelID">permissionAtLevelID</param>
    /// <param name="questionnaireData">questionnaireData</param>
    /// <returns>next/previous question with operation status</returns>
    public async Task<QuestionnaireDTO> GetNextQuestionAsync(byte languageID, long organisationID, long permissionAtLevelID, QuestionnaireDTO questionnaireData, short readingCategoryID)
    {
        try
        {
            if (permissionAtLevelID < 1 || string.IsNullOrWhiteSpace(questionnaireData.PatientTaskID))
            {
                questionnaireData.ErrCode = ErrorCode.InvalidData;
                return questionnaireData;
            }
            if (AccountID < 1)
            {
                questionnaireData.ErrCode = ErrorCode.Unauthorized;
                return questionnaireData;
            }
            questionnaireData.AccountID = AccountID;
            questionnaireData.PermissionAtLevelID = permissionAtLevelID;
            questionnaireData.LanguageID = languageID;
            questionnaireData.OrganisationID = organisationID;
            if (await GetSettingsResourcesAsync(questionnaireData, true
                , $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}"
                , $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_USER_TYPE_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_TASK_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}"
            ).ConfigureAwait(false))
            {
                questionnaireData.FeatureFor = FeatureFor;
                await new PatientProviderNoteDataLayer().GetNextQuestionAsync(questionnaireData, readingCategoryID).ConfigureAwait(false);
                await SaveQuestionnaireAnswerAsync(questionnaireData).ConfigureAwait(false);
                SetCurrentQuestionAnswer(questionnaireData);
                await SaveQuestionnaireFileDocumentAsync(questionnaireData).ConfigureAwait(false);
                await UpdateCompletedTaskStatusAsync(permissionAtLevelID, questionnaireData);
                await ReplaceAttachmentLinkAsync(questionnaireData);
            }
            await ReplaceInstructionsTextLinkAsync(questionnaireData);
            questionnaireData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            questionnaireData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
        return questionnaireData;
    }

    private async Task UpdateCompletedTaskStatusAsync(long permissionAtLevelID, QuestionnaireDTO questionnaireData)
    {
        if (questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.Finish)
        {
            ProgramDTO taskData = new ProgramDTO
            {
                Tasks = new List<TaskModel> { new TaskModel {
                    PatientTaskID = Convert.ToInt64(questionnaireData.PatientTaskID, CultureInfo.InvariantCulture),
                    Status  = ResourceConstants.R_COMPLETED_STATUS_KEY
                }}
            };
            await UpdatePatientTaskStatusAsync(taskData, permissionAtLevelID);
        }
    }

    private void SetCurrentQuestionAnswer(QuestionnaireDTO questionnaireData)
    {
        if (GenericMethods.IsListNotEmpty(questionnaireData.QuestionnaireQuestionAnswers))
        {
            questionnaireData.QuestionnaireQuestionAnswer = questionnaireData.QuestionnaireQuestionAnswers.First();
        }
        else
        {
            questionnaireData.QuestionnaireQuestionAnswer = null;
        }
    }

    private async Task ReplaceInstructionsTextLinkAsync(QuestionnaireDTO questionnaireData)
    {
        if (questionnaireData.ErrCode == ErrorCode.OK
            && questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.StartQuestionnaire
            && !string.IsNullOrWhiteSpace(questionnaireData.Questionnaire?.InstructionsText))
        {
            questionnaireData.Questionnaire.InstructionsText = await ReplaceCDNLinkAsync(questionnaireData.Questionnaire.InstructionsText, new BaseDTO());
        }
    }

    private async Task ReplaceAttachmentLinkAsync(QuestionnaireDTO questionnaireData)
    {
        if (questionnaireData.FileDocument != null && questionnaireData.FileDocument.FileDocumentID != Guid.Empty)
        {
            questionnaireData.FileDocuments = new List<FileDocumentModel> { questionnaireData.FileDocument };
            await new FilesServiceBusinessLayer(_httpContext).ReplaceFileAttachmentImageCdnLinkAsync(questionnaireData.FileDocuments);
        }
        else
        {
            questionnaireData.FileDocuments = new List<FileDocumentModel>();
        }
    }

    private async Task SaveQuestionnaireFileDocumentAsync(QuestionnaireDTO questionnaireData)
    {
        if (questionnaireData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(questionnaireData?.FileDocuments))
        {
            FileDTO fileData = new FileDTO()
            {
                AccountID = questionnaireData.AccountID,
                PermissionAtLevelID = questionnaireData.PermissionAtLevelID,
                FileDocuments = questionnaireData.FileDocuments,
                File = new FileModel()
                {
                    UserID = questionnaireData.SelectedUserID
                }
            };
            await new FilesServiceBusinessLayer(_httpContext).SaveNoteDocumentsAsyncs(fileData).ConfigureAwait(false);
        }
    }

    private async Task SaveQuestionnaireAnswerAsync(QuestionnaireDTO questionnaireData)
    {
        if (questionnaireData.ErrCode == ErrorCode.OK
            && (questionnaireData.QuestionnaireQuestionAnswer?.PatientAnswerID != Guid.Empty
            && questionnaireData.Questionnaire.QuestionnaireAction != QuestionnaireAction.PreviousAndNext)
            || (questionnaireData.Questionnaire.QuestionnaireAction == QuestionnaireAction.Finish
            && questionnaireData.QuestionnaireScore?.PatientScoreID != Guid.Empty))
        {
            questionnaireData.FeatureFor = FeatureFor;
            questionnaireData.QuestionnaireQuestionAnswer.NextQuestionID = questionnaireData.Questionnaire.QuestionnaireID;
            questionnaireData.QuestionnaireQuestionAnswer.ScoreValue = Convert.ToDecimal(questionnaireData.Questionnaire.QuestionnaireCode);
            QuestionnaireDTO questionAnswers = new QuestionnaireDTO
            {
                AccountID = questionnaireData.AccountID,
                PermissionAtLevelID = questionnaireData.PermissionAtLevelID,
                QuestionnaireQuestionAnswers = new List<PatientQuestionnaireQuestionAnswersModel> { questionnaireData.QuestionnaireQuestionAnswer },
                QuestionnaireScores = questionnaireData.QuestionnaireScore == null ? null : new List<PatientQuestionnaireScoresModel> { questionnaireData.QuestionnaireScore },
                FeatureFor = FeatureFor
            };
            await new QuestionnaireServiceDataLayer().SavePatientQuestionnaireResultsAsync(questionAnswers).ConfigureAwait(false);
            questionnaireData.ErrCode = questionAnswers.ErrCode;
        }
    }

    private bool ValidateTaskStatus(ProgramDTO taskData)
    {
        return !GenericMethods.IsListNotEmpty(taskData.Tasks) && taskData.Tasks.Any(x => x.Status == string.Empty || x.PatientTaskID == 0);
    }

    private bool ValidateTask(ProgramDTO taskData)
    {
        return taskData.Task.IsActive
            ? (taskData.Task.FromDate == default || taskData.Task.ToDate == default || string.IsNullOrWhiteSpace(taskData.Task.Status) || string.IsNullOrWhiteSpace(taskData.Task.TaskType) || taskData.Task.ItemID < 1)
            : taskData.Task.PatientTaskID < 1;
    }
}
