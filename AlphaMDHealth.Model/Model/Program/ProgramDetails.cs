using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class ProgramDetails : LanguageModel
    {
        [MyCustomAttributes($"{ResourceConstants.R_NAME_TXT_KEY},{ResourceConstants.R_SUBFLOW_NAME_KEY},{ResourceConstants.R_PROGRAM_NAME_KEY},{ResourceConstants.R_INSTRUCTION_NAME_KEY}")]
        public string Name { get; set; }

        [MyCustomAttributes($"{ResourceConstants.R_PROGRAM_DESCRIPTION_KEY},{ResourceConstants.R_SUBFLOW_DESCRIPTION_KEY},{ResourceConstants.R_INSTRUCTION_DESCRIPTION_KEY}")]
        public string Description { get; set; }
    }
}