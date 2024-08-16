using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ProgramBillingItemPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { ProgramBillItem = new PatientBillModel() };
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private double? _amount;
    private bool _isDisabled;
    private bool _isEditable;

    /// <summary>
    /// Program ID
    /// </summary>
    [Parameter]
    public long ProgramID { get; set; }

    /// <summary>
    /// selected Program Billing Item ID
    /// </summary>
    [Parameter]
    public long ProgramBillingItemID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programData.ProgramBillItem.ProgramID = ProgramID;
        _programData.ProgramBillItem.ProgramBillingItemID = ProgramBillingItemID;
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncProgramBillingItemsFromServer(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            if (_programData.ProgramBillItem.ProgramBillingItemID > 0)
            {
                _isDisabled = _programData.BillingItemOptionList.FirstOrDefault(x => x.IsSelected == true).IsDisabled == true;
                _amount = _programData.ProgramBillItem.Amount;
            }
            _isEditable = LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.ProgramBillingItemAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(null).ConfigureAwait(true);
        }
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
             new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
             new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
         };
        _hideConfirmationPopup = false;
    }

    private async Task DeletePopUpCallbackAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _programData.ProgramBillItem.IsActive = false;
                await SaveProgramProgramBillingItemAsync().ConfigureAwait(true);
            }
        }
    }
   
    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _programData.ProgramBillItem.ProgramBillingItemID = ProgramBillingItemID;
            _programData.ProgramBillItem.ProgramID = ProgramID;
            _programData.ProgramBillItem.BillingItemID = (short)_programData.BillingItemOptionList.FirstOrDefault(x => x.IsSelected).OptionID;
            _programData.ProgramBillItem.Amount = Convert.ToDouble(_amount);
            _programData.ProgramBillItem.IsActive = true;
            await SaveProgramProgramBillingItemAsync().ConfigureAwait(true);
        }
    }

    private async Task SaveProgramProgramBillingItemAsync()
    {
        _programData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync( new ProgramService(AppState.webEssentials).SyncProgramBillingItemToServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString());
        }
        else
        {
            Error = _programData.ErrCode.ToString();
        }
    }
}