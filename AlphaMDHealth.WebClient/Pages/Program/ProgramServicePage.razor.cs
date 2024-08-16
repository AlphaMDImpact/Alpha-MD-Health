using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient
{
    public partial class ProgramServicePage : BasePage
    {
        private readonly ProgramDTO _programData = new ProgramDTO
        {
            ProgramService = new ProgramServiceModel(),
        };
        private List<ButtonActionModel> _actionData;
        private bool _hideConfirmationPopup = true;
        private bool _isEditable;
        private double? _quantity;
        private double? _assignAfterDays;
        private double? _assignForDays;

        /// <summary>
        /// IsSynced
        /// </summary>
        [Parameter]
        public bool IsSynced { get; set; }

        /// <summary>
        /// Program ID
        /// </summary>
        [Parameter]
        public long ProgramID { get; set; }

        /// <summary>
        /// selected Program Reason ID
        /// </summary>
        [Parameter]
        public long ProgramExternalServiceID { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _programData.ProgramService.ProgramID = ProgramID;
            _programData.ProgramService.ProgramExternalServiceID = ProgramExternalServiceID;
            await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncProgramServicesFromServer(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
            if (_programData.ErrCode == ErrorCode.OK)
            {
                if (_programData.ProgramService.ProgramExternalServiceID > 0)
                {
                    _quantity = _programData.ProgramService.Quantity;
                    _assignAfterDays = _programData.ProgramService.AssignAfterDays;
                    _assignForDays = _programData.ProgramService.AssignForDays;
                }
                _isEditable = IsSynced && LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.ProgramServiceAddEdit.ToString());
                _isDataFetched = true;
            }
            else
            {
                await OnClose.InvokeAsync(_programData.ErrCode.ToString()).ConfigureAwait(true);
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
                    _programData.ProgramService.IsActive = false;
                    await SaveProgramServiceAsync().ConfigureAwait(true);
                }
            }
        }

        private async Task OnSaveButtonClickedAsync()
        {
            Success = Error = string.Empty;
            if (IsValid())
            {
                _programData.ProgramService.ProgramExternalServiceID = ProgramExternalServiceID;
                _programData.ProgramService.ProgramID = ProgramID;
                _programData.ProgramService.ExternalServiceID = (byte)(_programData?.Items.FirstOrDefault(x => x.IsSelected).OptionID);
                _programData.ProgramService.Quantity = (int)_quantity;
                _programData.ProgramService.AssignAfterDays = (short)_assignAfterDays;
                _programData.ProgramService.AssignForDays = (short)_assignForDays;
                _programData.ProgramService.IsActive = true;
                await SaveProgramServiceAsync().ConfigureAwait(true);
            }
        }

        private async Task SaveProgramServiceAsync()
        {
            _programData.ErrCode = ErrorCode.OK;
            await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncProgramServiceToServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
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
}
