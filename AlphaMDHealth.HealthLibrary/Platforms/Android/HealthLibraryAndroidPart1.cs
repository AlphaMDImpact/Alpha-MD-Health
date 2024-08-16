//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.Gms.Auth.Api.SignIn;
//using Android.Gms.Common.Apis;
//using Android.Gms.Fitness;
//using Android.Gms.Fitness.Data;
//using Android.Gms.Fitness.Request;
//using Android.Gms.Fitness.Result;
//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;
//using HealthLibrary.Android;
//using AlphaMDHealth.Model;
//using Java.Util.Concurrent;
//using Plugin.CurrentActivity;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xamarin.Essentials;
//using Xamarin.Forms;

using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
using Android.Gms.Fitness;
using Android.Gms.Fitness.Data;
using Android.Gms.Fitness.Request;
using Android.Gms.Fitness.Result;
using Java.Util.Concurrent;
using System.Data;
using DataSet = Android.Gms.Fitness.Data.DataSet;
using DataSource = Android.Gms.Fitness.Data.DataSource;
using DataType = Android.Gms.Fitness.Data.DataType;

//[assembly: Dependency(typeof(HealthLibraryAndroid))]
namespace AlphaMDHealth.HealthLibrary.Platforms
{
    public partial class HealthLibraryAndroid : IHealthLibrary
    {
        /// <summary>
        /// Returns TRUE as not required in case of Android
        /// </summary>
        /// <returns></returns>
        public Task<bool> OpenPermissionApp()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Task completion for permission request from GoogleFit
        /// </summary>
        public static TaskCompletionSource<bool> FitPermissionCompletionSource { get; private set; }

        /// <summary>
        /// Checks if the user has provided the permission for the given reading types
        /// If user has not provided the permission will ask for permission
        /// </summary>
        /// <param name="readingTypes">Type of reading</param>
        /// <param name="permissionType">Type of permission required</param>
        /// <returns>Status of permission</returns>
        public async Task<ErrorCode> CheckPermissionAsync(List<ReadingType> readingTypes, PermissionType permissionType)
        {
            FitnessOptions fitnessOptions = GetFitnessOptionsBasedReadingTypes(readingTypes, permissionType);
            GoogleSignInAccount googleAccount = GetLastGoogleSignInAccount();
            return HasPermissions(fitnessOptions)
                ? ErrorCode.OK
                : await RequestUserPermissionAsync(googleAccount, fitnessOptions).ConfigureAwait(false);
        }

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
        public async Task GetDataAsync(HealthReadingDTO readings, ReadingType readingType, DateTimeOffset fromDate, DateTimeOffset toDate,
            AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe, bool shouldInclude, params ReadingType[] readingTypes)
        {
            if (HasPermissions(GetFitnessOptionsBasedReadingTypes(new List<ReadingType> { readingType }, PermissionType.Read)))
            {
                // Start of POSIX time
                DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                // Converts DateTime to Miliseconds
                long startTime = (long)(fromDate.UtcDateTime - unixEpoch).TotalMilliseconds;
                // Last millisecond of the current date
                long endTime = (long)(toDate.UtcDateTime - unixEpoch).TotalMilliseconds;
                if (readingType == ReadingType.Sleep)
                {
                    await FetchSleepReadingAsync(readings, startTime, endTime, aggregateType, aggregateTimeframe).ConfigureAwait(false);
                }
                else
                {
                    await FetchReadingAsync(readings, readingType, startTime, endTime, aggregateType, aggregateTimeframe, shouldInclude, readingTypes).ConfigureAwait(false);
                }
            }
            else
            {

                readings.ErrCode = ErrorCode.Unauthorized;
                readings.Response = "Permission Denied.";
            }
        }

