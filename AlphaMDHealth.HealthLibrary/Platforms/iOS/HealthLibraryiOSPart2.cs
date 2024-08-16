//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;
//using Foundation;
//using HealthKit;
//using AlphaMDHealth.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;

using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Foundation;
using HealthKit;

namespace AlphaMDHealth.HealthLibrary.Platforms
{
    public partial class HealthLibraryiOS
    {
        private double GetRoundedValue(double readingValue, ReadingType readingType)
        {
            if (readingType == ReadingType.BodyFat || readingType == ReadingType.SpO2)
            {
                return readingValue * 100;
            }
            if (readingType == ReadingType.Height)
            {
                return Math.Round(readingValue, 2);
            }
            if (readingType == ReadingType.Hydration)
            {
                return Math.Round(readingValue, 3);
            }
            return GenericMethods.IsRoundOffValueRequired(readingType) ? Math.Ceiling(readingValue) : Math.Round(readingValue, 1);
        }

        private NSDate GetActivityDate(HKStatistics sampleData, ReadingType readingType)
        {
            switch (readingType)
            {
                case ReadingType.Steps:
                case ReadingType.Hydration:
                case ReadingType.CaloriesConsumed:
                case ReadingType.CaloriesExpended:
                case ReadingType.Weight:
                case ReadingType.Height:
                    return sampleData.StartDate;
                default:
                    return sampleData.EndDate;
            }
        }

