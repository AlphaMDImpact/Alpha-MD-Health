using AlphaMDHealth.Utility;
using System.Xml.Serialization;

namespace AlphaMDHealth.Model
{
    [XmlRoot(ElementName = "Control")]
    public class ControlModel
    {
        /// <summary>
        /// Type of control to display
        /// </summary>
        [XmlAttribute(AttributeName = "ControlType")]
        public string ControlType { get; set; } = string.Empty;

        /// <summary>
        /// Name of the control
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Text of that control
        /// </summary>
        [XmlAttribute(AttributeName = "Text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Placeholder of that control
        /// </summary>
        [XmlAttribute(AttributeName = "Placeholder")]
        public string Placeholder { get; set; } = string.Empty;

        /// <summary>
        /// IsRequired property of the control
        /// </summary>
        [XmlAttribute(AttributeName = "IsRequired")]
        public string IsRequired { get; set; } = string.Empty;

        /// <summary>
        /// If IsRequired true for a control, then the error message for that control
        /// </summary>
        [XmlAttribute(AttributeName = "RequiredErrorMessage")]
        public string RequiredErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Regex string for validation
        /// </summary>
        [XmlAttribute(AttributeName = "ValidationRegxString")]
        public string ValidationRegxString { get; set; } = string.Empty;

        /// <summary>
        /// If Regex string format not satisfied then error message to be displayed in that case
        /// </summary>
        [XmlAttribute(AttributeName = "RegxErrorMessage")]
        public string RegxErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Range value for a control
        /// </summary>
        [XmlAttribute(AttributeName = "ValidationRange")]
        public string ValidationRange { get; set; } = string.Empty;

        /// <summary>
        /// In case Range values assigned, the error message for range
        /// </summary>
        [XmlAttribute(AttributeName = "RangeErrorMessage")]
        public string RangeErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Alignment of the control
        /// </summary>
        [XmlAttribute(AttributeName = "Align")]
        public string Align { get; set; } = string.Empty;

        /// <summary>
        /// Fontsize of the control
        /// </summary>
        [XmlAttribute(AttributeName = "FontSize")]
        public string FontSize { get; set; } = string.Empty;

        /// <summary>
        /// TextColor of the control
        /// </summary>
        [XmlAttribute(AttributeName = "TextColor")]
        public string TextColor { get; set; } = string.Empty;

        /// <summary>
        /// Options to be shown for the control
        /// </summary>
        [XmlAttribute(AttributeName = "ShowOptions")]
        public string ShowOptions { get; set; } = string.Empty;

        /// <summary>
        /// Keyboard type of the control
        /// </summary>
        [XmlAttribute(AttributeName = "Keyboard")]
        public string Keyboard { get; set; } = "Default";

        /// <summary>
        /// CommandType for the control
        /// </summary>
        [XmlAttribute(AttributeName = "CommandType")]
        public string CommandType { get; set; } = DButtonCommandType.Default.ToString();

        /// <summary>
        /// CommandParameter of the control
        /// </summary>
        [XmlAttribute(AttributeName = "CommandParameter")]
        public string CommandParameter { get; set; } = string.Empty;

        /// <summary>
        /// Type of button
        /// </summary>
        [XmlAttribute(AttributeName = "ButtonType")]
        public string ButtonType { get; set; } = DynamicButtonType.Default.ToString();

        /// <summary>
        /// Possible values of the control
        /// </summary>
        [XmlAttribute(AttributeName = "Values")]
        public string Values { get; set; } = string.Empty;

        /// <summary>
        /// URL of the content to be shown in webview control
        /// </summary>
        [XmlAttribute(AttributeName = "URL")]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Index value of selected checkbox or radio control
        /// </summary>
        [XmlAttribute(AttributeName = "SelectedIndex")]
        public string SelectedIndexs { get; set; }

        /// <summary>
        /// Format for Date and time control
        /// </summary>
        [XmlAttribute(AttributeName = "Format")]
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Format for Date and time control
        /// </summary>
        [XmlAttribute(AttributeName = "Format2")]
        public string Format2 { get; set; } = string.Empty;

        /// <summary>
        /// BackgroundColor for the selected values in checkbox and radio control
        /// </summary>
        [XmlAttribute(AttributeName = "SelectedBackgroundColor")]
        public string SelectedBackgroundColor { get; set; } = string.Empty;

        public string QuestionID { get; set; }
        public string QuestionNumber { get; set; }
        public string QuestionLabel { get; set; }
        public string InstructionText { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsReadOnly { get; set; }
        public double NumberDecimals { get; set; }
        public string SlidebarMinValue { get; set; }
        public string SlidebarMaxValue { get; set; }
        public double SlidebarStepSize { get; set; }
        public string SlidebarLeftLabelText { get; set; }
        public string SlidebarRightLabelText { get; set; }
        public string SlidebarQuestionText { get; set; }
        public bool DoShowFinishButton { get; set; }
        public bool DoShowNextButton { get; set; }
        public bool DoShowPreviousButton { get; set; }
        public double ProgressQuestionnaire { get; set; }
        public double TotalNumberQuestions { get; set; }
        public string QuestionValue { get; set; }
        public string TaskName { get; set; }
        public string TaskServerID { get; set; }
        public string QuestionnaireStatus { get; set; }
        public string UnitCode { get; set; }
        public BaseDTO CustomResources { get; set; }
        public string QuestionnaireHelpText { get; set; }
    }
}