using SQLite;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class TaskModel : BaseListItemModel
    {
        /// <summary>
        /// ID of PatientTask
        /// </summary>
        [PrimaryKey]
        public long PatientTaskID { get; set; }

        /// <summary>
        /// Name of the Task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name of Selected Item
        /// </summary>
        public string SelectedItemName { get; set; }

        /// <summary>
        /// Type of Task
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_TASK_TYPE_KEY)]
        public string TaskType { get; set; }



        public string FromDateValue { get; set; }

        public string ToDateValue { get; set; }

        /// <summary>
        /// ID of Item
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ITEM_KEY)]
        public long ItemID { get; set; }

        /// <summary>
        /// ID of User
        /// </summary>
        public long UserID { get; set; }

       

        /// <summary>
        /// Represents weather Task needs to be executed on login 
        /// </summary>
        public bool ExecuteOnLogin { get; set; }

        /// <summary>
        /// Flag represents weather record is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Date from which Task is assigned
        /// </summary>
        //[MyCustomAttributes(ResourceConstants.R_START_DATE_KEY)]
        public DateTimeOffset? FromDate { get; set; }

        /// <summary>
        /// Date until which Task is assigned
        /// </summary>
        //[MyCustomAttributes(ResourceConstants.R_END_DATE_KEY)]
        public DateTimeOffset? ToDate { get; set; }

        /// <summary>
        /// Completion Date of the Task
        /// </summary>
        public DateTimeOffset CompletionDate { get; set; }

        /// <summary>
        /// Represents the Status of Task
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Result Value of Task
        /// </summary>
        public string ResultValue { get; set; }

        /// <summary>
        /// Information based on Task score
        /// </summary>
        public string Recommendation { get; set; }

        /// <summary>
        /// ID of Program
        /// </summary>
        public long ProgramID { get; set; }

        /// <summary>
        /// decides task for Patient/Provider
        /// </summary>
        public string TaskRespondent { get; set; }

        /// <summary>
        /// Color of Program
        /// </summary>
        public string ProgramColor { get; set; }

        /// <summary>
        /// Flag represents weather data is synced from server or not
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Task AddedOn DateTime 
        /// </summary>
        public DateTimeOffset AddedOn { get; set; }

        /// <summary>
        /// Task modified on date time
        /// </summary>
        public DateTimeOffset LastModifiedON { get; set; }

        /// <summary>
        /// Value of Status
        /// </summary>
        [Ignore]
        public string StatusValue { get; set; }

        /// <summary>
        /// ID of Task
        /// </summary>
        [Ignore]
        public long TaskID { get; set; }

        /// <summary>
        /// ID of ProgramTask
        /// </summary>
        [Ignore]
        public long ProgramTaskID { get; set; }

        /// <summary>
        /// String Format of FromDate
        /// </summary>
        [Ignore]
        public string FromDateString { get; set; }

        /// <summary>
        /// String Format of ToDate
        /// </summary>
        [Ignore]
        public string ToDateString { get; set; }

        /// <summary>
        /// String Format of Completion Date
        /// </summary>
        [Ignore]
        public string CompletionDateString { get; set; }

        /// <summary>
        /// Name of Image
        /// </summary>
        [Ignore]
        public string ImageName { get; set; }

        /// <summary>
        /// Number of Subflows
        /// </summary>
        [Ignore]
        public int NoOfSubflows { get; set; }

        /// <summary>
        /// No of days After which Task is Assigned
        /// </summary>
        [Ignore]
        public short AssignAfterDays { get; set; }

        /// <summary>
        /// No of Days For which Task is assigned
        /// </summary>
        [Ignore]
        public short AssignForDays { get; set; }

        /// <summary>
        /// Color of Status
        /// </summary>
        [Ignore]
        public string StatusColor { get; set; }

        /// <summary>
        /// user who did action
        /// </summary>
        public string LastActionBy { get; set; }

        /// <summary>
        /// Stores Created By User ID 
        /// </summary>
        public long LastModifiedByID { get; set; }

       

        [Ignore]
        public string DateStyle { get; set; }
    }
}