        private ReadingType GetReadingTypeOfActivity(HKWorkoutActivityType activityType)
        {
            return activityType switch
            {
                HKWorkoutActivityType.AmericanFootball => ReadingType.FootballAmerican,
                HKWorkoutActivityType.Archery => ReadingType.Archery,
                HKWorkoutActivityType.AustralianFootball => ReadingType.FootballAustralian,
                HKWorkoutActivityType.Badminton => ReadingType.Badminton,
                HKWorkoutActivityType.Baseball => ReadingType.Baseball,
                HKWorkoutActivityType.Basketball => ReadingType.Basketball,
                HKWorkoutActivityType.Bowling => ReadingType.Bowling,
                HKWorkoutActivityType.Boxing => ReadingType.Boxing,
                HKWorkoutActivityType.Climbing => ReadingType.Climbing,
                HKWorkoutActivityType.Cricket => ReadingType.Cricket,
                HKWorkoutActivityType.CrossTraining => ReadingType.Crossfit,
                HKWorkoutActivityType.Curling => ReadingType.Curling,
                HKWorkoutActivityType.Cycling => ReadingType.Bicycling,
                HKWorkoutActivityType.Dance => ReadingType.Dancing,
                HKWorkoutActivityType.Elliptical => ReadingType.Elliptical,
                HKWorkoutActivityType.Fencing => ReadingType.Fencing,
                HKWorkoutActivityType.Fishing => ReadingType.Fishing,
                HKWorkoutActivityType.FunctionalStrengthTraining => ReadingType.StrengthTraining,
                HKWorkoutActivityType.Golf => ReadingType.Golf,
                HKWorkoutActivityType.Gymnastics => ReadingType.Gymnastics,
                HKWorkoutActivityType.Handball => ReadingType.Handball,
                HKWorkoutActivityType.Hiking => ReadingType.Hiking,
                HKWorkoutActivityType.Hockey => ReadingType.Hockey,
                HKWorkoutActivityType.Hunting => ReadingType.Hunting,
                HKWorkoutActivityType.Lacrosse => ReadingType.Lacrosse,
                HKWorkoutActivityType.MartialArts => ReadingType.MartialArts,
                HKWorkoutActivityType.MindAndBody => ReadingType.Meditation,
                HKWorkoutActivityType.MixedMetabolicCardioTraining => ReadingType.Cardio,
                HKWorkoutActivityType.PaddleSports => ReadingType.StandupPaddleboarding,
                HKWorkoutActivityType.Play => ReadingType.TeamSports,
                HKWorkoutActivityType.PreparationAndRecovery => ReadingType.Stretching,
                HKWorkoutActivityType.Racquetball => ReadingType.Racquetball,
                HKWorkoutActivityType.Rowing => ReadingType.Rowing,
                HKWorkoutActivityType.Rugby => ReadingType.Rugby,
                HKWorkoutActivityType.Running => ReadingType.Running,
                HKWorkoutActivityType.Sailing => ReadingType.Sailing,
                HKWorkoutActivityType.SkatingSports => ReadingType.Skating,
                HKWorkoutActivityType.SnowSports => ReadingType.SnowSports,
                HKWorkoutActivityType.Soccer => ReadingType.Soccer,
                HKWorkoutActivityType.Softball => ReadingType.Softball,
                HKWorkoutActivityType.Squash => ReadingType.Squash,
                HKWorkoutActivityType.StairClimbing => ReadingType.StairClimbing,
                HKWorkoutActivityType.SurfingSports => ReadingType.Surfing,
                HKWorkoutActivityType.Swimming => ReadingType.Swimming,
                HKWorkoutActivityType.TableTennis => ReadingType.TableTennis,
                HKWorkoutActivityType.Tennis => ReadingType.Tennis,
                HKWorkoutActivityType.TrackAndField => ReadingType.TrackAndField,
                HKWorkoutActivityType.TraditionalStrengthTraining => ReadingType.StrengthTraining,
                HKWorkoutActivityType.Volleyball => ReadingType.Volleyball,
                HKWorkoutActivityType.Walking => ReadingType.Walking,
                HKWorkoutActivityType.WaterFitness => ReadingType.WaterSports,
                HKWorkoutActivityType.WaterPolo => ReadingType.WaterSports,
                HKWorkoutActivityType.WaterSports => ReadingType.WaterSports,
                HKWorkoutActivityType.Wrestling => ReadingType.Wrestling,
                HKWorkoutActivityType.Yoga => ReadingType.Yoga,
                HKWorkoutActivityType.Barre => ReadingType.Barre,
                HKWorkoutActivityType.CoreTraining => ReadingType.CoreTraining,
                HKWorkoutActivityType.CrossCountrySkiing => ReadingType.Skiing,
                HKWorkoutActivityType.DownhillSkiing => ReadingType.Skiing,
                HKWorkoutActivityType.Flexibility => ReadingType.Stretching,
                HKWorkoutActivityType.HighIntensityIntervalTraining => ReadingType.HighIntensityIntervalTraining,
                HKWorkoutActivityType.JumpRope => ReadingType.JumpRope,
                HKWorkoutActivityType.Kickboxing => ReadingType.Kickboxing,
                HKWorkoutActivityType.Pilates => ReadingType.Pilates,
                HKWorkoutActivityType.Snowboarding => ReadingType.Snowboarding,
                HKWorkoutActivityType.Stairs => ReadingType.StairClimbing,
                HKWorkoutActivityType.StepTraining => ReadingType.StepTraining,
                HKWorkoutActivityType.WheelchairWalkPace => ReadingType.WheelChair,
                HKWorkoutActivityType.WheelchairRunPace => ReadingType.WheelChair,
                HKWorkoutActivityType.TaiChi => ReadingType.MartialArts,
                HKWorkoutActivityType.MixedCardio => ReadingType.Cardio,
                HKWorkoutActivityType.HandCycling => ReadingType.HandCycling,
                HKWorkoutActivityType.DiscSports => ReadingType.FrisbeeDisc,
                _ => ReadingType.Others
            };
        }

