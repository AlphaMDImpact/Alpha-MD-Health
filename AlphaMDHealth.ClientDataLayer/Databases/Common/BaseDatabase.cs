using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public class BaseDatabase : IDisposable
    {
        /// <summary>
        /// Gets the Sqlite Database connection
        /// </summary>
        /// <returns>Sqlite connection</returns>
        public static SQLiteAsyncConnection SqlConnection { get; protected set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        /// <summary>
        /// Method to fetch shared programs data
        /// </summary>
        /// <param name="pageData"></param>
        /// <returns></returns>
        protected async Task<List<PatientProgramModel>> GetActiveSharedProgramsAsync(BaseDTO pageData)
        {
            var sharedPrograms = await SqlConnection.QueryAsync<PatientProgramModel>(
                "SELECT DISTINCT P.ProgramID, PP.PatientProgramID, PP.PatientID, UR.FromDate AS AddedON, UR.ToDate AS LastModifiedON, " +
                "(CASE WHEN UR.IsActive = 1 AND PSP.IsActive = 1 AND PP.IsActive = 1 AND P.IsActive = 1 THEN 1 ELSE 0 END) AS IsActive " +
                "FROM UserModel U " +
                "JOIN UserRelationModel UR ON UR.CareGiverID = U.UserID AND UR.RelationID > 0 " +
                "JOIN PatientsSharedProgramsModel PSP ON PSP.PatientCareGiverID = UR.PatientCareGiverID " +
                "JOIN PatientProgramModel PP ON PP.PatientProgramID = PSP.PatientProgramID AND PP.PatientID = UR.PatientID " +
                "JOIN ProgramModel P ON PP.ProgramID= P.ProgramID  " +
                "WHERE U.AccountID = ? AND UR.PatientID = ? AND UR.IsActive = 1 AND PSP.IsActive = 1 AND PP.IsActive = 1 AND P.IsActive = 1 "
                , pageData.AccountID, pageData.SelectedUserID
            ).ConfigureAwait(false);
            sharedPrograms = sharedPrograms?.Where(y =>
                y.AddedON.Ticks <= GenericMethods.GetUtcDateTime.Ticks &&
                y.LastModifiedON.Value.Ticks > GenericMethods.GetUtcDateTime.Ticks).ToList();
            return sharedPrograms;
        }
    }
}