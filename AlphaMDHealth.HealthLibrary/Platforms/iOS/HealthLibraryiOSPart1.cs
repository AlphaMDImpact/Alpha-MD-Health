//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;
//using Foundation;
//using HealthKit;
//using HealthLibrary.iOS;
//using AlphaMDHealth.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using UIKit;
//using Xamarin.Forms;

//[assembly: Dependency(typeof(HealthLibraryiOS))]

using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Foundation;
using HealthKit;
using UIKit;

namespace AlphaMDHealth.HealthLibrary.Platforms
{
    public partial class HealthLibraryiOS : IHealthLibrary
    {
        /// <summary>
        /// Opens AppleHealth App to give permissions to requested Observation types
        /// </summary>
        /// <returns>True if succesfully redirected to URL</returns>
        public async Task<bool> OpenPermissionApp()
        {
            return await UIApplication.SharedApplication.OpenUrlAsync(new NSUrl("x-apple-health://"), new UIApplicationOpenUrlOptions()).ConfigureAwait(true);
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
            if (HKHealthStore.IsHealthDataAvailable)
            {
                NSSet typesToReadAndWrite = NSSet.MakeNSObjectSet(GetPermissionTypeForReadingTypes(readingTypes));
                var status = (await new HKHealthStore().RequestAuthorizationToShareAsync(
                    permissionType == PermissionType.Read ? new NSSet() : typesToReadAndWrite,
                    permissionType == PermissionType.Write ? new NSSet() : typesToReadAndWrite).ConfigureAwait(false)
                ).Item1;
                if (!status)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        status = await UIApplication.SharedApplication.OpenUrlAsync(new NSUrl("x-apple-health://"), new UIApplicationOpenUrlOptions()).ConfigureAwait(false);
                    });
                }
                return status ? ErrorCode.OK : ErrorCode.ErrorWhileRetrievingRecords;
            }
            else
            {
                return ErrorCode.ErrorWhileRetrievingRecords;
            }
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
        public async Task GetDataAsync(HealthReadingDTO readings, ReadingType readingType, DateTimeOffset fromDate, DateTimeOffset toDate, AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe, bool shouldInclude, params ReadingType[] readingTypes)
        {
            if (HKHealthStore.IsHealthDataAvailable)
            {
                TaskCompletionSource<int> getHeathDataTask = new TaskCompletionSource<int>();
                if (await CheckIfPermissionGivenAsync(readingType, fromDate, PermissionType.Read, 0).ConfigureAwait(true))
                {
                    NSDate startDate = ConvertToNSDate(fromDate.LocalDateTime);
                    NSDate endDate = ConvertToNSDateTime(toDate.LocalDateTime);
                    if (aggregateType == AggregateType.None || readingType == ReadingType.Workout)
                    {
                        GetAllHealthData(readingType, startDate, endDate, ConvertToHKSampleType(readingType), (array, type) =>
                        {
                            MapSingleResponse(readings, array, type, getHeathDataTask, shouldInclude, readingTypes);
                            if (aggregateType != AggregateType.None)
                            {
                                GenericMethods.GetAggregatedReadings(readings, readingType, aggregateType, aggregateTimeframe);
                            }
                        });
                    }
                    else
                    {
                        if (IsCorrelationTypeReading(readingType))
                        {
                            // Separate aggregation required
                            GetAllHealthData(readingType, startDate, endDate, ConvertToHKSampleType(readingType), (array, type) =>
                            {
                                MapCorrelatedResponse(readings, array, readingType, aggregateType, aggregateTimeframe, getHeathDataTask);
                            });
                        }
                        else
                        {
                            GetAggregateHealthData(readingType, startDate, endDate, aggregateType, aggregateTimeframe, ConvertToHKSampleType(readingType),
                                                (list, type) =>
                                                {
                                                    MapStatisticalResponse(readings, list, type, aggregateType, getHeathDataTask);
                                                });
                        }
                    }
                    await getHeathDataTask.Task.ConfigureAwait(false);
                }
                else
                {
                    readings.ErrCode = ErrorCode.Unauthorized;
                    readings.Response = "Permission Denied.";
                }
            }
            else
            {
                readings.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                readings.Response = "Permission Denied.";
            }
        }

        /// <summary>
        /// Writes the given readings to HealthKit
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <param name="healthReadings">List of health readings to be stored</param>
        /// <returns>Status of operation</returns>
        public async Task WriteDataAsync(ReadingType readingType, HealthReadingDTO healthReadings)
        {
            try
            {
                if (HKHealthStore.IsHealthDataAvailable)
                {
                    HKHealthStore hKHealth = new HKHealthStore();
                    HKUnit sampleUnit = GetUnitBasedOnReadingType(readingType);
                    if (readingType == ReadingType.Nutrition)
                    {
                        await WriteNutritionAsync(healthReadings, hKHealth).ConfigureAwait(false);
                    }
                    else if (readingType == ReadingType.BloodPressure)
                    {
                        await WriteBloodPressureAsync(sampleUnit, healthReadings, hKHealth).ConfigureAwait(false);
                    }
                    else
                    {
                        HKSample sampleData;
                        HKSampleType sampleType;
                        HKQuantity sampleQuantity;
                        foreach (HealthReadingModel item in healthReadings.HealthReadings)
                        {
                            sampleType = ConvertToHKSampleType(item.ReadingType);
                            sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.ReadingType == ReadingType.BodyFat || item.ReadingType == ReadingType.SpO2 ? item.ReadingValue / 100 : item.ReadingValue);
                            sampleData = MakeSampleDataObject(sampleType, sampleQuantity, item);
                            item.ErrCode = (await hKHealth.SaveObjectAsync(sampleData).ConfigureAwait(false)).Item1 ? ErrorCode.OK : ErrorCode.ErrorWhileSavingRecords;
                        }
                    }
                    healthReadings.ErrCode = ErrorCode.OK;
                }
                else
                {
                    healthReadings.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                }
            }
            catch
            {
                healthReadings.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        private async Task WriteBloodPressureAsync(HKUnit sampleUnit, HealthReadingDTO healthReadings, HKHealthStore hKHealth)
        {
            HKSample sampleData;
            HKQuantity sampleQuantity;
            List<List<HealthReadingModel>> bloodPressureGroup = healthReadings.HealthReadings.GroupBy(x => x.CreatedOn).Select(y => y.ToList()).ToList();
            HKQuantitySample systolicQuantity, diastolicQuantity;
            foreach (List<HealthReadingModel> item in bloodPressureGroup)
            {
                NSDate date = ConvertToNSDateTime(item.First().CreatedOn.LocalDateTime);
                ///Systolic Data
                HKSampleType sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.BloodPressureSystolic);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Find(x => x.ReadingType == ReadingType.BPSystolic).ReadingValue);
                systolicQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);
                ///Diastolic Data
                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.BloodPressureDiastolic);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Find(x => x.ReadingType == ReadingType.BPDiastolic).ReadingValue);
                diastolicQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                var correlationType = HKCorrelationType.Create(HKCorrelationTypeIdentifier.BloodPressure);
                sampleData = HKCorrelation.Create(correlationType, date, date, NSSet.MakeNSObjectSet(new HKSample[] { systolicQuantity, diastolicQuantity }));
                item[0].ErrCode = (await hKHealth.SaveObjectAsync(sampleData).ConfigureAwait(false)).Item1 ? ErrorCode.OK : ErrorCode.ErrorWhileSavingRecords;
                item.ForEach((data) => { data.ErrCode = item[0].ErrCode; });
            }
        }

        private async Task WriteNutritionAsync(HealthReadingDTO healthReadings, HKHealthStore hKHealth)
        {
            HKSample sampleData;
            HKSampleType sampleType;
            HKQuantity sampleQuantity;
            HKQuantitySample fatQuantity, carbsQuantity, proteinQuantity, sugarQuantity, fiberQuantity, calorieIntakeQuantity, cholesterolQuantity, sodiumQuantity;
            HKQuantitySample vitaminAQuantity, vitaminCQuantity, potassiumQuantity, calciumQuantity, ironQuantity, saturatedFatQuantity, unsaturatedFatQuantity;
            foreach (NutritionReadingModel item in healthReadings.NutritionReadings)
            {
                var correlationType = HKCorrelationType.Create(HKCorrelationTypeIdentifier.Food);
                NSDate date = ConvertToNSDateTime(item.ReadingDateTime.LocalDateTime);

                HKUnit sampleUnit = HKUnit.Gram;
                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatTotal);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Fat);
                fatQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCarbohydrates);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Carbs);
                carbsQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryProtein);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Protein);
                proteinQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietarySugar);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Sugar);
                sugarQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFiber);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Fiber);
                fiberQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleUnit = HKUnit.Kilocalorie;
                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryEnergyConsumed);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.CalorieIntake);
                calorieIntakeQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleUnit = HKUnit.FromGramUnit(HKMetricPrefix.Milli);
                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCholesterol);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Cholesterol);
                cholesterolQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatSaturated);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.SaturatedFat);
                saturatedFatQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatPolyunsaturated);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.UnsaturatedFat);
                unsaturatedFatQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietarySodium);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Sodium);
                sodiumQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryVitaminA);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.VitaminA);
                vitaminAQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryVitaminC);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.VitaminC);
                vitaminCQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryPotassium);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Potassium);
                potassiumQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCalcium);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Calcium);
                calciumQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                sampleType = HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryIron);
                sampleQuantity = HKQuantity.FromQuantity(sampleUnit, item.Iron);
                ironQuantity = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, date, date);

                var foodMetaData = new Dictionary<NSObject, NSObject>
                {
                    {new NSString("HKMetadataKeyFoodType"), new NSString(item.FoodName)},
                    {new NSString("HKFoodMeal"), new NSString(ConvertMealType(item.FoodType))},
                };
                sampleData = HKCorrelation.Create(correlationType, date, date, NSSet.MakeNSObjectSet(new HKSample[]
                {
                                fatQuantity, carbsQuantity, proteinQuantity, sugarQuantity, fiberQuantity, calorieIntakeQuantity, cholesterolQuantity, sodiumQuantity,
                                vitaminAQuantity, vitaminCQuantity, potassiumQuantity, calciumQuantity, ironQuantity, saturatedFatQuantity, unsaturatedFatQuantity
                }), NSDictionary.FromObjectsAndKeys(foodMetaData.Values.ToArray(), foodMetaData.Keys.ToArray()));

                item.ErrCode = (await hKHealth.SaveObjectAsync(sampleData).ConfigureAwait(false)).Item1 ? ErrorCode.OK : ErrorCode.ErrorWhileSavingRecords;
            }
        }

        private HKSample MakeSampleDataObject(HKSampleType sampleType, HKQuantity sampleQuantity, HealthReadingModel item)
        {
            HKSample sampleData = null;
            NSDate startDateTime = ConvertToNSDateTime(item.CreatedOn.LocalDateTime);
            switch (item.ReadingType)
            {
                case ReadingType.BloodGlucose:
                    var glucoseMetaData = new Dictionary<NSObject, NSObject>
                    {
                        { new NSString("BloodGlucoseMealTime"), new NSString(GetMealTypeFromReadingMoment(item.ReadingMoment.Value))}
                    };
                    sampleData = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, startDateTime, startDateTime, NSDictionary.FromObjectsAndKeys(glucoseMetaData.Values.ToArray(), glucoseMetaData.Keys.ToArray()));
                    break;
                case ReadingType.BodyTemperature:
                case ReadingType.BodyFat:
                case ReadingType.Steps:
                case ReadingType.Weight:
                case ReadingType.HeartRate:
                case ReadingType.Height:
                case ReadingType.SpO2:
                    sampleData = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, startDateTime, startDateTime);
                    break;
                case ReadingType.CaloriesConsumed:
                case ReadingType.CaloriesExpended:
                case ReadingType.DistanceWalkingRunning:
                case ReadingType.DistanceCycling:
                    sampleData = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, startDateTime, ConvertToNSDateTime(item.CreatedOn.AddHours(1).LocalDateTime));
                    break;
                case ReadingType.Hydration:
                    var quantitySample = HKQuantitySample.FromType(sampleType as HKQuantityType, sampleQuantity, startDateTime, startDateTime);
                    var correlationType = HKCorrelationType.Create(HKCorrelationTypeIdentifier.Food);
                    sampleData = HKCorrelation.Create(correlationType, startDateTime, startDateTime, NSSet.MakeNSObjectSet(new HKSample[] { quantitySample }));
                    break;
                case ReadingType.Sleep:
                    NSDate endDate = ConvertToNSDateTime(item.CreatedOn.AddMinutes(item.ReadingValue).LocalDateTime);
                    sampleData = HKCategorySample.FromType(HKCategoryType.Create(HKCategoryTypeIdentifier.SleepAnalysis), 0, startDateTime, endDate);
                    break;
                case ReadingType.Workout:
                    NSDate endDateTime = ConvertToNSDateTime(item.CreatedOn.AddMinutes(item.Duration).LocalDateTime);
                    //// total calories burned
                    //// total distance travelled
                    //// metadata
                    var workoutMetaData = new Dictionary<NSObject, NSObject>
                    {
                        { new NSString("HKMetadataKeyGroupFitness"), new NSString("true") },
                        ////{ new NSString("HKMetadataKeyIndoorWorkout"), new NSString("false") },
                        { new NSString("HKMetadataKeyCoachedWorkout"), new NSString("true") }
                    };
                    sampleData = HKWorkout.Create(GetActivityForReadingType((ReadingType)Enum.Parse(typeof(ReadingType), item.ActivityType)), startDateTime, endDateTime, null, HKQuantity.FromQuantity(HKUnit.Kilocalorie, item.ReadingValue), null, NSDictionary.FromObjectsAndKeys(workoutMetaData.Values.ToArray(), workoutMetaData.Keys.ToArray()));
                    break;
                default:
                    // Intentionally kept empty
                    break;
            }

            return sampleData;
        }

        /// <summary>
        /// Retrieve aggregated readings from Healthkit for given datatype
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <param name="fromDate">Date from which the data is to be fetched</param>
        /// <param name="toDate">Date up to which the data is to be fetched</param>
        /// <param name="aggregateType">Type of aggregation</param>
        /// <param name="aggregateTimeFrame">Timeframe for aggregation</param>
        /// <param name="sampleType">Healthkit datatype to retrieve</param>
        /// <param name="completion">Completion action for datatypes which return staticstics object list</param>
        private void GetAggregateHealthData(ReadingType readingType, NSDate fromDate, NSDate toDate, AggregateType aggregateType, AggregateTimeFrame aggregateTimeFrame,
            HKSampleType sampleType, Action<List<HKStatistics>, ReadingType> completion)
        {
            var predicate = HKQuery.GetPredicateForSamples(fromDate, toDate, HKQueryOptions.None);
            // Since we need aggregate of a day
            NSDateComponents intervalComponents = new NSDateComponents();
            if (aggregateTimeFrame == AggregateTimeFrame.Day)
            {
                intervalComponents.Day = 1;
            }
            else if (aggregateTimeFrame == AggregateTimeFrame.Hour)
            {
                intervalComponents.Hour = 1;
            }
            else
            {
                intervalComponents.Year = 1;
            }
            HKQuery readingQuery = new HKStatisticsCollectionQuery(sampleType as HKQuantityType, predicate, GetStatisticsOptions(aggregateType), fromDate, intervalComponents)
            {
                InitialResultsHandler = (query, results, error) =>
                {
                    if (error != null)
                    {
                        completion?.Invoke(null, readingType);
                        return;
                    }
                    List<HKStatistics> aggregateReadings = new List<HKStatistics>();
                    results.EnumerateStatistics(fromDate, toDate, (result, stop) =>
                    {
                        AddAggregateReadingsToHKObject(aggregateType, result, aggregateReadings);
                    });
                    completion?.Invoke(aggregateReadings, readingType);
                }
            };
            new HKHealthStore().ExecuteQuery(readingQuery);
        }

        /// <summary>
        /// Retrieve all readings from Healthkit for given datatype
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <param name="fromDate">Date from which the data is to be fetched</param>
        /// <param name="toDate">Date up to which the data is to be fetched</param>
        /// <param name="sampleType">Healthkit datatype to retrieve</param>
        /// <param name="completion">Completion action for datatypes which return staticstics object list</param>
        private void GetAllHealthData(ReadingType readingType, NSDate fromDate, NSDate toDate, HKSampleType sampleType, Action<HKSample[], ReadingType> completion)
        {
            var predicate = HKQuery.GetPredicateForSamples(fromDate, toDate, HKQueryOptions.None);
            NSSortDescriptor timeSortDescriptor = new NSSortDescriptor(HKSample.SortIdentifierEndDate, false);
            HKQuery readingQuery = new HKSampleQuery(sampleType, predicate, nuint.MaxValue, new[] { timeSortDescriptor }, (resultQuery, results, error) =>
            {
                if (error != null)
                {
                    completion?.Invoke(null, readingType);
                    return;
                }
                completion?.Invoke(results, readingType);
            });
            new HKHealthStore().ExecuteQuery(readingQuery);
        }

        private async Task<bool> CheckIfPermissionGivenAsync(ReadingType readingType, DateTimeOffset fromDate, PermissionType permissionType, int retryCount)
        {
            if (retryCount < 3)
            {
                if (permissionType == PermissionType.Read)
                {
                    TaskCompletionSource<int> getHealthPermissionTask = new TaskCompletionSource<int>();
                    NSDate sDate = ConvertToNSDate(fromDate.AddYears(-retryCount).LocalDateTime);
                    NSDate eDate = ConvertToNSDateTime(DateTimeOffset.UtcNow.LocalDateTime);
                    GetAllHealthData(readingType, sDate, eDate, ConvertToHKSampleType(readingType), (array, type) =>
                    {
                        if (readingType == ReadingType.BloodPressure)
                        {
                            int resultState = 0;
                            if (array != null)
                            {
                                foreach (HKCorrelation item in array.OfType<HKCorrelation>())
                                {
                                    IEnumerable<HKQuantitySample> quantitySamples = from sample in item.Objects select sample as HKQuantitySample;
                                    if (quantitySamples.Count() > 1)
                                    {
                                        // This indicates that we have permission for both Systolic and Diastolic
                                        resultState = 1;
                                        break;
                                    }
                                }
                            }
                            getHealthPermissionTask.TrySetResult(resultState);
                        }
                        else
                        {
                            getHealthPermissionTask.TrySetResult(array?.Length > 0 ? 1 : 0);
                        }
                    });
                    var count = await getHealthPermissionTask.Task.ConfigureAwait(false);
                    if (count > 0)
                    {
                        return true;
                    }
                    return await CheckIfPermissionGivenAsync(readingType, fromDate.AddYears(-1), permissionType, retryCount + 1).ConfigureAwait(false);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private void MapStatisticalResponse(HealthReadingDTO healthReading, List<HKStatistics> healthSampleList, ReadingType readingType, AggregateType aggregateType, TaskCompletionSource<int> getHeathDataTask)
        {
            healthReading.HealthReadings = new List<HealthReadingModel>();
            if (healthSampleList != null)
            {
                if (healthSampleList.Count > 0)
                {
                    HKUnit unit = GetUnitBasedOnReadingType(readingType);
                    foreach (HKStatistics item in healthSampleList)
                    {
                        healthReading.HealthReadings.Add(
                            new HealthReadingModel
                            {
                                ReadingType = readingType,
                                ReadingUnit = GenericMethods.GetReadingUnitForType(readingType),
                                ReadingValue = GetRoundedValue(GetQuantityForAggregationType(item, aggregateType).GetDoubleValue(unit), readingType),
                                CreatedOn = ConvertToUtcDateTime(GetActivityDate(item, readingType))
                            });
                    }
                    getHeathDataTask.TrySetResult(healthReading.HealthReadings.Count);
                }
                else
                {
                    getHeathDataTask.TrySetResult(0);
                }
                healthReading.ErrCode = ErrorCode.OK;
            }
            else
            {
                healthReading.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                healthReading.Response = "Error in MapStatisticalResponse()";
                getHeathDataTask.TrySetResult(0);
            }
        }

        private void MapCorrelatedResponse(HealthReadingDTO healthReading, HKSample[] healthSampleList, ReadingType readingType, AggregateType aggregateType, AggregateTimeFrame aggregateTimeFrame, TaskCompletionSource<int> getHeathDataTask)
        {
            healthReading.HealthReadings = new List<HealthReadingModel>();
            if (healthSampleList != null)
            {
                if (healthSampleList.Length > 0)
                {
                    if (readingType == ReadingType.BloodPressure)
                    {
                        HKUnit unit = GetUnitBasedOnReadingType(ReadingType.BloodPressure);
                        foreach (HKCorrelation item in healthSampleList.OfType<HKCorrelation>())
                        {
                            DateTimeOffset readingDateTime = ConvertToUtcDateTime(item.EndDate);
                            IEnumerable<HKQuantitySample> quantitySamples = from sample in item.Objects select sample as HKQuantitySample;
                            if (quantitySamples.Count() > 1)
                            {
                                healthReading.HealthReadings.Add(new HealthReadingModel
                                {
                                    ReadingType = ReadingType.BPSystolic,
                                    ReadingUnit = GenericMethods.GetReadingUnitForType(ReadingType.BPSystolic),
                                    ReadingValue = quantitySamples.First(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.BloodPressureSystolic))).Quantity.GetDoubleValue(unit),
                                    CreatedOn = readingDateTime,
                                });
                                healthReading.HealthReadings.Add(new HealthReadingModel
                                {
                                    ReadingType = ReadingType.BPDiastolic,
                                    ReadingUnit = GenericMethods.GetReadingUnitForType(ReadingType.BPDiastolic),
                                    ReadingValue = quantitySamples.First(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.BloodPressureDiastolic))).Quantity.GetDoubleValue(unit),
                                    CreatedOn = readingDateTime,
                                });
                            }
                        }
                        List<HealthReadingModel> mergedReadings = new List<HealthReadingModel>();
                        mergedReadings.AddRange(MapAggregatedDataForCorrelatedType(readingType, healthReading.HealthReadings.Where(x => x.ReadingType == ReadingType.BPSystolic).ToList(), aggregateType, aggregateTimeFrame));
                        mergedReadings.AddRange(MapAggregatedDataForCorrelatedType(readingType, healthReading.HealthReadings.Where(x => x.ReadingType == ReadingType.BPDiastolic).ToList(), aggregateType, aggregateTimeFrame));
                        healthReading.ErrCode = ErrorCode.OK;
                        healthReading.HealthReadings = mergedReadings;
                        getHeathDataTask.TrySetResult(healthReading.HealthReadings.Count);
                    }
                    else
                    {
                        healthReading.NutritionReadings = new List<NutritionReadingModel>();
                        foreach (HKCorrelation correlationSample in healthSampleList.OfType<HKCorrelation>())
                        {
                            DateTimeOffset readingDateTime = ConvertToUtcDateTime(correlationSample.EndDate);
                            IEnumerable<HKQuantitySample> quantitySamples = from sample in correlationSample.Objects select sample as HKQuantitySample;
                            if (quantitySamples.Any())
                            {
                                MealType mealType = GetFoodType(GetDataFromDictionary(correlationSample.Metadata.Dictionary, "HKFoodMeal"));
                                var foodData = healthReading.NutritionReadings.Find(x => x.ReadingDateTime == readingDateTime && x.FoodType == mealType);
                                if (foodData == null)
                                {
                                    foodData = new NutritionReadingModel
                                    {
                                        FoodName = GetDataFromDictionary(correlationSample.Metadata.Dictionary, "HKMetadataKeyFoodType", "HKFoodType"),
                                        ReadingDateTime = readingDateTime,
                                        FoodType = mealType
                                    };
                                    healthReading.NutritionReadings.Add(foodData);
                                }
                                MapNutritionDataFromSamples(healthReading, quantitySamples, foodData);
                            }
                        }
                        healthReading.ErrCode = ErrorCode.OK;
                        getHeathDataTask.TrySetResult(healthReading.NutritionReadings.Count);
                    }
                }
                else
                {
                    healthReading.ErrCode = ErrorCode.OK;
                    getHeathDataTask.TrySetResult(0);
                }
            }
            else
            {
                healthReading.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                healthReading.Response = "Error in MapCorrelatedResponse()";
                getHeathDataTask.TrySetResult(0);
            }
        }

        private void MapSingleResponse(HealthReadingDTO healthReading, HKSample[] healthSampleArray, ReadingType readingType, TaskCompletionSource<int> getHeathDataTask, bool shouldInclude, params ReadingType[] readingTypes)
        {
            if (healthSampleArray != null)
            {
                if (healthSampleArray.Length > 0)
                {
                    HKUnit unit = GetUnitBasedOnReadingType(readingType);
                    if (readingType == ReadingType.BloodPressure)
                    {
                        MapSingleResponseBloodPressureData(healthReading, healthSampleArray, unit);
                    }
                    else if (readingType == ReadingType.Food)
                    {
                        MapSingleResponseNutritionData(healthReading, healthSampleArray);
                    }
                    else if (readingType == ReadingType.Workout)
                    {
                        MapActivityResponse(healthReading, healthSampleArray, shouldInclude, readingTypes);
                    }
                    else if (readingType == ReadingType.Sleep)
                    {
                        healthReading.HealthReadings = new List<HealthReadingModel>();
                        foreach (HKCategorySample item in healthSampleArray.OfType<HKCategorySample>())
                        {
                            double timeDiff = (ConvertToUtcDateTime(item.EndDate) - ConvertToUtcDateTime(item.StartDate)).TotalMinutes;
                            healthReading.HealthReadings.Add(new HealthReadingModel
                            {
                                ReadingType = readingType,
                                ReadingUnit = GenericMethods.GetReadingUnitForType(readingType),
                                CreatedOn = ConvertToUtcDateTime(item.StartDate),
                                ReadingMoment = GetReadingMomentForSleep((int)item.Value),
                                ReadingValue = timeDiff
                            });
                        }
                    }
                    else
                    {
                        healthReading.HealthReadings = (from sample in healthSampleArray
                                                        select new HealthReadingModel
                                                        {
                                                            ReadingType = readingType,
                                                            CreatedOn = ConvertToUtcDateTime(sample.EndDate),
                                                            ReadingMoment = GetReadingMoment(sample, readingType),
                                                            ReadingUnit = GenericMethods.GetReadingUnitForType(readingType),
                                                            ReadingValue = GetRoundedValue(((HKQuantitySample)sample).Quantity.GetDoubleValue(unit), readingType),
                                                        }).ToList();
                    }
                    healthReading.ErrCode = ErrorCode.OK;
                    getHeathDataTask.TrySetResult(readingType == ReadingType.Nutrition ? healthReading.NutritionReadings.Count : healthReading.HealthReadings.Count);
                }
                else
                {
                    healthReading.ErrCode = ErrorCode.OK;
                    getHeathDataTask.TrySetResult(0);
                }
            }
            else
            {
                healthReading.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                healthReading.Response = "Error in MapSingleResponse()";
                getHeathDataTask.TrySetResult(0);
            }
        }

        private void MapActivityResponse(HealthReadingDTO healthReading, HKSample[] healthSampleArray, bool shouldInclude, ReadingType[] readingTypes)
        {
            healthReading.HealthReadings = new List<HealthReadingModel>();
            foreach (HKWorkout result in healthSampleArray.OfType<HKWorkout>())
            {
                var duration = (int)(ConvertToUtcDateTime(result.EndDate) - ConvertToUtcDateTime(result.StartDate)).TotalMinutes;
                ReadingType currentType = GetReadingTypeOfActivity(result.WorkoutActivityType);
                if (readingTypes == null || readingTypes.Contains(ReadingType.Workout) || (shouldInclude && readingTypes.Contains(currentType)) || (!shouldInclude && !readingTypes.Contains(currentType)))
                {
                    healthReading.HealthReadings.Add(new HealthReadingModel
                    {
                        ActivityType = currentType.ToString(),
                        ReadingType = ReadingType.Workout,
                        ReadingValue = result.TotalEnergyBurned?.GetDoubleValue(HKUnit.Kilocalorie) ?? 0,
                        ReadingUnit = GenericMethods.GetReadingUnitForType(ReadingType.Workout),
                        CreatedOn = ConvertToUtcDateTime(result.StartDate),
                        Duration = duration
                    });
                }
            }
        }

        private void MapSingleResponseBloodPressureData(HealthReadingDTO healthReading, HKSample[] healthSampleArray, HKUnit unit)
        {
            healthReading.HealthReadings = new List<HealthReadingModel>();
            foreach (HKCorrelation item in healthSampleArray.OfType<HKCorrelation>())
            {
                DateTimeOffset readingDateTime = ConvertToUtcDateTime(item.EndDate);
                IEnumerable<HKQuantitySample> quantitySamples = from sample in item.Objects select sample as HKQuantitySample;
                if (quantitySamples.Count() > 1)
                {
                    healthReading.HealthReadings.Add(new HealthReadingModel
                    {
                        ReadingType = ReadingType.BPSystolic,
                        ReadingUnit = GenericMethods.GetReadingUnitForType(ReadingType.BPSystolic),
                        ReadingValue = quantitySamples.First(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.BloodPressureSystolic))).Quantity.GetDoubleValue(unit),
                        CreatedOn = readingDateTime,
                    });
                    healthReading.HealthReadings.Add(new HealthReadingModel
                    {
                        ReadingType = ReadingType.BPDiastolic,
                        ReadingUnit = GenericMethods.GetReadingUnitForType(ReadingType.BPDiastolic),
                        ReadingValue = quantitySamples.First(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.BloodPressureDiastolic))).Quantity.GetDoubleValue(unit),
                        CreatedOn = readingDateTime,
                    });
                }
            }
        }

        private void MapSingleResponseNutritionData(HealthReadingDTO healthReading, HKSample[] healthSampleArray)
        {
            healthReading.NutritionReadings = new List<NutritionReadingModel>();
            foreach (HKCorrelation correlationSample in healthSampleArray.OfType<HKCorrelation>())
            {
                DateTimeOffset readingDateTime = ConvertToUtcDateTime(correlationSample.EndDate);
                IEnumerable<HKQuantitySample> quantitySamples = from sample in correlationSample.Objects select sample as HKQuantitySample;
                if (quantitySamples.Any())
                {
                    MealType mealType = GetFoodType(GetDataFromDictionary(correlationSample.Metadata.Dictionary, "HKFoodMeal"));
                    var foodData = healthReading.NutritionReadings.Find(x => x.ReadingDateTime == readingDateTime && x.FoodType == mealType);
                    if (foodData == null)
                    {
                        foodData = new NutritionReadingModel
                        {
                            FoodName = GetDataFromDictionary(correlationSample.Metadata.Dictionary, "HKMetadataKeyFoodType", "HKFoodType"),
                            ReadingDateTime = readingDateTime,
                            FoodType = mealType
                        };
                        healthReading.NutritionReadings.Add(foodData);
                    }
                    MapNutritionDataFromSamples(healthReading, quantitySamples, foodData);
                }
            }
        }

        private List<HealthReadingModel> MapAggregatedDataForCorrelatedType(ReadingType readingType, List<HealthReadingModel> allReadings, AggregateType aggregateType, AggregateTimeFrame aggregateTimeFrame)
        {
            if (aggregateTimeFrame == AggregateTimeFrame.Hour)
            {
                var groupingByHour = allReadings.GroupBy(a =>
                {
                    var dt = a.CreatedOn;
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
                }).Select(b => b.ToList()).ToList();
                return (from item in groupingByHour
                        select new HealthReadingModel
                        {
                            CreatedOn = item[0].CreatedOn,
                            ReadingValue = GenericMethods.GetAggregatedValue(aggregateType, item, false),
                            Duration = GenericMethods.GetAggregatedValue(aggregateType, item, true),
                            ReadingUnit = GenericMethods.GetReadingUnitForType(readingType),
                            ReadingType = readingType
                        }).ToList();
            }
            else if (aggregateTimeFrame == AggregateTimeFrame.Day)
            {
                var groupingByDay = allReadings.GroupBy(a =>
                {
                    var dt = a.CreatedOn;
                    return new DateTime(dt.Year, dt.Month, dt.Day);
                }).Select(b => b.ToList()).ToList();
                return (from item in groupingByDay
                        select new HealthReadingModel
                        {
                            CreatedOn = item[0].CreatedOn,
                            ReadingValue = GenericMethods.GetAggregatedValue(aggregateType, item, false),
                            Duration = GenericMethods.GetAggregatedValue(aggregateType, item, true),
                            ReadingUnit = GenericMethods.GetReadingUnitForType(readingType),
                            ReadingType = readingType
                        }).ToList();
            }
            else
            {
                var groupingByYear = allReadings.GroupBy(a => a.CreatedOn.Year)
                        .Select(b => b.ToList()).ToList();
                return (from item in groupingByYear
                        select new HealthReadingModel
                        {
                            CreatedOn = item[0].CreatedOn,
                            ReadingValue = GenericMethods.GetAggregatedValue(aggregateType, item, false),
                            Duration = GenericMethods.GetAggregatedValue(aggregateType, item, true),
                            ReadingUnit = GenericMethods.GetReadingUnitForType(readingType),
                            ReadingType = readingType
                        }).ToList();
            }
        }
    }
}