using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Utility;
using System.Diagnostics;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class BaseService : BaseLibService
{
    public BaseService(IEssentials serviceEssentials, IHttpService httpService) : base(serviceEssentials)
    {
        HttpService = httpService;
    }

    /// <summary>
    /// Replace cdn link url once data is downloaded
    /// </summary>
    /// <param name="imageBytes"></param>
    /// <param name="image"></param>
    /// <returns></returns>
    protected string ResetCdnLink(byte[] imageBytes, string image)
    {
        if (imageBytes != default && imageBytes.Length > 0)
        {
            image = string.Empty;
        }
        return image;
    }

    /// <summary>
    /// Get Base64 image string from cdn url
    /// </summary>
    /// <param name="cdnLink">cdn url</param>
    /// <returns>Base64string</returns>
    protected async Task<byte[]> GetImageAsByteArrayAsync(string cdnLink)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(cdnLink) && cdnLink.StartsWith(Constants.HTTP_TAG_PREFIX))
            {
                using (var client = new HttpClient())
                {
                    return await client.GetByteArrayAsync(new Uri(cdnLink)).ConfigureAwait(false);
                }
            }
            else
            {
                return default;
            }
        }
        catch (Exception ex)
        {
            ///new BaseLibService().LogError($"CDN Link Retrival Error \n {ex.Message} =====>>>>>> File Name=>{cdnLink}", ex);
            Debug.WriteLine($"CDN Link Retrival Error \n {ex.Message} =====>>>>>> File Name=>{cdnLink}");
            return default;
        }
    }

    /// <summary>
    /// Get Base64 image string from cdn url
    /// </summary>
    /// <param name="cdnLink">cdn url</param>
    /// <returns>Base64string</returns>
    protected async Task<string> GetImageAsBase64Async(string cdnLink)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(cdnLink) && cdnLink.StartsWith(Constants.HTTP_TAG_PREFIX))
            {
                using var client = new HttpClient();
                var bytes = await client.GetByteArrayAsync(new Uri(cdnLink)).ConfigureAwait(false);
                //var prefix = GenericMethods.GetImagePrefix(Path.GetExtension(cdnLink));
                //return "data:" + prefix + Convert.ToBase64String(bytes);
                return Convert.ToBase64String(bytes);
            }
            else
            {
                return cdnLink;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CDN Link Retrival Error \n {ex.Message} =====>>>>>> File Name=>{cdnLink}");
            return cdnLink;
        }
    }

    /// <summary>
    /// Get richtext box images from cdn lik
    /// </summary>
    /// <param name="richTextData">rich textbox data</param>
    /// <returns>updated richtextbox data</returns>
    protected async Task<string> GetImageContentAsync(string richTextData)
    {
        //// bool endOfString = false;
        int startPosition = 0;
        int endPosition;
        string imageValue;
        string imageLink;

        for (int i = 0; i < richTextData?.Length; i++)
        {
            startPosition = richTextData.IndexOf(Constants.IMAGE_START_TAG, startPosition, StringComparison.InvariantCultureIgnoreCase);
            if (startPosition == -1 || startPosition > richTextData?.Length)
            {
                break;
            }
            else
            {
                endPosition = richTextData.IndexOf(@""" ", startPosition, StringComparison.InvariantCultureIgnoreCase);
                startPosition += 10;
                endPosition -= startPosition;
                imageLink = richTextData.Substring(startPosition, endPosition);
                if (!string.IsNullOrWhiteSpace(imageLink) && imageLink.Contains(Constants.HTTP_TAG_PREFIX))
                {
                    imageValue = await GetImageAsBase64Async(imageLink).ConfigureAwait(false);
                    richTextData = richTextData.Replace(imageLink, imageValue);
                }
            }
        }
        return richTextData;
    }
}