using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class AppointmentDTO : BaseDTO
    {
        public AppointmentModel Appointment { get; set; }
        public ParticipantsModel ExternalParticipant { get; set; }
        [DataMember]
        public List<AppointmentModel> Appointments { get; set; }
        [DataMember]
        public List<ContentDetailModel> AppointmentDetails { get; set; }
        [DataMember]
        public List<AppointmentDetailModel> AppointmentsDetails { get; set; }
        [DataMember]
        public List<OptionModel> AppointmentOptions { get; set; }

        [DataMember]
        public List<OptionModel> AppointmentTypes { get; set; }
        [DataMember]
        public List<LanguageModel> LanguageTabs { get; set; }
        [DataMember]
        public List<OptionModel> AppointmentParticipantsDropdown { get; set; }
        [DataMember]
        public List<ParticipantsModel> AppointmentParticipants { get; set; }
        public IEnumerable<IGrouping<DateTimeOffset, AppointmentModel>> SheduledAppoinments { get; set; }
        [DataMember]
        public List<LocalNotificationModel> Notifications { get; set; }
        public bool IsExternalParticipant { get; set; }

        [DataMember]
        public List<ParticipantsModel> ExternalParticipants { get; set; }
        [DataMember]
        public List<ParticipantsModel> InviteParticipants { get; set; } = new List<ParticipantsModel>();
    }
}
