using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.HealthLibrary
{
    public class HealthLibrary
    {
        private readonly IHealthLibrary _HealthLibrary = DependencyService.Get<IHealthLibrary>();

        /// <summary>
        /// Opens AppleHealth App to manually give permission
        /// </summary>
        /// <returns>true if successfully navigated to AppleHealth App</returns>
        public async Task<bool> OpenPermissionApp()
        {
            try
            {
                return await _HealthLibrary.OpenPermissionApp().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the user has provided the permission for the given reading types
        /// If user has not provided the permission will ask for permission
        /// </summary>
        /// <param name="readingTypes">Type of reading</param>
        /// <param name="permissionType">Type of permission required</param>
        /// <returns>Status of permission</returns>
        public async Task<ErrorCode> CheckPermissionAsync(List<ReadingType> readingTypes, PermissionType permissionType)
        {
            try
            {
                return await _HealthLibrary.CheckPermissionAsync(readingTypes, permissionType).ConfigureAwait(false);
            }
            catch
            {
                return ErrorCode.Unauthorized;
            }
        }

        /// <summary>
        /// Gets the health readings based on the given fromData and toDate aggregated by the aggregate type and timeframe
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <param name="fromDate">Date from which the data is to be fetched</param>
        /// <param name="toDate">Date up to which the data is to be fetched</param>
        /// <param name="aggregateType">Type of aggregation</param>
        /// <param name="aggregateTimeframe">Timeframe on which the data is to be aggregated</param>
        /// <returns>List of readings along with the value, unit and datetime. Also returns the status of operation.</returns>
        public async Task<HealthReadingDTO> ReadDataAsync(ReadingType readingType, DateTimeOffset fromDate, DateTimeOffset toDate, AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe)
        {
            return await ReadDataAsync(fromDate, toDate, aggregateType, aggregateTimeframe, readingType).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the health readings based on the given activity types, fromDate and toDate aggregated by the aggregate type and timeframe
        /// </summary>
        /// <param name="fromDate">Date from which the data is to be fetched</param>
        /// <param name="toDate">Date up to which the data is to be fetched</param>
        /// <param name="aggregateType">Type of aggregation</param>
        /// <param name="aggregateTimeframe">Timeframe on which the data is to be aggregated</param>
        /// <param name="readingTypes">Type of readings</param>
        /// <returns>List of readings along with the value, unit and datetime. Also returns the status of operation.</returns>
        [Obsolete]
        public async Task<HealthReadingDTO> ReadDataAsync(DateTimeOffset fromDate, DateTimeOffset toDate, AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe, params ReadingType[] readingTypes)
        {
            HealthReadingDTO readingData = new HealthReadingDTO { ErrCode = ErrorCode.Unauthorized, Errors = new Dictionary<ReadingType, ErrorCode>(), HealthReadings = new List<HealthReadingModel>(), NutritionReadings = new List<NutritionReadingModel>() };
            try
            {
                List<ReadingType> readingTypeList = readingTypes.ToList();
                if (readingTypes.Contains(ReadingType.Vitals))
                {
                    readingTypeList.Remove(ReadingType.Vitals);
                    readingTypeList.AddRange(GenericMethods.GetVitalReadingTypes());
                }
                List<ReadingType> workoutTypes = new List<ReadingType>();
                List<ReadingType> nutritionTypes = new List<ReadingType>();
                foreach (var readingType in readingTypeList)
                {
                    if (GenericMethods.IsReadingWorkout(readingType))
                    {
                        workoutTypes.Add(readingType);
                    }
                    else if (Microsoft.Maui.Controls.Device.RuntimePlatform == DevicePlatform.Android.ToString() && GenericMethods.IsReadingNutrition(readingType))
                    {
                        nutritionTypes.Add(readingType);
                    }
                    else
                    {
                        await ReadDataAndMapToResult(readingType, fromDate, toDate, aggregateType, aggregateTimeframe, readingData, false).ConfigureAwait(false);
                    }
                }
                if (workoutTypes.Count > 0)
                {
                    await ReadDataAndMapToResult(ReadingType.Workout, fromDate, toDate, aggregateType, aggregateTimeframe, readingData, true, workoutTypes.ToArray()).ConfigureAwait(false);
                }
                if (nutritionTypes.Count > 0)
                {
                    await ReadDataAndMapToResult(ReadingType.Nutrition, fromDate, toDate, aggregateType, aggregateTimeframe, readingData, true, nutritionTypes.ToArray()).ConfigureAwait(false);
                }
            }
            catch
            {
                readingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return readingData;
        }

        /// <summary>
        /// Gets the health readings based on the given fromDate and toDate aggregated by the aggregate type and timeframe
        /// </summary>
        /// <param name="readingType"></param>
        /// <param name="fromDate">Date from which the data is to be fetched</param>
        /// <param name="toDate">Date up to which the data is to be fetched</param>
        /// <param name="aggregateType">Type of aggregation</param>
        /// <param name="aggregateTimeframe">Timeframe on which the data is to be aggregated</param>
        /// <param name="excludedTypes">The type of activities that are to be excluded</param>
        /// <returns>List of readings along with the value, unit and datetime. Also returns the status of operation.</returns>
        public async Task<HealthReadingDTO> ReadOtherReadingTypesAsync(ReadingType readingType, DateTimeOffset fromDate, DateTimeOffset toDate, AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe, params ReadingType[] excludedTypes)
        {
            HealthReadingDTO readingData = new HealthReadingDTO { ErrCode = ErrorCode.Unauthorized, Errors = new Dictionary<ReadingType, ErrorCode>(), HealthReadings = new List<HealthReadingModel>(), NutritionReadings = new List<NutritionReadingModel>() };
            if (readingType == ReadingType.Workout)
            {
                await ReadDataAndMapToResult(ReadingType.Workout, fromDate, toDate, aggregateType, aggregateTimeframe, readingData, false, excludedTypes).ConfigureAwait(false);
            }
            else
            {
                if(readingType == ReadingType.Vitals)
                {
                    readingData = await ReadDataAsync(fromDate, toDate, aggregateType, aggregateTimeframe, GenericMethods.GetVitalReadingTypes().Except(excludedTypes).ToArray()).ConfigureAwait(false);
                }
            }
            return readingData;
        }

        /// <summary>
        /// Writes the given readings to HealthKit/Google Fit
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <param name="healthReadings">List of health readings to be stored</param>
        /// <returns>Status of operation in healthReadings as reference</returns>
        public async Task WriteDataAsync(ReadingType readingType, HealthReadingDTO healthReadings)
        {
            try
            {
                await _HealthLibrary.WriteDataAsync(readingType, healthReadings).ConfigureAwait(false);
            }
            catch
            {
                healthReadings.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        private async Task ReadDataAndMapToResult(ReadingType readingType, DateTimeOffset fromDate, DateTimeOffset toDate, AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe, HealthReadingDTO readings, bool shouldInclude, params ReadingType[] readingTypes)
        {
            HealthReadingDTO reading = new HealthReadingDTO();
            try
            {
                if (GenericMethods.IsValidAggregationRequested(readingType, aggregateType, aggregateTimeframe))
                {
                    await _HealthLibrary.GetDataAsync(reading, readingType, fromDate, toDate, aggregateType, aggregateTimeframe, shouldInclude, readingTypes).ConfigureAwait(false);
                }
                else
                {
                    reading.ErrCode = ErrorCode.BadRequest;
                    reading.Response = "Invalid Aggregation request.";
                }
            }
            catch
            {
                reading.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            if (reading.ErrCode == ErrorCode.OK)
            {
                if (reading.HealthReadings?.Count > 0)
                {
                    readings.HealthReadings.AddRange(reading.HealthReadings);
                }
                if (reading.NutritionReadings?.Count > 0)
                {
                    readings.NutritionReadings.AddRange(reading.NutritionReadings);
                }
                readings.ErrCode = ErrorCode.OK;
            }
            else
            {
                GenericMethods.AddErrorCodeToResponse(readings.Errors, readingType, reading.ErrCode);
            }
        }
    }
}