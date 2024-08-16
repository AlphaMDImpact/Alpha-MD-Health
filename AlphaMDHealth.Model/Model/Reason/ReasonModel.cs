using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Reason Model
    /// </summary>
    public class ReasonModel : LanguageModel
    {
        /// <summary>
        /// Id Of Reason
        /// </summary>
        public byte ReasonID { get; set; }

        /// <summary>
        /// ID of ProgramReason 
        /// </summary>
        public long ProgramReasonID { get; set; }

        /// <summary>
        /// ID of Program
        /// </summary>
        public long ProgramID { get; set; }

        /// <summary>
        /// Reason Information
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_REASON_NAME_TEXT_KEY)]
        public string Reason { get; set; }

        /// <summary>
        /// Detail Of Reason Description
        /// </summary>
        public string ReasonDescription { get; set; }
    }
}
