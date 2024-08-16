using AlphaMDHealth.Model;

namespace AlphaMDHealth.ClientDataLayer
{
    public class CaregiverDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="programData">Program caregiver data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveProgramCaregiversAsync(ProgramDTO programData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (CaregiverModel program in programData.ProgramCareGivers)
                {
                    transaction.InsertOrReplace(program);
                }
            }).ConfigureAwait(false);
        }
    }
}