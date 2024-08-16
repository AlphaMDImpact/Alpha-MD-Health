using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class BillsReportPage : BasePage
{
    private BillingItemDTO _billingData = new BillingItemDTO { FromDate = DateTime.Now.ToString(CultureInfo.InvariantCulture), ToDate = DateTime.Now.ToString(CultureInfo.InvariantCulture) };
    private DateTimeOffset? _fromDate;
    private DateTimeOffset? _toDate;
    private List<CardModel> _billingCards;
    private long _patientID;
    private Guid _patientBillID;
    private bool _isDashboardView;

    [Parameter]
    public long PatientId
    {
        get { return _patientID; }
        set
        {
            if (_patientID != value)
            {
                _patientID = value;
                SetDetailPage();
            }
        }
    }

    [Parameter]
    public Guid PatientBillID
    {
        get { return _patientBillID; }
        set
        {
            if (_patientBillID != value)
            {
                _patientBillID = value;
                SetDetailPage();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _billingData.RecordCount = -1;
        _billingData.PatientBillItem = new PatientBillModel { PatientBillID = Guid.Empty };
		if (_fromDate != null)
		{
			_billingData.FromDate = _fromDate.Value.ToOffset(TimeSpan.FromMinutes(AppState.LocalOffset)).ToString();
		}
		if (_toDate != null)
		{
			_billingData.ToDate = _toDate.Value.ToOffset(TimeSpan.FromMinutes(AppState.LocalOffset)).ToString();
		}
		if (_fromDate > _toDate)
		{
			Error = string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(_billingData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY)
                , LibResources.GetResourceValueByKey(_billingData.Resources, ResourceConstants.R_START_DATE_KEY)
                , LibResources.GetResourceValueByKey(_billingData.Resources, ResourceConstants.R_END_DATE_KEY));
			return;
		}
		await SendServiceRequestAsync(new ReportsService(AppState.webEssentials).SyncBillsFromServerAsync(_billingData, CancellationToken.None), _billingData).ConfigureAwait(true);
		if (_billingData.ErrCode == ErrorCode.OK)
		{
			_fromDate = DateTimeOffset.Parse(_billingData.FromDate, CultureInfo.InvariantCulture);
			_toDate = DateTimeOffset.Parse(_billingData.ToDate, CultureInfo.InvariantCulture);
			_billingCards = new List<CardModel>();
            GetPaymentDetail();
            _isDashboardView = _billingData.RecordCount > 0;
            _isDataFetched = true;
        }
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var tableStructure = new List<TableDataStructureModel>()
        {
            new TableDataStructureModel{DataField=nameof(PatientBillModel.PatientBillID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{BorderColorDataField=nameof(PatientBillModel.ProgramColor)},
            new TableDataStructureModel{DataField=nameof(PatientBillModel.PatientName),DataHeader=ResourceConstants.R_PATIENT_KEY, IsSearchable = true},
            new TableDataStructureModel{DataField=nameof(PatientBillModel.BillDateTimeString),DataHeader=ResourceConstants.R_BILL_DATE_KEY, IsSearchable = true},
        };
        if (_isDashboardView)
        {
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientBillModel.TotalPaid), DataHeader = ResourceConstants.R_TOTAL_AMOUNT_KEY, IsSearchable = true });
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientBillModel.PaymentMode), DataHeader = ResourceConstants.R_PAYMENT_MODE_NAME_KEY, IsSearchable = true });
        }
        else
        {
            tableStructure.AddRange(new List<TableDataStructureModel>
            {
                new TableDataStructureModel{DataField=nameof(PatientBillModel.ProviderName),DataHeader=ResourceConstants.R_PROVIDER_KEY, IsSearchable = true},
                new TableDataStructureModel{DataField=nameof(PatientBillModel.ProgramName),DataHeader=ResourceConstants.R_PROGRAM_NAME_KEY,IsSortable=false,IsSearchable = true},
                new TableDataStructureModel{DataField=nameof(PatientBillModel.GrossTotal),DataHeader=ResourceConstants.R_GROSS_TOTAL_KEY, IsSearchable = true},
                new TableDataStructureModel{DataField=nameof(PatientBillModel.Discount),DataHeader=ResourceConstants.R_DISCOUNT_KEY, IsSearchable = true},
                new TableDataStructureModel{DataField=nameof(PatientBillModel.TotalPaid),DataHeader=ResourceConstants.R_TOTAL_AMOUNT_KEY, IsSearchable = true},
                new TableDataStructureModel{DataField=nameof(PatientBillModel.PaymentMode),DataHeader=ResourceConstants.R_PAYMENT_MODE_NAME_KEY, IsSearchable = true},
                new TableDataStructureModel{DataField=nameof(PatientBillModel.Status),DataHeader=ResourceConstants.R_STATUS_KEY, IsSearchable = true},
            });
        }
        return tableStructure;
    }

    private async Task OnSearchClickAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.BillingReportsView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClick(PatientBillModel patientBill)
    {
        Success = Error = string.Empty;
        if (_billingData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.BillingReportsView.ToString(), (patientBill?.PatientBillID ?? Guid.Empty).ToString(), patientBill.PatientID.ToString()).ConfigureAwait(false);
        }
        else
        {
            _patientBillID = patientBill?.PatientBillID ?? Guid.Empty;
            _patientID = patientBill.PatientID;
            ShowDetailPage = true;
        }
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        Success = Error = string.Empty;
        _patientBillID = Guid.Empty;
        if (errorMessage == ErrorCode.OK.ToString())
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

    private void SetDetailPage()
    {
        if (_billingData.RecordCount > 0 && _patientBillID != Guid.Empty && _patientID != 0)
        {
            _billingData.RecordCount = default;
            ShowDetailPage = true;
        }
    }

    private void GetPaymentDetail()
    {
        _billingCards = new List<CardModel>();
        if (_billingData?.PatientBills?.Count > 0)
        {
            AddInCards(_billingData.PatientBills.Sum(x => Convert.ToInt64(x.GrossTotal)).ToString(), ResourceConstants.R_GROSS_TOTAL_KEY);
            AddInCards(_billingData.PatientBills.Sum(x => Convert.ToInt64(x.Discount)).ToString(), ResourceConstants.R_DISCOUNT_KEY);
            AddInCards(_billingData.PatientBills.Sum(x => Convert.ToInt64(x.TotalPaid)).ToString(), ResourceConstants.R_TOTAL_AMOUNT_KEY);
        }
    }

    private void AddInCards(string header, string subHeaderKey)
    {
        _billingCards.Add(new CardModel
        {
            CardId = Guid.NewGuid().ToString(),
            Header = header,
            SubHeader = LibResources.GetResourceValueByKey(_billingData.Resources, subHeaderKey)
        });
    }
}