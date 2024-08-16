using AlphaMDHealth.Model;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    interface ICommunicationLibrary
	{
		/// <summary>
		/// Send sms
		/// </summary>
		/// <param name="communicationData">data required to send sms</param>
		Task SendSmsAsync(CommunicationDTO communicationData);

		/// <summary>
		/// Send email
		/// </summary>
		/// <param name="communicationData">data required to send email</param>
		Task SendEmailAsync(CommunicationDTO communicationData);

		/// <summary>
		/// Send notification
		/// </summary>
		/// <param name="communicationData">data required to send notification</param>
		Task SendNotificationAsync(NotificationDTO communicationData);

		/// <summary>
		/// Register device for notification
		/// </summary>
		/// <param name="communicationData">data required to register device</param>
		Task RegisterDeviceForNotificationAsync(NotificationDTO communicationData);

		/// <summary>
		/// Send WhatsApp message
		/// </summary>
		/// <param name="communicationData">Message data and detials</param>
		/// <returns></returns>
		Task SendWhatsAppMessageAsync(CommunicationDTO communicationData);
	}
}
