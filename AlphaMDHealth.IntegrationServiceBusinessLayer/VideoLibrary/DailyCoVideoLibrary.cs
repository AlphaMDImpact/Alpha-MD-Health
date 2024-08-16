using AlphaMDHealth.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    internal class DailyCoVideoLibrary : BaseService, IVideoLibrary
    {
        private readonly List<LibraryServiceModel> _libraryServiceDetails;

        public DailyCoVideoLibrary(List<LibraryServiceModel> libraryServiceDetails)
        {
            _libraryServiceDetails = libraryServiceDetails;
        }

        /// <summary>
        /// Generate session and room link
        /// </summary>
        /// <returns></returns>
        public async Task GenerateSessionAsync(VideoDTO videoData)
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                _libraryServiceDetails[0].ServiceTarget = "https://api.daily.co/v1/meeting-tokens";
                _libraryServiceDetails[0].ClientIdentifier = "2b44b81f6882784d42cb9fba4aa7fd2fd33996221ba43bc530d892509d48e581";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _libraryServiceDetails[0].ServiceTarget);
                request.Content = new StringContent(CreateTokenBody(videoData), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", string.Concat("Bearer ", _libraryServiceDetails[0].ClientIdentifier));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage response =  await client.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    JToken data = JToken.Parse(result);
                    videoData.Video.VideoToken = data["token"].ToString();
                    videoData.Video.VideoRoomID = await CreateRoom(videoData.Video.VideoRoomID);
                }
            }
        }

        /// <summary>
        /// Create SessionID 
        /// </summary>
        /// <param name="videoData">object to return session ID</param>
        /// <returns>Session ID</returns>
        public Task CreateSessionAsync(VideoDTO videoData)
        {
            videoData.Video.VideoRoomID = Guid.NewGuid().ToString();
            return Task.CompletedTask;
        }

        private string CreateTokenBody(VideoDTO videoData)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            writer.WritePropertyName("room_name");
            writer.WriteValue(videoData.Video.VideoRoomID);

            writer.WritePropertyName("user_name");
            writer.WriteValue(videoData.LastModifiedBy);

            writer.WritePropertyName("user_id");
            writer.WriteValue(videoData.AddedBy);
            writer.WriteEndObject();
            writer.WriteEndObject();
            return sw.ToString();
        }

        private async Task<string> CreateRoom(string roomID)
        {
            string roomLink = string.Empty;

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                _libraryServiceDetails[0].ClientIdentifier = "2b44b81f6882784d42cb9fba4aa7fd2fd33996221ba43bc530d892509d48e581";
                _libraryServiceDetails[1].ServiceTarget = "https://api.daily.co/v1/rooms";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,_libraryServiceDetails[1].ServiceTarget);
                request.Content = new StringContent(CreateRoomBody(roomID), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", string.Concat("Bearer ", _libraryServiceDetails[1].ClientIdentifier));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    JToken data = JToken.Parse(result);
                    roomLink= data["url"].ToString();
                }
            }
            return roomLink;
        }

        private string CreateRoomBody(string roomID)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(roomID);
            writer.WritePropertyName("privacy");
            writer.WriteValue("private");
            writer.WriteEndObject();
            return sw.ToString();
        }
    }
}