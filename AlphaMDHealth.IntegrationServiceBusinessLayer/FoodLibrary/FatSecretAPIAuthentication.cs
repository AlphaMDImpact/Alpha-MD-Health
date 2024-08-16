using AlphaMDHealth.Utility;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class FatSecretApiAuthentication
    {
        private readonly string _clientID;
        private readonly string _clientSecret;
        private readonly string _serviceTarget;

        public FatSecretApiAuthentication(string clientID, string clientSecret, string serviceTarget)
        {
            _clientID = clientID;
            _clientSecret = clientSecret;
            _serviceTarget = serviceTarget;
        }

        /// <summary>
        /// Create a new profile
        /// </summary>
        /// <returns>valid token and secret</returns>
        public Tuple<string, string> CreateTokenAndSecretKey()
        {
            var signatureData = new FatSecretOAuthBase().GenerateSignature(new Uri(_serviceTarget), _clientID, _clientSecret, null, null);

            XmlDocument doc = LoadXMLDocument(GetQueryResponse(signatureData.Item1, signatureData.Item2 + Constants.SYMBOL_AMPERSAND + FoodConstants.OAUTH_SIGNATURE + Constants.SYMBOL_EQUAL + HttpUtility.UrlEncode(signatureData.Item3)));

            string token = doc[FoodConstants.FATSECRET_PROFILE_TEXT_KEY][FoodConstants.AUTH_TOKEN_TEXT_KEY].InnerText;
            string secret = doc[FoodConstants.FATSECRET_PROFILE_TEXT_KEY][FoodConstants.AUTH_SECRET_TEXT_KEY].InnerText;
            return new Tuple<string, string>(token, secret);
        }

        #region Private Methods

        internal static XmlDocument LoadXMLDocument(string rawXML)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(rawXML);
            return xmlDocument;
        }

        internal static string GetQueryResponse(string requestTarget, string postString)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(requestTarget));

            webRequest.Method = Constants.AUTH_POST_TEXT_KEY;
            webRequest.ContentType = FoodConstants.AUTH_CONTENT_TYPE_TEXT_KEY;

            byte[] parameterString = Encoding.ASCII.GetBytes(postString);
            webRequest.ContentLength = parameterString.Length;

            using (Stream buffer = webRequest.GetRequestStream())
            {
                buffer.Write(parameterString, 0, parameterString.Length);
                buffer.Close();
            }

            WebResponse webResponse = webRequest.GetResponse();

            string responseData;
            using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
            {
                responseData = streamReader.ReadToEnd();
            }
            return responseData;
        }
        #endregion
    }
}
