using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class OrganisationTagModel : LanguageModel
    {
        //// <summary>
        /// Id Of OrganisationTag
        /// </summary>
        public long OrganisationTagID { get; set; }

        /// <summary>
        /// OrganisationTag Text
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ORGANISATION_TAG_TEXT_KEY)]
        public string TagText { get; set; }

        /// <summary>
        /// Detail of OrganisationTag Description
        /// </summary> 
        public string TagDescription { get; set; }
    }
}
