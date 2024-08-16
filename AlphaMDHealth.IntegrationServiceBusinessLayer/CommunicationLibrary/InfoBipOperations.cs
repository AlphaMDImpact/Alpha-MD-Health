using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    class InfoBipOperations : BaseService, ICommunicationLibrary
	{
		private readonly LibraryServiceModel _libraryServiceData;

		public InfoBipOperations(LibraryServiceModel libraryServiceData)
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
		public Task SendSmsAsync(CommunicationDTO communicationData)
		{
			throw new NotImplementedException();
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
		/// Sends whatsapp message using InfoBIP service
		/// </summary>
		/// <param name="communicationData">Contains Data to be send</param>
		/// <returns>Operation Status</returns>
		public async Task SendWhatsAppMessageAsync(CommunicationDTO communicationData)
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(_libraryServiceData.ServiceTarget.Trim());
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("App", _libraryServiceData.ServiceClientSecrete.Trim());
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			string message = $@"
                {{
                    ""messages"": [
                    {{
                        ""from"": ""{communicationData.FromId}"",
                        ""to"": ""{communicationData.ToIds}"",
                        ""content"": {{
                        ""templateName"": ""{communicationData.TeampleteName}"",
                        ""templateData"": {{
                            ""body"": {{
                            ""placeholders"": [
                                ""Value 1"",
                                ""Value 2"",
                                ""Value 3"",
                                ""Value 4"",
								""Value 5""
                            ]
                            }}
                        }},
                        ""language"": ""en""
                    }}
                    }}
                ]
                }}";

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/whatsapp/1/message/template");
			httpRequest.Content = new StringContent(message, Encoding.UTF8, "application/json");
			var response = await client.SendAsync(httpRequest);
			communicationData.ErrCode = response.StatusCode == HttpStatusCode.Accepted ? ErrorCode.OK : ErrorCode.BadRequest;
			//var responseContent = await response.Content.ReadAsStringAsync();
		}
	}
}


