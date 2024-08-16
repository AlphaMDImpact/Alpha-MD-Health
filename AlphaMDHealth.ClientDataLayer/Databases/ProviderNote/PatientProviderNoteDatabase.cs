using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public class PatientProviderNoteDatabase : BaseDatabase
    {
        /// <summary>
        /// Save program note to local database
        /// </summary>
        /// <param name="noteData">Reference object of note</param>
        /// <returns>operation status</returns>
        public async Task SaveProgramNotesAsync(ProgramDTO noteData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(noteData.ProgramNotes))
                {
                    foreach (var programNote in noteData.ProgramNotes)
                    {
                        transaction.InsertOrReplace(programNote);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="noteData">Prgogram note I18n data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveProgramNotesI18NAsync(ProgramDTO noteData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(noteData.ProgramNotesI18N))
                {
                    foreach (ProgramNoteI18NModel item in noteData.ProgramNotesI18N)
                    {
                        if (transaction.FindWithQuery<ProgramNoteI18NModel>("SELECT 1 FROM ProgramNoteI18NModel WHERE ProgramNoteID = ? AND LanguageID = ?", item.ProgramNoteID, item.LanguageID) == null)
                        {
                            transaction.Insert(item);
                        }
                        else
                        {
                            transaction.Execute($"UPDATE ProgramNoteI18NModel SET NoteText = ?, IsActive = ? WHERE ProgramNoteID = ? AND LanguageID = ?",
                                item.NoteText, item.IsActive, item.ProgramNoteID, item.LanguageID);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Save patient provider notes to local database
        /// </summary>
        /// <param name="noteData">Reference object of note</param>
        /// <returns>operation status</returns>
        public async Task SavePatientProviderNotesAsync(PatientProviderNoteDTO noteData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(noteData.PatientProviderNotes))
                {
                    foreach (var providerNote in noteData.PatientProviderNotes)
                    {
                        SaveProviderNoteAsync(transaction, providerNote);
                    }
                }
            }).ConfigureAwait(false);
        }

        private void SaveProviderNoteAsync(SQLiteConnection transaction, PatientProviderNoteModel providerNote)
        {
            if (transaction.FindWithQuery<PatientProviderNoteModel>(
                "SELECT 1 FROM PatientProviderNoteModel WHERE ProviderNoteID=?", providerNote.ProviderNoteID) == null)
            {
                InsertProviderNote(transaction, providerNote);
            }
            else
            {
                UpdateProvidernote(transaction, providerNote);
            }
        }

        /// <summary>
        /// Get list of questions with details of a task
        /// </summary>
        /// <param name="taskData">Reference object to return patient task question data</param>
        public async Task GetTaskQuestionDetailsAsync(ProgramDTO taskData)
        {
            taskData.QuestionnaireQuestions = await GetPatinetQuestionQuestionAsync($"({Convert.ToString(taskData.Task.PatientTaskID)})", 1);
            taskData.QuestionnaireQuestiosDetails = await GetPatinetQuestionQuestionDetailAsync($"({Convert.ToString(taskData.Task.PatientTaskID)})", 1, taskData.LanguageID);
            taskData.QuestionnaireQuestionOptions = await GetPatinetQuestionQuestionOptionsAsync($"({Convert.ToString(taskData.Task.PatientTaskID)})", 1, taskData.LanguageID);
        }

        /// <summary>
        /// Get list of patient provider notes
        /// </summary>
        /// <param name="patientProviderNote">Reference object to return patient provider notes data</param>
        /// <returns>Operation status with patient provider notes in reference object</returns>
        public async Task GetPatientProviderNotesAsync(PatientProviderNoteDTO patientProviderNote)
        {
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            string pColor = (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker) ? "ifnull(PP.ProgramGroupIdentifier , P.ProgramGroupIdentifier)" : "P.ProgramGroupIdentifier";

            patientProviderNote.PatientProviderNotes = await SqlConnection.QueryAsync<PatientProviderNoteModel>(
                "SELECT DISTINCT A.ProviderNoteID, A.NoteDateTime, B.ProgramID, P.Name AS ProgramName, E.FirstName || ' ' || E.LastName AS UserName, " +
                $"{pColor} AS ProgramGroupIdentifier " +
                "FROM PatientProviderNoteModel A " +
                "LEFT JOIN UserModel E ON A.CareGiverID = E.UserID " +
                "LEFT JOIN ProgramNoteModel B ON B.ProgramNoteID = A.ProgramNoteID " +
                "LEFT JOIN ProgramModel P ON P.ProgramID = B.ProgramID " +
                "LEFT JOIN PatientProgramModel PP ON PP.ProgramID = P.ProgramID AND PP.PatientID = ? " +
                $"WHERE A.PatientID = ? AND A.IsActive = 1 " +
                $"ORDER BY A.NoteDateTime DESC ",
                patientProviderNote.SelectedUserID, patientProviderNote.SelectedUserID
            ).ConfigureAwait(false);
            if (roleID == (int)RoleName.CareTaker && patientProviderNote.PatientProviderNotes?.Count > 0)
            {
                List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(patientProviderNote).ConfigureAwait(false);
                patientProviderNote.PatientProviderNotes = sharedPrograms?.Count > 0
                    ? patientProviderNote.PatientProviderNotes.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ProgramID))?.ToList()
                    : null;
            }
            if (patientProviderNote.RecordCount > 0)
            {
                patientProviderNote.PatientProviderNotes = patientProviderNote.PatientProviderNotes?.Take((int)patientProviderNote.RecordCount)?.ToList();
            }
            string noteIDs = $" ('{string.Join("', '", patientProviderNote.PatientProviderNotes.Select(x => x.ProviderNoteID))}') ";
            patientProviderNote.QuestionnaireQuestions = await GetPatinetQuestionQuestionAsync(noteIDs, 2);
            patientProviderNote.QuestionnaireQuestiosDetails = await GetPatinetQuestionQuestionDetailAsync(noteIDs, 2, patientProviderNote.LanguageID);
            patientProviderNote.QuestionnaireQuestionOptions = await GetPatinetQuestionQuestionOptionsAsync(noteIDs, 2, patientProviderNote.LanguageID);
        }

        /// <summary>
        /// Get provider note data
        /// </summary>
        /// <param name="patientProviderNote">Reference object to return patient provider note data</param>
        /// <returns>Operation status with provider note in reference object</returns>
        public async Task GetPatientProviderNoteAsync(PatientProviderNoteDTO patientProviderNote)
        {
            patientProviderNote.PatientPrograms = await SqlConnection.QueryAsync<OptionModel>("SELECT DISTINCT A.ProgramID AS OptionID, B.Name AS OptionText FROM PatientProgramModel A " +
                $"LEFT JOIN ProgramModel B ON A.ProgramID = B.ProgramID WHERE A.PatientID = {patientProviderNote.SelectedUserID} AND A.IsActive = 1").ConfigureAwait(false);

            if (patientProviderNote.PatientProviderNote.ProviderNoteID != Guid.Empty)
            {
                patientProviderNote.PatientProviderNote = await SqlConnection.FindWithQueryAsync<PatientProviderNoteModel>("SELECT A.ProviderNoteID, A.CareGiverID, A.NoteDateTime, A.ProgramNoteID, B.ProgramID, B.QuestionnaireID " +
                    $"FROM PatientProviderNoteModel A " +
                    $"LEFT JOIN ProgramNoteModel B ON B.ProgramNoteID = A.ProgramNoteID WHERE A.ProviderNoteID = ?", patientProviderNote.PatientProviderNote.ProviderNoteID).ConfigureAwait(false);

                patientProviderNote.QuestionnaireQuestionAnswers = await SqlConnection.QueryAsync<PatientQuestionnaireQuestionAnswersModel>("SELECT * FROM PatientQuestionnaireQuestionAnswersModel " +
                     $"WHERE PatientTaskID = ? AND TaskType = 2", patientProviderNote.PatientProviderNote.ProviderNoteID.ToString()).ConfigureAwait(false);

                string dataSourceIds = $"('{string.Join("', '", patientProviderNote.QuestionnaireQuestionAnswers.Select(x => x.PatientAnswerID))}')";

                patientProviderNote.FileDocuments = await SqlConnection.QueryAsync<FileDocumentModel>
                  ($"SELECT * FROM FileDocumentModel WHERE DocumentSourceID IN {dataSourceIds} AND IsActive = 1").ConfigureAwait(false);

                if (patientProviderNote.PatientProviderNote?.ProgramID > 0 && !patientProviderNote.PatientPrograms.Any(x => x.OptionID == patientProviderNote.PatientProviderNote.ProgramID))
                {
                    patientProviderNote.PatientPrograms.Add(await SqlConnection.FindWithQueryAsync<OptionModel>("SELECT ProgramID AS OptionID, Name AS OptionText, 1 AS IsDefault FROM ProgramModel " +
                        $"WHERE ProgramID = {patientProviderNote.PatientProviderNote.ProgramID}").ConfigureAwait(false));
                }
            }
            else
            {
                if (patientProviderNote.PatientPrograms?.Count == 1)
                {
                    patientProviderNote.PatientProviderNote.ProgramID = patientProviderNote.PatientPrograms.FirstOrDefault()?.OptionID ?? 0;
                }
            }
            if (patientProviderNote.RecordCount == -2 || patientProviderNote.PatientProviderNote.ProgramID > 0)
            {
                patientProviderNote.Providers = await SqlConnection.QueryAsync<OptionModel>(
                               "SELECT DISTINCT A.CareGiverID AS OptionID, B.FirstName || ' ' || B.LastName AS OptionText " +
                               "FROM UserRelationModel A " +
                               "JOIN UserModel B ON A.CareGiverID = B.UserID AND B.IsActive = 1 " +
                               "LEFT JOIN ProgramModel D ON D.ProgramID = A.ProgramID AND D.IsActive = 1 " +
                               "LEFT JOIN PatientProgramModel E ON E.ProgramID = D.ProgramID AND E.PatientID = A.PatientID " +
                               $"WHERE A.PatientID = {patientProviderNote.SelectedUserID} AND A.IsActive = 1 AND (A.ProgramID = {0} OR A.ProgramID = {patientProviderNote.PatientProviderNote.ProgramID}) AND A.RelationID = 0").ConfigureAwait(false);
                patientProviderNote.ProgramNotes = await SqlConnection.QueryAsync<OptionModel>("SELECT DISTINCT B.QuestionnaireID AS ParentOptionID, B.ProgramNoteID AS OptionID, B.IsActive AS IsDefault, C.NoteText AS OptionText FROM PatientProgramModel A " +
                                $"JOIN ProgramNoteModel B ON A.ProgramID = B.ProgramID " +
                                $"JOIN ProgramNoteI18NModel C ON B.ProgramNoteID = C.ProgramNoteID " +
                                $"AND C.LanguageID = {patientProviderNote.LanguageID} " +
                                $"WHERE A.PatientID = {patientProviderNote.SelectedUserID} AND A.ProgramID = {patientProviderNote.PatientProviderNote.ProgramID}").ConfigureAwait(false);
             }
            if (patientProviderNote.RecordCount == -3 || patientProviderNote.PatientProviderNote.ProviderNoteID != Guid.Empty)
            {
                patientProviderNote.QuestionConditions = await SqlConnection.QueryAsync<QuestionConditionModel>("SELECT B.* FROM QuestionnaireQuestionModel A " +
                    "JOIN QuestionConditionModel B ON A.QuestionID = B.QuestionID " +
                   $"WHERE A.QuestionnaireID = {patientProviderNote.PatientProviderNote.QuestionnaireID} AND A.IsActive = 1 ").ConfigureAwait(false);

                patientProviderNote.QuestionnaireQuestions = await SqlConnection.QueryAsync<QuestionnaireQuestionModel>("SELECT * FROM QuestionnaireQuestionModel A " +
                    $"WHERE A.QuestionnaireID = {patientProviderNote.PatientProviderNote.QuestionnaireID} AND A.IsActive = 1 ").ConfigureAwait(false);
                if (patientProviderNote.QuestionnaireQuestions.Any(x => x.CategoryID > 0))
                {
                    string categoryIds = $"('{string.Join("', '", patientProviderNote.QuestionnaireQuestions.Where(x => x.CategoryID > 0).Select(x => x.CategoryID))}')";

                    patientProviderNote.Files = await SqlConnection.QueryAsync<FileModel>("SELECT * FROM FileModel " +
                   $"WHERE FileCategoryID IN {categoryIds} AND UserID = {patientProviderNote.SelectedUserID}");
                }
                patientProviderNote.QuestionnaireQuestiosDetails = await SqlConnection.QueryAsync<QuestionnaireQuestionDetailsModel>("SELECT B.* FROM QuestionnaireQuestionModel A " +
                   $"JOIN QuestionnaireQuestionDetailsModel B ON A.QuestionID = B.QuestionID AND B.LanguageID = {patientProviderNote.LanguageID} " +
                   $"WHERE A.QuestionnaireID = {patientProviderNote.PatientProviderNote.QuestionnaireID} AND A.IsActive = 1 ").ConfigureAwait(false);

                patientProviderNote.QuestionnaireQuestionOptions = await SqlConnection.QueryAsync<QuestionnaireQuestionOptionModel>("SELECT B.QuestionOptionID, B.QuestionID, C.CaptionText FROM QuestionnaireQuestionModel A " +
                    $"LEFT JOIN QuestionnaireQuestionOptionModel B ON A.QuestionID = B.QuestionID " +
                    $"LEFT JOIN QuestionnaireQuestionOptionDetailsModel C ON B.QuestionOptionID = C.QuestionOptionID " +
                    $"WHERE A.QuestionnaireID = {patientProviderNote.PatientProviderNote.QuestionnaireID} AND A.IsActive = 1 AND B.IsActive = 1 ").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Save patient provider note data
        /// </summary>
        /// <param name="noteData">object to save patient provider note data</param>
        /// <returns>Operation Status</returns>
        public async Task SaveProviderNoteAsync(PatientProviderNoteDTO noteData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveProviderNoteAsync(transaction, noteData.PatientProviderNote);
                noteData.QuestionnaireQuestionAnswers?.ForEach(x => x.PatientTaskID = noteData.PatientProviderNote.ProviderNoteID.ToString());
            }).ConfigureAwait(false);
            await SavePatientQuestionAnswerAsync(noteData).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Provider notes to sync
        /// </summary>
        /// <param name="providerNotes">Reference object to return provider notes results records</param>
        /// <returns>operation status</returns>
        public async Task GetPatientProviderNotesForSyncAsync(PatientProviderNoteDTO providerNotes)
        {
            providerNotes.PatientProviderNotes = await SqlConnection.QueryAsync<PatientProviderNoteModel>
                ($"SELECT * FROM PatientProviderNoteModel WHERE IsSynced = 0");
            providerNotes.QuestionnaireQuestionAnswers = await SqlConnection.QueryAsync<PatientQuestionnaireQuestionAnswersModel>
                ($"SELECT * FROM PatientQuestionnaireQuestionAnswersModel WHERE IsSynced = 0 AND TaskType = 2");
        }

        /// <summary>
        /// update is sync status and Provider Note id
        /// </summary>
        /// <param name="oldProviderNoteID">temprary provider Note id</param>
        /// <param name="newProviderNoteID">actual provider Note id</param>
        /// <returns><update issynced and Patient provider Note id/returns>
        public async Task UpdateProviderNoteIDAsync(string oldProviderNoteID, string newProviderNoteID)
        {
            if (oldProviderNoteID == newProviderNoteID)
            {
                await SqlConnection.RunInTransactionAsync(transaction =>
                {
                    transaction.Execute("UPDATE PatientProviderNoteModel SET IsSynced = 1 WHERE ProviderNoteID = ?", new Guid(newProviderNoteID));
                    transaction.Execute("UPDATE PatientQuestionnaireQuestionAnswersModel SET IsSynced = 1 WHERE PatientTaskID=? AND TaskType = 2", new Guid(newProviderNoteID));
                }).ConfigureAwait(false);
            }
            else
            {
                await SqlConnection.RunInTransactionAsync(transaction =>
                {
                    transaction.Execute("UPDATE PatientProviderNoteModel SET IsSynced = 1, ProviderNoteID = ? WHERE ProviderNoteID = ?", new Guid(newProviderNoteID), new Guid(oldProviderNoteID));
                    transaction.Execute("UPDATE PatientQuestionnaireQuestionAnswersModel SET IsSynced = 1, PatientTaskID=? WHERE PatientTaskID=? AND TaskType = 2",
                            new Guid(newProviderNoteID), new Guid(oldProviderNoteID));
                }).ConfigureAwait(false);
            }
        }

        private async Task<List<QuestionnaireQuestionModel>> GetPatinetQuestionQuestionAsync(string IDs, int TaskType)
        {
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            string showValueToPatientCheck = (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                ? "AND D.ShowValueToPatient = 1"
                : String.Empty;

            return (await SqlConnection.QueryAsync<QuestionnaireQuestionModel>("SELECT DISTINCT B.QuestionID, B.PatientTaskID AS ProviderNoteID, D.QuestionTypeID " +
              "FROM PatientQuestionnaireQuestionAnswersModel B " +
              $"LEFT JOIN QuestionnaireQuestionModel D ON B.QuestionID = D.QuestionID " +
              $"WHERE B.PatientTaskID IN {IDs}  AND B.TaskType = {TaskType} {showValueToPatientCheck} AND B.IsActive = 1 ORDER BY B.LastModifiedON ").ConfigureAwait(false))?.ToList();
        }

        private async Task<List<QuestionnaireQuestionDetailsModel>> GetPatinetQuestionQuestionDetailAsync(string IDs, int TaskType, byte LanguageID)
        {
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            string showValueToPatientCheck = (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                ? "AND D.ShowValueToPatient = 1" : String.Empty;

            List<QuestionnaireQuestionDetailsModel>   questionnaireQuestionDetails = await SqlConnection.QueryAsync<QuestionnaireQuestionDetailsModel>("SELECT DISTINCT B.PatientTaskID AS AnswerPlaceHolder, B.QuestionID, E.CaptionText," +
                " B.AnswerValue AS InstructionsText " +
                   "FROM PatientQuestionnaireQuestionAnswersModel B " +
                  "LEFT JOIN QuestionnaireQuestionModel D ON B.QuestionID = D.QuestionID " +
                  $"LEFT JOIN QuestionnaireQuestionDetailsModel E ON D.QuestionID = E.QuestionID AND E.LanguageID = {LanguageID} " +
                  $"WHERE B.PatientTaskID IN {IDs} AND B.TaskType = {TaskType} {showValueToPatientCheck} AND B.IsActive = 1 ").ConfigureAwait(false);

            if (TaskType == 1)
            {
                foreach (QuestionnaireQuestionDetailsModel patientAnswer in questionnaireQuestionDetails)
                {
                    Guid PatientReadingID;
                    if (Guid.TryParse(patientAnswer.InstructionsText, out PatientReadingID))
                    {
                        var AnswerValue = await SqlConnection.ExecuteScalarAsync<string>("SELECT  CASE " +
                            "WHEN R.ReadingValueTypeID IN (20, 67, 68, 69) " +
                           $"THEN (SELECT ResourceValue FROM ResourceModel WHERE ResourceKeyID = PR.ReadingValue  AND LanguageID = {LanguageID}) " +
                             "ELSE PR.ReadingValue " +
                              "END AS InstructionsText " +
                             "FROM  PatientReadingModel PR " +
                             "LEFT JOIN ReadingModel R  ON R.ReadingID = PR.ReadingID WHERE PR.PatientReadingID = ?", PatientReadingID).ConfigureAwait(false);
                        patientAnswer.InstructionsText = AnswerValue;
                    }
                }
            }
             return questionnaireQuestionDetails;
        }

        private async Task<List<QuestionnaireQuestionOptionModel>> GetPatinetQuestionQuestionOptionsAsync(string IDs, int TaskType, byte LanguageID)
        {
            return await SqlConnection.QueryAsync<QuestionnaireQuestionOptionModel>("SELECT DISTINCT B.QuestionID, C.QuestionOptionID, D.CaptionText FROM " +
                  "PatientQuestionnaireQuestionAnswersModel B " +
                  "JOIN QuestionnaireQuestionOptionModel C ON B.QuestionID = C.QuestionID " +
                  "JOIN QuestionnaireQuestionOptionDetailsModel D ON C.QuestionOptionID = D.QuestionOptionID " +
                  $"WHERE D.LanguageID = {LanguageID} AND B.PatientTaskID IN {IDs} AND B.TaskType = {TaskType} ").ConfigureAwait(false);
        }

        private async Task SavePatientQuestionAnswerAsync(PatientProviderNoteDTO providerNoteData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                transaction.Execute("UPDATE PatientQuestionnaireQuestionAnswersModel SET IsActive = 0 WHERE PatientTaskID = ? AND TaskType = 2", providerNoteData.PatientProviderNote.ProviderNoteID);
                if (providerNoteData.PatientProviderNote.IsActive)
                {
                    foreach (var answer in providerNoteData.QuestionnaireQuestionAnswers.Where(x => x.IsActive)?.ToList())
                    {
                        transaction.InsertOrReplace(answer);
                    }
                }
            }).ConfigureAwait(false);
        }

        private int InsertProviderNote(SQLiteConnection transaction, PatientProviderNoteModel providerNote)
        {
            return transaction.Execute("INSERT INTO PatientProviderNoteModel(ProviderNoteID, NoteDateTime, ProgramNoteID, CareGiverID, PatientID, IsSynced, IsActive, AddedOn) " +
                                            "VALUES(?, ?, ?, ?, ?, ?, ?, ?)", providerNote.ProviderNoteID, providerNote.NoteDateTime, providerNote.ProgramNoteID, providerNote.CareGiverID,
                                            providerNote.PatientID, providerNote.IsSynced, providerNote.IsActive, providerNote.AddedOn);
        }

        private void UpdateProvidernote(SQLiteConnection transaction, PatientProviderNoteModel providerNote)
        {
            transaction.Execute("UPDATE PatientProviderNoteModel SET ProgramNoteID=?, NoteDateTime =?, CareGiverID=?, PatientID=?, IsSynced=?, IsActive=?, AddedOn=? " +
                                            "WHERE ProviderNoteID = ?", providerNote.ProgramNoteID, providerNote.NoteDateTime, providerNote.CareGiverID,
                                            providerNote.PatientID, providerNote.IsSynced, providerNote.IsActive, providerNote.AddedOn, providerNote.ProviderNoteID);
        }
    }
}