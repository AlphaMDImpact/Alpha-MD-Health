using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Web;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    class TextLocalOperations : BaseService, ICommunicationLibrary
	{
		private readonly LibraryServiceModel _libraryServiceData;

		public TextLocalOperations(LibraryServiceModel libraryServiceData)
		{
			_libraryServiceData = libraryServiceData;
		}

		/// <summary>
		/// Send sms
		/// </summary>
		/// <param name="communicationData">data required to send sms</param>
		public Task SendEmailAsync(CommunicationDTO communicationData)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Send sms
		/// </summary>
		/// <param name="communicationData">data to send sms</param>
		/// <returns>Operation status code</returns>
		public async Task SendSmsAsync(CommunicationDTO communicationData)
		{
			communicationData.PhoneNumber = CreatePhoneNoForSms(Constants.SYMBOL_DASH, communicationData.PhoneNumber);
			_libraryServiceData.ServiceTarget = "https://api.textlocal.in/send/";
			_libraryServiceData.ServiceClientIdentifier = "SjS/UbBhnlo-9iNzEPKtpALbNcX1caxd0f021y3Bgd";
			communicationData.SenderId = "TXTLCL";

			////communicationData.SmsProviderDomain = "https://api.textlocal.in/send/";
			////communicationData.SmsApiKeyId = "SjS/UbBhnlo-9iNzEPKtpALbNcX1caxd0f021y3Bgd";
			////communicationData.SmsSenderId = "TXTLCL";

			using HttpClient httpClient = new HttpClient(new HttpClientHandler());
			NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
			query.Add("apikey", _libraryServiceData.ServiceClientIdentifier);
			query.Add("numbers", communicationData.PhoneNumber);//"918169423625");// 
			query.Add("message", HttpUtility.UrlEncode(communicationData.MessageBody));
			query.Add("sender", communicationData.SenderId);
			HttpRequestMessage smsRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_libraryServiceData.ServiceTarget + Constants.SYMBOL_QUESTION_MARK + query));
			HttpResponseMessage smsResponse = await httpClient.SendAsync(smsRequest).ConfigureAwait(false);
			if (smsResponse.IsSuccessStatusCode)
			{
				string smsResultString = await smsResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
				if (!string.IsNullOrWhiteSpace(smsResultString))
				{
					JToken smsResult = JToken.Parse(smsResultString);
					if ((string)smsResult["status"] == "success")
					{
						communicationData.ErrCode = ErrorCode.OK;
						return;
					}
				}
			}
			communicationData.ErrCode = ErrorCode.BadRequest;
			communicationData.PhoneNumber = communicationData.SenderId = communicationData.MessageBody = string.Empty;
		}

		private static string CreatePhoneNoForSms(char seperator, string phoneNumber)
		{
			if (string.IsNullOrWhiteSpace(phoneNumber))
			{
				return string.Empty;
			}
			int index = phoneNumber.IndexOf(seperator) + 1;
			return phoneNumber.Substring(index, phoneNumber.Length - index);
		}

		/// <summary>
		/// Send notification
		/// </summary>
		/// <param name="communicationData">data to send notification</param>
		public Task SendNotificationAsync(NotificationDTO communicationData)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Register device for notification
		/// </summary>
		/// <param name="communicationData">data to register device</param>
		public Task RegisterDeviceForNotificationAsync(NotificationDTO communicationData)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Send WhatsApp message
		/// </summary>
		/// <param name="communicationData">Message data and detials</param>
		/// <returns></returns>
		public Task SendWhatsAppMessageAsync(CommunicationDTO communicationData)
		{
			throw new NotImplementedException();
		}
	}
}
