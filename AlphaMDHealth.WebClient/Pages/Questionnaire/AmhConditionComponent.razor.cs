using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient
{
    public partial class AmhConditionComponent : AmhBaseControl
    {
        private AmhNumericEntryControl InputRef { get; set; }
        private AmhNumericEntryControl InputRef2 { get; set; }
        private AmhNumericEntryControl InputRef3 { get; set; }
        private AmhDropdownControl _actionDropdown { get; set; }

        #region Props
        /// <summary>
        /// Questionnaire data contains master data and questionID
        /// </summary>
        [Parameter]
        public QuestionnaireDTO QuestionnaireData { get; set; }

        /// <summary>
        /// Question conditions to be mapped
        /// </summary>
        [Parameter]
        public QuestionConditionModel QuestionConditionDataToBeMapped { get; set; }

        /// <summary>
        /// Question score conditions to be mapped
        /// </summary>
        [Parameter]
        public QuestionScoreModel QuestionScoreDataToBeMapped { get; set; }

        /// <summary>
        /// Contains page resources
        /// </summary>
        [Parameter]
        public IEnumerable<ResourceModel> ConditionPageResources { get; set; }

        /// <summary>
        /// It is used for Condition number
        /// </summary>
        [Parameter]
        public string ConditionNumber { get; set; }

        /// <summary>
        /// IsValue Changed
        /// </summary>
        [Parameter]
        public bool IsForScorePage { get; set; }

        /// <summary>
        /// It is used for datasource
        /// </summary>
        [Parameter]
        public List<OptionModel> ConditionDataSource { get; set; }

        /// <summary>
        /// Used for selectedvalue
        /// </summary>
        [Parameter]
        public long SelectedValue { get; set; }

        /// <summary>
        /// Value Change Event Callback to handle custom events
        /// </summary>
        [Parameter]
        public EventCallback<int> DeleteButtonClicked { get; set; }

        #endregion

        /// <summary>
        /// will fired on textbox 1(value1) value change
        /// </summary>
        public void OnValue1Changed(object value)
        {
            if (IsForScorePage)
            {
                QuestionScoreDataToBeMapped.Value1 = Convert.ToDouble(value);
            }
            else
            {
                QuestionConditionDataToBeMapped.Value1 = Convert.ToDouble(value);
            }
        }

        /// <summary>
        /// will fired on textbox 2(value2) value change
        /// </summary>
        public void OnValue2Changed(object value)
        {
            if (IsForScorePage)
            {
                QuestionScoreDataToBeMapped.Value2 = Convert.ToDouble(value);
            }
            else
            {
                QuestionConditionDataToBeMapped.Value2 = Convert.ToDouble(value);
            }
            IsValidBetweenConditions();
        }

        /// <summary>
        /// will fired on textbox 3(value3) value change
        /// </summary>
        public void OnScoreValueChange(object value)
        {
            QuestionScoreDataToBeMapped.ScoreValue = Convert.ToDouble(value);
        }

        /// <summary>
        /// use for validation
        /// </summary>
        /// <returns>validation status true/false</returns>
        public override bool ValidateControl(bool IsButtonClick)
        {
            return IsValidBetweenConditions();
        }

        /// <summary>
        /// will get fired question type change event
        /// </summary>
        /// <param name="optionID">select questionid</param>
        public void OnQuestionTypeChanged(object optionID)
        {
            if (!string.IsNullOrWhiteSpace(optionID as string))
            {
                SelectedValue = Convert.ToInt64(optionID as string);
                QuestionConditionDataToBeMapped.TargetQuestionID = SelectedValue;
            }
            StateHasChanged();
        }

        private async Task OnDeleteButtonClicked(int optionID)
        {
            await DeleteButtonClicked.InvokeAsync(optionID);
        }

        private string GetDeleteKey(string ConditionNumber)
        {
            if (IsForScorePage)
            {
                return "Score_key" + ConditionNumber;
            }
            else
            {
                return QuestionConditionDataToBeMapped.OptionText;
            }
        }

        private double GetEntryValue1()
        {
            if (IsForScorePage)
            {
                return QuestionScoreDataToBeMapped.Value1;
            }
            else
            {
                return QuestionConditionDataToBeMapped.Value1;
            }
        }

        private double GetEntryValue2()
        {
            if (IsForScorePage)
            {
                return QuestionScoreDataToBeMapped.Value2;
            }
            else
            {
                return QuestionConditionDataToBeMapped.Value2;
            }
        }

        private double GetScoreValue()
        {
            if (QuestionScoreDataToBeMapped.ScoreValue == 0)
            {
                return 0;
            }
            else
            {
                return Convert.ToDouble(QuestionScoreDataToBeMapped.ScoreValue);
            }
        }

        public bool IsValidBetweenConditions()
        {
            ErrorMessage = string.Empty;
            if (!InputRef2.ValidateControl(IsButtonClick) || !InputRef.ValidateControl(IsButtonClick) 
                || (!IsForScorePage && !_actionDropdown.ValidateControl(IsButtonClick)) || (IsForScorePage && !InputRef3.ValidateControl(IsButtonClick)))
            {
                ErrorMessage = LibResources.GetResourceValueByKey(ConditionPageResources, ErrorCode.InvalidData.ToString());
            }
            else if (InputRef2.Value < InputRef.Value)
            {
                ErrorMessage = LibResources.GetResourceValueByKey(ConditionPageResources, ResourceConstants.R_VALUE2_GREATER_ERROR_KEY);
            }
            SetValidationResult(IsButtonClick);
            return IsValid;
        }
    }
}