        private HKWorkoutActivityType GetActivityForReadingType(ReadingType activityType)
        {
            return activityType switch
            {
                ReadingType.FootballAmerican => HKWorkoutActivityType.AmericanFootball,
                ReadingType.Archery => HKWorkoutActivityType.Archery,
                ReadingType.FootballAustralian => HKWorkoutActivityType.AustralianFootball,
                ReadingType.Badminton => HKWorkoutActivityType.Badminton,
                ReadingType.Baseball => HKWorkoutActivityType.Baseball,
                ReadingType.Basketball => HKWorkoutActivityType.Basketball,
                ReadingType.Bowling => HKWorkoutActivityType.Bowling,
                ReadingType.Boxing => HKWorkoutActivityType.Boxing,
                ReadingType.Climbing => HKWorkoutActivityType.Climbing,
                ReadingType.Cricket => HKWorkoutActivityType.Cricket,
                ReadingType.Crossfit => HKWorkoutActivityType.CrossTraining,
                ReadingType.Curling => HKWorkoutActivityType.Curling,
                ReadingType.Bicycling => HKWorkoutActivityType.Cycling,
                ReadingType.Dancing => HKWorkoutActivityType.Dance,
                ReadingType.Elliptical => HKWorkoutActivityType.Elliptical,
                ReadingType.Fencing => HKWorkoutActivityType.Fencing,
                ReadingType.Fishing => HKWorkoutActivityType.Fishing,
                ReadingType.StrengthTraining => HKWorkoutActivityType.FunctionalStrengthTraining,
                ReadingType.Golf => HKWorkoutActivityType.Golf,
                ReadingType.Gymnastics => HKWorkoutActivityType.Gymnastics,
                ReadingType.Handball => HKWorkoutActivityType.Handball,
                ReadingType.Hiking => HKWorkoutActivityType.Hiking,
                ReadingType.Hockey => HKWorkoutActivityType.Hockey,
                ReadingType.Hunting => HKWorkoutActivityType.Hunting,
                ReadingType.Lacrosse => HKWorkoutActivityType.Lacrosse,
                ReadingType.MartialArts => HKWorkoutActivityType.MartialArts,
                ReadingType.Meditation => HKWorkoutActivityType.MindAndBody,
                ReadingType.StandupPaddleboarding => HKWorkoutActivityType.PaddleSports,
                ReadingType.TeamSports => HKWorkoutActivityType.Play,
                ReadingType.Racquetball => HKWorkoutActivityType.Racquetball,
                ReadingType.Rowing => HKWorkoutActivityType.Rowing,
                ReadingType.Rugby => HKWorkoutActivityType.Rugby,
                ReadingType.Running => HKWorkoutActivityType.Running,
                ReadingType.Jogging => HKWorkoutActivityType.Running,
                ReadingType.Sailing => HKWorkoutActivityType.Sailing,
                ReadingType.Skating => HKWorkoutActivityType.SkatingSports,
                ReadingType.SnowSports => HKWorkoutActivityType.SnowSports,
                ReadingType.Soccer => HKWorkoutActivityType.Soccer,
                ReadingType.Softball => HKWorkoutActivityType.Softball,
                ReadingType.Squash => HKWorkoutActivityType.Squash,
                ReadingType.StairClimbing => HKWorkoutActivityType.StairClimbing,
                ReadingType.Surfing => HKWorkoutActivityType.SurfingSports,
                ReadingType.Swimming => HKWorkoutActivityType.Swimming,
                ReadingType.TableTennis => HKWorkoutActivityType.TableTennis,
                ReadingType.Tennis => HKWorkoutActivityType.Tennis,
                ReadingType.TrackAndField => HKWorkoutActivityType.TrackAndField,
                ReadingType.Weightlifting => HKWorkoutActivityType.TraditionalStrengthTraining,
                ReadingType.Volleyball => HKWorkoutActivityType.Volleyball,
                ReadingType.Walking => HKWorkoutActivityType.Walking,
                ReadingType.WaterSports => HKWorkoutActivityType.WaterSports,
                ReadingType.Wrestling => HKWorkoutActivityType.Wrestling,
                ReadingType.Yoga => HKWorkoutActivityType.Yoga,
                ReadingType.Barre => HKWorkoutActivityType.Barre,
                ReadingType.CoreTraining => HKWorkoutActivityType.CoreTraining,
                ReadingType.Skiing => HKWorkoutActivityType.CrossCountrySkiing,
                ReadingType.Stretching => HKWorkoutActivityType.Flexibility,
                ReadingType.HighIntensityIntervalTraining => HKWorkoutActivityType.HighIntensityIntervalTraining,
                ReadingType.JumpRope => HKWorkoutActivityType.JumpRope,
                ReadingType.Kickboxing => HKWorkoutActivityType.Kickboxing,
                ReadingType.Pilates => HKWorkoutActivityType.Pilates,
                ReadingType.Snowboarding => HKWorkoutActivityType.Snowboarding,
                ReadingType.StepTraining => HKWorkoutActivityType.StepTraining,
                ReadingType.WheelChair => HKWorkoutActivityType.WheelchairWalkPace,
                ReadingType.Cardio => HKWorkoutActivityType.MixedCardio,
                ReadingType.HandCycling => HKWorkoutActivityType.HandCycling,
                ReadingType.FrisbeeDisc => HKWorkoutActivityType.DiscSports,
                _ => HKWorkoutActivityType.Other
            };
        }
        private ReadingMoment? GetReadingMoment(HKSample sample, ReadingType readingType)
        {
            if (readingType == ReadingType.BloodGlucose)
            {
                DateTimeOffset readingDate = ConvertToUtcDateTime(sample.EndDate).LocalDateTime;
                HKBloodGlucoseMealTime? bloodGlucoseMealTime = sample.Metadata.BloodGlucoseMealTime;
                //  var dataFromDictionary = GetDataFromDictionary(sample.Metadata.Dictionary, "BloodGlucoseMealTime");
                bloodGlucoseMealTime = bloodGlucoseMealTime == null ? HKBloodGlucoseMealTime.Preprandial : sample.Metadata.BloodGlucoseMealTime;//(HKBloodGlucoseMealTime)Enum.Parse(typeof(HKBloodGlucoseMealTime), dataFromDictionary);
                if (bloodGlucoseMealTime == HKBloodGlucoseMealTime.Postprandial
                    //todo:
                    //|| bloodGlucoseMealTime == HKBloodGlucoseMealTime.Ostprandial
                    )
                {
                    if (readingDate.Hour < HealthConstants.POST_BREAKFAST_TIME)
                    {
                        return ReadingMoment.GlucoseAfterBreakfast;
                    }
                    else if (readingDate.Hour < HealthConstants.POST_LUNCH_TIME)
                    {
                        return ReadingMoment.GlucoseAfterLunch;
                    }
                    else
                    {
                        return ReadingMoment.GlucoseAfterDinner;
                    }
                }
                else if (bloodGlucoseMealTime == HKBloodGlucoseMealTime.Preprandial)
                {
                    if (readingDate.Hour < HealthConstants.PRE_BREAKFAST_TIME)
                    {
                        return ReadingMoment.GlucoseFasting;
                    }
                    else if (readingDate.Hour < HealthConstants.PRE_LUNCH_TIME)
                    {
                        return ReadingMoment.GlucoseBeforeLunch;
                    }
                    else if (readingDate.Hour < HealthConstants.PRE_DINNER_TIME)
                    {
                        return ReadingMoment.GlucoseBeforeDinner;
                    }
                    else
                    {
                        return ReadingMoment.GlucoseBeforeBed;
                    }
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Converts .net DateTime to NSDate
        /// </summary>
        /// <param name="date">.net DateTime object</param>
        /// <returns>NSDate object with only date</returns>
        private NSDate ConvertToNSDate(DateTime date)
        {
            NSDate nsDate = ConvertToNSDateTime(date);
            const NSCalendarUnit preservedComponents = (NSCalendarUnit.Year | NSCalendarUnit.Month | NSCalendarUnit.Day);
            NSDateComponents components = NSCalendar.CurrentCalendar.Components(preservedComponents, nsDate);
            NSDate normalizedDate = NSCalendar.CurrentCalendar.DateFromComponents(components);
            return NSDate.FromTimeIntervalSinceReferenceDate(normalizedDate.SecondsSinceReferenceDate + NSTimeZone.DefaultTimeZone.SecondsFromGMT(normalizedDate));
        }

        /// <summary>
        /// Converts .net DateTime to NSDate 
        /// </summary>
        /// <param name="dateTime">.net DateTime object</param>
        /// <returns>NSDate object with DateTime</returns>
        private NSDate ConvertToNSDateTime(DateTime dateTime)
        {
            DateTime reference = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2001, 1, 1, 0, 0, 0), TimeZoneInfo.Local);
            return NSDate.FromTimeIntervalSinceReferenceDate((dateTime - reference).TotalSeconds);
        }

        /// <summary>
        /// Converts NSDate object to .net DateTimeOffset
        /// </summary>
        /// <param name="date">NSDate object</param>
        /// <returns>.net DateTimeOffset object</returns>
        private DateTimeOffset ConvertToUtcDateTime(NSDate date)
        {
            DateTimeOffset reference = new DateTimeOffset(2001, 1, 1, 0, 0, 0, TimeSpan.Zero);
            return reference.AddSeconds(date.SecondsSinceReferenceDate);
        }

        private HKSampleType ConvertToHKSampleType(ReadingType readingType)
        {
            return readingType switch
            {
                ReadingType.HeartRate => HKQuantityType.Create(HKQuantityTypeIdentifier.HeartRate),
                ReadingType.Steps => HKQuantityType.Create(HKQuantityTypeIdentifier.StepCount),
                ReadingType.Weight => HKQuantityType.Create(HKQuantityTypeIdentifier.BodyMass),
                ReadingType.Height => HKQuantityType.Create(HKQuantityTypeIdentifier.Height),
                ReadingType.BloodPressure => HKCorrelationType.Create(HKCorrelationTypeIdentifier.BloodPressure),
                ReadingType.BloodGlucose => HKQuantityType.Create(HKQuantityTypeIdentifier.BloodGlucose),
                ReadingType.CaloriesConsumed => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryEnergyConsumed),
                ReadingType.CaloriesExpended => HKQuantityType.Create(HKQuantityTypeIdentifier.BasalEnergyBurned),
                ReadingType.Nutrition => HKCorrelationType.Create(HKCorrelationTypeIdentifier.Food),
                ReadingType.BodyFat => HKQuantityType.Create(HKQuantityTypeIdentifier.BodyFatPercentage),
                ReadingType.DistanceWalkingRunning => HKQuantityType.Create(HKQuantityTypeIdentifier.DistanceWalkingRunning),
                ReadingType.DistanceCycling => HKQuantityType.Create(HKQuantityTypeIdentifier.DistanceCycling),
                ReadingType.BodyTemperature => HKQuantityType.Create(HKQuantityTypeIdentifier.BodyTemperature),
                ReadingType.Hydration => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryWater),
                ReadingType.Sleep => HKCategoryType.Create(HKCategoryTypeIdentifier.SleepAnalysis),
                //todo:
                //ReadingType.Workout => HKObjectType.GetWorkoutType(),
                ReadingType.SpO2 => HKQuantityType.Create(HKQuantityTypeIdentifier.OxygenSaturation),
                ReadingType.BMI => HKQuantityType.Create(HKQuantityTypeIdentifier.BodyMassIndex),
                ReadingType.NutritionProtein => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryProtein),
                ReadingType.NutritionSodium => HKQuantityType.Create(HKQuantityTypeIdentifier.DietarySodium),
                ReadingType.NutritionSugar => HKQuantityType.Create(HKQuantityTypeIdentifier.DietarySugar),
                ReadingType.NutritionTotalCarbs => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCarbohydrates),
                ReadingType.NutritionTotalFat => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatTotal),
                ReadingType.NutritionSaturatedFat => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatSaturated),
                ReadingType.NutritionUnsaturatedFat => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatPolyunsaturated),
                ReadingType.NutritionMonounsaturatedFat => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatMonounsaturated),
                ReadingType.NutritionPolyunsaturatedFat => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatPolyunsaturated),
                //ReadingType.NutritionTransFat => HKQuantityType.Create(HKQuantityTypeIdentifier.),
                ReadingType.NutritionVitaminA => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryVitaminA),
                ReadingType.NutritionVitaminC => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryVitaminC),
                ReadingType.NutritionPotassium => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryPotassium),
                ReadingType.NutritionCalcium => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCalcium),
                ReadingType.NutritionCalories => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryEnergyConsumed),
                ReadingType.NutritionCholesterol => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCholesterol),
                ReadingType.NutritionDietaryFiber => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFiber),
                ReadingType.NutritionIron => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryIron),
                ReadingType.NutritionZinc => HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryZinc),
                _ => throw new NotSupportedException($"Not supported for {readingType}"),
            };
        }

        private HKStatisticsOptions GetStatisticsOptions(AggregateType aggregateType)
        {
            return aggregateType switch
            {
                AggregateType.Average => HKStatisticsOptions.DiscreteAverage,
                AggregateType.Maximum => HKStatisticsOptions.DiscreteMax,
                AggregateType.Minimum => HKStatisticsOptions.DiscreteMin,
                AggregateType.Sum => HKStatisticsOptions.CumulativeSum,
                AggregateType.Mod => HKStatisticsOptions.DiscreteAverage,
                _ => HKStatisticsOptions.None,
            };
        }

        private void AddAggregateReadingsToHKObject(AggregateType aggregateType, HKStatistics result, List<HKStatistics> aggregateReadings)
        {
            switch (aggregateType)
            {
                case AggregateType.Mod:
                case AggregateType.Average:
                    if (result.AverageQuantity() != null)
                    {
                        aggregateReadings.Add(result);
                    }
                    break;
                case AggregateType.Maximum:
                    if (result.MaximumQuantity() != null)
                    {
                        aggregateReadings.Add(result);
                    }
                    break;
                case AggregateType.Minimum:
                    if (result.MinimumQuantity() != null)
                    {
                        aggregateReadings.Add(result);
                    }
                    break;
                case AggregateType.Sum:
                    if (result.SumQuantity() != null)
                    {
                        aggregateReadings.Add(result);
                    }
                    break;
                default:
                    // Intentionally put empty
                    break;
            }
        }

        private string GetDataFromDictionary(NSDictionary metaDataDictionary, params string[] keysToCheck)
        {
            string valueToReturn;
            foreach (string key in keysToCheck)
            {
                valueToReturn = GetData(metaDataDictionary, key);
                if (!string.IsNullOrWhiteSpace(valueToReturn))
                {
                    return valueToReturn;
                }
            }
            return string.Empty;
        }

        private string GetData(NSDictionary metaDataDictionary, string key)
        {
            NSObject nSObjectOfValue = metaDataDictionary.ValueForKey(new NSString(key));
            return nSObjectOfValue != null ? nSObjectOfValue.Description : string.Empty;
        }

        private HKQuantity GetQuantityForAggregationType(HKStatistics readingObject, AggregateType aggregateType)
        {
            switch (aggregateType)
            {
                case AggregateType.Average:
                    return readingObject.AverageQuantity();
                case AggregateType.Maximum:
                    return readingObject.MaximumQuantity();
                case AggregateType.Minimum:
                    return readingObject.MinimumQuantity();
                case AggregateType.Sum:
                    return readingObject.SumQuantity();
                case AggregateType.Mod:
                    return readingObject.AverageQuantity();
                case AggregateType.None:
                default:
                    return HKQuantity.FromQuantity(HKUnit.Inch, 0);
            }
        }

        private HKUnit GetUnitBasedOnReadingType(ReadingType readingType)
        {
            switch (readingType)
            {
                case ReadingType.SpO2:
                case ReadingType.BodyFat:
                    return HKUnit.Percent;
                case ReadingType.Steps:
                    return HKUnit.Count;
                case ReadingType.Hydration:
                    return HKUnit.Liter;
                case ReadingType.Weight:
                    return HKUnit.FromGramUnit(HKMetricPrefix.Kilo);
                case ReadingType.Height:
                case ReadingType.DistanceCycling:
                case ReadingType.DistanceWalkingRunning:
                    return HKUnit.FromLengthFormatterUnit(NSLengthFormatterUnit.Meter);
                case ReadingType.HeartRate:
                    return HKUnit.Count.UnitDividedBy(HKUnit.Minute);
                case ReadingType.BloodGlucose:
                    return HKUnit.CreateMoleUnit(HKMetricPrefix.Milli, HKUnit.MolarMassBloodGlucose).UnitDividedBy(HKUnit.Liter);
                case ReadingType.BloodPressure:
                    return HKUnit.MillimeterOfMercury;
                case ReadingType.CaloriesConsumed:
                case ReadingType.CaloriesExpended:
                case ReadingType.NutritionCalories:
                case ReadingType.Workout:
                    return HKUnit.Kilocalorie;
                case ReadingType.BodyTemperature:
                    return HKUnit.DegreeCelsius;
                case ReadingType.Sleep:
                    return HKUnit.Minute;
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
                    return HKUnit.FromGramUnit(HKMetricPrefix.Milli);
                default:
                    return null;
            }
        }

        private HKSampleType[] GetPermissionTypeForReadingTypes(List<ReadingType> readingTypes)
        {
            List<HKSampleType> permissionTypes = new List<HKSampleType>();
            MapPermissionTypes1(readingTypes, permissionTypes);
            MapPermissionTypes2(readingTypes, permissionTypes);
            MapPermissionTypes3(readingTypes, permissionTypes);
            MapPermissionTypes4(readingTypes, permissionTypes);
            MapPermissionTypes5(readingTypes, permissionTypes);
            MapPermissionTypes6(readingTypes, permissionTypes);
            if (readingTypes.Contains(ReadingType.Sleep))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.Sleep));
            }
            if (readingTypes.Contains(ReadingType.SpO2))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.SpO2));
            }
            return permissionTypes.ToArray();
        }

        private void MapPermissionTypes1(List<ReadingType> readingTypes, List<HKSampleType> permissionTypes)
        {
            if (readingTypes.Contains(ReadingType.Weight))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.Weight));
            }
            if (readingTypes.Contains(ReadingType.Height))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.Height));
            }
            if (readingTypes.Contains(ReadingType.Steps))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.Steps));
            }
            if (readingTypes.Contains(ReadingType.HeartRate))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.HeartRate));
            }
            if (readingTypes.Contains(ReadingType.BloodPressure))
            {
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.BloodPressureSystolic));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.BloodPressureDiastolic));
            }
        }

        private void MapPermissionTypes2(List<ReadingType> readingTypes, List<HKSampleType> permissionTypes)
        {
            if (readingTypes.Contains(ReadingType.BloodGlucose))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.BloodGlucose));
            }
            if (readingTypes.Contains(ReadingType.CaloriesConsumed))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.CaloriesConsumed));
            }
            if (readingTypes.Contains(ReadingType.CaloriesExpended))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.CaloriesExpended));
            }
            if (readingTypes.Contains(ReadingType.Food))
            {
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietarySodium));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietarySugar));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCarbohydrates));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCholesterol));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatTotal));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatSaturated));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatPolyunsaturated));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatMonounsaturated));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryVitaminA));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryVitaminC));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryProtein));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryPotassium));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCalcium));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryEnergyConsumed));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFiber));
                permissionTypes.Add(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryIron));
            }
            if (readingTypes.Contains(ReadingType.BodyFat))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.BodyFat));
            }
        }

        private void MapPermissionTypes3(List<ReadingType> readingTypes, List<HKSampleType> permissionTypes)
        {
            if (readingTypes.Contains(ReadingType.DistanceCycling))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.DistanceCycling));
            }
            if (readingTypes.Contains(ReadingType.DistanceWalkingRunning))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.DistanceWalkingRunning));
            }
            if (readingTypes.Contains(ReadingType.Hydration))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.Hydration));
            }
            if (readingTypes.Contains(ReadingType.BodyTemperature))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.BodyTemperature));
            }
            if (readingTypes.Contains(ReadingType.Workout))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.Workout));
            }
            if (readingTypes.Contains(ReadingType.BMI))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.BMI));
            }
        }

        private void MapPermissionTypes4(List<ReadingType> readingTypes, List<HKSampleType> permissionTypes)
        {
            if (readingTypes.Contains(ReadingType.NutritionSodium))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionSodium));
            }
            if (readingTypes.Contains(ReadingType.NutritionSugar))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionSugar));
            }
            if (readingTypes.Contains(ReadingType.NutritionTotalCarbs))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionTotalCarbs));
            }
            if (readingTypes.Contains(ReadingType.NutritionCholesterol))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionCholesterol));
            }
            if (readingTypes.Contains(ReadingType.NutritionTotalFat))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionTotalFat));
            }
            if (readingTypes.Contains(ReadingType.NutritionCalories))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionCalories));
            }
        }

        private void MapPermissionTypes5(List<ReadingType> readingTypes, List<HKSampleType> permissionTypes)
        {
            if (readingTypes.Contains(ReadingType.NutritionSaturatedFat))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionSaturatedFat));
            }
            if (readingTypes.Contains(ReadingType.NutritionPolyunsaturatedFat))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionPolyunsaturatedFat));
            }
            if (readingTypes.Contains(ReadingType.NutritionMonounsaturatedFat))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionMonounsaturatedFat));
            }
            if (readingTypes.Contains(ReadingType.NutritionVitaminA))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionVitaminA));
            }
            if (readingTypes.Contains(ReadingType.NutritionVitaminC))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionVitaminC));
            }
        }

        private void MapPermissionTypes6(List<ReadingType> readingTypes, List<HKSampleType> permissionTypes)
        {
            if (readingTypes.Contains(ReadingType.NutritionProtein))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionProtein));
            }
            if (readingTypes.Contains(ReadingType.NutritionPotassium))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionPotassium));
            }
            if (readingTypes.Contains(ReadingType.NutritionCalcium))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionCalcium));
            }
            if (readingTypes.Contains(ReadingType.NutritionDietaryFiber))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionDietaryFiber));
            }
            if (readingTypes.Contains(ReadingType.NutritionIron))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionIron));
            }
            if (readingTypes.Contains(ReadingType.NutritionZinc))
            {
                permissionTypes.Add(ConvertToHKSampleType(ReadingType.NutritionZinc));
            }
        }

        private void MapNutritionDataFromSamples(HealthReadingDTO healthReading, IEnumerable<HKQuantitySample> quantitySamples, NutritionReadingModel foodData)
        {
            MapFoodData1(quantitySamples, foodData);
            MapFoodData2(quantitySamples, foodData);
            MapFoodData3(quantitySamples, foodData);
            MapFoodData4(quantitySamples, foodData);
            if (GenericMethods.GetIfAllValuesAreZero(foodData.CalorieIntake, foodData.Protein, foodData.Carbs, foodData.Cholesterol, foodData.Fat, foodData.Fiber, foodData.Sodium, foodData.Sugar, foodData.VitaminA, foodData.VitaminC, foodData.Potassium, foodData.Calcium, foodData.Iron, foodData.SaturatedFat, foodData.UnsaturatedFat))
            {
                healthReading.NutritionReadings.Remove(foodData);
            }
        }

        private void MapFoodData1(IEnumerable<HKQuantitySample> quantitySamples, NutritionReadingModel foodData)
        {
            foodData.Fat += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatTotal)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.Carbs += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCarbohydrates)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.Protein += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryProtein)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.Sugar += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietarySugar)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
        }

        private void MapFoodData2(IEnumerable<HKQuantitySample> quantitySamples, NutritionReadingModel foodData)
        {
            foodData.Fiber += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFiber)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.CalorieIntake += Math.Ceiling(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryEnergyConsumed)))?.Quantity.GetDoubleValue(HKUnit.Kilocalorie) ?? 0);
            foodData.Cholesterol += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCholesterol)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.Sodium += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietarySodium)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
        }

        private void MapFoodData3(IEnumerable<HKQuantitySample> quantitySamples, NutritionReadingModel foodData)
        {
            foodData.VitaminA += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryVitaminA)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.VitaminC += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryVitaminC)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.Potassium += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryPotassium)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.Calcium += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryCalcium)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
        }

        private void MapFoodData4(IEnumerable<HKQuantitySample> quantitySamples, NutritionReadingModel foodData)
        {
            foodData.Iron += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryIron)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.SaturatedFat += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatSaturated)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
            foodData.UnsaturatedFat += Math.Round(quantitySamples.FirstOrDefault(x => x.SampleType.Equals(HKQuantityType.Create(HKQuantityTypeIdentifier.DietaryFatPolyunsaturated)))?.Quantity.GetDoubleValue(GetUnitBasedOnReadingType(ReadingType.Nutrition)) ?? 0, Constants.DIGITS_AFTER_DECIMAL);
        }

        private bool IsCorrelationTypeReading(ReadingType readingType)
        {
            switch (readingType)
            {
                case ReadingType.BloodPressure:
                case ReadingType.Nutrition:
                    return true;
                default:
                    return false;
            }
        }

        private MealType GetFoodType(string foodType)
        {
            switch (foodType)
            {
                case "Breakfast":
                    return MealType.Breakfast;
                case "Lunch":
                    return MealType.Lunch;
                case "Dinner":
                    return MealType.Dinner;
                case "Snacks":
                    return MealType.Snacks;
                default:
                    return MealType.Unknown;
            }
        }

        private ReadingMoment GetReadingMomentForSleep(int a)
        {
            switch (a)
            {
                case 0:
                    return ReadingMoment.SleepLight;
                case 1:
                    return ReadingMoment.SleepDeep;
                case 2:
                    return ReadingMoment.SleepLight;
                default:
                    return ReadingMoment.Sleep;
            }
        }

        private string GetMealTypeFromReadingMoment(ReadingMoment readingMoment)
        {
            switch (readingMoment)
            {
                default:
                case ReadingMoment.GlucoseAfterBreakfast:
                case ReadingMoment.GlucoseAfterLunch:
                    return HKBloodGlucoseMealTime.Postprandial.ToString();
                case ReadingMoment.GlucoseBeforeBed:
                case ReadingMoment.GlucoseBeforeDinner:
                case ReadingMoment.GlucoseBeforeLunch:
                case ReadingMoment.GlucoseFasting:
                    return HKBloodGlucoseMealTime.Preprandial.ToString();
            }
        }

        private string ConvertMealType(MealType mealType)
        {
            switch (mealType)
            {
                case MealType.Breakfast:
                    return "Breakfast";
                case MealType.Lunch:
                    return "Lunch";
                case MealType.Dinner:
                    return "Dinner";
                case MealType.Snacks:
                    return "Snacks";
                default:
                    return "";
            }
        }
    }
}