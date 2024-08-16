using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class CommunicationLibrary : LibraryService
	{
		private readonly ICommunicationLibrary _communication;

		public CommunicationLibrary(HttpContext httpContext) : base(httpContext)
		{
			switch (_libraryServiceData.LibraryInfo.ServiceType)
			{
				case ServiceType.SendGrid:
					_communication = new SendGridOperations(_libraryServiceData.LibraryInfo);
					break;
				case ServiceType.TextLocal:
					_communication = new TextLocalOperations(_libraryServiceData.LibraryInfo);
					break;
				case ServiceType.AlertInSMS:
					_communication = new AlertinSMSOperations(_libraryServiceData.LibraryInfo);
					break;
				case ServiceType.InfoBip:
					_communication = new InfoBipOperations(_libraryServiceData.LibraryInfo);
					break;
				case ServiceType.AzureNotification:
					_communication = new AzureNotificationOperations(_libraryServiceData.LibraryDetails);
					break;
				default:
					return;
			}
		}

		/// <summary>
		/// Send sms
		/// </summary>
		/// <param name="communicationData">data to send sms</param>
		/// <returns>Operation status code</returns>
		public async Task<CommunicationDTO> SendSmsMessageAsync(CommunicationDTO communicationData)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(communicationData.PhoneNumber) || string.IsNullOrWhiteSpace(communicationData.MessageBody))
				{
					communicationData.ErrCode = ErrorCode.InvalidData;
					return communicationData;
				}
				if (_libraryServiceData?.LibraryInfo != null)
				{
					await _communication.SendSmsAsync(communicationData).ConfigureAwait(true);
					if (_libraryServiceData.LibraryInfo.LogCalls)
					{
						_ = SaveServiceCallLogsAsync(_libraryServiceData.LibraryInfo.OrganisationServiceID, communicationData.ErrCode.ToString(), communicationData.ErrorDescription).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				communicationData.ErrCode = ErrorCode.BadRequest;
				communicationData.ErrorDescription = ex.Message;
				LogError(ex.Message, ex.StackTrace);
			}
			return communicationData;
		}

		/// <summary>
		/// Send email
		/// </summary>
		/// <param name="communicationData">data to send email</param>
		/// <returns>Operation status code</returns>
		public async Task<CommunicationDTO> SendEmailMessageAsync(CommunicationDTO communicationData)
		{
			try
			{
				if (communicationData.ToIds == null || communicationData.ToIds.Count < 1
					|| string.IsNullOrWhiteSpace(communicationData.FromId) || string.IsNullOrWhiteSpace(communicationData.MessageBody))
				{
					communicationData.ErrCode = ErrorCode.InvalidData;
					return communicationData;
				}
				if (_libraryServiceData?.LibraryInfo != null && communicationData.ApplicationName == _libraryServiceData.LibraryInfo.ForApplication)
				{
					await _communication.SendEmailAsync(communicationData).ConfigureAwait(true);
					if (_libraryServiceData.LibraryInfo.LogCalls)
					{
						_ = SaveServiceCallLogsAsync(_libraryServiceData.LibraryInfo.OrganisationServiceID, communicationData.ErrCode.ToString(), communicationData.ErrorDescription).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				communicationData.ErrCode = ErrorCode.BadRequest;
				communicationData.ErrorDescription = ex.Message;
				LogError(ex.Message, ex.StackTrace);
			}
			return communicationData;
		}

		/// <summary>
		/// Register device for notification
		/// </summary>
		/// <param name="communicationData">data to register device for notification</param>
		/// <returns>Operation status code</returns>
		public async Task<NotificationDTO> RegisterDeviceForNotificationAsync(NotificationDTO communicationData)
		{
			try
			{
				if (communicationData == null || communicationData.NotificationData == null || communicationData.NotificationData.HubID < 1
					|| string.IsNullOrWhiteSpace(communicationData.NotificationData.UserName) || string.IsNullOrWhiteSpace(communicationData.NotificationData.Tags))
				{
					communicationData.ErrCode = ErrorCode.InvalidData;
					return communicationData;
				}
				if (_libraryServiceData?.LibraryInfo != null && communicationData.ForApplication == _libraryServiceData.LibraryInfo.ForApplication)
				{
					await _communication.RegisterDeviceForNotificationAsync(communicationData).ConfigureAwait(true);
					if (_libraryServiceData.LibraryInfo.LogCalls)
					{
						_ = SaveServiceCallLogsAsync(_libraryServiceData.LibraryInfo.OrganisationServiceID, communicationData.ErrCode.ToString(), communicationData.ErrorDescription).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				communicationData.ErrCode = ErrorCode.BadRequest;
				communicationData.ErrorDescription = ex.Message;
				LogError(ex.Message, ex.StackTrace);
			}
			return communicationData;
		}

		/// <summary>
		/// Send notification
		/// </summary>
		/// <param name="communicationData">data to send notification</param>
		/// <returns>Operation status code</returns>
		public async Task<NotificationDTO> SendNotificationsMessageAsync(NotificationDTO communicationData)
		{
			try
			{
				if (communicationData == null || communicationData.NotificationMessage == null || communicationData.NotificationMessage.HubID < 1)
				{
					communicationData.ErrCode = ErrorCode.InvalidData;
					return communicationData;
				}
				if (_libraryServiceData?.LibraryInfo != null && communicationData.ForApplication == _libraryServiceData.LibraryInfo.ForApplication)
				{
					await _communication.SendNotificationAsync(communicationData).ConfigureAwait(true);
					if (_libraryServiceData.LibraryInfo.LogCalls)
					{
						_ = SaveServiceCallLogsAsync(_libraryServiceData.LibraryInfo.OrganisationServiceID, communicationData.ErrCode.ToString(), communicationData.ErrorDescription).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				communicationData.ErrCode = ErrorCode.BadRequest;
				communicationData.ErrorDescription = ex.Message;
				LogError(ex.Message, ex.StackTrace);
			}
			return communicationData;
		}

		/// <summary>
		/// Send WhatsApp message
		/// </summary>
		/// <param name="communicationData">Message data and detials</param>
		/// <returns></returns>
		public async Task<CommunicationDTO> SendWhatsAppMessageAsync(CommunicationDTO communicationData)
		{
			try
			{
				if (communicationData.ToIds == null || communicationData.ToIds.Count < 1
					|| string.IsNullOrWhiteSpace(communicationData.PhoneNumber) || string.IsNullOrWhiteSpace(communicationData.MessageBody))
				{
					communicationData.ErrCode = ErrorCode.InvalidData;
					return communicationData;
				}
				if (_libraryServiceData?.LibraryInfo != null && communicationData.ApplicationName == _libraryServiceData.LibraryInfo.ForApplication)
				{
					await _communication.SendWhatsAppMessageAsync(communicationData).ConfigureAwait(true);
					if (_libraryServiceData.LibraryInfo.LogCalls)
					{
						_ = SaveServiceCallLogsAsync(_libraryServiceData.LibraryInfo.OrganisationServiceID, communicationData.ErrCode.ToString(), communicationData.ErrorDescription).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				communicationData.ErrCode = ErrorCode.BadRequest;
				communicationData.ErrorDescription = ex.Message;
				LogError(ex.Message, ex.StackTrace);
			}
			return communicationData;
		}
	}
}