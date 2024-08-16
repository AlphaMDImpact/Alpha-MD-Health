using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AlphaMDHealth.WebClient;

public partial class PatientBillPage : BasePage
{
    private readonly BillingItemDTO _patientBillData = new BillingItemDTO
    {
        RecordCount = -1,
        PatientBillItem = new PatientBillModel(),
        PatientBillingItems = new List<OptionModel>(),
        PatientBillItems = new List<PatientBillItemModel>(),
        PatientProgramOptionList = new List<OptionModel>(),
        PaymentModeOptionList = new List<OptionModel>()
    };
    private PatientBillService _billService;
    public List<PatientBillItemModel> _numericConditions = new List<PatientBillItemModel>();

    private List<OptionModel> _patientProviderOptionList = new List<OptionModel>();
    private List<OptionModel> _patientBillingItemsOptionList = new List<OptionModel>();
    private List<ButtonActionModel> _actionData;
    private double? _totalAmount;
    private double? _discount;
    private double? _totalPaid;
    private bool _hideConfirmationPopup = true;
    private bool _isProgramEnabled;
    private bool _isEditable;

    /// <summary>
    /// Patient Bill ID 
    /// </summary>
    [Parameter]
    public Guid PatientBillID { get; set; }

    /// <summary>
    /// Patient  ID 
    /// </summary>
    [Parameter]
    public long PatientID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _billService = new PatientBillService(AppState.webEssentials);
        _patientBillData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        _patientBillData.SelectedUserID = PatientID != 0 ? PatientID : _patientBillData.SelectedUserID;
        _patientBillData.PatientBillItem.PatientBillID = PatientBillID;
        await SendServiceRequestAsync(_billService.GetPatientBillingsDataAsync(_patientBillData), _patientBillData).ConfigureAwait(true);
        if (_patientBillData.ErrCode == ErrorCode.OK)
        {
            _discount = 0;
            _isEditable = LibPermissions.HasPermission(_patientBillData.FeaturePermissions, AppPermissions.PatientBillAddEdit.ToString());
            if (PatientBillID != Guid.Empty)
            {
                _isProgramEnabled = _patientBillData.PatientProgramOptionList?.Any(x => x.IsSelected && !x.IsDisabled) ?? false;
                _isEditable = _isEditable && _isProgramEnabled && _patientBillData.PatientBillItem.IsActive;
                _numericConditions = _patientBillData.PatientBillItems;
                SetProvidersOptions(_patientBillData.PatientBillItem.ProgramID);
                SelectOptionById(_patientProviderOptionList, _patientBillData.PatientBillItem.ProviderID);
                SetBillingItemsOptions(_patientBillData.PatientBillItem.ProgramID);
                _totalAmount = GrossTotalCalculator();
                _totalPaid = _patientBillData.PatientBillItem.TotalPaid;
                _discount = _patientBillData.PatientBillItem.Discount;
                if (_patientBillData.PatientBillItem.IsActive)
                {
                    _numericConditions = _patientBillData.PatientBillItems.FindAll(x => x.IsActive);
                }
            }
            else
            {
                _patientBillData.PatientProgramOptionList = _patientBillData?.PatientProgramOptionList.FindAll(x => x.IsDisabled == false);
                _patientBillData.PatientBillItems = new List<PatientBillItemModel>() { new PatientBillItemModel { BillingItemID = 0, IsActive = true } };
                _numericConditions = _patientBillData.PatientBillItems;
                _patientBillData.PatientBillItem = new PatientBillModel();
            }
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_patientBillData.ErrCode.ToString());
        }
    }

    private void OnPatientProgramSelectionChange(object e)
    {
        if (long.TryParse(e as string, out long optionId) && optionId > 0)
        {
            SetProvidersOptions(optionId);
            SetBillingItemsOptions(optionId);
        }
    }

    private void SetProvidersOptions(long optionId)
    {
        _patientProviderOptionList = _patientBillData.PatientProvidersOptionList.FindAll(x => x.ParentOptionID == optionId);
    }

    private void SetBillingItemsOptions(long optionId)
    {
        _patientBillingItemsOptionList = _patientBillData.PatientBillingItems.FindAll(x => Convert.ToInt64(x.GroupName) == optionId);
    }

    private void SelectOptionById(List<OptionModel> options, long id)
    {
        var option = options?.FirstOrDefault(x => x.OptionID == id);
        if (option != null)
        {
            option.IsSelected = true;
        }
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY }
         };
        _hideConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationPopUpClickedAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _patientBillData.PatientBillItem.IsActive = false;
                await SaveData();
            }
        }
    }

    private List<OptionModel> CreateOptionList(string targetQuestionID)
    {

        var isMatching = _patientBillData.PatientBillItems.Any(x => 
                            x.IsSynced == true
                            && _patientBillingItemsOptionList.Any(y => y.OptionID == x.BillingItemID && y.IsDisabled == false)
                            && x.BillingItemID == Convert.ToInt64(targetQuestionID));

        List<OptionModel> optionList = (from resource in _patientBillingItemsOptionList
                                        where (isMatching ? !resource.IsDisabled : resource.IsDisabled)
                                        select new OptionModel
                                        {
                                            OptionID = resource.OptionID,
                                            OptionText = resource.OptionText,
                                            ParentOptionID = resource.ParentOptionID,
                                            GroupName = resource.GroupName,
                                            IsSelected = resource.OptionID.ToString() == targetQuestionID
                                        }).ToList();

        return optionList ?? new List<OptionModel>();
    }

    private void OnAddClicked()
    {
        var IsValid = true;
        foreach (var control in _numericConditions.Select((value, i) => new { i, value }))
        {
            IsValid = _controls.FirstOrDefault(x => x.Key == $"{Constants.ADD_BETWEEN_CONDITION}_{control.i}").Value != null
                ? (_controls.FirstOrDefault(x => x.Key == $"{Constants.ADD_BETWEEN_CONDITION}_{control.i}").Value as AmhBillingItemComponent).ValidateControl(true)
                : true;
        }
        if (IsValid)
        {
            _patientBillData.PatientBillItems.Add(new PatientBillItemModel { BillingItemID = 0, IsActive = true });
            _numericConditions = _patientBillData.PatientBillItems.FindAll(x => x.IsActive);
        }
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void TotalAndGrossAmountCalculator()
    {
        double TotalAmount = 0;
        foreach (var x in _patientBillData.PatientBillItems.FindAll(x => x.IsActive))
        {
            TotalAmount += x.Amount;
        }
        _totalAmount = TotalAmount;
        _totalPaid = TotalAmount - _discount;
    }

    private double GrossTotalCalculator()
    {
        double grossTotal = 0;
        foreach (var item in _patientBillData.PatientBillItems.FindAll(x => x.IsActive == true))
        {
            grossTotal += item.Amount;
        }
        return grossTotal;
    }

    private void OnConditionDelete(int optionID)
    {
        if (_patientBillData.PatientBillItems[optionID].IsSynced)
        {
            var patientBillItem = _patientBillData.PatientBillItems.Find(x => x.BillingItemID == _numericConditions[optionID].BillingItemID);
            if (patientBillItem != null)
            {
                patientBillItem.IsActive = false;
            }
        }
        else
        {
            _patientBillData.PatientBillItems.RemoveAt(optionID);
        }
        _numericConditions.RemoveAll(x => x.BillingItemID == 0);
        _numericConditions = _patientBillData.PatientBillItems.FindAll(x => x.IsActive == true && x.BillingItemID != 0);
        RemoveControlByKey(Constants.ADD_BETWEEN_CONDITION);
        TotalAndGrossAmountCalculator();
        StateHasChanged();
    }

    private async Task OnSaveActionClickedAsync()
    {
        if (IsValid())
        {
            _patientBillData.PatientBillItems.RemoveAll(x => x.BillingItemID == 0);
            _patientBillData.PatientBillItem.PatientBillID = PatientBillID;
            _patientBillData.PatientBillItem.PatientID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
            _patientBillData.PatientBillItem.ProgramID = _patientBillData.PatientProgramOptionList.Find(x => x.IsSelected).OptionID;
            _patientBillData.PatientBillItem.ProviderID = _patientProviderOptionList.Find(x => x.IsSelected).OptionID;
            _patientBillData.PatientBillItem.GrossTotal = GrossTotalCalculator();
            _patientBillData.PatientBillItem.Discount = _discount.Value;
            _patientBillData.PatientBillItem.TotalPaid = _patientBillData.PatientBillItem.GrossTotal - _patientBillData.PatientBillItem.Discount;
            _patientBillData.PatientBillItem.PaymentModeID = (short)_patientBillData.PaymentModeOptionList.Find(x => x.IsSelected).OptionID;
            _patientBillData.PatientBillItem.IsActive = true;
            _patientBillData.PatientBillItems.ForEach(x => x.PatientBillID = PatientBillID);
            await SaveData();
        }
    }
    private async Task SaveData()
    {
        await SendServiceRequestAsync(_billService.SavePatientBillingDataAsync(_patientBillData), _patientBillData).ConfigureAwait(true);
        if (_patientBillData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_patientBillData.ErrCode.ToString());
        }
        else
        {
            Error = _patientBillData.ErrCode.ToString();
        }
    }

    private async Task OnPrintClickAsync()
    {
        var data = new PatientBillService(AppState.webEssentials).GetPrintData(_patientBillData, AppState.MasterData.Settings.Find(x => x.SettingKey == SettingsConstants.S_LOGO_KEY).SettingValue, AppState.MasterData.OrganisationName);
        if (data.Item2 == ErrorCode.OK)
        {
            await JSRuntime.InvokeVoidAsync("printData", data.Item1);
        }
    }
}