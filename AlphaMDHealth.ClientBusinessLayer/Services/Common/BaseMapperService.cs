using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class BaseService
{
    //public byte[] ConvertBase64StrinngToByteArray(string base64)
    //{
    //    if (!string.IsNullOrWhiteSpace(base64) && base64.Length > 100)
    //    {
    //        return Convert.FromBase64String(base64);
    //    }
    //    return default;
    //}

    protected double CalculateUtcAndLocalTimeDifferenceInSeconds()
    {
        DateTimeOffset currentUtcDateTime = GenericMethods.GetUtcDateTime;
        var currentLocalDateTime = _essentials.ConvertToLocalTime(currentUtcDateTime);
        var diff = (currentLocalDateTime.Ticks - currentUtcDateTime.Ticks);
        return TimeSpan.FromTicks(diff).TotalSeconds;
    }

    /// <summary>
    /// Check token and Extract Value
    /// </summary>
    /// <typeparam name="T">DataType of Value</typeparam>
    /// <param name="dataItem">Jtoken item</param>
    /// <param name="fieldName">Key of the DataItem</param>
    /// <returns></returns>
    protected T GetDataItem<T>(JToken dataItem, string fieldName)
    {
        return string.IsNullOrWhiteSpace((string)dataItem[fieldName])
            ? default
            : (T)Convert.ChangeType(dataItem[fieldName], typeof(T), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Map response for save records
    /// </summary>
    /// <param name="data">Data received from server</param>
    /// <returns>Response data mapped in SaveResultModel</returns>
    protected List<SaveResultModel> MapSaveResponse(JToken data)
    {
        return MapSaveResponse(data, nameof(ContactDTO.SaveResults));
    }

    /// <summary>
    /// Map response for save records
    /// </summary>
    /// <param name="data">Data received from server</param>
    /// <returns>Response data mapped in SaveResultModel</returns>
    protected List<SaveResultModel>? MapSaveResponse(JToken data, string jsonKey)
    {
        return (data[jsonKey]?.Count() > 0)
            ? (from dataItem in data[jsonKey]
               select MapResult(dataItem)).ToList()
            : null;
    }

    /// <summary>
    /// Map languages json into model
    /// </summary>
    /// <param name="data">Languages json data</param>
    /// <returns>List of Langages</returns>
    protected List<OptionModel>? MapOptions(JToken data)
    {
        return (data[nameof(OrganisationDTO.DropDownOptions)]?.Count() > 0)
            ? (from dataItem in data[nameof(OrganisationDTO.DropDownOptions)]
               select new OptionModel
               {
                   OptionID = (int)dataItem[nameof(OptionModel.OptionID)],
                   OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                   IsSelected = (bool)dataItem[nameof(OptionModel.IsSelected)],
                   IsDefault = (bool)dataItem[nameof(OptionModel.IsDefault)],
               }).ToList()
            : null;
    }

    protected void MapCommonData(BaseDTO responseData, JToken data)
    {
        if (!MobileConstants.IsMobilePlatform)
        {
            responseData.Resources = new ResourceService(_essentials).MapResourcesData(data);
            responseData.Settings = new SettingService(_essentials).MapSettingsData(data);
            responseData.FeaturePermissions = new FeatureService(_essentials).MapFeatures(data, nameof(BaseDTO.FeaturePermissions));
        }
    }

    private SaveResultModel MapResult(JToken dataItem)
    {
        return new SaveResultModel
        {
            ID = GetDataItem<long>(dataItem, nameof(SaveResultModel.ID)),
            ClientGuid = GetDataItem<Guid>(dataItem, nameof(SaveResultModel.ClientGuid)),
            ErrCode = (ErrorCode)GetDataItem<int>(dataItem, nameof(SaveResultModel.ErrCode)),
            ServerGuid = GetDataItem<Guid>(dataItem, nameof(SaveResultModel.ServerGuid)) == null ? Guid.Empty : GetDataItem<Guid>(dataItem, nameof(SaveResultModel.ServerGuid))
        };
    }

    public string GetFileNameWithExtension(string documentLink)
    {
        if (string.IsNullOrWhiteSpace(documentLink))
        {
            return string.Empty;
        }
        else
        {
            Uri uri = new Uri(documentLink);
            return Path.GetFileName(uri.Segments.Last());
        }
    }
}