using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public class PatientBillService : BaseService
{
    private BillingDataBase _billingDB { get; set; }

    /// <summary>
    /// PatientBill Service
    /// </summary>
    public PatientBillService(IEssentials essentials) : base(essentials)
    {
        _billingDB = new BillingDataBase();
    }

    /// <summary>
    /// Get Patient Bills
    /// </summary>
    /// <param name="billingsData"> Billing Data</param>
    /// <returns>Return operation status</returns>
    public async Task GetPatientBillingsDataAsync(BillingItemDTO billingsData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                if (billingsData.SelectedUserID == 0)
                {
                    billingsData.SelectedUserID = GetUserID();
                }
                billingsData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                billingsData.OrganisationID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 0);
                await Task.WhenAll(
                    GetFeaturesAsync(AppPermissions.PatientBillsView.ToString(), AppPermissions.PatientBillAddEdit.ToString()),
                    GetSettingsAsync(GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                    GetResourcesAsync(GroupConstants.RS_BILLING_GROUP),
                    _billingDB.GetPatientBillingsAsync(billingsData)
                ).ConfigureAwait(false);
                billingsData.Resources = PageData.Resources;
                billingsData.Settings = PageData.Settings;
                billingsData.ErrCode = ErrorCode.OK;
            }
            else
            {
                await SyncPatientBillsFromServerAsync(billingsData, CancellationToken.None);
                ConvertBillDateToLocal(billingsData);
            }
            if (GenericMethods.IsListNotEmpty(billingsData.PatientBills))
            {
                LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                foreach (var billingData in billingsData.PatientBills)
                {
                    if (MobileConstants.IsMobilePlatform)
                    {
                        billingData.ProgramDiscription = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_AMOUNT_PAID_KEY) + Constants.STRING_SPACE + Constants.DASH_INDICATOR + Constants.STRING_SPACE + billingData.TotalPaid + Constants.STRING_SPACE + Constants.PIPE_SEPERATOR + Constants.STRING_SPACE + LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_PAYMENY_MODE_KEY) + Constants.STRING_SPACE + billingData.PaymentMode;
                        billingData.LeftDefaultIcon = ImageConstants.I_PATIENT_BILLING_ITEM_SVG;
                        billingData.ProgramName = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_PROGRAM_TITLE_KEY) + Constants.DASH_INDICATOR + billingData.ProgramName;
                    }
                    billingData.BillDateTimeString = GenericMethods.GetLocalDateTimeBasedOnCulture(billingData.BillDateTime.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                }
            }
        }
        catch (Exception ex)
        {
            billingsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// get Billing Data Based on id
    /// </summary>
    /// <param name="billingsData"> Billing Data</param>
    /// <returns>Error Code</returns>
    public async Task GetPatientBillingDataAsync(BillingItemDTO billingsData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                billingsData.OrganisationID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 0);
                await Task.WhenAll(
                    GetFeaturesAsync(AppPermissions.PatientBillAddEdit.ToString(), AppPermissions.PatientBillsView.ToString(), AppPermissions.PatientBillDelete.ToString(), AppPermissions.PatientBillShare.ToString()),
                    _billingDB.GetPatientBillingDataAsync(billingsData),
                    GetResourcesAsync(GroupConstants.RS_BILLING_GROUP, GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_MENU_ACTION_GROUP),
                    GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, GroupConstants.RS_ORGANISATION_THEMES_STYLES_GROUP)
                ).ConfigureAwait(false);
                billingsData.Resources = PageData.Resources;
                billingsData.Settings = PageData.Settings;
                if (billingsData.IsBillDetailViews)
                {
                    billingsData.PatientBillItem.OrganisationDetail = GetOrganisationDetails(billingsData.PatientBillItem?.OrganisationAddress, billingsData.PatientBillItem?.OrganisationContact);
                    billingsData.LastModifiedBy = GetInitials(billingsData.PatientBillItem.OrganisationName);
                    //todo:
                    //billingsData.AddedBy = await new SettingsService().GetSettingsValueByKeyAsync(SettingsConstants.S_LOGO_KEY).ConfigureAwait(false);
                    LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                    billingsData.PatientBillItem.BillDateTimeString = GenericMethods.GetLocalDateTimeBasedOnCulture(billingsData.PatientBillItem.BillDateTime.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                }
                ConvertBillDateToLocal(billingsData);
                billingsData.ErrCode = ErrorCode.OK;
            }
        }
        catch (Exception ex)
        {
            billingsData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void ConvertBillDateToLocal(BillingItemDTO billingsData)
    {
        LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        if (billingsData.PatientBillItem?.BillDateTime.HasValue ?? false)
        {
            billingsData.PatientBillItem.BillDateTime = _essentials.ConvertToLocalTime(billingsData.PatientBillItem.BillDateTime.Value);
            billingsData.PatientBillItem.BillDateTimeString = GenericMethods.GetLocalDateTimeBasedOnCulture(billingsData.PatientBillItem.BillDateTime.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
        }
    }

    /// <summary>
    /// Save Patient Bill
    /// </summary>
    /// <param name="billingsData">billingsData Patient Bill</param>
    /// <returns>operation status</returns>
    public async Task SavePatientBillingDataAsync(BillingItemDTO billingsData)
    {
        try
        {
            billingsData.PatientBillItem.IsSynced = false;
            billingsData.PatientBillItem.AddedOn = GenericMethods.GetUtcDateTime;
            if (billingsData.PatientBillItem.PatientBillID == Guid.Empty)
            {
                billingsData.PatientBillItem.PatientBillID = GenericMethods.GenerateGuid();
                if (MobileConstants.IsMobilePlatform)
                {
                    billingsData.PatientBillItem.AddedById = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                }
            }
            if (GenericMethods.IsListNotEmpty(billingsData.PatientBillItems))
            {
                billingsData.PatientBillItems.ForEach(x =>
                {
                    if (x.PatientBillID == Guid.Empty)
                    {
                        x.PatientBillID = billingsData.PatientBillItem.PatientBillID;
                    }
                });
            }
            if (MobileConstants.IsMobilePlatform)
            {
                billingsData.PatientBillItem.OrganisationID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 0);
                billingsData.PatientBills = new List<PatientBillModel> { billingsData.PatientBillItem };
                await _billingDB.SavePatientBillsAsync(billingsData).ConfigureAwait(false);
                billingsData.ErrCode = ErrorCode.OK;
            }
            else
            {
                await SyncPatientBillToServerAsync(billingsData, CancellationToken.None);

            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            billingsData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
    }

    /// <summary>
    /// Saves Patient Bill To Server
    /// </summary>
    /// <param name="billData">Patient Bill data to return status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status</returns>
    internal async Task SyncPatientBillToServerAsync(BillingItemDTO billData, CancellationToken cancellationToken)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                billData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                await _billingDB.GetPatientBillingsForSyncToServerAsync(billData).ConfigureAwait(true);
            }
            else
            {
                billData.PatientBills = new List<PatientBillModel> { billData.PatientBillItem };
            }
            if (GenericMethods.IsListNotEmpty(billData.PatientBills) || GenericMethods.IsListNotEmpty(billData.PatientBillItems))
            {
                var httpData = new HttpServiceModel<BillingItemDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PATIENT_BILL_ASYNC_PATH,
                    ContentToSend = billData,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() }
                    }
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                billData.ErrCode = httpData.ErrCode;
                if (billData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data?.HasValues == true)
                    {
                        billData.SavePatientBills = MapSaveResponse(data, nameof(BillingItemDTO.SavePatientBills));
                        await SavePatientBillResultsAsync(billData, cancellationToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            billData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Map And Save PaymentModes and PaymentModeI18N
    /// </summary>
    /// <param name="result">sync result</param>
    /// <param name="data">json data from sync response</param>
    /// <returns>operation and status code</returns>
    internal async Task MapAndSavePaymentModes(DataSyncModel result, JToken data)
    {
        try
        {
            PaymentModeDTO paymentData = new PaymentModeDTO();
            if (result.SyncFor == DataSyncFor.PaymentModes.ToString())
            {
                paymentData.BillPaymentModes = MapPaymentModes(data, nameof(DataSyncDTO.BillPaymentModes));
                if (GenericMethods.IsListNotEmpty(paymentData.BillPaymentModes))
                {
                    await _billingDB.SavePaymentModesAsync(paymentData).ConfigureAwait(false);
                    result.RecordCount = paymentData.BillPaymentModes.Count;
                }
            }
            else
            {
                paymentData.PaymentModeI18N = MapPaymentI18N(data, nameof(DataSyncDTO.PaymentModeI18N));
                if (GenericMethods.IsListNotEmpty(paymentData.PaymentModeI18N))
                {
                    await _billingDB.SavePaymentModeI18NAsync(paymentData).ConfigureAwait(false);
                    result.RecordCount = paymentData.PaymentModeI18N.Count;
                }
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Map And Save PatientBills and PatientBill Items
    /// </summary>
    /// <param name="result">sync result</param>
    /// <param name="data">json data from sync response</param>
    /// <returns>operation and status code</returns>
    internal async Task MapAndSavePatientBillsAsync(DataSyncModel result, JToken data)
    {
        try
        {
            BillingItemDTO paymentData = new BillingItemDTO
            {
                PatientBills = MapPatientBills(data, nameof(DataSyncDTO.PatientBills)),
                PatientBillItems = MapPatientBillItems(data, nameof(DataSyncDTO.PatientBillItems))
            };
            if (GenericMethods.IsListNotEmpty(paymentData.PatientBills) || GenericMethods.IsListNotEmpty(paymentData.PatientBillItems))
            {
                await _billingDB.SavePatientBillsAsync(paymentData).ConfigureAwait(false);
                result.RecordCount = 1;
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Map And Save Billing Items
    /// </summary>
    /// <param name="result">sync result</param>
    /// <param name="data">json data from sync response</param>
    /// <returns>operation and status code</returns>
    internal async Task MapAndSaveBillingItemsAsync(DataSyncModel result, JToken data)
    {
        try
        {
            BillingItemDTO billingData = new BillingItemDTO
            {
                BillItems = MapBillingItems(data, nameof(DataSyncDTO.BillingItems))
            };
            if (GenericMethods.IsListNotEmpty(billingData.BillItems))
            {
                await _billingDB.SaveBillItemsAsync(billingData).ConfigureAwait(false);
                result.RecordCount = billingData.BillItems.Count;
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Map And Save Billing Items
    /// </summary>
    /// <param name="result">sync result</param>
    /// <param name="data">json data from sync response</param>
    /// <returns>operation and status code</returns>
    internal async Task MapAndSaveBillingItemsI18N(DataSyncModel result, JToken data)
    {
        try
        {
            BillingItemDTO billingData = new BillingItemDTO
            {
                BillingItemsI18N = MapBillingItemsI18N(data, nameof(DataSyncDTO.BillingItemsI18N))
            };
            if (GenericMethods.IsListNotEmpty(billingData.BillingItemsI18N))
            {
                await _billingDB.SaveBillingItemsI18NAsync(billingData).ConfigureAwait(false);
                result.RecordCount = billingData.BillingItemsI18N.Count;
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Map and save Program Billing Items
    /// </summary>
    /// <param name="result">sync result</param>
    /// <param name="data">json data from sync response</param>
    /// <returns>operation and status code</returns>
    internal async Task MapAndSaveProgramBillingItems(DataSyncModel result, JToken data)
    {
        try
        {
            BillingItemDTO paymentData = new BillingItemDTO
            {
                ProgramBillingItems = MapProgramBillingItems(data, nameof(DataSyncDTO.ProgramBillingItems))
            };
            if (GenericMethods.IsListNotEmpty(paymentData.ProgramBillingItems))
            {
                await _billingDB.SaveProgramBillingItemsAsync(paymentData).ConfigureAwait(false);
                result.RecordCount = paymentData.ProgramBillingItems.Count;
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    private async Task SyncPatientBillsFromServerAsync(BillingItemDTO billData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_PATIENT_BILLS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(billData.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_PATIENT_BILL_ID, Convert.ToString(billData.PatientBillItem.PatientBillID, CultureInfo.InvariantCulture) },
                    { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(billData.SelectedUserID, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            billData.ErrCode = httpData.ErrCode;
            if (billData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(billData, data);
                    SetResourcesAndSettings(billData);
                    MapBillData(billData, data);
                }
            }
        }
        catch (Exception ex)
        {
            billData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapBillData(BillingItemDTO billData, JToken data)
    {
        billData.PatientBills = MapPatientBills(data, nameof(BillingItemDTO.PatientBills));
        billData.PatientBillItems = MapPatientBillItems(data, nameof(BillingItemDTO.PatientBillItems));

        if (billData.PatientBillItem == null || billData.PatientBillItem.PatientBillID != Guid.Empty)
        {
            var patientProviderNoteJData = data[nameof(BillingItemDTO.PatientBillItem)];
            if (patientProviderNoteJData.HasValues)
            {
                billData.PatientBillItem = MapPatientBill(patientProviderNoteJData);
            }
            billData.PatientBillItems.All(x => x.IsSynced = true);
        }

        billData.PatientProgramOptionList = GetPickerSource(data, nameof(BillingItemDTO.PatientProgramOptionList), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), billData.PatientBillItem?.ProgramID ?? 0, false, nameof(OptionModel.ParentOptionID));
        billData.PatientProvidersOptionList = GetPickerSource(data, nameof(BillingItemDTO.PatientProvidersOptionList), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), 0, false, nameof(OptionModel.ParentOptionID));
        billData.PaymentModeOptionList = GetPickerSource(data, nameof(BillingItemDTO.PaymentModeOptionList), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), billData.PatientBillItem?.PaymentModeID??0, false, nameof(OptionModel.ParentOptionID));
        billData.PatientBillingItems = GetPickerSource(data, nameof(BillingItemDTO.PatientBillingItems), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), 0, false, nameof(OptionModel.ParentOptionID));
    }

    private List<PatientBillModel> MapPatientBills(JToken data, string collectionName)
    {
        return data[collectionName].Any()
            ? (from dataItem in data[collectionName]
               select MapPatientBill(dataItem)).ToList()
            : new List<PatientBillModel>();
    }

    private PatientBillModel MapPatientBill(JToken dataItem)
    {
        return new PatientBillModel
        {
            PatientBillID = GetDataItem<Guid>(dataItem, nameof(PatientBillModel.PatientBillID)),
            BillDateTime = GetDataItem<DateTimeOffset?>(dataItem, nameof(PatientBillModel.BillDateTime)),
            Amount = GetDataItem<double>(dataItem, nameof(PatientBillModel.Amount)),
            BillingItemID = GetDataItem<short>(dataItem, nameof(PatientBillModel.BillingItemID)),
            OrganisationID = GetDataItem<long>(dataItem, nameof(PatientBillModel.OrganisationID)),
            OrganisationAddress = GetDataItem<string>(dataItem, nameof(PatientBillModel.OrganisationAddress)),
            OrganisationContact = GetDataItem<string>(dataItem, nameof(PatientBillModel.OrganisationContact)),
            PatientID = GetDataItem<long>(dataItem, nameof(PatientBillModel.PatientID)),
            PatientName = GetDataItem<string>(dataItem, nameof(PatientBillModel.PatientName)),
            ProgramID = GetDataItem<long>(dataItem, nameof(PatientBillModel.ProgramID)),
            ProviderID = GetDataItem<long>(dataItem, nameof(PatientBillModel.ProviderID)),
            Discount = GetDataItem<double>(dataItem, nameof(PatientBillModel.Discount)),
            ProgramName = GetDataItem<string>(dataItem, nameof(PatientBillModel.ProgramName)),
            ProgramColor = GetDataItem<string>(dataItem, nameof(PatientBillModel.ProgramColor)),
            PaymentMode = GetDataItem<string>(dataItem, nameof(PatientBillModel.PaymentMode)),
            PaymentModeID = GetDataItem<short>(dataItem, nameof(PatientBillModel.PaymentModeID)),
            TotalPaid = GetDataItem<double>(dataItem, nameof(PatientBillModel.TotalPaid)),
            GrossTotal = GetDataItem<double>(dataItem, nameof(PatientBillModel.GrossTotal)),
            AddedOn = GetDataItem<DateTimeOffset>(dataItem, nameof(PatientBillModel.AddedOn)),
            IsActive = GetDataItem<bool>(dataItem, nameof(PatientBillModel.IsActive)),
            ErrCode = ErrorCode.OK,
            IsSynced = true,
        };
    }

    private async Task SavePatientBillResultsAsync(BillingItemDTO billData, CancellationToken cancellationToken)
    {
        if (MobileConstants.IsMobilePlatform)
        {
            await _billingDB.UpdatePatientBillsSyncStatusAsync(billData);
        }
        else
        {
            // Map error result to main object as web will call save for single record
            billData.ErrCode = billData.SavePatientBills.FirstOrDefault(x => x.ErrCode != ErrorCode.OK)?.ErrCode ?? ErrorCode.OK;
            if (billData.ErrCode == ErrorCode.DuplicateGuid)
            {
                billData.PatientBillItem.PatientBillID = GenericMethods.GenerateGuid();
                billData.PatientBillItems.ForEach(x => x.PatientBillID = billData.PatientBillItem.PatientBillID);
            }
            else
            {
                //Empty case to handle duplicate Data Error.
            }
        }
        if (billData.ErrCode == ErrorCode.DuplicateGuid)
        {
            billData.ErrCode = ErrorCode.OK;
            await SyncPatientBillToServerAsync(billData, cancellationToken).ConfigureAwait(false);
        }
        billData.RecordCount = billData.PatientBills?.Count ?? 0 + billData.PatientBillItems?.Count ?? 0;
    }

    private List<PaymentModeI18NModel> MapPaymentI18N(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select new PaymentModeI18NModel
               {
                   PaymentModeID = (byte)dataItem[nameof(PaymentModeI18NModel.PaymentModeID)],
                   LanguageID = (byte)dataItem[nameof(PaymentModeI18NModel.LanguageID)],
                   Name = (string)dataItem[nameof(PaymentModeI18NModel.Name)],
                   IsActive = (bool)dataItem[nameof(PaymentModeI18NModel.IsActive)],
               }).ToList()
            : null;
    }

    private List<BillPaymentModel> MapPaymentModes(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
           ? (from dataItem in data[collectionName]
              select new BillPaymentModel
              {
                  PaymentModeID = (byte)dataItem[nameof(BillPaymentModel.PaymentModeID)],
                  IsActive = (bool)dataItem[nameof(BillPaymentModel.IsActive)],
              }).ToList()
           : null;
    }

    private List<PatientBillItemModel> MapPatientBillItems(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
           ? (from dataItem in data[collectionName]
              select new PatientBillItemModel
              {
                  PatientBillID = (Guid)dataItem[nameof(PatientBillItemModel.PatientBillID)],
                  BillingItemID = (short)dataItem[nameof(PatientBillItemModel.BillingItemID)],
                  Amount = (double)dataItem[nameof(PatientBillItemModel.Amount)],
                  IsActive = (bool)dataItem[nameof(PatientBillItemModel.IsActive)],
                  IsSynced = true,
              }).ToList()
           : null;
    }

    private List<BillingItemsI18NModel> MapBillingItemsI18N(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
           ? (from dataItem in data[collectionName]
              select new BillingItemsI18NModel
              {
                  BillingItemID = (short)dataItem[nameof(BillingItemsI18NModel.BillingItemID)],
                  LanguageID = (byte)dataItem[nameof(BillingItemsI18NModel.LanguageID)],
                  Name = (string)dataItem[nameof(BillingItemsI18NModel.Name)],
                  IsActive = (bool)dataItem[nameof(BillingItemsI18NModel.IsActive)],
              }).ToList()
           : null;
    }

    private List<BillItemModel> MapBillingItems(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
           ? (from dataItem in data[collectionName]
              select new BillItemModel
              {
                  BillingItemID = (short)dataItem[nameof(BillItemModel.BillingItemID)],
                  IsActive = (bool)dataItem[nameof(BillItemModel.IsActive)],
              }).ToList()
           : null;
    }

    private List<ProgramBillingModel> MapProgramBillingItems(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
          ? (from dataItem in data[collectionName]
             select new ProgramBillingModel
             {
                 ProgramBillingItemID = (long)dataItem[nameof(ProgramBillingModel.ProgramBillingItemID)],
                 ProgramID = (long)dataItem[nameof(ProgramBillingModel.ProgramID)],
                 OrganisationID = (long)dataItem[nameof(ProgramBillingModel.OrganisationID)],
                 BillingItemID = (short)dataItem[nameof(ProgramBillingModel.BillingItemID)],
                 Amount = (double)dataItem[nameof(ProgramBillingModel.Amount)],
                 IsActive = (bool)dataItem[nameof(ProgramBillingModel.IsActive)],
             }).ToList()
          : null;
    }

    private Dictionary<string, string> GetPatientDetails(List<ResourceModel> resources, string programTitle, string doctorName, string patientName, string billDate)
    {
        return new Dictionary<string, string>
        {
            {
                GetResourceValue(resources,ResourceConstants.R_PROGRAM_TITLE_KEY), programTitle
            },
            {
                GetResourceValue(resources,ResourceConstants.R_DOCTOR_NAME_KEY), doctorName
            },
            {
                GetResourceValue(resources,ResourceConstants.R_PATIENT_KEY), patientName
            },
            {
                GetResourceValue(resources,ResourceConstants.R_ENTER_DATE_KEY),  billDate
            }
        };
    }

    private string GetResourceValue(List<ResourceModel> resources, string resourceKey)
    {
        var result = resources.Find(x => x.ResourceKey.Trim() == resourceKey);
        if (result != null)
        {
            return result?.ResourceValue.Trim();
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Get Data from Printing Bill
    /// </summary>
    /// <param name="patientBillData">Billing Data</param>
    /// <param name="OrganisationLogo">Organisation Logo</param>
    /// <param name="OrganisationName">Organisation Name</param>
    /// <returns>HTML Data, ErrorCode and FileName</returns>
    public (string, ErrorCode, string) GetPrintData(BillingItemDTO patientBillData, string OrganisationLogo, string OrganisationName)
    {
        try
        {
            string programTitle = MobileConstants.IsMobilePlatform ? patientBillData.PatientBillItem.ProgramName : patientBillData.PatientProgramOptionList.Find(x => x.OptionID == patientBillData.PatientBillItem.ProgramID).OptionText;
            string doctorName = MobileConstants.IsMobilePlatform ? patientBillData.PatientBillItem.ProviderName : patientBillData.PatientProvidersOptionList.Find(x => x.OptionID == patientBillData.PatientBillItem.ProviderID).OptionText;
            string billDate = MobileConstants.IsMobilePlatform ? patientBillData.PatientBillItem.BillDateTimeString : patientBillData.PatientBillItem.BillDateTimeString;

            Dictionary<string, string> details = GetPatientDetails(patientBillData.Resources, programTitle, doctorName, patientBillData.PatientBillItem.PatientName, billDate);

            List<TableModel> tableData = new List<TableModel>();
            foreach (var item in patientBillData.PatientBillItems.Where(x => x.IsActive))
            {
                tableData.Add(new TableModel
                {
                    Item = MobileConstants.IsMobilePlatform ? item.Name : patientBillData.PatientBillingItems.Find(x => x.OptionID == item.BillingItemID).OptionText,
                    Amount = item.Amount.ToString()
                });
            }
            tableData.Add(new TableModel
            {
                Item = GetResourceValue(patientBillData.Resources, ResourceConstants.R_TOTAL_AMOUNT_KEY),
                Amount = Convert.ToString(patientBillData.PatientBillItem.GrossTotal, CultureInfo.InvariantCulture)
            });
            tableData.Add(new TableModel
            {
                Item = GetResourceValue(patientBillData.Resources, ResourceConstants.R_DISCOUNT_KEY),
                Amount = Convert.ToString(patientBillData.PatientBillItem.Discount, CultureInfo.InvariantCulture)
            });
            tableData.Add(new TableModel
            {
                Item = GetResourceValue(patientBillData.Resources, ResourceConstants.R_AMOUNT_PAID_KEY),
                Amount = Convert.ToString(patientBillData.PatientBillItem.TotalPaid, CultureInfo.InvariantCulture)
            });
            tableData.Add(new TableModel
            {
                Item = GetResourceValue(patientBillData.Resources, ResourceConstants.R_PAYMENT_MODE_NAME_KEY),
                Amount = patientBillData.PatientBillItem.PaymentMode
            });

            List<string> OrgDetails = new List<string>
            {
                OrganisationLogo,
                OrganisationName,
                patientBillData.PatientBillItem.OrganisationAddress,
                patientBillData.PatientBillItem.OrganisationContact
            };

            string filename = patientBillData.PatientBillItem.PatientBillID.ToString();
            return (new HtmlDataService(_essentials).CreateHtml(filename, OrgDetails, details, tableData), ErrorCode.OK, filename);
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            return (string.Empty, ErrorCode.InvalidData, string.Empty);
        }
    }
}
