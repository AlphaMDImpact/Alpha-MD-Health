namespace AlphaMDHealth.CommonBusinessLayer
{
    /// <summary>
    /// http client handler object
    /// </summary>
    public interface IHttpClientHandler
    {
        /// <summary>
        /// get http client handler object
        /// </summary>
        /// <returns>http client handler object</returns>
        HttpMessageHandler GetHttpClientHandler();
    }
}
