using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Android.Gms.Fitness;
using Android.Gms.Fitness.Data;
using Android.Gms.Fitness.Request;
using Android.Gms.Fitness.Result;
using Java.Util.Concurrent;
using System.Data;
using System.Globalization;
using DataSet = Android.Gms.Fitness.Data.DataSet;

namespace AlphaMDHealth.HealthLibrary.Platforms
{
    public partial class HealthLibraryAndroid
    {
        #region Supporting Methods
        private void MapReadings(HealthReadingDTO readings, DataReadResponse readResponse, ReadingType readingType, bool isAggregatedResponse,
            AggregateType aggregateType, bool shouldInclude, ReadingType[] readingTypes)
        {
            System.Diagnostics.Debug.WriteLine($"Mapdata from device=={readingType}");
            if (readingType == ReadingType.Food || GenericMethods.IsReadingNutrition(readingType))
            {
                MapFoodData(readings, readResponse, isAggregatedResponse, readingType, shouldInclude, readingTypes);
            }
            else if (readingType == ReadingType.SpO2)
            {
                MapSpO2Data(readings, readResponse, isAggregatedResponse, aggregateType);
            }
            else
            {
                MapReadingsData(readings, readResponse, isAggregatedResponse, aggregateType, readingType);
            }
        }

        private ReadingType GetReadingTypeForBloodPressure(Field val)
        {
            return val.Name == HealthFields.FieldBloodPressureSystolic.Name ? ReadingType.BPSystolic : ReadingType.BPDiastolic;
        }

        private void MapReadingsData(HealthReadingDTO readings, DataReadResponse readResponse, bool isAggregated,
            AggregateType aggregateType, ReadingType readingType)
        {
            bool isBloodPressure = readingType == ReadingType.BloodPressure;
            if (isAggregated)
            {
                readings.HealthReadings = (from item in readResponse.Buckets
                                           from data in item.DataSets
                                           from val in data.DataPoints
                                           from reading in val.DataType.Fields
                                           let bloodPressureValue = ExtractDataOnAggregation(isBloodPressure, aggregateType, reading)
                                           where isBloodPressure
                                                ? bloodPressureValue?.Item1 == 1
                                                : IsCorrectAggregationField(aggregateType, reading, readingType)
                                           select new HealthReadingModel
                                           {
                                               ReadingType = isBloodPressure ? bloodPressureValue.Item2 : readingType,
                                               ReadingUnit = GenericMethods.GetReadingUnitForType(readingType),
                                               ReadingValue = GetReadingFromDataPoint(readingType, val, reading),
                                               CreatedOn = FromUnixTime(val.GetStartTime(TimeUnit.Milliseconds))
                                           }).ToList();
            }
            else
            {

                readings.HealthReadings = (from item in readResponse.DataSets
                                           from data in item.DataPoints
                                           from val in data.DataType.Fields
                                           where !isBloodPressure
                                            || val.Name == HealthFields.FieldBloodPressureSystolic.Name
                                            || val.Name == HealthFields.FieldBloodPressureDiastolic.Name
                                           select new HealthReadingModel
                                           {
                                               ReadingType = isBloodPressure ? GetReadingTypeForBloodPressure(val) : readingType,
                                               ReadingUnit = GenericMethods.GetReadingUnitForType(readingType),
                                               ReadingValue = GetReadingFromDataPoint(readingType, data, val),
                                               CreatedOn = FromUnixTime(data.GetStartTime(TimeUnit.Milliseconds))
                                           }).ToList();
                readings.HealthReadings = readings.HealthReadings?.Where(x => x.ReadingValue > 0).ToList();
            }
        }

        private void MapSpO2Data(HealthReadingDTO readings, DataReadResponse readResponse, bool isAggregated, AggregateType aggregateType)
        {
            if (isAggregated)
            {
                readings.HealthReadings = (from item in readResponse.Buckets
                                           from data in item.DataSets
                                           from val in data.DataPoints
                                           from reading in val.DataType.Fields
                                           where CheckIfSpO2AggregationIsCorrect(aggregateType, reading)
                                           select new HealthReadingModel
                                           {
                                               ReadingType = ReadingType.SpO2,
                                               ReadingUnit = GenericMethods.GetReadingUnitForType(ReadingType.SpO2),
                                               ReadingValue = GetReadingFromDataPoint(ReadingType.SpO2, val, reading),
                                               CreatedOn = FromUnixTime(val.GetStartTime(TimeUnit.Milliseconds))
                                           }).ToList();
            }
            else
            {
                readings.HealthReadings = (from item in readResponse.DataSets
                                           from data in item.DataPoints
                                           from val in data.DataType.Fields
                                           where val.Name == HealthFields.FieldOxygenSaturation.Name
                                           select new HealthReadingModel
                                           {
                                               ReadingType = ReadingType.SpO2,
                                               ReadingUnit = GenericMethods.GetReadingUnitForType(ReadingType.SpO2),
                                               ReadingValue = GetReadingFromDataPoint(ReadingType.SpO2, data, val),
                                               CreatedOn = FromUnixTime(data.GetStartTime(TimeUnit.Milliseconds))
                                           }).ToList();
            }
        }

