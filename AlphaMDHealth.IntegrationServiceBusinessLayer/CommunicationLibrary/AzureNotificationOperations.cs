using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class AzureNotificationOperations : BaseService, ICommunicationLibrary
	{
		private readonly List<LibraryServiceModel> _libraryServiceData;
		public static string SE_HMAC_SIGNATURE_HEADER_KEY => "X-Signature";
		public static string SE_NOTIFICATION_CLIENT_IDENTIFIER_HEADER_KEY => "X-ClientId";

		public AzureNotificationOperations(List<LibraryServiceModel> libraryServiceData)
		{
			_libraryServiceData = libraryServiceData;
		}

		/// <summary>
		/// Register device for notification
		/// </summary>
		/// <param name="communicationData">data required to register device</param>
		/// <returns>Operation status code</returns>
		public async Task RegisterDeviceForNotificationAsync(NotificationDTO communicationData)
		{
			communicationData.ErrCode = await SendRequestToNotificationServerAsync(communicationData.NotificationData, ServiceIdentifierFor.RegisterDeviceForNotification);
		}

		/// <summary>
		/// Send notification
		/// </summary>
		/// <param name="communicationData">data required to send notification</param>
		/// <returns>Operation status code</returns>
		public async Task SendNotificationAsync(NotificationDTO communicationData)
		{
			communicationData.ErrCode = await SendRequestToNotificationServerAsync(communicationData.NotificationMessage, ServiceIdentifierFor.SendNotification);
		}

		/// <summary>
		/// Send sms
		/// </summary>
		/// <param name="communicationData">data required to send sms</param>
		public Task SendSmsAsync(CommunicationDTO communicationData)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Send email
		/// </summary>
		/// <param name="communicationData">data required to send email</param>
		public Task SendEmailAsync(CommunicationDTO communicationData)
		{
			throw new NotSupportedException();
		}

		private async Task<ErrorCode> SendRequestToNotificationServerAsync<T>(T communicationData, ServiceIdentifierFor serviceIdentifierFor)
		{
			LibraryServiceModel libraryDetail = _libraryServiceData.Find(x => x.IdentifierFor == serviceIdentifierFor.ToString());
			HttpClient httpClient = new HttpClient(new HttpClientHandler());
			StringContent requestString = new StringContent(JsonConvert.SerializeObject(communicationData), Encoding.UTF8, Constants.SE_ACCEPT_HEADER_JSON_KEY);
			string notificationClientId = libraryDetail.ServiceClientIdentifier;
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation(SE_NOTIFICATION_CLIENT_IDENTIFIER_HEADER_KEY, notificationClientId);
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation(SE_HMAC_SIGNATURE_HEADER_KEY, GenerateNotificationHmacSignature(libraryDetail.ServiceClientSecrete, notificationClientId, HttpMethods.POST.ToString(), libraryDetail.ServiceTarget, string.Empty, await requestString.ReadAsByteArrayAsync()));
			HttpResponseMessage response = await httpClient.PostAsync(new Uri(libraryDetail.ServiceTarget), requestString);
			return response.IsSuccessStatusCode ? ErrorCode.OK : ErrorCode.ErrorWhileSavingRecords;
		}

		private string GenerateNotificationHmacSignature(string clientSecret, string clientID, string method, string endPoint, string accessToken, byte[] content)
		{
			// Compute Md5 Hash when content length >0 and convert that data into base 64 string to generate hash
			string data = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}", clientID, method, endPoint, accessToken, Convert.ToBase64String((content.Length != 0) ? new MD5CryptoServiceProvider().ComputeHash(content) : null));
			string clientSecretBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientSecret));
			// Compute HMAC 256 Hash and convert into base 64 string
			return Convert.ToBase64String(new HMACSHA256 { Key = Convert.FromBase64String(clientSecretBase64) }.ComputeHash(Encoding.UTF8.GetBytes(data)));
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
