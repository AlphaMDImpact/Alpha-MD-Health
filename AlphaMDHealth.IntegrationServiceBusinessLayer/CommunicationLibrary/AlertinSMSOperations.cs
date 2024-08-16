using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceConfiguration;
using AlphaMDHealth.Utility;
using System.Collections.Specialized;
using System.Net;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    class AlertinSMSOperations : BaseService, ICommunicationLibrary
	{
		private readonly LibraryServiceModel _libraryServiceData;

		public AlertinSMSOperations(LibraryServiceModel libraryServiceData)
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
		/// Send sms using AlertIn Service
		/// </summary>
		/// <param name="communicationData">SMS data to send</param>
		/// <returns>Operation status code</returns>
		public async Task SendSmsAsync(CommunicationDTO communicationData)
		{
			if (!communicationData.MessageSubject.EndsWith("EN", StringComparison.InvariantCultureIgnoreCase))
			{
				_libraryServiceData.ServiceTarget = _libraryServiceData.ServiceTarget + "//sendunicodesms";
			}
			else
			{
				_libraryServiceData.ServiceTarget = _libraryServiceData.ServiceTarget + "//sendsms";
			}
			communicationData.PhoneNumber = CreatePhoneNoForSms(Constants.SYMBOL_DASH, communicationData.PhoneNumber);
			var myConfig = MyConfiguration.GetInstance;
			using (var webClient = new WebClient())
			{
				byte[] response = webClient.UploadValues(_libraryServiceData.ServiceTarget, new NameValueCollection(){
								{"uname", myConfig.GetConfigurationValue(ConfigurationConstants.CS_SMS_ACCOUNT_NAME)},
								{"pwd", myConfig.GetConfigurationValue(ConfigurationConstants.CS_SMS_ACCOUNT_PASSWORD)},
								{"senderid", communicationData.MessageSubject},
								{"to", communicationData.PhoneNumber},
								{"msg", communicationData.MessageBody},
								{"route", "T"},
								{ "peid", myConfig.GetConfigurationValue(ConfigurationConstants.CS_SMS_PE_ID)},
								{"tempid", communicationData.FromId}
						});
				string result = System.Text.Encoding.UTF8.GetString(response);
				if (long.TryParse(result, out _))
				{
					communicationData.ErrCode = ErrorCode.OK;
				}
				else
				{
					communicationData.ErrCode = ErrorCode.BadRequest;
				}
				communicationData.PhoneNumber = communicationData.SenderId = communicationData.MessageBody = string.Empty;
			}
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