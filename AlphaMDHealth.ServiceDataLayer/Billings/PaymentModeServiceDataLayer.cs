using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class PaymentModeServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get PaymentModes from Database
        /// </summary>
        /// <param name="paymentModeData">Object to hold PaymentModes data</param>
        /// <returns>List of PaymentModes in paymentModeData  and Operation Status</returns>
        public async Task GetPaymentModesAsync(PaymentModeDTO paymentModeData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), paymentModeData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PaymentModeModel.PaymentModeID), paymentModeData.PaymentMode.PaymentModeID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), paymentModeData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(paymentModeData, parameter,
                AppPermissions.PaymentModesView.ToString(), $"{AppPermissions.PaymentModeDelete},{AppPermissions.PaymentModeAddEdit}"
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PAYMENT_MODES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                paymentModeData.PaymentModes = (await result.ReadAsync<PaymentModeModel>().ConfigureAwait(false))?.ToList();
                await MapReturnPermissionsAsync(paymentModeData, result).ConfigureAwait(false);
            }
            paymentModeData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Payment Mode Data to database
        /// </summary>
        /// <param name="paymentModeData">Payment Mode to be saved</param>
        /// <returns>Operation status</returns>
        public async Task SavePaymentModeAsync(PaymentModeDTO paymentModeData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), paymentModeData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PaymentModeModel.PaymentModeID), paymentModeData.PaymentMode.PaymentModeID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapPaymentModesToTable(paymentModeData.PaymentModes).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), paymentModeData.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(paymentModeData, parameter, paymentModeData.IsActive ? AppPermissions.PaymentModeAddEdit.ToString() : AppPermissions.PaymentModeDelete.ToString());
            await connection.ExecuteAsync(SPNameConstants.USP_SAVE_PAYMENT_MODE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            paymentModeData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        protected DataTable MapPaymentModesToTable(List<PaymentModeModel> paymentModeModels)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(paymentModeModels))
            {
                foreach (PaymentModeModel record in paymentModeModels)
                {
                    dataTable.Rows.Add(record.PaymentModeID, Guid.Empty, record.LanguageID, record.Name, string.Empty, string.Empty);
                }
            }
            return dataTable;
        }
    }
}