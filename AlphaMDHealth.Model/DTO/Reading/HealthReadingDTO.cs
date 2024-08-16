using AlphaMDHealth.Utility;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Health reading data
    /// </summary>
    public class HealthReadingDTO : BaseDTO
    {
        /// <summary>
        /// List of readings
        /// </summary>
        [DataMember]
        public List<HealthReadingModel> HealthReadings { get; set; }

        /// <summary>
        /// List of Nutrition Data Received from HealthKit
        /// </summary>
        [DataMember]
        public List<NutritionReadingModel> NutritionReadings { get; set; }

        /// <summary>
        /// Errors for each reading type sent in request
        /// </summary>
        [DataMember]
        public Dictionary<ReadingType, ErrorCode> Errors { get; set; }
    }
}