        /// <summary>
        /// Writes the given readings to Google Fit
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <param name="healthReadings">List of health readings to be stored</param>
        /// <returns>Status of operation</returns>
        public async Task WriteDataAsync(ReadingType readingType, HealthReadingDTO healthReadings)
        {
            try
            {
                List<ErrorCode> statuses = new List<ErrorCode>();
                if (HasPermissions(GetFitnessOptionsBasedReadingTypes(new List<ReadingType> { readingType }, PermissionType.Write)))
                {
                    var historyClient = FitnessClass.GetHistoryClient(Platform.CurrentActivity, GetLastGoogleSignInAccount());
                    DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    if (readingType == ReadingType.Nutrition)
                    {
                        await WriteNutritionAsync(healthReadings, historyClient, unixEpoch).ConfigureAwait(false);
                    }
                    else if (readingType == ReadingType.BloodPressure)
                    {
                        await WriteBloodPressureAsync(healthReadings, historyClient, unixEpoch).ConfigureAwait(false);
                    }
                    else if (readingType == ReadingType.Sleep)
                    {
                        await WriteSleepAsync(healthReadings, unixEpoch).ConfigureAwait(false);
                    }
                    else if (readingType == ReadingType.Workout)
                    {
                        await WriteWorkoutAsync(healthReadings, unixEpoch).ConfigureAwait(false);
                    }
                    else
                    {
                        DataSource dataSourceObject;
                        DataSet dataSetObject;
                        DataPoint dataPointObject;
                        foreach (HealthReadingModel item in healthReadings.HealthReadings)
                        {
                            dataSourceObject = GetDataSourceBuilder().SetDataType(GetDataTypeForReading(item.ReadingType)).Build();
                            dataSetObject = DataSet.Create(dataSourceObject);
                            dataPointObject = DataPoint.Create(dataSourceObject);
                            MakeDataPointObjectForReading(unixEpoch, dataPointObject, item);
                            dataSetObject.Add(dataPointObject);
                            await InsertHistoryReadingAsync(historyClient, dataSetObject, item).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    healthReadings.ErrCode = ErrorCode.InternalServerError;
                }
            }
            catch (ApiException ex)
            {
                healthReadings.ErrCode = SetPermissionDenied(ex) ? ErrorCode.Unauthorized : ErrorCode.ErrorWhileSavingRecords;
            }
            catch
            {
                healthReadings.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        private async Task InsertHistoryReadingAsync(HistoryClient historyClient, DataSet dataSetObject, HealthReadingModel item)
        {
            try
            {
                await historyClient.InsertDataAsync(dataSetObject).ConfigureAwait(false);
                item.ErrCode = ErrorCode.OK;
            }
            catch (ApiException ex)
            {
                item.ErrCode = SetPermissionDenied(ex) ? ErrorCode.Unauthorized : ErrorCode.ErrorWhileSavingRecords;
            }
            catch
            {
                item.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        private async Task InsertSessionReadingAsync(SessionsClient sessionsClient, HealthReadingModel item, SessionInsertRequest request)
        {
            try
            {
                await sessionsClient.InsertSessionAsync(request).ConfigureAwait(false);
                item.ErrCode = ErrorCode.OK;
            }
            catch (ApiException ex)
            {
                item.ErrCode = SetPermissionDenied(ex) ? ErrorCode.Unauthorized : ErrorCode.ErrorWhileSavingRecords;
            }
            catch
            {
                item.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        private async Task WriteWorkoutAsync(HealthReadingDTO healthReadings, DateTime unixEpoch)
        {
            DataSource dataSourceObject;
            DataSet dataSetObject;
            DataPoint dataPointObject;
            SessionsClient sessionsClient = FitnessClass.GetSessionsClient(Platform.CurrentActivity, GetLastGoogleSignInAccount());
            foreach (HealthReadingModel item in healthReadings.HealthReadings)
            {
                DateTimeOffset endDateTime = item.CreatedOn.AddMinutes(item.Duration);
                long startTimeStamp = (long)(item.CreatedOn.UtcDateTime - unixEpoch).TotalMilliseconds;
                long endTimeStamp = (long)(endDateTime.UtcDateTime - unixEpoch).TotalMilliseconds;
                Session session = new Session.Builder().SetName($"IntegrationLibrary - {item.ActivityType}")
                    .SetIdentifier("From IntegrationLibrary")
                    .SetDescription("Data Save Request")
                    .SetStartTime(startTimeStamp, TimeUnit.Milliseconds)
                    .SetEndTime(endTimeStamp, TimeUnit.Milliseconds)
                    .SetActivity(GetActivityForReadingType((ReadingType)Enum.Parse(typeof(ReadingType), item.ActivityType)))
                    .SetActiveTime(Convert.ToInt64(item.Duration), TimeUnit.Minutes)
                    .Build();

                dataSourceObject = GetDataSourceBuilder().SetDataType(DataType.TypeCaloriesExpended).Build();
                dataSetObject = DataSet.Create(dataSourceObject);
                dataPointObject = DataPoint.Create(dataSourceObject).SetTimeInterval(startTimeStamp, endTimeStamp, TimeUnit.Milliseconds);
                dataPointObject.GetValue(Field.FieldCalories).SetFloat((float)item.ReadingValue);
                dataSetObject.Add(dataPointObject);
                SessionInsertRequest request = new SessionInsertRequest.Builder().SetSession(session).AddDataSet(dataSetObject).Build();
                await InsertSessionReadingAsync(sessionsClient, item, request).ConfigureAwait(false);
            }
        }

        private async Task WriteSleepAsync(HealthReadingDTO healthReadings, DateTime unixEpoch)
        {
            var sessionsClient = FitnessClass.GetSessionsClient(Platform.CurrentActivity, GetLastGoogleSignInAccount());
            foreach (HealthReadingModel item in healthReadings.HealthReadings)
            {
                DateTimeOffset endDateTime = item.CreatedOn.AddMinutes(item.ReadingValue);
                long startTimeStamp = (long)(item.CreatedOn.UtcDateTime - unixEpoch).TotalMilliseconds;
                long endTimeStamp = (long)(endDateTime.UtcDateTime - unixEpoch).TotalMilliseconds;
                Session session = new Session.Builder().SetName("From IntegrationLibrary")
                    .SetIdentifier("From IntegrationLibrary")
                    .SetDescription("Data Save Request")
                    .SetStartTime(startTimeStamp, TimeUnit.Milliseconds)
                    .SetEndTime(endTimeStamp, TimeUnit.Milliseconds)
                    .SetActivity(FitnessActivities.Sleep)
                    .Build();
                SessionInsertRequest request = new SessionInsertRequest.Builder().SetSession(session).Build();
                await InsertSessionReadingAsync(sessionsClient, item, request).ConfigureAwait(false);
            }
        }

        private async Task WriteBloodPressureAsync(HealthReadingDTO healthReadings, HistoryClient historyClient, DateTime unixEpoch)
        {
            DataSource dataSourceObject;
            DataSet dataSetObject;
            DataPoint dataPointObject;
            List<List<HealthReadingModel>> bloodPressureGroup = healthReadings.HealthReadings.GroupBy(x => x.CreatedOn).Select(y => y.ToList()).ToList();
            foreach (List<HealthReadingModel> item in bloodPressureGroup)
            {
                long readingTimeStamp = (long)(item.First().CreatedOn.UtcDateTime - unixEpoch).TotalMilliseconds;
                dataSourceObject = GetDataSourceBuilder().SetDataType(HealthDataTypes.TypeBloodPressure).Build();
                dataSetObject = DataSet.Create(dataSourceObject);
                dataPointObject = DataPoint.Create(dataSourceObject);
                dataPointObject.SetTimestamp(readingTimeStamp, TimeUnit.Milliseconds);
                dataPointObject.GetValue(HealthFields.FieldBloodPressureSystolic)
                    .SetFloat((float)item.Find(x => x.ReadingType == ReadingType.BPSystolic).ReadingValue);
                dataPointObject.GetValue(HealthFields.FieldBloodPressureDiastolic)
                    .SetFloat((float)item.Find(x => x.ReadingType == ReadingType.BPDiastolic).ReadingValue);
                dataPointObject.GetValue(HealthFields.FieldBodyPosition)
                    .SetInt(HealthFields.BodyPositionSitting);
                dataPointObject.GetValue(HealthFields.FieldBloodPressureMeasurementLocation)
                    .SetInt(HealthFields.BloodPressureMeasurementLocationLeftUpperArm);
                dataSetObject.Add(dataPointObject);
                await InsertHistoryReadingAsync(historyClient, dataSetObject, item[0]).ConfigureAwait(false);
                item.ForEach((data) => { data.ErrCode = item[0].ErrCode; });
            }
        }

        private async Task WriteNutritionAsync(HealthReadingDTO healthReadings, HistoryClient historyClient, DateTime unixEpoch)
        {
            DataSource dataSourceObject;
            DataSet dataSetObject;
            DataPoint dataPointObject;
            foreach (NutritionReadingModel item in healthReadings.NutritionReadings)
            {
                long readingTimeStamp = (long)(item.ReadingDateTime.UtcDateTime - unixEpoch).TotalMilliseconds;
                dataSourceObject = GetDataSourceBuilder().SetDataType(DataType.TypeNutrition).Build();
                dataSetObject = DataSet.Create(dataSourceObject);
                dataPointObject = DataPoint.Create(dataSourceObject);
                dataPointObject.SetTimestamp(readingTimeStamp, TimeUnit.Milliseconds);
                dataPointObject.GetValue(Field.FieldFoodItem).SetString(item.FoodName);
                dataPointObject.GetValue(Field.FieldMealType).SetInt(GetMealType(item.FoodType));
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientCalories, (float)item.CalorieIntake);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientProtein, (float)item.Protein);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientTotalCarbs, (float)item.Carbs);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientCholesterol, (float)item.Cholesterol);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientTotalFat, (float)item.Fat);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientSaturatedFat, (float)item.SaturatedFat);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientUnsaturatedFat, (float)item.UnsaturatedFat);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientDietaryFiber, (float)item.Fiber);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientSodium, (float)item.Sodium);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientSugar, (float)item.Sugar);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientVitaminA, (float)item.VitaminA);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientVitaminC, (float)item.VitaminC);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientPotassium, (float)item.Potassium);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientCalcium, (float)item.Calcium);
                dataPointObject.GetValue(Field.FieldNutrients).SetKeyValue(Field.NutrientIron, (float)item.Iron);
                dataSetObject.Add(dataPointObject);
                await historyClient.InsertDataAsync(dataSetObject).ConfigureAwait(false);
                item.ErrCode = ErrorCode.OK;
            }
        }

        private void MakeDataPointObjectForReading(DateTime unixEpoch, DataPoint dataPointObject, HealthReadingModel item)
        {
            long readingTimeStamp = (long)(item.CreatedOn.UtcDateTime - unixEpoch).TotalMilliseconds;
            dataPointObject.SetTimestamp(readingTimeStamp, TimeUnit.Milliseconds);
            switch (item.ReadingType)
            {
                case ReadingType.BloodGlucose:
                    Tuple<int, int, int> fieldValues = GetMealRelation(item.ReadingMoment.Value);
                    dataPointObject.GetValue(HealthFields.FieldBloodGlucoseLevel).SetFloat((float)item.ReadingValue); // in mmol/L
                    dataPointObject.GetValue(HealthFields.FieldTemporalRelationToMeal).SetInt(fieldValues.Item1);
                    dataPointObject.GetValue(Field.FieldMealType).SetInt(fieldValues.Item2);
                    dataPointObject.GetValue(HealthFields.FieldTemporalRelationToSleep).SetInt(fieldValues.Item3);
                    dataPointObject.GetValue(HealthFields.FieldBloodGlucoseSpecimenSource).SetInt(HealthFields.BloodGlucoseSpecimenSourceCapillaryBlood);
                    break;
                case ReadingType.BodyTemperature:
                    dataPointObject.GetValue(HealthFields.FieldBodyTemperature).SetFloat((float)item.ReadingValue);
                    dataPointObject.GetValue(HealthFields.FieldBodyTemperatureMeasurementLocation).SetInt(HealthFields.BodyTemperatureMeasurementLocationAxillary);
                    break;
                case ReadingType.BodyFat:
                    dataPointObject.GetValue(Field.FieldPercentage).SetFloat((float)item.ReadingValue);
                    break;
                case ReadingType.Steps:
                    long startTimeSteps = (long)(item.CreatedOn.UtcDateTime - unixEpoch).TotalMilliseconds;
                    long endTimeSteps = (long)(item.CreatedOn.AddHours(1).UtcDateTime - unixEpoch).TotalMilliseconds;
                    dataPointObject.SetTimeInterval(startTimeSteps, endTimeSteps, TimeUnit.Milliseconds);
                    dataPointObject.GetValue(Field.FieldSteps).SetInt((int)item.ReadingValue);
                    break;
                case ReadingType.Weight:
                    dataPointObject.GetValue(Field.FieldWeight).SetFloat((float)item.ReadingValue);
                    break;
                case ReadingType.HeartRate:
                    dataPointObject.GetValue(Field.FieldBpm).SetFloat((float)item.ReadingValue);
                    break;
                case ReadingType.Height:
                    dataPointObject.GetValue(Field.FieldHeight).SetFloat((float)item.ReadingValue);
                    break;
                case ReadingType.SpO2:
                    dataPointObject.GetValue(HealthFields.FieldOxygenSaturation).SetFloat((float)item.ReadingValue);
                    dataPointObject.GetValue(HealthFields.FieldSupplementalOxygenFlowRate).SetFloat(0);
                    break;
                case ReadingType.CaloriesConsumed:
                    long startTimeCaloriesConsumed = (long)(item.CreatedOn.UtcDateTime - unixEpoch).TotalMilliseconds;
                    long endTimeCaloriesConsumed = (long)(item.CreatedOn.AddHours(1).UtcDateTime - unixEpoch).TotalMilliseconds;
                    dataPointObject.SetTimeInterval(startTimeCaloriesConsumed, endTimeCaloriesConsumed, TimeUnit.Milliseconds);
                    dataPointObject.GetValue(Field.FieldCalories).SetFloat((float)item.ReadingValue);
                    break;
                case ReadingType.CaloriesExpended:
                    long startTimeCaloriesExpended = (long)(item.CreatedOn.UtcDateTime - unixEpoch).TotalMilliseconds;
                    long endTimeCaloriesExpended = (long)(item.CreatedOn.AddHours(1).UtcDateTime - unixEpoch).TotalMilliseconds;
                    dataPointObject.SetTimeInterval(startTimeCaloriesExpended, endTimeCaloriesExpended, TimeUnit.Milliseconds);
                    dataPointObject.GetValue(Field.FieldCalories).SetFloat((float)item.ReadingValue);
                    break;
                case ReadingType.DistanceWalkingRunning:
                    long startTimeDistanceWalkingRunning = (long)(item.CreatedOn.UtcDateTime - unixEpoch).TotalMilliseconds;
                    long endTimeDistanceWalkingRunning = (long)(item.CreatedOn.AddHours(1).UtcDateTime - unixEpoch).TotalMilliseconds;
                    dataPointObject.SetTimeInterval(startTimeDistanceWalkingRunning, endTimeDistanceWalkingRunning, TimeUnit.Milliseconds);
                    dataPointObject.GetValue(Field.FieldDistance).SetFloat((float)item.ReadingValue);
                    break;
                case ReadingType.DistanceCycling:
                    long startTimeDistanceCycling = (long)(item.CreatedOn.UtcDateTime - unixEpoch).TotalMilliseconds;
                    long endTimeDistanceCycling = (long)(item.CreatedOn.AddHours(1).UtcDateTime - unixEpoch).TotalMilliseconds;
                    dataPointObject.SetTimeInterval(startTimeDistanceCycling, endTimeDistanceCycling, TimeUnit.Milliseconds);
                    dataPointObject.GetValue(Field.FieldRpm).SetFloat((float)item.ReadingValue);
                    break;
                case ReadingType.Hydration:
                    dataPointObject.GetValue(Field.FieldVolume).SetFloat((float)item.ReadingValue);
                    break;
                default:
                    // Intentionaaly kept empty
                    break;
            }
        }

        public async Task FetchReadingAsync(HealthReadingDTO readingsData, ReadingType readingType, long startTime, long endTime,
            AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe, bool shouldInclude, params ReadingType[] readingTypes)
        {
            HistoryClient historyClient = FitnessClass.GetHistoryClient(Platform.CurrentActivity, GetLastGoogleSignInAccount());
            DataReadRequest readRequest;
            bool shouldAggregate;
            if (readingType == ReadingType.Workout)
            {
                readRequest = ConstructReadRequest(ReadingType.CaloriesExpended, false)
                    .BucketByActivitySegment(1, TimeUnit.Minutes)
                    .SetTimeRange(startTime, endTime, TimeUnit.Milliseconds)
                    .Build();
                await FetchWorkoutAsync(readingsData, shouldInclude, readingTypes, historyClient, readRequest).ConfigureAwait(false);
                shouldAggregate = IsListNotEmpty(readingsData.HealthReadings);
            }
            else
            {
                if (ShouldExecuteAggregateMethod(readingType, aggregateType, aggregateTimeframe))
                {
                    readRequest = ConstructReadRequest(readingType, true)
                        .SetTimeRange(startTime, endTime, TimeUnit.Milliseconds)
                        .BucketByTime(1, aggregateTimeframe == AggregateTimeFrame.Day ? TimeUnit.Days : TimeUnit.Hours)
                        .Build();
                    await FetchAggregatedReadingsAsync(readingsData, readingType, aggregateType, shouldInclude, readingTypes, historyClient, readRequest).ConfigureAwait(false);
                    shouldAggregate = aggregateTimeframe == AggregateTimeFrame.All && IsListNotEmpty(readingsData.HealthReadings);
                }
                else
                {
                    readRequest = ConstructReadRequest(readingType, false)
                        .SetTimeRange(startTime, endTime, TimeUnit.Milliseconds)
                        .Build();
                    try
                    {
                        DataReadResponse readResponse = await historyClient.ReadDataAsync(readRequest).ConfigureAwait(false);
                        if (IsListNotEmpty(readResponse.DataSets))
                        {
                            MapReadings(readingsData, readResponse, readingType, false, aggregateType, shouldInclude, readingTypes);
                        }
                    }
                    catch (ApiException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"{readingType}=ex1=={ex.Message}");
                        readingsData.ErrCode = SetPermissionDenied(ex) ? ErrorCode.Unauthorized : ErrorCode.ErrorWhileRetrievingRecords;
                    }
                    catch (Exception ex)
                    {
                        readingsData.ErrCode = ErrorCode.Unauthorized;
                        readingsData.Response = "App is not accepted as source";
                    }
                    shouldAggregate = IsListNotEmpty(readingsData.HealthReadings);
                }
            }
            if (shouldAggregate)
            {
                GenericMethods.GetAggregatedReadings(readingsData, readingType, aggregateType, aggregateTimeframe);
            }
            if (readingsData.ErrCode != ErrorCode.Unauthorized)
            {
                readingsData.ErrCode = ErrorCode.OK;
            }
        }

        private async Task FetchAggregatedReadingsAsync(HealthReadingDTO readingsData, ReadingType readingType, AggregateType aggregateType,
            bool shouldInclude, ReadingType[] readingTypes, HistoryClient historyClient, DataReadRequest readRequest)
        {
            try
            {
                DataReadResponse readResponse = await historyClient.ReadDataAsync(readRequest).ConfigureAwait(false);
                if (IsListNotEmpty(readResponse.Buckets))
                {
                    MapReadings(readingsData, readResponse, readingType, true, aggregateType, shouldInclude, readingTypes);
                }
            }
            catch (ApiException ex)
            {
                readingsData.ErrCode = SetPermissionDenied(ex) ? ErrorCode.Unauthorized : ErrorCode.ErrorWhileRetrievingRecords;
            }
            catch
            {
                readingsData.ErrCode = ErrorCode.Unauthorized;
                readingsData.Response = "App is not accepted as source";
            }
        }

        private async Task FetchWorkoutAsync(HealthReadingDTO readingsData, bool shouldInclude, ReadingType[] readingTypes,
            HistoryClient historyClient, DataReadRequest readRequest)
        {
            try
            {
                DataReadResponse readResponse = await historyClient.ReadDataAsync(readRequest).ConfigureAwait(false);
                if (IsListNotEmpty(readResponse.Buckets))
                {
                    MapActivityResponse(readingsData, readResponse, shouldInclude, readingTypes);
                }
            }
            catch (ApiException ex)
            {
                readingsData.ErrCode = SetPermissionDenied(ex) ? ErrorCode.Unauthorized : ErrorCode.ErrorWhileRetrievingRecords;
            }
            catch
            {
                readingsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                readingsData.Response = "App is not accepted as source";
            }
        }

        private bool SetPermissionDenied(ApiException ex)
        {
            if (ex.StatusCode == 4 || ex.StatusCode == 17)
            {
                Preferences.Set(HealthConstants.HEALTH_LIBRARY_PERMISSION_DENIED_PREFERENCE_KEY, true);
                return true;
            }
            return false;
        }

        private bool IsListNotEmpty<T>(IList<T> listData)
        {
            return listData?.Count > 0;
        }

        private async Task<HealthReadingDTO> FetchSleepReadingAsync(HealthReadingDTO readingsData, long startTime, long endTime,
            AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe)
        {
            SessionsClient sessionClient = FitnessClass.GetSessionsClient(Platform.CurrentActivity, GetLastGoogleSignInAccount());
            SessionReadRequest readRequest = new SessionReadRequest.Builder().ReadSessionsFromAllApps().Read(GetDataTypeForReading(ReadingType.Sleep))
                .EnableServerQueries()
                .SetTimeInterval(startTime, endTime, TimeUnit.Milliseconds)
                .Build();
            try
            {
                SessionReadResponse readResponse = await sessionClient.ReadSessionAsync(readRequest).ConfigureAwait(false);
                readingsData.HealthReadings = new List<HealthReadingModel>();
                foreach (Session session in readResponse.Sessions.Where(x => x.Activity == FitnessActivities.Sleep))
                {
                    readingsData.HealthReadings.Add(new HealthReadingModel
                    {
                        ReadingValue = session.GetEndTime(TimeUnit.Minutes) - session.GetStartTime(TimeUnit.Minutes),
                        ReadingUnit = GenericMethods.GetReadingUnitForType(ReadingType.Sleep),
                        ReadingMoment = GetSleepMoment(session.Activity),
                        ReadingType = ReadingType.Sleep,
                        CreatedOn = FromUnixTime(session.GetStartTime(TimeUnit.Milliseconds))
                    });
                }
            }
            catch (ApiException ex)
            {
                readingsData.ErrCode = SetPermissionDenied(ex) ? ErrorCode.Unauthorized : ErrorCode.ErrorWhileRetrievingRecords;
            }
            catch
            {
                readingsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                readingsData.Response = "App is not accepted as source";
            }
            if (IsListNotEmpty(readingsData.HealthReadings))
            {
                GenericMethods.GetAggregatedReadings(readingsData, ReadingType.Sleep, aggregateType, aggregateTimeframe);
            }
            if (readingsData.ErrCode != ErrorCode.Unauthorized)
            {
                readingsData.ErrCode = ErrorCode.OK;
            }
            return readingsData;
        }

        private FitnessOptions GetFitnessOptionsBasedReadingTypes(List<ReadingType> readingTypes, PermissionType permissionType)
        {
            FitnessOptions.Builder fitnessOptionsBuilder = FitnessOptions.InvokeBuilder();
            foreach (var item in readingTypes)
            {
                System.Diagnostics.Debug.WriteLine($" item==={item} GetDataTypeForReading(item)={GetDataTypeForReading(item)}");
                if (GetDataTypeForReading(item) != null)
                {

                    if (permissionType == PermissionType.Read)
                    {
                        fitnessOptionsBuilder.AddDataType(GetDataTypeForReading(item), FitnessOptions.AccessRead);
                        if (item != ReadingType.DistanceCycling && item != ReadingType.Workout)
                        {
                            fitnessOptionsBuilder.AddDataType(GetAggregateDataTypeForReading(item), FitnessOptions.AccessRead);
                        }
                    }
                    else
                    {
                        fitnessOptionsBuilder.AddDataType(GetDataTypeForReading(item), FitnessOptions.AccessWrite);
                    }
                }
            }
            return fitnessOptionsBuilder.Build();
        }

        private bool HasPermissions(FitnessOptions requiredFitnessOptions, GoogleSignInAccount googleAccount = null)
        {
            googleAccount ??= GetLastGoogleSignInAccount();
            System.Diagnostics.Debug.WriteLine($"googleAccount in HasPermissions{googleAccount}");
            var dd = !Preferences.Get(HealthConstants.HEALTH_LIBRARY_PERMISSION_DENIED_PREFERENCE_KEY, false)
                && HasGooglePermissions(googleAccount, requiredFitnessOptions);
            System.Diagnostics.Debug.WriteLine($"googleAccount in HasPermissions access=={dd}");
            return dd;
        }

        private async Task<ErrorCode> RequestUserPermissionAsync(GoogleSignInAccount googleAccount, FitnessOptions requiredFitnessOptions)
        {
            FitPermissionCompletionSource = new TaskCompletionSource<bool>();
            RequestGooglePermission(googleAccount, requiredFitnessOptions);
            if (await FitPermissionCompletionSource.Task.ConfigureAwait(false))
            {
                Preferences.Set(HealthConstants.HEALTH_LIBRARY_PERMISSION_DENIED_PREFERENCE_KEY, false);
                return ErrorCode.OK;
            }
            else
            {
                return ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        private bool HasGooglePermissions(GoogleSignInAccount googleAccount, FitnessOptions requiredFitnessOptions)
        {
            //todo:
            return true; //GoogleSignIn.HasPermissions(googleAccount, requiredFitnessOptions);
        }

        private void RequestGooglePermission(GoogleSignInAccount googleAccount, FitnessOptions requiredFitnessOptions)
        {
            //todo:
            //GoogleSignIn.RequestPermissions(Platform.CurrentActivity, HealthConstants.GOOGLE_FIT_PERMISSIONS_REQUEST_CODE, googleAccount, requiredFitnessOptions);
        }

        private GoogleSignInAccount GetLastGoogleSignInAccount()
        {
            //todo:
            return null; //GoogleSignIn.GetLastSignedInAccount(Platform.CurrentActivity);
        }

        //private string AuthenticateAsync(GoogleSignInAccount googleAccount, FitnessOptions requiredFitnessOptions)
        //{
        //    string accessToken = string.Empty;
        //    try
        //    {
        //        WebAuthenticatorResult authResult = await WebAuthenticator.AuthenticateAsync(googleAccount,
        //            new Uri("https://mysite.com/mobileauth/Microsoft"),
        //            new Uri("myapp://"));

        //        accessToken = authResult?.AccessToken;

        //        // Do something with the token
        //    }
        //    catch (TaskCanceledException e)
        //    {
        //        // Use stopped auth
        //    }
        //    return accessToken;
        //}
    }
}