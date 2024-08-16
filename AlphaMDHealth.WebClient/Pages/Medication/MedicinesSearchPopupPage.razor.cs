using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class MedicinesSearchPopupPage : BasePage
{
    /// <summary>
    /// Medication Data
    /// </summary>
    [Parameter]
    public PatientMedicationDTO MedicationData { get; set; }

    /// <summary>
    /// Callback Event to execute code after Edit page is closed
    /// </summary>
    [Parameter]
    public EventCallback<PatientMedicationDTO> PopUpClosed { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isDataFetched = true;
        await Task.Run(() => { }).ConfigureAwait(true);
    }

    private async Task OnOptionSelectedAsync(object model)
    {
        var medicitionModel = model as MedicineModel;
        Success = string.Empty;
        Error = string.Empty;
        MedicationData.Medication.ShortName = medicitionModel.ShortName;
        MedicationData.Medication.FullName = medicitionModel.FullName;
        MedicationData.Medication.UnitIdentifier = medicitionModel.UnitIdentifier;
        await PopUpClosed.InvokeAsync(MedicationData);
    }

    private void OnCloseIconClicked(bool isPopupShowing)
    {
        if (!isPopupShowing)
        {
            OnCancelClick();
        }
    }

    private void OnCancelClick()
    {
        PopUpClosed.InvokeAsync(null);
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
                {
                new TableDataStructureModel{DataField=nameof(MedicineModel.ShortName),DataHeader= ResourceConstants.R_MEDICINE_NAME_KEY},
            };
    }
    private AmhViewCellModel getViewCellModel()
    {
        return new AmhViewCellModel
        {
            ID = nameof(MedicineModel.FullName),
            LeftHeader = nameof(MedicineModel.ShortName),
            LeftHeaderFieldType = FieldTypes.PrimarySmallHStartVCenterBoldLabelControl,
        };
    }
}
