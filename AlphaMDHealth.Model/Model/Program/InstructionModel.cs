using SQLite;

namespace AlphaMDHealth.Model
{
    public class InstructionModel
    {
        /// <summary>
        /// Id of Instruction
        /// </summary>
        [PrimaryKey]
        public long InstructionID { get; set; }

        /// <summary>
        /// Name of Instruction
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Flag representing record is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Description of Instruction
        /// </summary>
        [Ignore]
        public string Description { get; set; }
    }
}
