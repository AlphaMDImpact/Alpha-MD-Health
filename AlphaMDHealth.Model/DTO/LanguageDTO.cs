using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// DTO used to store list of languages
    /// </summary>
    public class LanguageDTO : BaseDTO
    {
        /// <summary>
        /// List of languages
        /// </summary>
        [DataMember]
        public List<LanguageModel> Languages { get; set; }

        /// <summary>
        /// Language data
        /// </summary>
        public LanguageModel Language { get; set; }
    }
}