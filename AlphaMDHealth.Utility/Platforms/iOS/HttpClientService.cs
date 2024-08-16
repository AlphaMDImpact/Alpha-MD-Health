
namespace AlphaMDHealth.Utility;

public class HttpClientIosService 
{
    public HttpMessageHandler GetPlatfromSpecificHttpMessageHandler()
    {
        var handler = new NSUrlSessionHandler
        {
            TrustOverrideForUrl = IsHttpsLocalhost
          //  TrustOverrideForUrl = bool (NSUrlSessionHandlerSender, url, secTrust) => url.StartsWith("https://10.0.2.2")
        };
        return handler;
    }

    public bool IsHttpsLocalhost(NSUrlSessionHandler sender, string url, Security.SecTrust trust)
    {
        if (url.StartsWith("https://simpledosesservicesdev.azurewebsites.net") || url.StartsWith("https://10.0.2.2:5001"))
            return true;
        return false;
    }
}
