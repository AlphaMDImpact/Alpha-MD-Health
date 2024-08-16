using AlphaMDHealth.Utility;
namespace AlphaMDHealth.Model

{
    /// <summary>
    /// Patient Reading model
    /// </summary>
    public class PatientReadingUIModel : PatientReadingModel
    {
        /// <summary>
        /// Sequence No
        /// </summary>
        public byte SequenceNo { get; set; }

        /// <summary>
        /// Added By Text
        /// </summary>
        public string AddedByText { get; set; }

        /// <summary>
        /// Patient Reading identifier text (BP Systolic, BP Diastolic etc)
        /// </summary>
        /// 
        [MyCustomAttributes(ResourceConstants.R_READING_TYPE_KEY)]
        public string Reading { get; set; }

        /// <summary>
        /// Reading UnitIdentifier
        /// </summary>
        public string UnitIdentifier { get; set; }

        /// <summary>
        /// Patient Reading unit
        /// </summary>
        /// 
        [MyCustomAttributes(ResourceConstants.R_MEASUREMENT_TYPE_TEXT_KEY)]
        public string Unit { get; set; }

        /// <summary>
        /// Patient Reading display value text
        /// </summary>
        public string ReadingValueText { get; set; }

        /// <summary>
        /// Patient Reading display date time text
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_DATE_TIME_TEXT_KEY)]

        public string ReadingDateTimeText { get; set; }

        /// <summary>
        /// ReadingUnitValueText
        /// </summary>
        public string UnitText { get; set; }

        /// <summary>
        /// Patient Reading source icon name
        /// </summary>
        public string ReadingSourceIcon { get; set; }

        /// <summary>
        /// Patient Reading moment icon name
        /// </summary>
        public string ReadingMomentIcon { get; set; }

        /// <summary>
        /// Patient Reading notes icon name
        /// </summary>
        public string ReadingNotesIcon { get; set; }
        public string ImageSource { get;set; }

        ///// <summary>
        ///// Local date time of Patient Reading
        ///// </summary>
        //public DateTime ReadingLocalDateTime
        //{
        //    get
        //    {
        //        return ReadingDateTime.LocalDateTime;
        //    }
        //}

        /// <summary>
        /// Is Patient Reading should show in 1 row or 2 row
        /// </summary>
        public bool IsTwoRowList { get; set; }
    }
}