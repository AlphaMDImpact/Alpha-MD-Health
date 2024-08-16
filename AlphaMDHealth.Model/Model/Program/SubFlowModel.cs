using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class SubFlowModel
    {
        public long SubFlowID { get; set; }
        public long TaskID { get; set; }
        public long ProgramID { get; set; }
        public long ProgramSubFlowID { get; set; }
        public long TaskSubFlowID { get; set; }
        public string Name { get; set; }
        public string OperationType { get; set; }

        public float Value1 { get; set; }

        public float Value2 { get; set; }
        public string TaskType { get; set; }

        [MyCustomAttributes(ResourceConstants.R_ITEM_KEY)]
        public long ItemID { get; set; }
        public string Item { get; set; }
        public short TemplateID { get; set; }
        [MyCustomAttributes(ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY)]
        public short AssignAfterDays { get; set; }
        [MyCustomAttributes(ResourceConstants.R_SHOW_FOR_DAYS_KEY)]
        public short AssignForDays { get; set; }
        public bool IsActive { get; set; }
        public bool IsSynced { get; set; }
    }
}
