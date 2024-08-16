using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class BillingItemServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get BillingItems from server
        /// </summary>
        /// <param name="billingItemData">Object to hold BillingItems data</param>
        /// <returns>List of BillingItems in billingItemData  and Operation Status</returns>
        public async Task GetBillingItemsAsync(BillingItemDTO billingItemData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), billingItemData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BillingItemModel.BillingItemID), billingItemData.BillingItem.BillingItemID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), billingItemData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(billingItemData, parameter,
                AppPermissions.BillingItemsView.ToString(), $"{AppPermissions.BillingItemDelete},{AppPermissions.BillingItemAddEdit}"
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_BILLING_ITEMS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                billingItemData.BillingItems = (await result.ReadAsync<BillingItemModel>().ConfigureAwait(false))?.ToList();
                await MapReturnPermissionsAsync(billingItemData, result).ConfigureAwait(false);
            }
            billingItemData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Billing Item Data to database
        /// </summary>
        /// <param name="billingItemData">Billing Item to be saved</param>
        /// <returns>Operation status</returns>
        public async Task SaveBillingItemAsync(BillingItemDTO billingItemData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), billingItemData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BillingItemModel.BillingItemID), billingItemData.BillingItem.BillingItemID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapBillingItemsToTable(billingItemData.BillingItems).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), billingItemData.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(billingItemData, parameter, billingItemData.IsActive ? AppPermissions.BillingItemAddEdit.ToString() : AppPermissions.BillingItemDelete.ToString());
            await connection.ExecuteAsync(SPNameConstants.USP_SAVE_BILLING_ITEM, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            billingItemData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        protected DataTable MapBillingItemsToTable(List<BillingItemModel> BillingItems)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(BillingItems))
            {
                foreach (BillingItemModel record in BillingItems)
                {
                    dataTable.Rows.Add(record.BillingItemID, Guid.Empty, record.LanguageID, record.Name, string.Empty, string.Empty);
                }
            }
            return dataTable;
        }

        protected DataTable MapPatientBillsToTable(List<PatientBillModel> patientBills)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(PatientBillModel.PatientBillID), typeof(Guid)),
                    new DataColumn(nameof(PatientBillModel.PatientID), typeof(long)),
                    new DataColumn(nameof(PatientBillModel.ProgramID), typeof(long)),
                    new DataColumn(nameof(PatientBillModel.ProviderID), typeof(long)),
                    new DataColumn(nameof(PatientBillModel.GrossTotal), typeof(decimal)),
                    new DataColumn(nameof(PatientBillModel.Discount), typeof(decimal)),
                    new DataColumn(nameof(PatientBillModel.TotalPaid), typeof(decimal)),
                    new DataColumn(nameof(PatientBillModel.PaymentModeID), typeof(byte)),
                    new DataColumn(nameof(PatientBillModel.BillDateTime), typeof(DateTimeOffset)),
                    new DataColumn(nameof(PatientBillModel.AddedOn), typeof(DateTimeOffset)),
                    new DataColumn(nameof(PatientBillModel.IsActive), typeof(bool)),
                    new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(short)),
                }
            };
            int count = 0;
            if (GenericMethods.IsListNotEmpty(patientBills))
            {
                foreach (PatientBillModel record in patientBills)
                {
                    dataTable.Rows.Add(record.PatientBillID, record.PatientID, record.ProgramID, record.ProviderID, record.GrossTotal, record.Discount
                        , record.TotalPaid, record.PaymentModeID, record.BillDateTime, record.AddedOn, record.IsActive, ++count);
                }
            }
            return dataTable;
        }

        protected DataTable MapPatientBillItemsToTable(List<PatientBillItemModel> BillingItems)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(PatientBillItemModel.PatientBillID), typeof(Guid)),
                    new DataColumn(nameof(PatientBillItemModel.BillingItemID), typeof(long)),
                    new DataColumn(nameof(PatientBillItemModel.Amount), typeof(double)),
                    new DataColumn(nameof(PatientBillItemModel.IsActive), typeof(bool))
                }
            };
            foreach (PatientBillItemModel record in BillingItems)
            {
                dataTable.Rows.Add(record.PatientBillID, record.BillingItemID, record.Amount, record.IsActive);
            }
            return dataTable;
        }

        /// <summary>
        /// Gets Patietn Bill(S)
        /// </summary>
        /// <param name="billData">Object to return Data</param>
        /// <returns>Data and operation status</returns>
        public async Task GetPatientBillsAsync(BillingItemDTO billData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), billData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(PatientBillModel.PatientBillID)), billData.PatientBillItem.PatientBillID, DbType.Guid, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), billData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.RecordCount)), billData.RecordCount, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.LanguageID)), billData.LanguageID, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), null, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(billData, parameter, AppPermissions.PatientBillsView.ToString(), $"{AppPermissions.PatientBillDelete},{AppPermissions.PatientBillAddEdit}"
            );


            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PATIENT_BILLS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                if (billData.RecordCount == 0)
                {
                    billData.PatientBills = (await result.ReadAsync<PatientBillModel>().ConfigureAwait(false))?.ToList(); //ask Manoj to change this
                }
                if (billData.RecordCount == -1)
                {
                    billData.PatientProgramOptionList = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                    billData.PatientProvidersOptionList = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                    billData.PaymentModeOptionList = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                    billData.PatientBillingItems = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                }
                if (billData.PatientBillItem.PatientBillID != Guid.Empty)
                {
                    billData.PatientBillItem = (await result.ReadAsync<PatientBillModel>().ConfigureAwait(false))?.FirstOrDefault();
                    billData.PatientBillItems = (await result.ReadAsync<PatientBillItemModel>().ConfigureAwait(false))?.ToList();//ask Manoj to change this
                }
                await MapReturnPermissionsAsync(billData, result).ConfigureAwait(false);
            }
            billData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Patient Bill data
        /// </summary>
        /// <param name="billData">Object to save</param>
        /// <returns>Operation status</returns>
        public async Task SavePatientBillAsync(BillingItemDTO billData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), billData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapPatientBillsToTable(billData.PatientBills).AsTableValuedParameter());
            parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS_2), MapPatientBillItemsToTable(billData.PatientBillItems).AsTableValuedParameter());
            MapCommonSPParameters(billData, parameter, billData.IsActive ? AppPermissions.PatientBillAddEdit.ToString() : AppPermissions.PatientBillDelete.ToString());
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_PATIENT_BILL, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                billData.SavePatientBills = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
            }
            billData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }
    }
}