using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public static class Mapping
    {
        /// <summary>
        /// Map reading type
        /// </summary>
        /// <param name="title">title comming from API and attr_id in case of Nutritionix API</param>
        /// <returns>apprpriate reading type</returns>
        internal static ReadingType ReadingTypeBinding(string title)
        {
            switch (title.ToLower(CultureInfo.InvariantCulture))
            {
                case "calories":
                case "208":
                    return ReadingType.NutritionCalories;
                case "fat":
                case "204":
                    return ReadingType.NutritionTotalFat;
                case "saturated fat":
                case "606":
                    return ReadingType.NutritionSaturatedFat;
                case "carbohydrates":
                case "carbohydrate":
                case "205":
                    return ReadingType.NutritionTotalCarbs;
                case "sugar":
                case "269":
                    return ReadingType.NutritionSugar;
                case "cholesterol":
                case "601":
                    return ReadingType.NutritionCholesterol;
                case "sodium":
                case "307":
                    return ReadingType.NutritionSodium;
                case "protein":
                case "203":
                    return ReadingType.NutritionProtein;
                case "calcium":
                case "301":
                    return ReadingType.NutritionCalcium;
                case "iron":
                case "303":
                    return ReadingType.NutritionIron;
                case "fiber":
                case "291":
                    return ReadingType.NutritionDietaryFiber;
                case "vitamin_a":
                case "318":
                    return ReadingType.NutritionVitaminA;
                case "Zinc":
                case "309":
                    return ReadingType.NutritionZinc;
                case "vitamin_c":
                case "401":
                    return ReadingType.NutritionVitaminC;
                case "potassium":
                case "306":
                    return ReadingType.NutritionPotassium;
                default:
                    return ReadingType.None;
            }
        }

        /// <summary>
        /// Map reading unit
        /// </summary>
        /// <param name="unit">unit comming from API</param>
        /// <returns>apprpriate reading unit</returns>
        internal static ReadingUnit ReadingUnitBinding(string unit)
        {
            switch (unit.ToLower(CultureInfo.InvariantCulture))
            {
                case "g":
                case "µg":
                case "mg":
                    return ReadingUnit.g;
                case "kgs":
                    return ReadingUnit.Kgs;
                case "kcal":
                    return ReadingUnit.kcal;
                default:
                    return ReadingUnit.None;
            }
        }

        /// <summary>
        /// Calculate reading value as per unit
        /// </summary>
        /// <param name="readingValue">reading value comming from API</param>
        /// <param name="readingUnit">reading unit comming from API</param>
        /// <returns>conversion of reading value as per unit allowed</returns>
        internal static double GetReadingValueAsPerUnit(double readingValue, string readingUnit)
        {
            switch (readingUnit.ToLower(CultureInfo.InvariantCulture))
            {
                case "mg":
                    return readingValue / 1000;
                case "µg":
                    return readingValue / 10000;
                default:
                    return readingValue;
            }
        }


        /// <summary>
        /// Calculate reading value as per unit
        /// </summary>
        /// <param name="readingType">reading value comming from API</param>
        /// <returns>map reading type </returns>
        internal static string MappUnitForReadingType(string readingType)
        {
            switch (readingType.ToLower(CultureInfo.InvariantCulture))
            {
                case "208":
                case "calories":
                    return "kcal";
                case "204":
                case "606":
                case "205":
                case "269":
                case "203":
                case "291":
                case "carbohydrate":
                case "protein":
                case "fat":
                case "saturated_fat":
                case "monounsaturated_fat":
                case "fiber":
                case "sugar":
                    return "g";
                case "318":
                    return "IU";
                case "cholesterol":
                case "sodium":
                case "potassium":
                case "calcium":
                case "iron":
                case "601":
                case "307":
                case "301":
                case "303":
                case "409":
                case "401":
                case "306":
                case "vitamin_c":
                    return "mg";
                case "vitamin_a":
                    return "μg";
                default:
                    return string.Empty;
            }
        }
    }
}