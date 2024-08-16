using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AlphaMDHealth.WebClient;

public partial class SubscriptionPlansPage : BasePage
{
    private readonly SubscriptionDTO _subscriptionData = new SubscriptionDTO { Plans = new List<PlanModel>(), UserPlan = new UserPlanModel() };

    private IList<ButtonActionModel> _actionButtons = null;
    /// <summary>
    /// Plan ID parameter
    /// </summary>
    [Parameter]
    public short PlanID
    {
        get { return _subscriptionData.UserPlan.PlanID; }
        set
        {
            _subscriptionData.UserPlan.PlanID = value;
        }
    }
    [Inject]
    public IJSRuntime JsRuntime { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetPlansDataAsync().ConfigureAwait(true);
    }

    private async Task GetPlansDataAsync()
    {
        _subscriptionData.Plans = new List<PlanModel>();
        await SendServiceRequestAsync(new UserSubscriptionService(AppState.webEssentials).GetSubscriptionPlansAsync(_subscriptionData), _subscriptionData).ConfigureAwait(true);
        _isDataFetched = true;
        if (IsPatientMobileView)
        {
            if (!ShowDetailPage
          && _subscriptionData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(_subscriptionData.Plans)
          && LibPermissions.HasPermission(_subscriptionData.FeaturePermissions, AppPermissions.BuySubscriptionPlan.ToString()))
            {
                _actionButtons ??= new List<ButtonActionModel>();
                _actionButtons.Add(new ButtonActionModel
                {
                    FieldType = FieldTypes.PrimaryButtonControl,
                    ButtonResourceKey = AppPermissions.BuySubscriptionPlan.ToString(),
                    ButtonAction = () => { OnSubscribeAndPayClicked(); },
                    ButtonClass = "mobile-view-button",
                    Value = LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.BuySubscriptionPlan.ToString())
                });
                _actionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                    ButtonAction = () => { onBackButtonCLick(); },
                    Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
                });
            }
        }
    }

    private void OnCardClicked(PlanModel plan)
    {
        _subscriptionData.Plan = plan;
        _subscriptionData.UserPlan.PlanID = plan.PlanID;
    }

    private void OnSubscribeAndPayClicked()
    {
        _subscriptionData.ErrCode = ErrorCode.OK;
        Success = Error = string.Empty;
        if (_subscriptionData.UserPlan.PlanID > 0)
        {
            ShowDetailPage = true;
        }
        else
        {
            Error = ResourceConstants.R_SELECT_SUBSCRIPTION_PLAN_KEY;
        }
    }

    private async Task OnPaymentResponseReceivedAsync(RazorpayPaymentModel responseData)
    {
        DateTimeOffset currentDateTime = AppState.webEssentials.ConvertToLocalTime(GenericMethods.GetUtcDateTime);
        _subscriptionData.UserPlan.FromDate = currentDateTime.ToUniversalTime();
        _subscriptionData.UserPlan.ToDate = currentDateTime.AddDays(_subscriptionData.Plan.DurationInDays).ToUniversalTime();
        _subscriptionData.UserPlan.IsActive = true;
        _subscriptionData.UserPlan.PaymentID = responseData.PaymentID;

        if (responseData.Status != null && responseData.Status == "captured")
        {
            await SendServiceRequestAsync(new UserSubscriptionService(AppState.webEssentials).SaveUserSubscriptionPlanAsync(_subscriptionData, CancellationToken.None), _subscriptionData).ConfigureAwait(true);
            if (_subscriptionData.ErrCode == ErrorCode.OK)
            {
                Success = _subscriptionData.ErrCode.ToString();
                if (AppState.MasterData.IsSubscriptionRequired)
                {
                    AppState.MasterData.IsSubscriptionRequired = false;
                }
                await NavigateToAsync(AppPermissions.NavigationComponent.ToString(), true).ConfigureAwait(false);
            }
            else
            {
                Error = _subscriptionData.ErrCode.ToString();
            }
        }
        else
        {
            Error = ResourceConstants.R_PAYMENT_FAILED_KEY;
        }
        ShowDetailPage = false;
    }

    private async void onBackButtonCLick()
    {
        await JsRuntime.InvokeVoidAsync("invokeWebviewMethod", "backactionclicked", "200");
    }
}