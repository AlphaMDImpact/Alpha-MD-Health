using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer;

public class RazorpayServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Save Razorpay Payment & Order Detail
    /// </summary>
    /// <param name="razorpayData">Reference object which holds razorpay data</param>
    /// <returns>Operation Status Code/returns>
    public async Task SaveRazorpayPaymentDetailAsync(RazorpayDTO razorpayData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), razorpayData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(RazorpayDTO.Page)), razorpayData.Page, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapPaymentDetailToTable(razorpayData.RazorpayPayment).AsTableValuedParameter());
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS_2), MapOrderDetailToTable(razorpayData.RazorpayOrder).AsTableValuedParameter());
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS_3), MapCardDetailToTable(razorpayData.RazorpayPayment.CardDetail).AsTableValuedParameter());
        MapCommonSPParameters(razorpayData, parameter, null);
        await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_RAZORPAY_PAYMENT_DETAIL, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        razorpayData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    private DataTable MapPaymentDetailToTable(RazorpayPaymentModel razorpayPaymentData)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(RazorpayPaymentModel.PaymentID), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.OrderID), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Name), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Contact), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Email), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.IsInternational), typeof(bool)),
                new DataColumn(nameof(RazorpayPaymentModel.AmountRefunded), typeof(decimal)),
                new DataColumn(nameof(RazorpayPaymentModel.RefundStatus), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Method), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.IsCaptured), typeof(bool)),
                new DataColumn(nameof(RazorpayPaymentModel.CardID), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Bank), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Wallet), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.VPA), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.TokenID), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Fee), typeof(decimal)),
                new DataColumn(nameof(RazorpayPaymentModel.Tax), typeof(decimal)),
                new DataColumn(nameof(RazorpayPaymentModel.ErrorCode), typeof(long)),
                new DataColumn(nameof(RazorpayPaymentModel.ErrorDescription), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.ErrorSource), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.ErrorReason), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.ErrorStep), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.InvoiceID), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Amount), typeof(decimal)),
                new DataColumn(nameof(RazorpayPaymentModel.Currency), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Entity), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.Status), typeof(string)),
                new DataColumn(nameof(RazorpayPaymentModel.CreatedDateTime), typeof(DateTimeOffset)),
                new DataColumn(nameof(AcquirerData.AuthCode), typeof(string)),
                new DataColumn(nameof(AcquirerData.BankTransactionID), typeof(string)),
                new DataColumn(nameof(AcquirerData.TransactionID), typeof(string))
            }
        };

        if (razorpayPaymentData.PaymentID != null)
        {
            dataTable.Rows.Add(razorpayPaymentData.PaymentID, razorpayPaymentData.OrderID, razorpayPaymentData.Name, razorpayPaymentData.Contact,
            razorpayPaymentData.Email, razorpayPaymentData.IsInternational, razorpayPaymentData.AmountRefunded, razorpayPaymentData.RefundStatus, razorpayPaymentData.Method, razorpayPaymentData.IsCaptured, razorpayPaymentData.CardID, razorpayPaymentData.Bank,
            razorpayPaymentData.Wallet, razorpayPaymentData.VPA, razorpayPaymentData.TokenID, razorpayPaymentData.Fee, razorpayPaymentData.Tax, razorpayPaymentData.ErrorCode, razorpayPaymentData.ErrorDescription, razorpayPaymentData.ErrorSource, razorpayPaymentData.ErrorReason, razorpayPaymentData.ErrorStep,
            razorpayPaymentData.InvoiceID, razorpayPaymentData.Amount, razorpayPaymentData.Currency, razorpayPaymentData.Entity, razorpayPaymentData.Status, razorpayPaymentData.CreatedDateTime, razorpayPaymentData.AcquirerData.AuthCode, razorpayPaymentData.AcquirerData.BankTransactionID, razorpayPaymentData.AcquirerData.TransactionID);
        }
        return dataTable;
    }

    private DataTable MapOrderDetailToTable(RazorpayOrderModel razorpayOrderData)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(RazorpayOrderModel.OrderID), typeof(string)),
                new DataColumn(nameof(RazorpayOrderModel.AmountPaid), typeof(decimal)),
                new DataColumn(nameof(RazorpayOrderModel.AmountDue), typeof(decimal)),
                new DataColumn(nameof(RazorpayOrderModel.Amount), typeof(decimal)),
                new DataColumn(nameof(RazorpayOrderModel.Currency), typeof(string)),
                new DataColumn(nameof(RazorpayOrderModel.CreatedDateTime), typeof(DateTimeOffset)),
                new DataColumn(nameof(RazorpayOrderModel.Entity), typeof(string)),
                new DataColumn(nameof(RazorpayOrderModel.Status), typeof(string)),
                new DataColumn(nameof(RazorpayOrderModel.Attempts), typeof(short)),
                new DataColumn(nameof(RazorpayOrderModel.OfferID), typeof(string))
            }
        };

        dataTable.Rows.Add(razorpayOrderData.OrderID, razorpayOrderData.AmountPaid, razorpayOrderData.AmountDue, razorpayOrderData.Amount,
            razorpayOrderData.Currency, razorpayOrderData.CreatedDateTime, razorpayOrderData.Entity, razorpayOrderData.Status, razorpayOrderData.Attempts, razorpayOrderData.OfferID);

        return dataTable;
    }

    private DataTable MapCardDetailToTable(PaymentCardModel paymentCardData)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(PaymentCardModel.CardId), typeof(string)),
                new DataColumn(nameof(PaymentCardModel.Entity), typeof(string)),
                new DataColumn(nameof(PaymentCardModel.Name), typeof(string)),
                new DataColumn(nameof(PaymentCardModel.Last4Digit), typeof(long)),
                new DataColumn(nameof(PaymentCardModel.Network), typeof(string)),
                new DataColumn(nameof(PaymentCardModel.Type), typeof(string)),
                new DataColumn(nameof(PaymentCardModel.Issuer), typeof(string)),
                new DataColumn(nameof(PaymentCardModel.IsInternational), typeof(bool)),
                new DataColumn(nameof(PaymentCardModel.IsEMI), typeof(bool)),
                new DataColumn(nameof(PaymentCardModel.SubType), typeof(string)),
                new DataColumn(nameof(PaymentCardModel.TokenIin), typeof(string))
            }
        };

        if(paymentCardData != null)
        {
            dataTable.Rows.Add(paymentCardData.CardId, paymentCardData.Entity, paymentCardData.Name, paymentCardData.Last4Digit,
            paymentCardData.Network, paymentCardData.Type, paymentCardData.Issuer, paymentCardData.IsInternational, paymentCardData.IsEMI, paymentCardData.SubType, paymentCardData.TokenIin);
        }
        
        return dataTable;
    }
}