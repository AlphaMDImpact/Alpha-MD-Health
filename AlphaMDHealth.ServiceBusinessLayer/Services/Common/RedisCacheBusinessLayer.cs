using AlphaMDHealth.ServiceConfiguration;
using AlphaMDHealth.Utility;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class RedisCacheBusinessLayer
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var myConfig = MyConfiguration.GetInstance;
            string cacheConnection = myConfig.GetConfigurationValue(ConfigurationConstants.CS_REDIS_CACHE_CONNECTION_STRING);
            return ConnectionMultiplexer.Connect(cacheConnection);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        public static bool SetCacheData<T>(string cacheKey, T cacheData)
        {
            TimeSpan expiryTime = new TimeSpan(8, 0, 0);
            var isCacheSet = Connection.GetDatabase().StringSet(cacheKey, JsonConvert.SerializeObject(cacheData), expiryTime);
            return isCacheSet;
        }

		public static void ClearCacheKey(string cacheKey)
		{
            bool isKeyExists = Connection.GetDatabase().KeyExists(cacheKey);
            if (isKeyExists)
            {
                Connection.GetDatabase().KeyDelete(cacheKey);
            }
		}

		public static T GetCacheData<T>(string cachekey)
        {
            try
            {
                var cacheData = Connection.GetDatabase().StringGet(cachekey);
                if (!string.IsNullOrEmpty(cacheData))
                {
                    return JsonConvert.DeserializeObject<T>(cacheData);
                }
                return default;
            }
            catch
            {
                return default;
            }
        }
    }
}
