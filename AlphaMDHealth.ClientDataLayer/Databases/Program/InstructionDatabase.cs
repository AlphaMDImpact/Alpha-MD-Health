using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    public class InstructionDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="programData">Instruction data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveInstructionsAsync(ProgramDTO programData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(programData.Instructions))
                {
                    foreach (InstructionModel item in programData.Instructions)
                    {
                        transaction.InsertOrReplace(item);
                    }
                }
            });
        }

        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="programData">InstructionI18N data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveInstructionI18NAsync(ProgramDTO programData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(programData.InstructionsI18N))
                {
                    foreach (InstructionI18NModel item in programData.InstructionsI18N)
                    {
                        if (transaction.FindWithQuery<InstructionI18NModel>("SELECT 1 FROM InstructionI18NModel WHERE InstructionID = ? AND LanguageID = ?", item.InstructionID, item.LanguageID) == null)
                        {
                            transaction.Insert(item);
                        }
                        else
                        {
                            transaction.Execute($"UPDATE InstructionI18NModel SET Name = ?, Description = ?, IsActive = ? WHERE InstructionID = ? AND LanguageID = ?",
                                item.Name, item.Description, item.IsActive, item.InstructionID, item.LanguageID);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Gets Instruction(s) and task status
        /// </summary>
        /// <param name="instructionData">Object to receive transfer filter conditions and return instruction data</param>
        /// <returns>Instruction Data with Operation Status</returns>
        public async Task GetInstructionAsync(ProgramDTO programTaskData)
        {
            if (programTaskData.PermissionAtLevelID > 0)
            {
                var taskData = await SqlConnection.FindWithQueryAsync<TaskModel>("SELECT * FROM TaskModel WHERE PatientTaskID = ?", programTaskData.Task.PatientTaskID);
                if (taskData != null)
                {
                    programTaskData.AddedBy = taskData.Status;
                }
            }
            programTaskData.InstructionI18N = await SqlConnection.FindWithQueryAsync<InstructionI18NModel>
                ($"SELECT * FROM InstructionI18NModel WHERE InstructionID = ? AND IsActive = 1 AND LanguageID = ?", programTaskData.Task.ItemID, programTaskData.LanguageID).ConfigureAwait(false);
        }
    }
}
