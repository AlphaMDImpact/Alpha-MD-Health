using AlphaMDHealth.Model;

namespace AlphaMDHealth.ClientDataLayer
{
    public partial class ReadingDatabase : BaseDatabase
    {
        /// <summary>
        /// Gets User's Health Permission requests
        /// </summary>
        /// <param name="userID">ID of user for which data is to be retrived</param>
        /// <returns>list of Health permissions</returns>
        public async Task<List<UserHealthPermissionRequestModel>> GetUserHealthPermissionsAsync(long userID)
        {
            return await SqlConnection.QueryAsync<UserHealthPermissionRequestModel>
                ($"SELECT * FROM UserHealthPermissionRequestModel WHERE UserID = ?", userID).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the permission request status
        /// </summary>
        /// <param name="userHealthPermissionRequests">permission statuses to be updated</param>
        /// <returns>Returns task that updates the permission request status</returns>
        public async Task UpdatePermissionRequestStatusAsync(List<UserHealthPermissionRequestModel> userHealthPermissionRequests)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (UserHealthPermissionRequestModel type in userHealthPermissionRequests)
                {
                    if (transaction.FindWithQuery<UserHealthPermissionRequestModel>("SELECT 1 FROM UserHealthPermissionRequestModel WHERE ReadingID = ? AND UserID = ? ", type.ReadingID, type.UserID) == null)
                    {
                        transaction.Execute("INSERT INTO UserHealthPermissionRequestModel (UserID, ReadingID, IsRequested) VALUES (?,?,?)", type.UserID, type.ReadingID, type.IsRequested);
                    }
                    else
                    {
                        transaction.Execute("UPDATE UserHealthPermissionRequestModel SET IsRequested = ? WHERE ReadingID = ? AND UserID = ? ", type.IsRequested, type.ReadingID, type.UserID);
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}