        private void MapFoodData(HealthReadingDTO readings, DataReadResponse readResponse, bool isAggregated, ReadingType readingType,
            bool shouldInclude, ReadingType[] readingTypes)
        {
            readings.NutritionReadings = new List<NutritionReadingModel>();
            readings.HealthReadings = new List<HealthReadingModel>();
            if (isAggregated)
            {
                foreach (var dataBucket in readResponse.Buckets)
                {
                    foreach (var dataSet in dataBucket.DataSets)
                    {
                        foreach (var dataPoint in dataSet.DataPoints)
                        {
                            DateTime foodDateTime = FromUnixTime(dataPoint.GetTimestamp(TimeUnit.Milliseconds));
                            MealType mealType = GetFoodType(dataPoint.GetValue(dataSet.DataType.Fields.FirstOrDefault(x => x.Name == Field.FieldMealType.Name)).AsInt());
                            string foodName = dataPoint.GetValue(dataSet.DataType.Fields.FirstOrDefault(x => x.Name == Field.FieldFoodItem.Name)).AsString();
                            Field nutrientField = dataSet.DataType.Fields.FirstOrDefault(x => x.Name == Field.FieldNutrients.Name);
                            ///// Removed mealType condition as If multiple mealtypes are recorded then mealType is returned as unknown
                            ///// https://developers.google.com/fit/scenarios/read-daily-nutrition#android
                            if (nutrientField != null)
                            {
                                MapFoodData(readings, dataPoint, foodDateTime, mealType, nutrientField, foodName, readingType != ReadingType.Food, shouldInclude, readingTypes);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var dataSet in readResponse.DataSets)
                {
                    foreach (var dataPoint in dataSet.DataPoints)
                    {
                        DateTime foodDateTime = FromUnixTime(dataPoint.GetTimestamp(TimeUnit.Milliseconds));
                        MealType mealType = GetFoodType(dataPoint.GetValue(dataSet.DataType.Fields.FirstOrDefault(x => x.Name == Field.FieldMealType.Name)).AsInt());
                        string foodName = dataPoint.GetValue(dataSet.DataType.Fields.FirstOrDefault(x => x.Name == Field.FieldFoodItem.Name)).AsString();
                        Field nutrientField = dataSet.DataType.Fields.FirstOrDefault(x => x.Name == Field.FieldNutrients.Name);
                        if (mealType != MealType.Unknown && nutrientField != null)
                        {
                            MapFoodData(readings, dataPoint, foodDateTime, mealType, nutrientField, foodName, readingType != ReadingType.Food, shouldInclude, readingTypes);
                        }
                    }
                }
            }
        }

        private void MapActivityResponse(HealthReadingDTO readings, DataReadResponse readResponse, bool shouldInclude, ReadingType[] readingTypes)
        {
            List<string> notPermittedActivities = new List<string> { FitnessActivities.Unknown, FitnessActivities.Still, FitnessActivities.Sleep, FitnessActivities.SleepAwake
                , FitnessActivities.SleepDeep, FitnessActivities.SleepLight, FitnessActivities.SleepRem, FitnessActivities.InVehicle, FitnessActivities.Other };
            readings.HealthReadings = new List<HealthReadingModel>();
            foreach (Bucket dataBucket in readResponse.Buckets)
            {
                if (notPermittedActivities.Find(x => x == dataBucket.Activity) == null)
                {
                    foreach (DataSet dataSet in dataBucket.DataSets)
                    {
                        foreach (DataPoint dataPoint in dataSet.DataPoints)
                        {
                            MapActivityData(readings, shouldInclude, readingTypes, dataBucket, dataPoint);
                        }
                    }
                }
            }
        }

        private void MapActivityData(HealthReadingDTO readings, bool shouldInclude, ReadingType[] readingTypes, Bucket dataBucket, DataPoint dataPoint)
        {
            ReadingType currentType = GetReadingTypeOfActivity(dataBucket.Activity);
            if (readingTypes == null
                || readingTypes.Contains(ReadingType.Workout)
                || (shouldInclude && readingTypes.Contains(currentType))
                || (!shouldInclude && !readingTypes.Contains(currentType)))
            {
                double caloriesBurned = 0;
                foreach (Field field in dataPoint.DataType.Fields)
                {
                    if (field.Name == Field.FieldCalories.Name)
                    {
                        caloriesBurned += Math.Round(dataPoint.GetValue(field).AsFloat(), 1);
                    }
                }
                readings.HealthReadings.Add(new HealthReadingModel
                {
                    ActivityType = currentType.ToString(),
                    ReadingType = ReadingType.Workout,
                    CreatedOn = FromUnixTime(dataPoint.GetStartTime(TimeUnit.Milliseconds)),
                    Duration = (int)(dataPoint.GetEndTime(TimeUnit.Minutes) - dataPoint.GetStartTime(TimeUnit.Minutes)),
                    ReadingValue = caloriesBurned,
                    ReadingUnit = GenericMethods.GetReadingUnitForType(ReadingType.Workout)
                });
            }
        }

        private ReadingType GetReadingTypeOfActivity(string activityType)
        {
            return activityType switch
            {
                FitnessActivities.Aerobics => ReadingType.Cardio,
                FitnessActivities.Squash => ReadingType.Squash,
                FitnessActivities.Softball => ReadingType.Softball,
                FitnessActivities.Snowshoeing => ReadingType.SnowSports,
                FitnessActivities.Snowmobile => ReadingType.SnowSports,
                FitnessActivities.Snowboarding => ReadingType.Snowboarding,
                FitnessActivities.Sledding => ReadingType.Sledding,
                FitnessActivities.SkiingRoller => ReadingType.Skiing,
                FitnessActivities.SkiingKite => ReadingType.Skiing,
                FitnessActivities.SkiingDownhill => ReadingType.Skiing,
                FitnessActivities.SkiingCrossCountry => ReadingType.Skiing,
                FitnessActivities.SkiingBackCountry => ReadingType.Skiing,
                FitnessActivities.Skiing => ReadingType.Skiing,
                FitnessActivities.SkatingInline => ReadingType.Skating,
                FitnessActivities.SkatingIndoor => ReadingType.Skating,
                FitnessActivities.SkatingCross => ReadingType.Skating,
                FitnessActivities.Skating => ReadingType.Skating,
                FitnessActivities.Skateboarding => ReadingType.Skateboarding,
                FitnessActivities.ScubaDiving => ReadingType.ScubaDiving,
                FitnessActivities.Sailing => ReadingType.Sailing,
                FitnessActivities.RunningTreadmill => ReadingType.Treadmill,
                FitnessActivities.RunningSand => ReadingType.Running,
                FitnessActivities.RunningJogging => ReadingType.Jogging,
                FitnessActivities.StairClimbing => ReadingType.StairClimbing,
                FitnessActivities.StairClimbingMachine => ReadingType.StairClimbing,
                FitnessActivities.StandupPaddleboarding => ReadingType.StandupPaddleboarding,
                FitnessActivities.Zumba => ReadingType.Zumba,
                FitnessActivities.Yoga => ReadingType.Yoga,
                FitnessActivities.Windsurfing => ReadingType.Surfing,
                FitnessActivities.Wheelchair => ReadingType.WheelChair,
                FitnessActivities.Weightlifting => ReadingType.Weightlifting,
                FitnessActivities.WaterPolo => ReadingType.WaterSports,
                FitnessActivities.WalkingTreadmill => ReadingType.Treadmill,
                FitnessActivities.WalkingStroller => ReadingType.Walking,
                FitnessActivities.WalkingNordic => ReadingType.Walking,
                FitnessActivities.WalkingFitness => ReadingType.Walking,
                FitnessActivities.Walking => ReadingType.Walking,
                FitnessActivities.Wakeboarding => ReadingType.WaterSports,
                FitnessActivities.VolleyballIndoor => ReadingType.Volleyball,
                FitnessActivities.Rugby => ReadingType.Rugby,
                FitnessActivities.VolleyballBeach => ReadingType.Volleyball,
                FitnessActivities.Treadmill => ReadingType.Treadmill,
                FitnessActivities.Tennis => ReadingType.Tennis,
                FitnessActivities.TeamSports => ReadingType.TeamSports,
                FitnessActivities.TableTennis => ReadingType.TableTennis,
                FitnessActivities.SwimmingPool => ReadingType.Swimming,
                FitnessActivities.SwimmingOpenWater => ReadingType.Swimming,
                FitnessActivities.Swimming => ReadingType.Swimming,
                FitnessActivities.Surfing => ReadingType.Surfing,
                FitnessActivities.StrengthTraining => ReadingType.StrengthTraining,
                FitnessActivities.Volleyball => ReadingType.Volleyball,
                FitnessActivities.RowingMachine => ReadingType.RowingMachine,
                FitnessActivities.Running => ReadingType.Running,
                FitnessActivities.RockClimbing => ReadingType.RockClimbing,
                FitnessActivities.FootballAustralian => ReadingType.FootballAustralian,
                FitnessActivities.FootballAmerican => ReadingType.FootballAmerican,
                FitnessActivities.Fencing => ReadingType.Fencing,
                FitnessActivities.Ergometer => ReadingType.Ergometer,
                FitnessActivities.Elliptical => ReadingType.Elliptical,
                FitnessActivities.Diving => ReadingType.Diving,
                FitnessActivities.Dancing => ReadingType.Dancing,
                FitnessActivities.Curling => ReadingType.Curling,
                FitnessActivities.Crossfit => ReadingType.Crossfit,
                FitnessActivities.Cricket => ReadingType.Cricket,
                FitnessActivities.Rowing => ReadingType.Rowing,
                FitnessActivities.CircuitTraining => ReadingType.CircuitTraining,
                FitnessActivities.Boxing => ReadingType.Boxing,
                FitnessActivities.BikingUtility => ReadingType.Bicycling,
                FitnessActivities.BikingStationary => ReadingType.Bicycling,
                FitnessActivities.BikingSpinning => ReadingType.Bicycling,
                FitnessActivities.BikingRoad => ReadingType.Bicycling,
                FitnessActivities.BikingMountain => ReadingType.Bicycling,
                FitnessActivities.BikingHand => ReadingType.HandCycling,
                FitnessActivities.Biking => ReadingType.Bicycling,
                FitnessActivities.Biathlon => ReadingType.Biathlon,
                FitnessActivities.Basketball => ReadingType.Basketball,
                FitnessActivities.Baseball => ReadingType.Baseball,
                FitnessActivities.Badminton => ReadingType.Badminton,
                FitnessActivities.Archery => ReadingType.Archery,
                FitnessActivities.Calisthenics => ReadingType.Calisthenics,
                FitnessActivities.FrisbeeDisc => ReadingType.FrisbeeDisc,
                FitnessActivities.FootballSoccer => ReadingType.Soccer,
                FitnessActivities.Golf => ReadingType.Golf,
                FitnessActivities.Racquetball => ReadingType.Racquetball,
                FitnessActivities.Polo => ReadingType.Polo,
                FitnessActivities.Pilates => ReadingType.Pilates,
                FitnessActivities.Paragliding => ReadingType.Paragliding,
                FitnessActivities.P90x => ReadingType.P90X,
                FitnessActivities.OnFoot => ReadingType.Walking,
                FitnessActivities.MixedMartialArts => ReadingType.MixedMartialArts,
                FitnessActivities.Meditation => ReadingType.Meditation,
                FitnessActivities.MartialArts => ReadingType.MartialArts,
                FitnessActivities.Gardening => ReadingType.Gardening,
                FitnessActivities.KickScooter => ReadingType.KickScooter,
                FitnessActivities.Kitesurfing => ReadingType.Surfing,
                FitnessActivities.KettlebellTraining => ReadingType.KettlebellTraining,
                FitnessActivities.Gymnastics => ReadingType.Gymnastics,
                FitnessActivities.Handball => ReadingType.Handball,
                FitnessActivities.HighIntensityIntervalTraining => ReadingType.HighIntensityIntervalTraining,
                FitnessActivities.Kickboxing => ReadingType.Kickboxing,
                FitnessActivities.Hockey => ReadingType.Hockey,
                FitnessActivities.HorsebackRiding => ReadingType.HorsebackRiding,
                FitnessActivities.Hiking => ReadingType.Hiking,
                FitnessActivities.IceSkating => ReadingType.IceSkating,
                FitnessActivities.IntervalTraining => ReadingType.IntervalTraining,
                FitnessActivities.JumpRope => ReadingType.JumpRope,
                FitnessActivities.Kayaking => ReadingType.Kayaking,
                FitnessActivities.Housework => ReadingType.Housework,
                _ => ReadingType.Others,
            };
        }

        private string GetActivityForReadingType(ReadingType activityType)
        {
            return activityType switch
            {
                ReadingType.Cardio => FitnessActivities.Aerobics,
                ReadingType.Squash => FitnessActivities.Squash,
                ReadingType.Softball => FitnessActivities.Softball,
                ReadingType.SnowSports => FitnessActivities.Snowmobile,
                ReadingType.Snowboarding => FitnessActivities.Snowboarding,
                ReadingType.Sledding => FitnessActivities.Sledding,
                ReadingType.Skiing => FitnessActivities.Skiing,
                ReadingType.Skating => FitnessActivities.Skating,
                ReadingType.Skateboarding => FitnessActivities.Skateboarding,
                ReadingType.ScubaDiving => FitnessActivities.ScubaDiving,
                ReadingType.Sailing => FitnessActivities.Sailing,
                ReadingType.Jogging => FitnessActivities.RunningJogging,
                ReadingType.StairClimbing => FitnessActivities.StairClimbing,
                ReadingType.StandupPaddleboarding => FitnessActivities.StandupPaddleboarding,
                ReadingType.Zumba => FitnessActivities.Zumba,
                ReadingType.Yoga => FitnessActivities.Yoga,
                ReadingType.WheelChair => FitnessActivities.Wheelchair,
                ReadingType.Weightlifting => FitnessActivities.Weightlifting,
                ReadingType.Walking => FitnessActivities.Walking,
                ReadingType.WaterSports => FitnessActivities.Wakeboarding,
                ReadingType.Rugby => FitnessActivities.Rugby,
                ReadingType.Treadmill => FitnessActivities.Treadmill,
                ReadingType.Tennis => FitnessActivities.Tennis,
                ReadingType.TeamSports => FitnessActivities.TeamSports,
                ReadingType.TableTennis => FitnessActivities.TableTennis,
                ReadingType.Swimming => FitnessActivities.Swimming,
                ReadingType.Surfing => FitnessActivities.Surfing,
                ReadingType.StrengthTraining => FitnessActivities.StrengthTraining,
                ReadingType.Volleyball => FitnessActivities.Volleyball,
                ReadingType.RowingMachine => FitnessActivities.RowingMachine,
                ReadingType.Running => FitnessActivities.Running,
                ReadingType.RockClimbing => FitnessActivities.RockClimbing,
                ReadingType.FootballAustralian => FitnessActivities.FootballAustralian,
                ReadingType.FootballAmerican => FitnessActivities.FootballAmerican,
                ReadingType.Fencing => FitnessActivities.Fencing,
                ReadingType.Ergometer => FitnessActivities.Ergometer,
                ReadingType.Elliptical => FitnessActivities.Elliptical,
                ReadingType.Diving => FitnessActivities.Diving,
                ReadingType.Dancing => FitnessActivities.Dancing,
                ReadingType.Curling => FitnessActivities.Curling,
                ReadingType.Crossfit => FitnessActivities.Crossfit,
                ReadingType.Cricket => FitnessActivities.Cricket,
                ReadingType.Rowing => FitnessActivities.Rowing,
                ReadingType.CircuitTraining => FitnessActivities.CircuitTraining,
                ReadingType.Boxing => FitnessActivities.Boxing,
                ReadingType.Bicycling => FitnessActivities.Biking,
                ReadingType.Biathlon => FitnessActivities.Biathlon,
                ReadingType.Basketball => FitnessActivities.Basketball,
                ReadingType.Baseball => FitnessActivities.Baseball,
                ReadingType.Badminton => FitnessActivities.Badminton,
                ReadingType.Archery => FitnessActivities.Archery,
                ReadingType.Calisthenics => FitnessActivities.Calisthenics,
                ReadingType.FrisbeeDisc => FitnessActivities.FrisbeeDisc,
                ReadingType.Soccer => FitnessActivities.FootballSoccer,
                ReadingType.Golf => FitnessActivities.Golf,
                ReadingType.Racquetball => FitnessActivities.Racquetball,
                ReadingType.Polo => FitnessActivities.Polo,
                ReadingType.Pilates => FitnessActivities.Pilates,
                ReadingType.Paragliding => FitnessActivities.Paragliding,
                ReadingType.P90X => FitnessActivities.P90x,
                ReadingType.MixedMartialArts => FitnessActivities.MixedMartialArts,
                ReadingType.Meditation => FitnessActivities.Meditation,
                ReadingType.MartialArts => FitnessActivities.MartialArts,
                ReadingType.Gardening => FitnessActivities.Gardening,
                ReadingType.KickScooter => FitnessActivities.KickScooter,
                ReadingType.KettlebellTraining => FitnessActivities.KettlebellTraining,
                ReadingType.Gymnastics => FitnessActivities.Gymnastics,
                ReadingType.Handball => FitnessActivities.Handball,
                ReadingType.HighIntensityIntervalTraining => FitnessActivities.HighIntensityIntervalTraining,
                ReadingType.Kickboxing => FitnessActivities.Kickboxing,
                ReadingType.Hockey => FitnessActivities.Hockey,
                ReadingType.HorsebackRiding => FitnessActivities.HorsebackRiding,
                ReadingType.Hiking => FitnessActivities.Hiking,
                ReadingType.IceSkating => FitnessActivities.IceSkating,
                ReadingType.IntervalTraining => FitnessActivities.IntervalTraining,
                ReadingType.JumpRope => FitnessActivities.JumpRope,
                ReadingType.Kayaking => FitnessActivities.Kayaking,
                ReadingType.Housework => FitnessActivities.Housework,
                ReadingType.HandCycling => FitnessActivities.BikingHand,
                _ => FitnessActivities.Other,
            };
        }

        private void MapFoodData(HealthReadingDTO readings, DataPoint dataPoint, DateTime foodDateTime, MealType mealType, Field nutrientField,
            string foodName, bool isNutritionType, bool shouldInclude, ReadingType[] readingTypes)
        {
            NutritionReadingModel foodData = isNutritionType
                ? null
                : readings.NutritionReadings.Find(x => Math.Abs((x.ReadingDateTime - foodDateTime).TotalMinutes) < 10 && x.FoodType == mealType);
            if (foodData == null)
            {
                foodData = new NutritionReadingModel
                {
                    ReadingDateTime = foodDateTime,
                    FoodType = mealType,
                    FoodName = foodName
                };
                readings.NutritionReadings.Add(foodData);
            }
            Value data = dataPoint.GetValue(nutrientField);
            foodData.CalorieIntake += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientCalories), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL);
            foodData.Protein += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientProtein), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL) * 1000;
            foodData.Carbs += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientTotalCarbs), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL) * 1000;
            foodData.Cholesterol += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientCholesterol), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL);
            foodData.Fat += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientTotalFat), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL) * 1000;
            foodData.Fiber += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientDietaryFiber), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL) * 1000;
            foodData.Sodium += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientSodium), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL);
            foodData.Sugar += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientSugar), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL) * 1000;
            foodData.VitaminA += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientVitaminA), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL);
            foodData.VitaminC += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientVitaminC), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL);
            foodData.Potassium += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientPotassium), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL);
            foodData.Calcium += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientCalcium), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL);
            foodData.Iron += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientIron), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL);
            foodData.SaturatedFat += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientSaturatedFat), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL) * 1000;
            foodData.UnsaturatedFat += Math.Round(Convert.ToDouble(data.GetKeyValue(Field.NutrientUnsaturatedFat), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)), Constants.DIGITS_AFTER_DECIMAL) * 1000;
            if (GenericMethods.GetIfAllValuesAreZero(foodData.CalorieIntake, foodData.Protein, foodData.Carbs, foodData.Cholesterol, foodData.Fat, foodData.Fiber, foodData.Sodium, foodData.Sugar, foodData.VitaminA, foodData.VitaminC, foodData.Potassium, foodData.Calcium, foodData.Iron, foodData.SaturatedFat, foodData.UnsaturatedFat))
            {
                readings.NutritionReadings.Remove(foodData);
            }
            else
            {
                if (isNutritionType)
                {
                    AddNutritionHealthReading(readings, foodDateTime, foodData.CalorieIntake, ReadingType.NutritionCalories, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Protein, ReadingType.NutritionProtein, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Carbs, ReadingType.NutritionTotalCarbs, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Cholesterol, ReadingType.NutritionCholesterol, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Fat, ReadingType.NutritionTotalFat, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Fiber, ReadingType.NutritionDietaryFiber, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Sodium, ReadingType.NutritionSodium, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Iron, ReadingType.NutritionIron, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.VitaminA, ReadingType.NutritionVitaminA, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.VitaminC, ReadingType.NutritionVitaminC, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Calcium, ReadingType.NutritionCalcium, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Sugar, ReadingType.NutritionSugar, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.Potassium, ReadingType.NutritionPotassium, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.SaturatedFat, ReadingType.NutritionSaturatedFat, shouldInclude, readingTypes);
                    AddNutritionHealthReading(readings, foodDateTime, foodData.UnsaturatedFat, ReadingType.NutritionUnsaturatedFat, shouldInclude, readingTypes);
                }
            }
        }

        private void AddNutritionHealthReading(HealthReadingDTO readings, DateTime foodDateTime, double nutritionValue,
            ReadingType nutrientType, bool shouldInclude, ReadingType[] readingTypes)
        {
            if ((shouldInclude && readingTypes.Contains(nutrientType)) || (!shouldInclude && !readingTypes.Contains(nutrientType)))
            {
                readings.HealthReadings.Add(new HealthReadingModel
                {
                    ReadingValue = nutritionValue,
                    CreatedOn = foodDateTime,
                    ReadingType = nutrientType,
                });
            }
        }

        private double GetReadingFromDataPoint(ReadingType readingType, DataPoint dataPoint, Field field)
        {
            System.Diagnostics.Debug.WriteLine($"GetReadingFromDataPoint=={readingType}");
            switch (readingType)
            {
                case ReadingType.Steps:
                    return dataPoint.GetValue(field).AsInt();
                case ReadingType.Height:
                    return Math.Round(dataPoint.GetValue(field).AsFloat(), 2);
                case ReadingType.Hydration:
                    return Math.Round(dataPoint.GetValue(field).AsFloat(), 3);
                case ReadingType.CaloriesConsumed:
                case ReadingType.CaloriesExpended:
                case ReadingType.Weight:
                case ReadingType.HeartRate:
                // case ReadingType.BloodGlucose:
                case ReadingType.BloodPressure:
                case ReadingType.BodyFat:
                case ReadingType.DistanceCycling:
                case ReadingType.DistanceWalkingRunning:
                case ReadingType.SpO2:
                    return Math.Round(dataPoint.GetValue(field).AsFloat(), 1);
                case ReadingType.BloodGlucose:
                    float.TryParse(dataPoint.GetValue(field).ToString(), out float glucoseData);
                    return Math.Round(glucoseData, 1);
                case ReadingType.BodyTemperature:
                    float.TryParse(dataPoint.GetValue(field).ToString(), out float BodyTemperatureData);
                    return Math.Round(BodyTemperatureData, 1);
                default:
                    return 0;
            }
        }

        private bool ShouldExecuteAggregateMethod(ReadingType readingType, AggregateType aggregateType, AggregateTimeFrame aggregateTimeFrame)
        {
            if (aggregateType == AggregateType.None)
            {
                return false;
            }
            else
            {
                switch (readingType)
                {
                    case ReadingType.Steps:
                    case ReadingType.CaloriesConsumed:
                    case ReadingType.CaloriesExpended:
                    case ReadingType.DistanceWalkingRunning:
                    case ReadingType.DistanceCycling:
                    case ReadingType.Hydration:
                    case ReadingType.Workout:
                        if (aggregateType == AggregateType.Sum && aggregateTimeFrame != AggregateTimeFrame.All)
                        {
                            return true;
                        }
                        return false;
                    case ReadingType.Weight:
                    case ReadingType.Height:
                    case ReadingType.BodyTemperature:
                        if (aggregateType == AggregateType.Minimum || aggregateType == AggregateType.Maximum)
                        {
                            return true;
                        }
                        return false;
                    case ReadingType.HeartRate:
                    case ReadingType.BloodGlucose:
                    case ReadingType.BloodPressure:
                        if (aggregateType == AggregateType.Average || aggregateType == AggregateType.Minimum || aggregateType == AggregateType.Maximum)
                        {
                            return true;
                        }
                        return false;
                    case ReadingType.Nutrition:
                        return true;
                    default:
                        return false;
                }
            }

        }

        private DataReadRequest.Builder ConstructReadRequest(ReadingType readingType, bool isAggregate)
        {
            return isAggregate
                ? new DataReadRequest.Builder().Aggregate(GetDataTypeForReading(readingType), GetAggregateDataTypeForReading(readingType)).EnableServerQueries()
                : new DataReadRequest.Builder().Read(GetDataTypeForReading(readingType)).EnableServerQueries();
        }

        private DataType GetDataTypeForReading(ReadingType readingType)
        {
            switch (readingType)
            {
                case ReadingType.BodyFat:
                    return DataType.TypeBodyFatPercentage;
                case ReadingType.Steps:
                    return DataType.TypeStepCountDelta;
                //todo:
                //case ReadingType.CaloriesConsumed:
                //    return DataType.TypeCaloriesConsumed;
                case ReadingType.CaloriesExpended:
                    return DataType.TypeCaloriesExpended;
                case ReadingType.Weight:
                    return DataType.TypeWeight;
                case ReadingType.Height:
                    return DataType.TypeHeight;
                case ReadingType.HeartRate:
                    return DataType.TypeHeartRateBpm;
                case ReadingType.BloodGlucose:
                    return HealthDataTypes.TypeBloodGlucose;
                case ReadingType.BloodPressure:
                    return HealthDataTypes.TypeBloodPressure;
                case ReadingType.Nutrition:
                case ReadingType.Food:
                    return DataType.TypeNutrition;
                case ReadingType.DistanceCycling:
                    return DataType.TypeCyclingPedalingCadence;
                case ReadingType.DistanceWalkingRunning:
                    return DataType.TypeDistanceDelta;
                case ReadingType.Hydration:
                    return DataType.TypeHydration;
                case ReadingType.BodyTemperature:
                    return HealthDataTypes.TypeBodyTemperature;
                case ReadingType.Sleep:
                case ReadingType.Workout:
                    return DataType.TypeActivitySegment;
                case ReadingType.SpO2:
                    return HealthDataTypes.TypeOxygenSaturation;
                default:
                    return null;
            }
        }

        private DataType GetAggregateDataTypeForReading(ReadingType readingType)
        {
            switch (readingType)
            {
                case ReadingType.BodyFat:
                    return DataType.AggregateBodyFatPercentageSummary;
                case ReadingType.Steps:
                    return DataType.AggregateStepCountDelta;
                //todo:
                //case ReadingType.CaloriesConsumed:
                //    return DataType.AggregateCaloriesConsumed;
                case ReadingType.CaloriesExpended:
                    return DataType.AggregateCaloriesExpended;
                case ReadingType.Weight:
                    return DataType.AggregateWeightSummary;
                case ReadingType.Height:
                    return DataType.AggregateHeightSummary;
                case ReadingType.HeartRate:
                    return DataType.AggregateHeartRateSummary;
                case ReadingType.BloodGlucose:
                    return HealthDataTypes.AggregateBloodGlucoseSummary;
                case ReadingType.BloodPressure:
                    return HealthDataTypes.AggregateBloodPressureSummary;
                case ReadingType.Nutrition:
                case ReadingType.Food:
                    return DataType.AggregateNutritionSummary;
                case ReadingType.DistanceWalkingRunning:
                    return DataType.AggregateDistanceDelta;
                case ReadingType.Hydration:
                    return DataType.AggregateHydration;
                case ReadingType.BodyTemperature:
                    return HealthDataTypes.AggregateBodyTemperatureSummary;
                case ReadingType.Sleep:
                    return DataType.AggregateActivitySummary;
                case ReadingType.SpO2:
                    return HealthDataTypes.AggregateOxygenSaturationSummary;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts unix miliseconds time to .net DateTime
        /// </summary>
        /// <param name="unixTime">Miliseconds of the date received from android system</param>
        /// <returns>.net DateTime object</returns>
        private DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }

        private Tuple<int, ReadingType> ExtractDataOnAggregation(bool isBloodPressure, AggregateType aggregateType, Field field)
        {
            if (isBloodPressure)
            {
                switch (aggregateType)
                {
                    case AggregateType.Average:
                        return ExtractAvgBloodPressureData(field);
                    case AggregateType.Maximum:
                        return ExtractMaxBloodPressureData(field);
                    case AggregateType.Minimum:
                        return ExtractMinBloodPressureData(field);
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        private Tuple<int, ReadingType> ExtractMinBloodPressureData(Field field)
        {
            if (field.Name == HealthFields.FieldBloodPressureSystolicMin.Name || field.Name == HealthFields.FieldBloodPressureDiastolicMin.Name)
            {
                return Tuple.Create(1, field.Name == HealthFields.FieldBloodPressureSystolicMin.Name ? ReadingType.BPSystolic : ReadingType.BPDiastolic);
            }
            return null;
        }

        private Tuple<int, ReadingType> ExtractMaxBloodPressureData(Field field)
        {
            if (field.Name == HealthFields.FieldBloodPressureSystolicMax.Name || field.Name == HealthFields.FieldBloodPressureDiastolicMax.Name)
            {
                return Tuple.Create(1, field.Name == HealthFields.FieldBloodPressureSystolicMax.Name ? ReadingType.BPSystolic : ReadingType.BPDiastolic);
            }
            return null;
        }

        private Tuple<int, ReadingType> ExtractAvgBloodPressureData(Field field)
        {
            if (field.Name == HealthFields.FieldBloodPressureSystolicAverage.Name || field.Name == HealthFields.FieldBloodPressureDiastolicAverage.Name)
            {
                return Tuple.Create(1, field.Name == HealthFields.FieldBloodPressureSystolicAverage.Name ? ReadingType.BPSystolic : ReadingType.BPDiastolic);
            }
            return null;
        }

        private bool IsCorrectAggregationField(AggregateType aggregateType, Field field, ReadingType readingType)
        {
            switch (readingType)
            {
                case ReadingType.Steps:
                case ReadingType.CaloriesConsumed:
                case ReadingType.CaloriesExpended:
                case ReadingType.DistanceWalkingRunning:
                case ReadingType.DistanceCycling:
                case ReadingType.Hydration:
                    return true;
                default:
                    return CheckIfAggregationIsCorrect(aggregateType, field);
            }
        }

        private bool CheckIfAggregationIsCorrect(AggregateType aggregateType, Field field)
        {
            if (aggregateType == AggregateType.Average)
            {
                if (field.Name == Field.FieldAverage.Name)
                {
                    return true;
                }
                return false;
            }
            else if (aggregateType == AggregateType.Minimum)
            {
                if (field.Name == Field.FieldMin.Name)
                {
                    return true;
                }
                return false;
            }
            else if (aggregateType == AggregateType.Maximum)
            {
                if (field.Name == Field.FieldMax.Name)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        private bool CheckIfSpO2AggregationIsCorrect(AggregateType aggregateType, Field field)
        {
            if (aggregateType == AggregateType.Average)
            {
                if (field.Name == HealthFields.FieldOxygenSaturationAverage.Name)
                {
                    return true;
                }
                return false;
            }
            else if (aggregateType == AggregateType.Minimum)
            {
                if (field.Name == HealthFields.FieldOxygenSaturationMin.Name)
                {
                    return true;
                }
                return false;
            }
            else if (aggregateType == AggregateType.Maximum)
            {
                if (field.Name == HealthFields.FieldOxygenSaturationMax.Name)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        private MealType GetFoodType(int foodType)
        {
            return foodType switch
            {
                0 => MealType.Unknown,
                1 => MealType.Breakfast,
                2 => MealType.Lunch,
                3 => MealType.Dinner,
                4 => MealType.Snacks,
                _ => MealType.Unknown,
            };
        }

        private DataSource.Builder GetDataSourceBuilder()
        {
            return new DataSource.Builder()
                .SetAppPackageName(Platform.CurrentActivity.PackageName)
                .SetType(DataSource.TypeRaw);

            //return new DataSource.Builder().SetAppPackageName(Platform.CurrentActivity.PackageName).SetName("From IntegrationLibrary").SetType(DataSource.TypeRaw);
        }

        private ReadingMoment GetSleepMoment(string sleepStage)
        {
            switch (sleepStage)
            {
                case FitnessActivities.SleepDeep:
                case FitnessActivities.SleepRem:
                    return ReadingMoment.SleepDeep;
                case FitnessActivities.SleepLight:
                case FitnessActivities.SleepAwake:
                    return ReadingMoment.SleepLight;
                case FitnessActivities.Sleep:
                default:
                    return ReadingMoment.Sleep;
            }
        }

        private Tuple<int, int, int> GetMealRelation(ReadingMoment moment)
        {
            return moment switch
            {
                ReadingMoment.GlucoseFasting => new Tuple<int, int, int>(HealthFields.FieldTemporalRelationToMealFasting, Field.MealTypeBreakfast, HealthFields.TemporalRelationToSleepOnWaking),
                ReadingMoment.GlucoseAfterBreakfast => new Tuple<int, int, int>(HealthFields.FieldTemporalRelationToMealAfterMeal, Field.MealTypeBreakfast, HealthFields.TemporalRelationToSleepFullyAwake),
                ReadingMoment.GlucoseBeforeLunch => new Tuple<int, int, int>(HealthFields.FieldTemporalRelationToMealBeforeMeal, Field.MealTypeLunch, HealthFields.TemporalRelationToSleepFullyAwake),
                ReadingMoment.GlucoseAfterLunch => new Tuple<int, int, int>(HealthFields.FieldTemporalRelationToMealAfterMeal, Field.MealTypeLunch, HealthFields.TemporalRelationToSleepFullyAwake),
                ReadingMoment.GlucoseBeforeDinner => new Tuple<int, int, int>(HealthFields.FieldTemporalRelationToMealBeforeMeal, Field.MealTypeDinner, HealthFields.TemporalRelationToSleepFullyAwake),
                ReadingMoment.GlucoseAfterDinner => new Tuple<int, int, int>(HealthFields.FieldTemporalRelationToMealAfterMeal, Field.MealTypeDinner, HealthFields.TemporalRelationToSleepFullyAwake),
                ReadingMoment.GlucoseBeforeBed => new Tuple<int, int, int>(HealthFields.FieldTemporalRelationToMealAfterMeal, Field.MealTypeDinner, HealthFields.TemporalRelationToSleepBeforeSleep),
                _ => new Tuple<int, int, int>(0, 0, 0),
            };
        }

        private int GetMealType(MealType mealType)
        {
            return mealType switch
            {
                MealType.Breakfast => Field.MealTypeBreakfast,
                MealType.Lunch => Field.MealTypeLunch,
                MealType.Snacks => Field.MealTypeSnack,
                MealType.Dinner => Field.MealTypeDinner,
                _ => Field.MealTypeUnknown,
            };
        }
        #endregion
    }
}