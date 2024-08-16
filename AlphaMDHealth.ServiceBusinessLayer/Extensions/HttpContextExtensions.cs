using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public static class HttpContextExtensions
	{
		/// <summary>
		/// Gets the user id from the current Http context
		/// </summary>
		/// <param name="context">Http context</param>
		/// <returns>User id</returns>
		public static long GetAccountID(this HttpContext context)
		{
			if (context?.User != null && context.User is UserPrincipal)
			{
				return (context.User as UserPrincipal).AccountID;
			}
			return 0;
		}

		/// <summary>
		/// Gets the DeviceType from the current Http context
		/// </summary>
		/// <param name="context">Http context</param>
		/// <returns>DeviceType</returns>
		public static byte GetFeatureFor(this HttpContext context)
		{
			if (context?.Request?.Headers != null)
			{
				var headers = context.Request.Headers;
				headers.TryGetValue(Constants.SE_DEVICE_TYPE_HEADER_KEY, out StringValues headerValues);
				var deviceType = headerValues.FirstOrDefault() ?? string.Empty;
				if (!string.IsNullOrWhiteSpace(deviceType))
				{
					return deviceType == "W" ? (byte)2 : (byte)3;
				}
			}
			return 0;
		}
	}
}