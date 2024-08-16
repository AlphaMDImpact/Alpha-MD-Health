using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.HealthLibrary
{
    public interface IHealthLibrary
    {
        /// <summary>
        /// Opens AppleHealth App to manually give permission
        /// </summary>
        /// <returns>true if successfully navigated to AppleHealth App</returns>
        Task<bool> OpenPermissionApp();

        /// <summary>
        /// Checks if the user has provided the permission for the given reading types
        /// If user has not provided the permission will ask for permission
        /// </summary>
        /// <param name="readingTypes">Type of reading</param>
        /// <param name="permissionType">Type of permission required</param>
        /// <returns>Status of permission</returns>
        Task<ErrorCode> CheckPermissionAsync(List<ReadingType> readingTypes, PermissionType permissionType);

        /// <summary>
        /// Gets Readings From AppleHealth / Google fit and returns in DTO
        /// </summary>
        /// <param name="readings">DTO object to which data is returned</param>
        /// <param name="readingType">Type of reading</param>
        /// <param name="fromDate">Date from which the data is to be fetched</param>
        /// <param name="toDate">Date up to which the data is to be fetched</param>
        /// <param name="aggregateType">Type of aggregation</param>
        /// <param name="aggregateTimeframe">Timeframe on which the data is to be aggregated</param>
        /// <param name="shouldInclude">should include provided types or return all from reading data</param>
        /// <param name="readingTypes">Types of reading to include. (do not pass in case no types included)</param>
        /// <returns>If permission is given then list of readings otherwise appropriate errorcode</returns>
        Task GetDataAsync(HealthReadingDTO readings, ReadingType readingType, DateTimeOffset fromDate, DateTimeOffset toDate, AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe, bool shouldInclude, params ReadingType[] readingTypes);

        /// <summary>
        /// Writes the given readings to HealthKit/Google Fit
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <param name="healthReadings">List of health readings to be stored</param>
        /// <returns>Status of operation in healthReadings as reference</returns>
        Task WriteDataAsync(ReadingType readingType, HealthReadingDTO healthReadings);
    }
}