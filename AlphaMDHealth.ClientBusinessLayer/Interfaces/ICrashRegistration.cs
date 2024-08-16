namespace AlphaMDHealth.ClientBusinessLayer.Interfaces
{
    public interface ICrashRegistration
    {
        /// <summary>
        /// Register with new app secret
        /// </summary>
        void RegisterNewAppSecret(string appSecret);
    }
}
