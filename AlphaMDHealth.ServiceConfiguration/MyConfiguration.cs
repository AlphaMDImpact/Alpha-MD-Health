using Microsoft.Extensions.Configuration;

namespace AlphaMDHealth.ServiceConfiguration
{
    public sealed class MyConfiguration
    {
        private static MyConfiguration instance = null;
        private static IConfiguration _configuration;

        public static MyConfiguration GetInstance
        {
            get
            {
                if (instance == null)
                    instance = new MyConfiguration();
                return instance;
            }
        }

        private MyConfiguration()
        {
            //private constructor
        }

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConfigurationValue(string configKeyName)
        {
            var val = _configuration[configKeyName];
            return val;
        }
    }
}