using AlphaMDHealth.Model;

namespace AlphaMDHealth.ClientDataLayer
{
    public class ProfessionDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="professionsData">Profession data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveProfessionsAsync(ProfessionDTO professionsData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (UserProfessionModel profession in professionsData.UserProfessions)
                {
                    transaction.InsertOrReplace(profession);
                }
            }).ConfigureAwait(false);
        }

        public async Task GetProfessionsAsync(ProfessionDTO professionData)
        {
            professionData.UserProfessions = await SqlConnection.QueryAsync<UserProfessionModel>
                ("SELECT * FROM UserProfessionModel WHERE IsActive = 1 ORDER BY Profession ASC").ConfigureAwait(false);
        }
    }
}