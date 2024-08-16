using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public class HealthScanService : BaseService
{
    public HealthScanService(IEssentials essentials) : base(essentials)
    {

    }

    /// <summary>
    /// Get healthScan data based on ID
    /// </summary>
    /// <param name="healthScanData">healthScanData data</param>
    /// <returns>Consent details with operation status</returns>
    public async Task GetHealthScansAsync(HealthScanDTO healthScanData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform){}
            else
            {
                await SyncHealthScansFromServerAsync(healthScanData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            healthScanData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private async Task SyncHealthScansFromServerAsync(HealthScanDTO healthScanData)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                PathWithoutBasePath = UrlConstants.GET_HEALTH_SCANS_ASYNC,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },                       
                    { nameof(BaseDTO.RecordCount), Convert.ToString(healthScanData.RecordCount, CultureInfo.InvariantCulture) },
                    { nameof(HealthScanModel.TransactionID), Convert.ToString(healthScanData.ExternalServiceTransaction?.TransactionID ?? 0, CultureInfo.InvariantCulture) },                                       
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            healthScanData.ErrCode = httpData.ErrCode;
            if (healthScanData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(healthScanData, data);
                    MapHealthScansData(data, healthScanData);
                }
            }
        }
        catch (Exception ex)
        {
            healthScanData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapHealthScansData(JToken data, HealthScanDTO healthScanData)
    {
        SetPageResources(healthScanData.Resources);
        var dayFormat = LibSettings.GetSettingValueByKey(healthScanData?.Settings, SettingsConstants.S_DATE_DAY_FORMAT_KEY);
        var monthFormat = LibSettings.GetSettingValueByKey(healthScanData?.Settings, SettingsConstants.S_MONTH_FORMAT_KEY);
        var yearFormat = LibSettings.GetSettingValueByKey(healthScanData?.Settings, SettingsConstants.S_YEAR_FORMAT_KEY);
        if (healthScanData.RecordCount == -1)
        {         
            healthScanData.ExternalServiceTransaction = MapCareFlixHealthScan(data[nameof(HealthScanDTO.ExternalServiceTransaction)]);
            healthScanData.ExternalServiceTransaction.TransactionDateTime = _essentials.ConvertToLocalTime(healthScanData.ExternalServiceTransaction.TransactionDateTime.Value);
        }
        else if(healthScanData.RecordCount == 0)
        {
            healthScanData.ExternalServiceTransactions = MapHealthScans(data, nameof(healthScanData.ExternalServiceTransactions));
            healthScanData.OrganisationCredits = GetDataItem<int>(data, nameof(HealthScanDTO.OrganisationCredits));
            healthScanData.CreditsAssigned = GetDataItem<int>(data, nameof(HealthScanDTO.CreditsAssigned));
            healthScanData.CreditsAvailable = GetDataItem<int>(data, nameof(HealthScanDTO.CreditsAvailable));
            healthScanData.NumberOfPatient = GetDataItem<int>(data, nameof(HealthScanDTO.NumberOfPatient));
        }

        if (GenericMethods.IsListNotEmpty(healthScanData.ExternalServiceTransactions))
        {
            foreach (var healthScan in healthScanData.ExternalServiceTransactions)
            {
                healthScan.Discount = healthScan.DiscountPercentage.HasValue ? healthScan.DiscountPercentage.Value.ToString() + "%" : "";
                healthScan.TrasactionDateTimeValue = GenericMethods.GetLocalDateTimeBasedOnCulture(healthScan.TransactionDateTime.Value, DateTimeType.DateTime, dayFormat, monthFormat, yearFormat);
            }
        }
    }

    private List<HealthScanModel> MapHealthScans(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
           ? (from dataItem in data[collectionName]
              select new HealthScanModel
              {
                  TransactionID = GetDataItem<long>(dataItem, nameof(HealthScanModel.TransactionID)),
                  IsPatient = GetDataItem<bool>(dataItem, nameof(HealthScanModel.IsPatient)),
                  MinimumQuantityToBuy = GetDataItem<int>(dataItem, nameof(HealthScanModel.MinimumQuantityToBuy)),
                  Quantity = GetDataItem<int>(dataItem, nameof(HealthScanModel.Quantity)),
                  TransactionDateTime = GetDataItem<DateTimeOffset>(dataItem, nameof(HealthScanModel.TransactionDateTime)),
                  DiscountPercentage = GetDataItem<double>(dataItem, nameof(HealthScanModel.DiscountPercentage)),
                  DiscountPrice = GetDataItem<double>(dataItem, nameof(HealthScanModel.DiscountPrice)),
                  TotalPrice = GetDataItem<double>(dataItem, nameof(HealthScanModel.TotalPrice)),
                  UnitPrice = GetDataItem<double>(dataItem, nameof(HealthScanModel.UnitPrice))
              }).ToList()
           : null;
    }

    private HealthScanModel MapCareFlixHealthScan(JToken dataItem)
    {
        return dataItem.HasValues ? new HealthScanModel
        {
            TransactionID = GetDataItem<long>(dataItem, nameof(HealthScanModel.TransactionID)),
            IsPatient = GetDataItem<bool>(dataItem, nameof(HealthScanModel.IsPatient)),
            MinimumQuantityToBuy = GetDataItem<int>(dataItem, nameof(HealthScanModel.MinimumQuantityToBuy)),
            Quantity = GetDataItem<int>(dataItem, nameof(HealthScanModel.Quantity)),
            TransactionDateTime = GetDataItem<DateTimeOffset>(dataItem, nameof(HealthScanModel.TransactionDateTime)),
            DiscountPercentage = GetDataItem<double>(dataItem, nameof(HealthScanModel.DiscountPercentage)),
            DiscountPrice = GetDataItem<double>(dataItem, nameof(HealthScanModel.DiscountPrice)),
            TotalPrice = GetDataItem<double>(dataItem, nameof(HealthScanModel.TotalPrice)),
            UnitPrice = GetDataItem<double>(dataItem, nameof(HealthScanModel.UnitPrice))
        } : new HealthScanModel();
    }

    public async Task SyncHealthScansToServerAsync(HealthScanDTO careFlixHealthScanData, CancellationToken cancellationToken, bool isAssignScanPage)
    {
        try
        {
            var httpData = new HttpServiceModel<HealthScanDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_HEALTH_SCANS_ASYNC,               
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = careFlixHealthScanData,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            careFlixHealthScanData.ErrCode = httpData.ErrCode;          
        }
        catch (Exception ex)
        {
            careFlixHealthScanData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }
}