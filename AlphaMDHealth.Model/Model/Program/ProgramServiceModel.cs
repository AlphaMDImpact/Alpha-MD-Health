using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class ProgramServiceModel
    {/// <summary>
     ///  ID Of Program External Service 
     /// </summary>
        [PrimaryKey]
        public long ProgramExternalServiceID { get; set; }

        /// <summary>
        /// External Service ID
        /// </summary>
        public short ExternalServiceID { get; set; }

        /// <summary>
        /// Program Service Name a
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_NO_OF_SCANS_KEY)]
        public int Quantity { get; set; }

        /// <summary>
        /// ID Of Program
        /// </summary>
        public long ProgramID { get; set; }

        /// <summary>
        /// Represents Data added after days
        /// </summary>
        public short AssignAfterDays { get; set; }

        /// <summary>
        /// Represnts Data shown for days
        /// </summary>
        public short AssignForDays { get; set; }

        /// <summary>
        /// Value Added By 
        /// </summary>
        public short ValueAddedBy { get; set; }

        /// <summary>
        /// Flag representing data is synced to server or not
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Program Service added by id
        /// </summary>
        public string AddedByID { get; set; }

        /// <summary>
        /// Program Service added by id
        /// </summary>
        public string LastModifiedByID { get; set; }

        /// <summary>
        /// Program Service added on date time
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// Program Service added on date time
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }

        /// <summary>
        /// Flag representing file is active or deleted
        /// </summary>
        public bool IsActive { get; set; }
    }
}
