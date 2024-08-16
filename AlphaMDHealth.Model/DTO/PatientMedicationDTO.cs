using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class PatientMedicationDTO : BaseDTO
    {
        /// <summary>
        /// Flag representing data is for medical history or not
        /// </summary>
        public bool IsMedicalHistory { get; set; }

        public PatientMedicationModel Medication { get; set; }
        [DataMember]
        public List<MedicineModel> Medicines { get; set; }
        [DataMember]
        public List<PatientMedicationModel> Medications { get; set; }
        [DataMember]
        public List<MedicationReminderModel> Reminders { get; set; }
        [DataMember]
        public List<LocalNotificationModel> Notifications { get; set; }
        [DataMember]
        public List<OptionModel> UnitOptions { get; set; }

        [DataMember]
        public List<OptionModel> FrequencyTypeOptions { get; set; }

        [DataMember]
        public List<OptionModel> FrequencyOptions { get; set; }

        [DataMember]
        public List<OptionModel> AdditionalNotesOptions { get; set; }
        [DataMember]
        public List<SaveResultModel> SaveMedications { get; set; }
        [DataMember]
        public List<OrganisationModel> Organisations { get; set; }
        [DataMember]
        public OrganisationModel Organisation { get; set; }
        [DataMember]
        public List<UserModel> Users { get; set; }
        [DataMember]
        public UserModel User { get; set; }
        [DataMember]
        public CaregiverModel Caregiver { get; set; }
        public bool IsPrescriptionView { get; set; }
        [DataMember]
        public List<OptionModel> UserDegrees { get; set; }
        
    }
}