using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Razorpay.Api;

namespace AlphaMDHealth.WebClient;

public partial class RazorpayPaymentGateway : AmhBaseControl
{
    private RazorpayClient Client;

    [Inject]
    private IJSRuntime JSRuntime { get; set; }

    private RazorpayDTO RazorpayData { get; set; }

    [Parameter]
    public decimal Value { get; set; }

    /// <summary>
    /// On payment response recedived
    /// </summary>
    [Parameter]
    public EventCallback<RazorpayPaymentModel> OnPaymentComplete { get; set; }

    protected override async Task OnAfterRenderAsync(bool IsFirstRender)
    {
        if (IsFirstRender)
        {
            await JSRuntime.InvokeVoidAsync("registerDotNetObject", DotNetObjectReference.Create(this));
            await JSRuntime.InvokeVoidAsync("OpenRazorpayPopup", RazorpayData.RazorpayOrder.OrderID);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        RazorpayData = new RazorpayDTO()
        {
            RazorpayOrder = new RazorpayOrderModel(),
            RazorpayPayment = new RazorpayPaymentModel(),
            Page = AppState.RouterData.SelectedRoute.Page
        };

        string key = LibSettings.GetSettingValueByKey(AppState.MasterData.Settings, SettingsConstants.S_RAZORPAY_API_KEY);
        string secret = LibSettings.GetSettingValueByKey(AppState.MasterData.Settings, SettingsConstants.S_RAZORPAY_API_SECRET_KEY);
        Client = new RazorpayClient(key, secret);

        Dictionary<string, object> input = new Dictionary<string, object>();
        input.Add("amount", Value * 100);
        input.Add("currency", Constants.INDIAN_CURRENCY);

        Order order = Client.Order.Create(input);
        RazorpayData.RazorpayOrder.OrderID = order["id"].ToString();
    }

    [JSInvokable]
    public void RazorPayResponse(string paymentID, string signature, string orderID)
    {
        Dictionary<string, string> attributes = new Dictionary<string, string>();
        attributes.Add("razorpay_payment_id", paymentID);
        attributes.Add("razorpay_order_id", orderID);
        attributes.Add("razorpay_signature", signature);

        Utils.verifyPaymentSignature(attributes);

        MapRazorpayOrderDetails(orderID, paymentID);
    }

    [JSInvokable]
    public void OnRazorpayPopupClosed(string orderID)
    {
        List<Payment> payments = Client.Order.Fetch(orderID).Payments();

        var paymentID = GenericMethods.IsListNotEmpty(payments) ?
                        payments.FirstOrDefault()["id"].ToString() : null;

        MapRazorpayOrderDetails(orderID, paymentID);
    }

    private async void MapRazorpayOrderDetails(string orderID, string paymentID)
    {
        if (paymentID != null)
        {
            Payment payment = Client.Payment.Fetch(paymentID);
            RazorpayData.RazorpayPayment = JsonConvert.DeserializeObject<RazorpayPaymentModel>(payment.Attributes.ToString());
        }

        Order order = Client.Order.Fetch(orderID);
        RazorpayData.RazorpayOrder = JsonConvert.DeserializeObject<RazorpayOrderModel>(order.Attributes.ToString());
        if (RazorpayData != null)
        {
            if (RazorpayData.RazorpayPayment.PaymentID != null)
            {
                RazorpayData.RazorpayPayment.Amount = RazorpayData.RazorpayPayment.Amount / 100;
                RazorpayData.RazorpayPayment.Fee = RazorpayData.RazorpayPayment.Fee / 100;
                RazorpayData.RazorpayPayment.Tax = RazorpayData.RazorpayPayment.Tax / 100;
                RazorpayData.RazorpayPayment.CreatedDateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(RazorpayData.RazorpayPayment.CreatedAt)).DateTime.ToUniversalTime();
            }
            RazorpayData.RazorpayOrder.CreatedDateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(RazorpayData.RazorpayOrder.CreatedAt)).DateTime.ToUniversalTime();
            RazorpayData.RazorpayOrder.Amount = RazorpayData.RazorpayOrder.Amount / 100;
            RazorpayData.RazorpayOrder.AmountPaid = RazorpayData.RazorpayOrder.AmountPaid / 100;
            RazorpayData.RazorpayOrder.AmountDue = RazorpayData.RazorpayOrder.AmountDue / 100;
        }

        await new RazorpayService(AppState.webEssentials).SyncRazorpayPaymentDetailToServerAsync(RazorpayData, CancellationToken.None);
        await OnPaymentComplete.InvokeAsync(RazorpayData.RazorpayPayment);
    }
}