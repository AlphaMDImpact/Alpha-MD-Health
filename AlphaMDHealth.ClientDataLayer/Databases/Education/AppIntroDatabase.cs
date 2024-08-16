using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer;

public class AppIntroDatabase : BaseDatabase
{
    /// <summary>
    /// Get app intros from database
    /// </summary>
    /// <param name="appIntroData"> reference object data to get data</param>
    /// <returns>List of App intros</returns>
    public async Task GetAppIntrosAsync(AppIntroDTO appIntroData)
    {
        appIntroData.AppIntros = await SqlConnection.QueryAsync<AppIntroModel>("SELECT * FROM AppIntroModel WHERE IsActive = 1").ConfigureAwait(false);
    }

    /// <summary>
    /// Save app intros into database
    /// </summary>
    /// <param name="appIntroData"> reference object data to be saved</param>
    /// <returns> operation status </returns>
    public async Task SaveAppIntrosAsync(AppIntroDTO appIntroData)
    {
        await SqlConnection.RunInTransactionAsync(transaction =>
        {
            if (GenericMethods.IsListNotEmpty(appIntroData.AppIntros))
            {
                foreach (AppIntroModel file in appIntroData.AppIntros)
                {
                    transaction.InsertOrReplace(file);
                }
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Get app intro data for which base64 data is not downloaded
    /// </summary>
    /// <param name="appIntroData">Reference object to return app intro records</param>
    /// <returns>operation status</returns>
    public async Task GetImagesToDownloadAsync(AppIntroDTO appIntroData)
    {
        appIntroData.AppIntros = await SqlConnection.QueryAsync<AppIntroModel>("SELECT IntroSlideID, ImageName, IsActive FROM AppIntroModel WHERE IsActive = 1").ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if app intro is required
    /// </summary>
    /// <returns>true if required else false</returns>
    public async Task<bool> CheckIfAppIntroRequiredAsync()
    {
        return await SqlConnection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM AppIntroModel WHERE IsActive = 1").ConfigureAwait(false) > 0;
    }
}
