namespace AlphaMDHealth.Model
{
    /// <summary>
    ///  Model to store data of patient shared programs
    /// </summary>
    public class PatientsSharedProgramsModel
    {
        /// <summary>
        /// ID of patient program
        /// </summary>
        public long PatientProgramID { get; set; }

        /// <summary>
        /// Relation id of caregiver
        /// </summary>
        public long PatientCareGiverID { get; set; }

        /// <summary>
        /// Flag to store a isActive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Stores last modified on date by user
        /// </summary>
        public DateTimeOffset? LastModifiedON { get; set; }
    }
}