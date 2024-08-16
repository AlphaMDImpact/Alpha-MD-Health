using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Food item data
    /// </summary>
    public class FoodItemDTO : BaseDTO
    {
        /// <summary>
        /// List of food items
        /// </summary>
        [DataMember]
        public List<FoodItemModel> FoodItems { get; set; }
    }
}
