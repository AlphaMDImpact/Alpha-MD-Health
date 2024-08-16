using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class PatientBillsPage : BasePage
{
    private readonly BillingItemDTO _billData = new BillingItemDTO() { PatientBillItem = new PatientBillModel() };
    private Guid _patientBillID;

    /// <summary>
    /// PatientBill ID parameter
    /// </summary>
    [Parameter]
    public Guid PatientBillID
    {
        get { return _patientBillID; }
        set
        {
            if (_patientBillID != value)
            {
                if (_billData.RecordCount > 0 || _patientBillID == Guid.Empty)
                {
                    _billData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _patientBillID = value;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (Parameters?.Count > 0)
        {
            _billData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ContentPageDTO.RecordCount)));
        }
        _billData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await GetDataAsync().ConfigureAwait(false);
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new PatientBillService(AppState.webEssentials).GetPatientBillingsDataAsync(_billData), _billData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var tableStructure = new List<TableDataStructureModel>()
        {
            new TableDataStructureModel{DataField=nameof(PatientBillModel.PatientBillID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{BorderColorDataField=nameof(PatientBillModel.ProgramColor)},
            new TableDataStructureModel{DataField=nameof(PatientBillModel.BillDateTimeString),DataHeader=ResourceConstants.R_BILL_DATE_KEY},
            new TableDataStructureModel { DataField = nameof(PatientBillModel.Amount), DataHeader = ResourceConstants.R_AMOUNT_PAID_KEY },
        };
        if(_billData.RecordCount < 1)
        {
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientBillModel.PaymentMode), DataHeader = ResourceConstants.R_PAYMENT_MODE_NAME_KEY });
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientBillModel.ProgramName), DataHeader = ResourceConstants.R_PROGRAM_NAME_KEY });
        }
        return tableStructure;
    }

    private async Task OnAddEditClick(PatientBillModel patientBill)
    {
        Success = Error = string.Empty;
        if (_billData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.PatientBillsView.ToString(), (patientBill == null ? Guid.Empty : patientBill.PatientBillID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _patientBillID = patientBill?.PatientBillID ?? Guid.Empty;
            ShowDetailPage = true;
        }       
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.PatientBillsView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        Success = Error = string.Empty;
        await OnViewAllClickedAsync();
        if (errorMessage == ErrorCode.OK.ToString() || string.IsNullOrWhiteSpace(errorMessage)) 
        {
            _isDataFetched = false;
            Success = errorMessage;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = errorMessage;
        }
    }
}