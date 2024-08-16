using AlphaMDHealth.Utility;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class FatSecretOAuthBase
    {
        /// <summary>
        /// Generates a signature using the HMAC-SHA1 algorithm
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <returns>A base64 string of the hash value</returns>
        public Tuple<string, string, string> GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            string signatureBase = GenerateSignatureBase(url, consumerKey, token, Constants.AUTH_POST_TEXT_KEY, GenerateTimeStamp(), GenerateNonce(), FoodConstants.AUTH_SIGNATURE_TYPE_TEXT_KEY, out string normalizedServicePath, out string normalizedRequestParameters);
            HMACSHA1 hmacsha1 = new HMACSHA1
            {
                Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", EncodePath(consumerSecret), IsNullOrEmpty(tokenSecret) ? "" : EncodePath(tokenSecret)))
            };
            return new Tuple<string, string, string>(normalizedServicePath, normalizedRequestParameters, GenerateSignatureUsingHash(signatureBase, hmacsha1));
        }

        protected class QueryParameter
        {
            public QueryParameter(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; } = null;

            public string Value { get; } = null;
        }

        protected class QueryParameterComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                QueryParameter a = (QueryParameter)x;
                QueryParameter b = (QueryParameter)y;
                return a.Name == b.Name ? string.Compare(a.Value, b.Value, StringComparison.InvariantCulture) : string.Compare(a.Name, b.Name, StringComparison.InvariantCulture);
            }
        }

        private static bool IsNullOrEmpty(string str)
        {
            return (str == null || str.Length == 0);
        }

        private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            byte[] dataBuffer = Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        private void GetQueryParameters(string parameters, IList result)
        {
            if (parameters.StartsWith(Constants.SYMBOL_QUESTIONMARK))
            {
                parameters = parameters.Remove(0, 1);
            }

            if (!IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!IsNullOrEmpty(s) && !s.StartsWith(FoodConstants.OAUTH_PARAMETER_PREFIX) && !s.StartsWith(FoodConstants.XOAUTH_PARAMETER_PREFIX) && !s.StartsWith(FoodConstants.OPEN_SOCIAL_PARAMETER_PREFIX))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], EncodePath(temp[1])));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }
        }

        protected string EncodePath(string value)
        {
            StringBuilder result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (FoodConstants.UNRESERVED_CHARS.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + string.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }

        protected string NormalizeRequestParameters(IList parameters)
        {
            StringBuilder sb = new StringBuilder();
            QueryParameter p;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = (QueryParameter)parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                {
                    sb.Append(Constants.SYMBOL_AMPERSAND);
                }
            }
            return sb.ToString();
        }

        private string GenerateSignatureBase(Uri url, string consumerKey, string token, string httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedPath, out string normalizedRequestParameters)
        {
            IList parameters = new ArrayList();

            GetQueryParameters(url.Query, parameters);

            parameters.Add(new QueryParameter(FoodConstants.OAUTH_VERSION, FoodConstants.OAUTH_VERSION_NUMBER));
            parameters.Add(new QueryParameter(FoodConstants.OAUTH_NONCE, nonce));
            parameters.Add(new QueryParameter(FoodConstants.OAUTH_TIMESTAMP, timeStamp));
            parameters.Add(new QueryParameter(FoodConstants.OAUTH_SIGNATURE_METHOD, signatureType));
            parameters.Add(new QueryParameter(FoodConstants.OAUTH_CONSUMER_KEY, consumerKey));

            if (!IsNullOrEmpty(token))
            {
                parameters.Add(new QueryParameter(FoodConstants.OAUTH_TOKEN, token));
            }

            ((ArrayList)parameters).Sort(new QueryParameterComparer());

            normalizedPath = string.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == Constants.HTTP_TEXT_KEY && url.Port == 80) || (url.Scheme == Constants.HTTPS_TEXT_KEY && url.Port == 443)))
            {
                normalizedPath += ":" + url.Port;
            }

            normalizedPath += url.AbsolutePath;
            normalizedRequestParameters = NormalizeRequestParameters(parameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod);
            signatureBase.AppendFormat("{0}&", EncodePath(normalizedPath));
            signatureBase.AppendFormat("{0}", EncodePath(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        private string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        private string GenerateTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        private string GenerateNonce()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}