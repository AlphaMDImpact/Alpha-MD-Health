namespace AlphaMDHealth.Model
{
    /// <summary>
    /// OptionSelectModel used for check box and radio
    /// </summary>
    public class OptionSelectModel
    {
        /// <summary>
        /// used for image questionnaire
        /// </summary>
        public byte[] ImageValue { get; set; }
        /// <summary>
        /// value 
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        ///id
        /// </summary>
        public long OptionID { get; set; }
        /// <summary>
        /// IsSelected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// IsSelected
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// DisplayText
        /// </summary>
        public string DisplayText { get; set; }
    }
}
