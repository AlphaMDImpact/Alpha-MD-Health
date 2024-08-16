using AlphaMDHealth.Model;
using System.Security.Cryptography;
using System.Text;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    internal class VidyoIoVideoLibrary : BaseService, IVideoLibrary
    {
        private readonly LibraryServiceModel _libraryServiceDetails;

        public VidyoIoVideoLibrary(LibraryServiceModel libraryServiceDetails)
        {
            _libraryServiceDetails = libraryServiceDetails;
        }

        public async Task GenerateSessionAsync(VideoDTO videoData)
        {
            string userName = videoData.AddedBy;
            long expiresInSecs = 1000;

            long EPOCH_SECONDS = 62167219200;

            TimeSpan timeSinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            string expires = (Math.Floor(timeSinceEpoch.TotalSeconds) + EPOCH_SECONDS + expiresInSecs).ToString();

            string jid = userName + "@" + _libraryServiceDetails.ServiceClientIdentifier;
            string body = "provision" + "\0" + jid + "\0" + expires + "\0" + "";

            var encoder = new UTF8Encoding();
            var hmacsha = new HMACSHA384(encoder.GetBytes(_libraryServiceDetails.ServiceClientSecrete));
            byte[] mac = hmacsha.ComputeHash(encoder.GetBytes(body));

            string macHex = BytesToHex(mac);
            string serialized = body + '\0' + macHex;

            videoData.Video.VideoToken = Convert.ToBase64String(encoder.GetBytes(serialized));
            await Task.CompletedTask;
            
        }

        private static string BytesToHex(byte[] bytes)
        {
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        /// <summary>
        /// Create SessionID 
        /// </summary>
        /// <param name="videoData">object to return session ID</param>
        public Task CreateSessionAsync(VideoDTO videoData)
        {
            videoData.Video.VideoRoomID = Guid.NewGuid().ToString();
            return Task.CompletedTask;
        }
    }
}