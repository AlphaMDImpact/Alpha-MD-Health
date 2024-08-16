namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used to store unit data
    /// </summary>
    public class UnitModel
    {
        /// <summary>
        /// ID if Unit
        /// </summary>
        public byte UnitID { get; set; }

        /// <summary>
        /// Identifier of Unit
        /// </summary>
        public string UnitIdentifier { get; set; }
        
        /// <summary>
        /// Group ID of Unit
        /// </summary>
        public int UnitGroupID { get; set; }
        
        /// <summary>
        /// Is this Base Unit
        /// </summary>
        public bool IsBaseUnit { get; set; }
        
        /// <summary>
        /// Conversion factor to convert into unser unit or base unit 
        /// </summary>
        public float BaseConversionFactor { get; set; }

        /// <summary>
        /// Flag represent it is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Language specific short name of Unit to display in UI
        /// </summary>
        public string ShortUnitName { get; set; }

        /// <summary>
        /// Language specific long name of Unit to display in UI
        /// </summary>
        public string LongUnitName { get; set; }


    }
}