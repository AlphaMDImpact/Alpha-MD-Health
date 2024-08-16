using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AlphaMDHealth.Model
{
    public class LibraryServiceDTO : BaseDTO
    {
        public LibraryServiceModel LibraryInfo { get; set; }

        [DataMember]
        public List<LibraryServiceModel> LibraryDetails { get; set; }

        [JsonIgnore]
        public object Request { get; set; }

        public string RequestSignature { get; set; }
    }
}