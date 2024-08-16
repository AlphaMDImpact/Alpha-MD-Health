using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    public class RoleDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="professionsData">Profession data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SaveRolessAsync(RoleDTO roleData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (roleData.LastModifiedON == null)
                {
                    //When Last sync datetime is default datetime, clear all existing records from DB and insert new one for avoid duplicate record issue as each server will contains different ServerID(PrimaryKey) and InsertOrReplace() function update records based on PrimaryKey
                    transaction.Execute("DELETE FROM UserRolesModel");
                }
                if (GenericMethods.IsListNotEmpty(roleData.Roles))
                {
                    foreach (UserRolesModel role in roleData.Roles)
                    {
                        transaction.InsertOrReplace(role);
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task GetRolesAsync(RoleDTO roleData)
        {
            roleData.Roles = await SqlConnection.QueryAsync<UserRolesModel>
                ("SELECT * FROM UserRolesModel WHERE IsActive = 1 ORDER BY RoleName ASC").ConfigureAwait(false);
        }
    }
}
