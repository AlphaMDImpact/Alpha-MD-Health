using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    public class OrganisationDatabase : BaseDatabase
    {

        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="organisationData">Organisation Language data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveOrganisationDataAsync(OrganisationDTO organisationData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                organisationData.LastModifiedON = GenericMethods.GetUtcDateTime;
                foreach (OrganisationModel organisation in organisationData.Organisations)
                {
                    transaction.InsertOrReplace(organisation);
                }
            }).ConfigureAwait(false);
        }
    }
}