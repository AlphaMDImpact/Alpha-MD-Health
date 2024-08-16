using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Nutrition reading data
    /// </summary>
    public class NutritionReadingModel
    {
        /// <summary>
        /// Food item name
        /// </summary>
        public string FoodName { get; set; }

        /// <summary>
        /// Food type
        /// </summary>
        public MealType FoodType { get; set; }

        /// <summary>
        /// Calorie intake
        /// </summary>
        public double CalorieIntake { get; set; }

        /// <summary>
        /// Protein
        /// </summary>
        public double Protein { get; set; }

        /// <summary>
        /// Carbs
        /// </summary>
        public double Carbs { get; set; }

        /// <summary>
        /// Cholesterol
        /// </summary>
        public double Cholesterol { get; set; }

        /// <summary>
        /// Fat
        /// </summary>
        public double Fat { get; set; }

        /// <summary>
        /// Fiber
        /// </summary>
        public double Fiber { get; set; }

        /// <summary>
        /// Sodium
        /// </summary>
        public double Sodium { get; set; }

        /// <summary>
        /// Sugar
        /// </summary>
        public double Sugar { get; set; }

        /// <summary>
        /// Vitamin A
        /// </summary>
        public double VitaminA { get; set; }

        /// <summary>
        /// Vitamin C
        /// </summary>
        public double VitaminC { get; set; }

        /// <summary>
        /// Potassium
        /// </summary>
        public double Potassium { get; set; }

        /// <summary>
        /// Calcium
        /// </summary>
        public double Calcium { get; set; }

        /// <summary>
        /// Iron
        /// </summary>
        public double Iron { get; set; }

        /// <summary>
        /// Saturated fat
        /// </summary>
        public double SaturatedFat { get; set; }

        /// <summary>
        /// Unsaturated fat
        /// </summary>
        public double UnsaturatedFat { get; set; }

        /// <summary>
        /// Reading date time
        /// </summary>
        public DateTimeOffset ReadingDateTime { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public ErrorCode ErrCode { get; set; }
    }
}
