using SQLite;
using System.ComponentModel;

namespace AlphaMDHealth.Model
{
    public class ParticipantsModel: INotifyPropertyChanged
    {
        public long ParticipantID { get; set; }
        public long AccountID { get; set; }
        public long AppointmentID { get; set; }
        public byte RoleID { get; set; }
        public string RoleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NameInitials { get; set; }
        public string Profession { get; set; }
        public string ImageName { get; set; }
        public bool IsDataDownloaded { get; set; }
        public string ImageBase64 { get; set; }
        public string AppointmentStatusID { get; set; }
        public string AppointmentStatusName { get; set; }
        public bool IsSelected { get; set; }
        public bool IsActive { get; set; }

        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        [Ignore]
        public string AppointmentStatusColor { get; set; }

        //todo:
        //[Ignore]
        //public ImageSource ImageSource { get; set; }
        public byte[] ImageBytes { get; set; }

        [Ignore]
        public string FullName { get; set; }
        [Ignore]
        public bool ShowRemoveButton { get; set; }
        [Ignore]
        public string ShowRemoveButtonText { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _cellFirstMiddleSatusContentHeader;

        /// <summary>
        /// Flag to Store IsSelected
        /// </summary>
        [Ignore]
        public string CellFirstMiddleSatusContentHeader
        {
            get => _cellFirstMiddleSatusContentHeader;
            set
            {
                _cellFirstMiddleSatusContentHeader = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CellFirstMiddleSatusContentHeader"));
            }
        }

        private string _cellFirstMiddleContentHeaderColorr;

        /// <summary>
        /// Flag to Store IsSelected
        /// </summary>
        [Ignore]
        public string CellFirstMiddleContentHeaderColor
        {
            get => _cellFirstMiddleContentHeaderColorr;
            set
            {
                _cellFirstMiddleContentHeaderColorr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CellFirstMiddleContentHeaderColor"));
            }
        }

        private string _cellSecondMiddleSatusContentHeaderColor;

        /// <summary>
        /// Flag to Store IsSelected
        /// </summary>
        [Ignore]
        public string CellSecondMiddleSatusContentHeaderColor
        {
            get => _cellSecondMiddleSatusContentHeaderColor;
            set
            {
                _cellSecondMiddleSatusContentHeaderColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CellSecondMiddleSatusContentHeaderColor"));
            }
        }

        private string _cellSecondMiddleSatusContentHeader;

        /// <summary>
        /// Flag to Store IsSelected
        /// </summary>
        [Ignore]
        public string CellSecondMiddleSatusContentHeader
        {
            get => _cellSecondMiddleSatusContentHeader;
            set
            {
                _cellSecondMiddleSatusContentHeader = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CellSecondMiddleSatusContentHeader"));
            }
        }
    }
}