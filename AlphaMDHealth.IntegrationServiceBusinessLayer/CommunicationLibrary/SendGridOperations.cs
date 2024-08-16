using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    class SendGridOperations : BaseService, ICommunicationLibrary
	{
		private readonly LibraryServiceModel _libraryServiceData;
		public SendGridOperations(LibraryServiceModel libraryServiceData)
		{
			_libraryServiceData = libraryServiceData;
		}

		/// <summary>
		/// Send email
		/// </summary>
		/// <param name="communicationData">data to send email</param>
		/// <returns>Operation status</returns>
		public async Task SendEmailAsync(CommunicationDTO communicationData)
		{
			string apiKey = _libraryServiceData.ServiceClientSecrete;
			SendGridMessage msg = new SendGridMessage
			{
				From = new EmailAddress(communicationData.FromId),
				Subject = communicationData.MessageSubject,
				PlainTextContent = communicationData.MessageBody,
				HtmlContent = communicationData.MessageBody,
			};
			if (communicationData.ToIds.Any())
			{
				msg.AddTos(EmailIdList(communicationData.ToIds));
			}
			if (communicationData.BccIds?.Count > 0)
			{
				msg.AddBccs(EmailIdList(communicationData.BccIds));
			}
			if (communicationData.CcIds?.Count > 0)
			{
				msg.AddCcs(EmailIdList(communicationData.CcIds));
			}
			//Send File as attachment
			if (communicationData.MessageAttachments?.Count > 0)
			{
				msg.AddAttachments(GetAttachments(communicationData.MessageAttachments));
			}
			var response = await new SendGridClient(apiKey).SendEmailAsync(msg).ConfigureAwait(false);
			communicationData.ErrCode = response.StatusCode == HttpStatusCode.Accepted ? ErrorCode.OK : ErrorCode.BadRequest;
		}

		/// <summary>
		/// Send Sms
		/// </summary>
		/// <param name="communicationData">data required to send sms</param>
		public Task SendSmsAsync(CommunicationDTO communicationData)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Send notification
		/// </summary>
		/// <param name="communicationData">data required to send notification</param>
		public Task SendNotificationAsync(NotificationDTO communicationData)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Register device for notification
		/// </summary>
		/// <param name="communicationData">data required for device registration</param>
		public Task RegisterDeviceForNotificationAsync(NotificationDTO communicationData)
		{
			throw new NotImplementedException();
		}

		private List<EmailAddress> EmailIdList(IEnumerable<string> encryptedIds)
		{
			List<EmailAddress> emailIds = new List<EmailAddress>();
			foreach (string emailId in encryptedIds)
			{
				emailIds.Add(new EmailAddress(emailId));
			}
			return emailIds;
		}

		private List<Attachment> GetAttachments(List<FileDataModel> attachmentsData)
		{
			List<Attachment> attachments = new List<Attachment>();
			foreach (FileDataModel attachment in attachmentsData)
			{
				attachments.Add(new Attachment
				{
					Filename = attachment.RecordID,
					Content = attachment.Base64File
				});
			}
			return attachments;
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