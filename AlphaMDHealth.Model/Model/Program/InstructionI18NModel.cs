namespace AlphaMDHealth.Model
{
    public class InstructionI18NModel
    {
        public long InstructionID { get; set; }
        public byte LanguageID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
