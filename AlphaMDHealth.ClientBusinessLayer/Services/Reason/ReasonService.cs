using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public class ReasonService : BaseService
{
    public ReasonService(IEssentials serviceEssentials) : base(serviceEssentials)
    {
         
    }
    /// <summary>
    /// Sync Reasons and page recourses from service 
    /// </summary>
    /// <param name="reasonData">Reason DTO to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reasons received from server in reason data and operation status</returns>
    public async Task SyncReasonsFromServerAsync(ReasonDTO reasonData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_REASONS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(reasonData.RecordCount, CultureInfo.InvariantCulture) },
                    { Constants.SE_REASON_ID_QUERY_KEY, Convert.ToString(reasonData.Reason.ReasonID, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            reasonData.ErrCode = httpData.ErrCode;
            if (reasonData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(reasonData, data);
                    reasonData.Response = null;
                    reasonData.Reasons = MapReasons(data, nameof(ReasonDTO.Reasons));
                    reasonData.ErrCode = (ErrorCode)(int)data[nameof(ReasonDTO.ErrCode)];
                }
            }
        }
        catch (Exception ex)
        {
            reasonData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Reason Data to server
    /// </summary>
    /// <param name="reasonData">object to return operation status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation status call</returns>
    public async Task SyncReasonToServerAsync(ReasonDTO reasonData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<ReasonDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_REASON_ASYNC_PATH,
                ContentToSend = reasonData,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            reasonData.ErrCode = httpData.ErrCode;
            reasonData.Response = httpData.Response;
        }
        catch (Exception ex)
        {
            reasonData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }
    /// <summary>
    /// Gets Program Reasons Data
    /// </summary>
    /// <param name="programData">Object to get back Program Reason Data</param>
    /// <param name="cancellationToken">Cancelation Token</param>
    /// <returns>Object to get Reason Data and Operation status</returns>
    public async Task SyncProgramReasonsFromServer(ProgramDTO programData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<ProgramDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_PROGRAM_REASONS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    { nameof(ReasonModel.ProgramReasonID), Convert.ToString(programData.Reason.ProgramReasonID, CultureInfo.InvariantCulture) },
                    { nameof(BaseDTO.RecordCount), Convert.ToString(programData.RecordCount, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            programData.ErrCode = httpData.ErrCode;
            if (programData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(programData, data);
                    SetPageResources(programData.Resources);
                    programData.Reason = data[nameof(ProgramDTO.Reason)].Any()
                        ? new ReasonService(_essentials).MapReasons(data[nameof(ProgramDTO.Reason)])
                        : new ReasonModel();
                    programData.ReasonOptionList = GetPickerSource(data, nameof(ProgramDTO.ReasonOptionList), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), programData.Reason?.ReasonID??-1, false, null, nameof(OptionModel.ParentOptionText));
                }
            }
        }
        catch (Exception ex)
        {
            programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }


    /// <summary>
    /// Saves program reason
    /// </summary>
    /// <param name="programData">Object to be saved</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Operation status</returns>
    public async Task SyncProgramReasonsToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<ProgramDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_REASON_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                },
                ContentToSend = programData,
            };
            await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
            programData.ErrCode = httpData.ErrCode;
            programData.Response = httpData.Response;
            if (programData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    programData.Reason.ProgramReasonID = (long)data[nameof(ProgramDTO.Reason)][nameof(ReasonModel.ProgramReasonID)];
                }
            }
        }
        catch (Exception ex)
        {
            programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    private List<ReasonModel> MapReasons(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select MapReasons(dataItem)).ToList()
            : null;
    }

    public ReasonModel MapReasons(JToken dataItem)
    {
        return new ReasonModel
        {
            ProgramReasonID = GetDataItem<long>(dataItem, nameof(ReasonModel.ProgramReasonID)),
            ProgramID = GetDataItem<long>(dataItem, nameof(ReasonModel.ProgramID)),
            Reason = GetDataItem<string>(dataItem, nameof(ReasonModel.Reason)),
            ReasonID = GetDataItem<byte>(dataItem, nameof(ReasonModel.ReasonID)),
            IsActive = GetDataItem<bool>(dataItem, nameof(ReasonModel.IsActive)),
            ReasonDescription = GetDataItem<string>(dataItem, nameof(ReasonModel.ReasonDescription)),
            LanguageID = GetDataItem<byte>(dataItem, nameof(ReasonModel.LanguageID)),
            LanguageName = GetDataItem<string>(dataItem, nameof(ReasonModel.LanguageName)),
            IsDefault = GetDataItem<bool>(dataItem, nameof(ReasonModel.IsDefault))
        };
    }

}

