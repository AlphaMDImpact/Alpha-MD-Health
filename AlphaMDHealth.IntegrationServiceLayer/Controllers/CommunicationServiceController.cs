using AlphaMDHealth.IntegrationServiceBusinessLayer;
using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.IntegrationServiceLayer.Controllers
{
    [Route("api/CommunicationService")]
	[ApiController]
	public class CommunicationServiceController : BaseController
	{
		/// <summary>
		/// Send sms
		/// </summary>
		/// <param name="communicationData">data to send sms</param>
		/// <returns>Operation status code</returns>
		[Route("SendSmsMessageAsync")]
		[HttpPost]
		public async Task<IActionResult> SendSmsMessageAsync([FromBody] CommunicationDTO communicationData)
		{
			return HttpActionResult(await new CommunicationLibrary(HttpContext).SendSmsMessageAsync(communicationData), 1);
		}

		/// <summary>
		/// Send email
		/// </summary>
		/// <param name="communicationData">data to send email</param>
		/// <returns>Operation status code</returns>
		[Route("SendEmailMessageAsync")]
		[HttpPost]
		public async Task<CommunicationDTO> SendEmailMessageAsync(CommunicationDTO communicationData)
		{
			return await new CommunicationLibrary(HttpContext).SendEmailMessageAsync(communicationData).ConfigureAwait(false);
		}

		/// <summary>
		/// Send email
		/// </summary>
		/// <param name="communicationData">data to send email</param>
		/// <returns>Operation status code</returns>
		[Route("SendWhatsAppMessageAsync")]
		[HttpPost]
		public async Task<CommunicationDTO> SendWhatsAppMessageAsync(CommunicationDTO communicationData)
		{
			return await new CommunicationLibrary(HttpContext).SendWhatsAppMessageAsync(communicationData).ConfigureAwait(false);
		}

		/// <summary>
		/// Send notification
		/// </summary>
		/// <param name="communicationData">data to send email</param>
		/// <returns>Operation status code</returns>
		[Route("SendNotificationsMessageAsync")]
		[HttpPost]
		public async Task<NotificationDTO> SendNotificationsMessageAsync(NotificationDTO communicationData)
		{
			return await new CommunicationLibrary(HttpContext).SendNotificationsMessageAsync(communicationData).ConfigureAwait(false);
		}

		/// <summary>
		/// Register device for notification
		/// </summary>
		/// <param name="communicationData">data to register device</param>
		/// <returns>Operation status code</returns>
		[Route("RegisterDeviceForNotificationAsync")]
		[HttpPost]
		public async Task<NotificationDTO> RegisterDeviceForNotificationAsync(NotificationDTO communicationData)
		{
			return await new CommunicationLibrary(HttpContext).RegisterDeviceForNotificationAsync(communicationData).ConfigureAwait(false);
		}
	}
}