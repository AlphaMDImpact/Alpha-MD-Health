using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class ReportsService : BaseService
	{
        public ReportsService(IEssentials serviceEssentials) : base(serviceEssentials)
        {
            
        }
        /// <summary>
        /// Sync bills from service
        /// </summary>
        /// <param name="billingData">filter data</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>list of bills and operation status</returns>
        public async Task SyncBillsFromServerAsync(BillingItemDTO billingData, CancellationToken cancellationToken)
		{
			try
			{
				var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
				{
					CancellationToken = cancellationToken,
					PathWithoutBasePath = UrlConstants.GET_BILLS_ASYNC,
					QueryParameters = new NameValueCollection
					{
						{ Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
						{ nameof(billingData.FromDate),  billingData.FromDate },
						{ nameof(billingData.ToDate),  billingData.ToDate },
						{ Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(billingData.RecordCount, CultureInfo.InvariantCulture) },
					}
				};
				await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
				billingData.ErrCode = httpData.ErrCode;
				if (MobileConstants.IsMobilePlatform)
				{
					await GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_USER_PROFILE_PAGE_GROUP, GroupConstants.RS_BILLING_GROUP, GroupConstants.RS_REPORT_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP).ConfigureAwait(false);
					await GetSettingsAsync(GroupConstants.RS_ORGANISATION_SETTINGS_GROUP).ConfigureAwait(false);
					billingData.Resources = PageData.Resources;
					billingData.Settings = PageData.Settings;
				}
				if (billingData.ErrCode == ErrorCode.OK)
				{
					JToken data = JToken.Parse(httpData.Response);
					if (data != null && data.HasValues)
					{
						MapCommonData(billingData, data);
						MapBillsData(data, billingData);
						if (MobileConstants.IsMobilePlatform && billingData.PatientBills != null)
						{
							foreach (var billModel in billingData.PatientBills)
							{
								billModel.CurrentStatus = StatusColor(billModel.Status);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				billingData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
				LogError(ex.Message, ex);
			}
		}

		private void MapBillsData(JToken data, BillingItemDTO billingData)
        {
            LibSettings.TryGetDateFormatSettings(billingData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            billingData.PatientBills = (data[nameof(BillingItemDTO.PatientBills)]?.Count() > 0)
				? (from dataItem in data[nameof(BillingItemDTO.PatientBills)]
				   select new PatientBillModel
				   {
					   PatientBillID = (Guid)dataItem[nameof(PatientBillModel.PatientBillID)],
					   BillDateTime = (DateTimeOffset)dataItem[nameof(PatientBillModel.BillDateTime)],
					   PatientName = (string)dataItem[nameof(PatientBillModel.PatientName)],
					   ProviderName = (string)dataItem[nameof(PatientBillModel.ProviderName)],
					   PatientID = (int)dataItem[nameof(PatientBillModel.PatientID)],
					   ProgramName = (string)dataItem[nameof(PatientBillModel.ProgramName)],
					   OrganisationName = (string)dataItem[nameof(PatientBillModel.OrganisationName)],
					   GrossTotal = (double)dataItem[nameof(PatientBillModel.GrossTotal)],
					   Discount = (double)dataItem[nameof(PatientBillModel.Discount)],
					   TotalPaid = (double)dataItem[nameof(PatientBillModel.TotalPaid)],
					   PaymentMode = (string)dataItem[nameof(PatientBillModel.PaymentMode)],
					   IsActive = (bool)dataItem[nameof(PatientBillModel.IsActive)],
					   Status = Status((bool)dataItem[nameof(PatientBillModel.IsActive)]),
					   CurrentStatus = GetStatus((bool)dataItem[nameof(PatientBillModel.IsActive)]),
					   BillDateTimeString = GenericMethods.GetLocalDateTimeBasedOnCulture((DateTimeOffset)dataItem[nameof(PatientBillModel.BillDateTime)], DateTimeType.Date, dayFormat, monthFormat, yearFormat),
					   ProgramColor = (string)dataItem[nameof(PatientBillModel.ProgramColor)],
				   }).ToList()
				 : null;
			billingData.ErrCode = (ErrorCode)(int)data[nameof(BillingItemDTO.ErrCode)];
			MapDescriptionForMobile(billingData);
		}

		private void MapDescriptionForMobile(BillingItemDTO billingData)
		{
			if (billingData.PatientBills != null)
			{
				foreach (var x in billingData.PatientBills)
				{
					x.DescriptionInMobile = string.Concat(LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_AMOUNT_PAID_KEY) + ":" + x.TotalPaid + "/-" + " | " + LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_PAYMENT_MODE_NAME_KEY) + ":" + x.PaymentMode);
				}
			}
		}
		private string GetStatus(bool isActiveStatus)
		{
			string currentStatus = isActiveStatus ? Constants.ACTIVE_VARIABLE : Constants.INACTIVE_VARIABLE;
			string statusStyle;
			if (isActiveStatus)
			{
				statusStyle = "badge-done ";
			}
			else
			{
				statusStyle = "badge-error ";
			}
			return $"<label class ={statusStyle}>&nbsp{currentStatus}</label>";
		}

		private string StatusColor(string status)
		{
			switch (status)
			{
				case "Active":
					return StyleConstants.SUCCESS_COLOR;
				case "InActive":
					return StyleConstants.ERROR_COLOR;
				default:
					return "";
			}
		}

		private string Status(bool isActiveStatus)
		{
			string currentStatus = isActiveStatus ? Constants.ACTIVE_VARIABLE : Constants.INACTIVE_VARIABLE;
			return currentStatus;
		}
	}
}