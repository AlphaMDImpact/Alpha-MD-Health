using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.HealthLibrary
{
    public static class GenericMethods
    {
        /// <summary>
        /// Returns whether reading type requires round off value
        /// </summary>
        /// <param name="readingType">Reading type to check</param>
        /// <returns>true if readingType values requires int or else false</returns>
        public static bool IsRoundOffValueRequired(ReadingType readingType)
        {
            switch (readingType)
            {
                case ReadingType.Steps:
                case ReadingType.HeartRate:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets reading unit for reading type
        /// </summary>
        /// <param name="readingType">Reading type to check</param>
        /// <returns>Reading unit for given reading type</returns>
        public static ReadingUnit GetReadingUnitForType(ReadingType readingType)
        {
            switch (readingType)
            {
                case ReadingType.SpO2:
                case ReadingType.BodyFat:
                    return ReadingUnit.Percent;
                case ReadingType.Height:
                case ReadingType.DistanceWalkingRunning:
                case ReadingType.DistanceCycling:
                    return ReadingUnit.Meters;
                case ReadingType.Weight:
                    return ReadingUnit.Kgs;
                case ReadingType.HeartRate:
                    return ReadingUnit.BPM;
                case ReadingType.BloodPressure:
                case ReadingType.BPSystolic:
                case ReadingType.BPDiastolic:
                    return ReadingUnit.mmHg;
                case ReadingType.BloodGlucose:
                    return ReadingUnit.mmol_l;
                case ReadingType.CaloriesConsumed:
                case ReadingType.CaloriesExpended:
                case ReadingType.Workout:
                    return ReadingUnit.kcal;
                case ReadingType.BodyTemperature:
                    return ReadingUnit.Celsius;
                case ReadingType.Hydration:
                    return ReadingUnit.Liter;
                case ReadingType.Sleep:
                    return ReadingUnit.Minutes;
                case ReadingType.NutritionProtein:
                case ReadingType.NutritionTotalFat:
                case ReadingType.NutritionTotalCarbs:
                case ReadingType.NutritionSugar:
                case ReadingType.NutritionDietaryFiber:
                case ReadingType.NutritionCholesterol:
                case ReadingType.NutritionSodium:
                case ReadingType.NutritionVitaminA:
                case ReadingType.NutritionVitaminC:
                case ReadingType.NutritionPotassium:
                case ReadingType.NutritionCalcium:
                case ReadingType.NutritionIron:
                case ReadingType.NutritionUnsaturatedFat:
                case ReadingType.NutritionPolyunsaturatedFat:
                case ReadingType.NutritionMonounsaturatedFat:
                case ReadingType.NutritionZinc:
                case ReadingType.Nutrition:
                    return ReadingUnit.mg;
                case ReadingType.Steps:
                    return ReadingUnit.DailyCount;
                default:
                    return ReadingUnit.None;
            }
        }


        /// <summary>
        /// Gets if aggregation requested is valid for reading type
        /// </summary>
        /// <param name="readingType">Reading type to check</param>
        /// <param name="aggregateType">Aggregate type to check</param>
        /// <param name="aggregateTimeFrame">Aggregate timeframe to check</param>
        /// <returns>true if aggregation is valid or else false</returns>
        public static bool IsValidAggregationRequested(ReadingType readingType, AggregateType aggregateType, AggregateTimeFrame aggregateTimeFrame)
        {
            switch (readingType)
            {
                case ReadingType.Sleep:
                    //No Sum, Avg, Min, Max, Mod
                    if(aggregateType == AggregateType.None)
                    {
                        return true;
                    }
                    return false;
                case ReadingType.Steps:
                case ReadingType.CaloriesConsumed:
                case ReadingType.CaloriesExpended:
                case ReadingType.DistanceCycling:
                case ReadingType.DistanceWalkingRunning:
                case ReadingType.Hydration:
                    //No Avg, Min, Max, Mod
                    if (aggregateType != AggregateType.Sum && aggregateType != AggregateType.None)
                    {
                        return false;
                    }
                    else if (aggregateType == AggregateType.Sum && (aggregateTimeFrame == AggregateTimeFrame.All || aggregateTimeFrame == AggregateTimeFrame.Hour))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case ReadingType.Workout:
                    //No Avg, Min, Max, Mod
                    if (aggregateType != AggregateType.Sum && aggregateType != AggregateType.None)
                    {
                        return false;
                    }
                    else if (aggregateType == AggregateType.Sum && aggregateTimeFrame == AggregateTimeFrame.All)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case ReadingType.Weight:
                case ReadingType.Height:
                case ReadingType.BodyTemperature:
                    //No Sum, Avg
                    if (aggregateType == AggregateType.Sum || aggregateType == AggregateType.Average)
                    {
                        return false;
                    }
                    return true;
                case ReadingType.SpO2:
                case ReadingType.HeartRate:
                case ReadingType.BloodGlucose:
                case ReadingType.BloodPressure:
                    //No Sum, Mod
                    if (aggregateType == AggregateType.Sum || aggregateType == AggregateType.Mod)
                    {
                        return false;
                    }
                    return true;
                case ReadingType.Food:
                case ReadingType.Nutrition:
                case ReadingType.NutritionCalcium:
                case ReadingType.NutritionSodium:
                case ReadingType.NutritionSugar:
                case ReadingType.NutritionTotalCarbs:
                case ReadingType.NutritionTotalFat:
                case ReadingType.NutritionSaturatedFat:
                case ReadingType.NutritionUnsaturatedFat:
                case ReadingType.NutritionVitaminA:
                case ReadingType.NutritionVitaminC:
                case ReadingType.NutritionProtein:
                case ReadingType.NutritionPotassium:
                case ReadingType.NutritionCalories:
                case ReadingType.NutritionCholesterol:
                case ReadingType.NutritionDietaryFiber:
                case ReadingType.NutritionIron:
                    if (aggregateType == AggregateType.None || (aggregateType == AggregateType.Sum && aggregateTimeFrame == AggregateTimeFrame.Day))
                    {
                        return true;
                    }
                    return false;
                case ReadingType.BodyFat:
                    if (aggregateType == AggregateType.None)
                    {
                        return true;
                    }
                    return false;
                case ReadingType.BPSystolic:
                case ReadingType.BPDiastolic:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets aggregated value for provided grouped data
        /// </summary>
        /// <param name="aggregateType">Type of aggregation</param>
        /// <param name="grouedData">List of data for which value needs to be aggregated</param>
        /// <param name="isForDuration">true if average is for duration</param>
        /// <returns>Reading with needs to be added</returns>
        public static double GetAggregatedValue(AggregateType aggregateType, List<HealthReadingModel> grouedData, bool isForDuration)
        {
            bool isRoundOffValueNeeded = false;
            switch (aggregateType)
            {
                case AggregateType.Sum:
                    return (from item in grouedData select GetValueToBeSelected(item, isForDuration)).Sum();
                case AggregateType.Average:
                    isRoundOffValueNeeded = IsRoundOffValueRequired(grouedData[0].ReadingType);
                    var avg = (from item in grouedData select GetValueToBeSelected(item, isForDuration)).Average();
                    return isRoundOffValueNeeded ? Math.Ceiling(avg) : avg;
                case AggregateType.Maximum:
                    return (from item in grouedData select GetValueToBeSelected(item, isForDuration)).Max();
                case AggregateType.Minimum:
                    return (from item in grouedData select GetValueToBeSelected(item, isForDuration)).Min();
                case AggregateType.Mod:
                    isRoundOffValueNeeded = IsRoundOffValueRequired(grouedData[0].ReadingType);
                    var mod = ((from item in grouedData select GetValueToBeSelected(item, isForDuration)).Max() + (from item in grouedData select GetValueToBeSelected(item, isForDuration)).Min()) / 2;
                    return isRoundOffValueNeeded ? Math.Ceiling(mod) : mod;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Aggregates the readings based on the given duration
        /// </summary>
        /// <param name="healthReading">Reading values</param>
        /// <param name="readingType">Type of reading</param>
        /// <param name="aggregateType">Type of aggregation</param>
        /// <param name="aggregateTimeframe">Time frame over which aggregation is to be performed</param>
        public static void GetAggregatedReadings(HealthReadingDTO healthReading, ReadingType readingType, AggregateType aggregateType, AggregateTimeFrame aggregateTimeframe)
        {
            if (aggregateType != AggregateType.None)
            {
                List<List<HealthReadingModel>> groupedData;
                if (aggregateTimeframe == AggregateTimeFrame.Hour)
                {
                    groupedData = healthReading.HealthReadings.GroupBy(a =>
                    {
                        var dt = a.CreatedOn;
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
                    }).Select(b => b.ToList()).ToList();
                }
                else if (aggregateTimeframe == AggregateTimeFrame.Day)
                {
                    groupedData = healthReading.HealthReadings.GroupBy(a =>
                    {
                        var dt = a.CreatedOn;
                        return new DateTime(dt.Year, dt.Month, dt.Day);
                    }).Select(b => b.ToList()).ToList();
                }
                else
                {
                    groupedData = healthReading.HealthReadings.GroupBy(a => a.CreatedOn.Year)
                        .Select(b => b.ToList()).ToList();
                }
                var aggregatedData = (from item in groupedData
                                      select new HealthReadingModel
                                      {
                                          ActivityType = item.All(x => x.ActivityType == item[0].ActivityType) ? item[0].ActivityType : string.Empty,
                                          CreatedOn = item[0].CreatedOn,
                                          ReadingValue = GenericMethods.GetAggregatedValue(aggregateType, item, false),
                                          Duration = GenericMethods.GetAggregatedValue(aggregateType, item, true),
                                          ReadingUnit = GenericMethods.GetReadingUnitForType(readingType),
                                          ReadingType = readingType
                                      }).ToList();
                healthReading.HealthReadings = aggregatedData;
            }
        }

        /// <summary>
        /// Gets If all provided vales are below 1 (Cannot check equality in double therefore less than 1)
        /// </summary>
        /// <param name="values">nutrition values</param>
        /// <returns>true if all the values are below 1</returns>
        public static bool GetIfAllValuesAreZero(params double[] values)
        {
            return values.All(x => x < 1);
        }

        /// <summary>
        /// Gets all reading types belonging to vitals
        /// </summary>
        /// <returns>List of vitals</returns>
        public static List<ReadingType> GetVitalReadingTypes()
        {
            return new List<ReadingType>
            {
                ReadingType.Height,
                ReadingType.Weight,
                ReadingType.Steps,
                ReadingType.HeartRate,
                ReadingType.BloodPressure,
                ReadingType.BloodGlucose,
                ReadingType.SpO2,
                ReadingType.BodyFat,
                ReadingType.CaloriesConsumed,
                ReadingType.CaloriesExpended,
                ReadingType.Hydration,
                ReadingType.BodyTemperature,
                ReadingType.Sleep,
                ReadingType.BMI
            };
        }

        /// <summary>
        /// Checks if reading type belongs to workout
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <returns>true if reading type is workout, else false</returns>
        public static bool IsReadingWorkout(ReadingType readingType)
        {
            int readingTypeSequence = (int)readingType;
            return readingTypeSequence > 692 && readingTypeSequence < 715;
        }

        /// <summary>
        /// Checks if list contains workout type reading
        /// </summary>
        /// <param name="readingTypes">List of readings</param>
        /// <returns>true if list contains reading types workout, else false</returns>
        public static bool IsContainsReadingWorkout(List<ReadingType> readingTypes)
        {
            return readingTypes.Any(x => (int)x > 692 && (int)x < 715);
        }

        /// <summary>
        /// Checks if list contains workout type reading
        /// </summary>
        /// <param name="readingTypes">List of readings</param>
        /// <returns>true if list contains reading types workout, else false</returns>
        public static bool IsContainsReadingWorkout(List<short> readingTypes)
        {
            return readingTypes.Any(x => (short)x > 692 && (short)x < 715);
        }

        /// <summary>
        /// Checks if reading type belongs to nutrition
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <returns>true if reading type is nutrition, else false</returns>
        public static bool IsReadingNutrition(ReadingType readingType)
        {
            int readingTypeSequence = (int)readingType;
            System.Diagnostics.Debug.WriteLine($"readingType==={readingType}==readingTypeSequence=={readingTypeSequence}");
            return readingTypeSequence == 553||(readingTypeSequence > 677 && readingTypeSequence!= 681 && readingTypeSequence < 692);
            
        }

        /// <summary>
        /// Checks if reading type belongs to nutrition
        /// </summary>
        /// <param name="readingType">Type of reading</param>
        /// <returns>true if reading type is nutrition, else false</returns>
        public static bool IsReadingNutrition(short readingType)
        {
            int readingTypeSequence = (short)readingType;
            return readingTypeSequence > 300 && readingTypeSequence < 400;
        }

        /// <summary>
        /// Checks if list contains nutrition type reading
        /// </summary>
        /// <param name="readingTypes">List of readings</param>
        /// <returns>true if list contains reading types nutrition, else false</returns>
        public static bool IsContainsReadingNutrition(List<ReadingType> readingTypes)
        {
            return readingTypes.Any(x => (short)x == 553 || (short)x > 677 && (short)x < 692);
        }

        /// <summary>
        /// Checks if list contains nutrition type reading
        /// </summary>
        /// <param name="readingTypes">List of readings</param>
        /// <returns>true if list contains reading types nutrition, else false</returns>
        public static bool IsContainsReadingNutrition(List<short> readingTypes)
        {
            return readingTypes.Any(x => (short)x == 553|| (short)x > 677 && (short)x <692);
        }

        /// <summary>
        /// Updates/Adds errorcode for reading type
        /// </summary>
        /// <param name="errors">dictionary of readingtypes requested</param>
        /// <param name="readingType">reading type</param>
        /// <param name="errorCode">errorcode for reading type</param>
        public static void AddErrorCodeToResponse(Dictionary<ReadingType, ErrorCode> errors, ReadingType readingType, ErrorCode errorCode)
        {
            if (errors.ContainsKey(readingType))
            {
                errors[readingType] = errorCode;
            }
            else
            {
                errors.Add(readingType, errorCode);
            }
        }

        private static double GetValueToBeSelected(HealthReadingModel healthReadingModel, bool isForDuration)
        {
            return isForDuration ? healthReadingModel.Duration : healthReadingModel.ReadingValue;
        }
    }
}