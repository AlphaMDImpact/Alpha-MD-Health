using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class OrganisationService : BaseService
    {
        public OrganisationService(IEssentials essentials) : base(essentials)
        {
        }
        /// <summary>
        /// Sync organisation data from service
        /// </summary>
        /// <param name="organisationData">Organisation profile data reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Organisation data received from server</returns>
        public async Task SyncOrganisationProfileFromServerAsync(OrganisationDTO organisationData, CancellationToken cancellationToken)
        {
            try
            {
                if (organisationData.OrganisationID == 0)
                {
                    organisationData.OrganisationID = organisationData.IsActive
                        ? 0
                        : _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, Constants.DEFAULT_ORGANISATION_ID);
                }
                else if (organisationData.OrganisationID == -1)
                {
                    organisationData.OrganisationID = 0;
                }
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_ORGANISATION_DATA_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        {
                            Constants.SE_ORGANISATION_ID_FOR_SETUP_QUERY_KEY,
                            Convert.ToString(organisationData.OrganisationID, CultureInfo.InvariantCulture)
                        }
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                organisationData.ErrCode = httpData.ErrCode;
                if (organisationData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(organisationData, data);
                        MapOrganisationProfileData(data, organisationData);
                    }
                }
            }
            catch (Exception ex)
            {
                organisationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapOrganisationProfileData(JToken data, OrganisationDTO organisationData)
        {
            organisationData.OrganisationProfile = (data[nameof(OrganisationDTO.OrganisationProfile)]?.Count() > 0) ?
                new OrganisationModel
                {
                    OrganisationDomain = (string)data[nameof(OrganisationDTO.OrganisationProfile)][nameof(OrganisationModel.OrganisationDomain)],
                    TaxNumber = (string)data[nameof(OrganisationDTO.OrganisationProfile)][nameof(OrganisationModel.TaxNumber)],
                    AddedON = (DateTimeOffset)data[nameof(OrganisationDTO.OrganisationProfile)][nameof(OrganisationModel.AddedON)],
                    PlanID = (short)data[nameof(OrganisationDTO.OrganisationProfile)][nameof(OrganisationModel.PlanID)],
                } :
                new OrganisationModel
                {
                    TaxNumber = string.Empty,
                };

                if(data[nameof(OrganisationDTO.OrganisationExternalServices)]?.Count() > 0)
                {
                    organisationData.OrganisationExternalServices = (from dataItem in data[nameof(OrganisationDTO.OrganisationExternalServices)]
                                                                    select new OrganisationExternalServiceModel
                                                                    {
                                                                        OrganisationServiceID = (long)dataItem[nameof(OrganisationExternalServiceModel.OrganisationServiceID)],
                                                                        OrganisationID = (long)dataItem[nameof(OrganisationExternalServiceModel.OrganisationID)],
                                                                        ExternalServiceID = (int)dataItem[nameof(OrganisationExternalServiceModel.ExternalServiceID)],
                                                                        UnitPrice = (decimal)dataItem[nameof(OrganisationExternalServiceModel.UnitPrice)],
                                                                        DiscountPercentage = (decimal)dataItem[nameof(OrganisationExternalServiceModel.DiscountPercentage)],
                                                                        MinimumQuantityToBuy = (long)dataItem[nameof(OrganisationExternalServiceModel.MinimumQuantityToBuy)],
                                                                        ForPatient = (bool)dataItem[nameof(OrganisationExternalServiceModel.ForPatient)],
                                                                    }).ToList();
                }
            
            if (data[nameof(OrganisationDTO.PageDetails)]?.Count() > 0)
            {
                organisationData.PageDetails = (from dataItem in data[nameof(OrganisationDTO.PageDetails)]
                                                select new ContentDetailModel
                                                {
                                                    LanguageID = (byte)dataItem[nameof(ContentDetailModel.LanguageID)],
                                                    LanguageName = (string)dataItem[nameof(ContentDetailModel.LanguageName)],
                                                    PageHeading = (string)dataItem[nameof(ContentDetailModel.PageHeading)],
                                                }).ToList();
            }

            if (data[nameof(OrganisationDTO.DropDownOptions)]?.Count() > 0)
            {
                MapLanguageData(data, organisationData);
            }
            organisationData.ErrCode = (ErrorCode)(int)data[nameof(OrganisationDTO.ErrCode)];
        }

        private void MapLanguageData(JToken data, OrganisationDTO organisationData)
        {
            organisationData.DropDownOptions = (from dataItem in data[nameof(OrganisationDTO.DropDownOptions)]
                                                select new OptionModel
                                                {
                                                    OptionID = (int)dataItem[nameof(OptionModel.OptionID)],
                                                    OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                                                    IsSelected = (bool)dataItem[nameof(OptionModel.IsDefault)],
                                                    IsDefault = (bool)dataItem[nameof(OptionModel.IsDefault)],
                                                }).ToList();

            organisationData.Languages = (from dataItem in data[nameof(OrganisationDTO.DropDownOptions)]
                                          select new OptionModel
                                          {
                                              OptionID = (int)dataItem[nameof(OptionModel.OptionID)],
                                              OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                                              IsSelected = (bool)dataItem[nameof(OptionModel.IsDefault)] || (bool)dataItem[nameof(OptionModel.IsSelected)],
                                              IsDisabled = (bool)dataItem[nameof(OptionModel.IsDefault)]
                                          }).ToList();

            organisationData.DataLanguages = (from dataItem in data[nameof(OrganisationDTO.DropDownOptions)]
                                              select new LanguageModel
                                              {
                                                  LanguageID = (byte)dataItem[nameof(OptionModel.OptionID)],
                                                  LanguageName = (string)dataItem[nameof(OptionModel.OptionText)],
                                                  IsDefault = (bool)dataItem[nameof(OptionModel.IsDefault)],
                                                  IsActive = (bool)dataItem[nameof(OptionModel.IsDefault)] || (bool)dataItem[nameof(OptionModel.IsSelected)],
                                              }).Where(item => item.IsActive).ToList();

            organisationData.PaymentPlans = (from dataItem in data[nameof(OrganisationDTO.PaymentPlans)]
                                          select new OptionModel
                                          {
                                              OptionID = (int)dataItem[nameof(OptionModel.OptionID)],
                                              OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                                              IsSelected = (bool)dataItem[nameof(OptionModel.IsDefault)] || (bool)dataItem[nameof(OptionModel.IsSelected)],
                                              IsDisabled = (bool)dataItem[nameof(OptionModel.IsDefault)]
                                          }).ToList();

            if (GenericMethods.IsListNotEmpty(organisationData.PaymentPlans))
            {
                if(organisationData.PaymentPlans.Count == 1) 
                {
                    organisationData.PaymentPlans.FirstOrDefault().IsSelected = true;
                }
                else if (organisationData.PaymentPlans.Count > 1 && organisationData.OrganisationID > 0 && organisationData.OrganisationProfile.PlanID > 0)
                {
                    organisationData.PaymentPlans.FirstOrDefault(x => x.OptionID == organisationData.OrganisationProfile.PlanID).IsSelected = true;
                }
            }
            organisationData.ExternalServices = (from dataItem in data[nameof(OrganisationDTO.ExternalServices)]
                                          select new OptionModel
                                          {
                                              OptionID = (int)dataItem[nameof(OptionModel.OptionID)],
                                              OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                                              IsSelected = (bool)dataItem[nameof(OptionModel.IsDefault)] || (bool)dataItem[nameof(OptionModel.IsSelected)],
                                              IsDisabled = (bool)dataItem[nameof(OptionModel.IsDefault)]
                                          }).ToList();

            if (GenericMethods.IsListNotEmpty(organisationData.ExternalServices))
            {
                if (organisationData.ExternalServices.Count == 1)
                {
                    organisationData.ExternalServices.FirstOrDefault().IsSelected = true;
                }
                else if (organisationData.ExternalServices.Count > 1 && organisationData.OrganisationID > 0 && organisationData.OrganisationExternalServices.FirstOrDefault().ExternalServiceID > 0)
                {
                    organisationData.ExternalServices.FirstOrDefault(x => x.OptionID == organisationData.OrganisationExternalServices.FirstOrDefault().ExternalServiceID).IsSelected = true;
                }
            }

        }

        /// <summary>
        /// Sync Branches from service
        /// </summary>
        /// <param name="branchData">branchData reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Branches received from server in branchData</returns>
        public async Task SyncOrganisationBranchesFromServerAsync(BranchDTO branchData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_ORGANISATION_BRANCHES_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(branchData.RecordCount, CultureInfo.InvariantCulture) },
                        { Constants.SE_BRANCH_ID_QUERY_KEY, Convert.ToString(branchData.Branch.BranchID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                branchData.ErrCode = httpData.ErrCode;
                if (branchData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(branchData, data);
                        MapBranches(data, branchData);
                    }
                }
            }
            catch (Exception ex)
            {
                branchData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Organisation Branch data to server
        /// </summary>
        /// <param name="requestData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status call</returns>
        public async Task SyncOrganisationBranchToServerAsync(BranchDTO requestData, CancellationToken cancellationToken)
        {
            try
            {
                if (GenericMethods.IsListNotEmpty(requestData.Branches))
                {
                    var httpData = new HttpServiceModel<BranchDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_ORGANISATION_BRANCH_ASYNC_PATH,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        },
                        ContentToSend = requestData,
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    requestData.ErrCode = httpData.ErrCode;
                    requestData.Response = httpData.Response;
                }
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapBranches(JToken data, BranchDTO branchData)
        {
            branchData.Branches = (data[nameof(BranchDTO.Branches)]?.Count() > 0)
                ? (from dataItem in data[nameof(BranchDTO.Branches)]
                   select new BranchModel
                   {
                       BranchID = (long)dataItem[nameof(BranchModel.BranchID)],
                       BranchName = (string)dataItem[nameof(BranchModel.BranchName)],
                       DepartmentsCount = (long)dataItem[nameof(BranchModel.DepartmentsCount)],
                       LanguageID = (byte)dataItem[nameof(BranchModel.LanguageID)],
                       LanguageName = (string)dataItem[nameof(BranchModel.LanguageName)],
                       IsDefault = (bool)dataItem[nameof(BranchModel.IsDefault)],
                       IsRightToLeft = (bool)dataItem[nameof(BranchModel.IsRightToLeft)],
                   }).ToList()
                 : null;
            if (branchData.RecordCount < 0)
            {
                branchData.Departments = (data[nameof(BranchDTO.Departments)]?.Count() > 0)
                    ? (from dataItem in data[nameof(BranchDTO.Departments)]
                       select new DepartmentModel
                       {
                           DepartmentID = (byte)dataItem[nameof(DepartmentModel.DepartmentID)],
                           DepartmentName = (string)dataItem[nameof(DepartmentModel.DepartmentName)],
                           IsActive = (bool)dataItem[nameof(DepartmentModel.IsActive)],
                           LanguageID = (byte)dataItem[nameof(DepartmentModel.LanguageID)],
                       }).ToList()
                 : null;
            }
            branchData.ErrCode = (ErrorCode)(int)data[nameof(BranchDTO.ErrCode)];
        }

        /// <summary>
        /// Sync Organisation Settings or themes from service
        /// </summary>
        /// <param name="settings">Settings reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Settings based on the organisation</returns>
        public async Task SyncOrganisationSettingsFromServerAsync(BaseDTO settings, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_ORGANISATION_SETTINGS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_SETTING_GROUP_NAME, settings.LastModifiedBy },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                settings.ErrCode = httpData.ErrCode;
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(settings, data);
                }
            }
            catch (Exception ex)
            {
                settings.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Settings Data to server
        /// </summary>
        /// <param name="settingData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status call</returns>
        public async Task SyncOrganisationSettingsToServerAsync(BaseDTO settingData, CancellationToken cancellationToken)
        {
            try
            {
                if (GenericMethods.IsListNotEmpty(settingData.Settings))
                {
                    var httpData = new HttpServiceModel<BaseDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.UPDATE_ORGANISATION_SETTINGS_ASYNC_PATH,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                            { Constants.SE_SETTING_GROUP_NAME, settingData.LastModifiedBy },
                        },
                        ContentToSend = settingData,
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    settingData.ErrCode = httpData.ErrCode;
                    settingData.Response = httpData.Response;
                }
            }
            catch (Exception ex)
            {
                settingData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync organisation data to server
        /// </summary>
        /// <param name="requestData">object to holds organisation data</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status call</returns>
        public async Task SyncOrganisationProfileToServerAsync(OrganisationDTO requestData, CancellationToken cancellationToken, bool CanAddAdmin)
        {
            try
            {
                var httpData = new HttpServiceModel<OrganisationDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_ORGANISATION_PROFILE_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                    ContentToSend = requestData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                requestData.ErrCode = httpData.ErrCode;
                if (requestData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data?.HasValues == true)
                    {
                        if (CanAddAdmin)
                        {
                            requestData.OrganisationID = (long)data[nameof(BaseDTO.OrganisationID)];
                        }
                        else
                        {
                            _essentials.SetPreferenceValue(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, (string)data[nameof(BaseDTO.OrganisationID)]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync organisation view from service
        /// </summary>
        /// <param name="organisationData">Organisation view data reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Organisation view data received from server</returns>
        public async Task SyncOrganisationViewFromServerAsync(OrganisationDTO organisationData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_ORGANISATION_VIEW_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        {
                            Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(organisationData.RecordCount, CultureInfo.InvariantCulture)
                        }
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                organisationData.ErrCode = httpData.ErrCode;
                if (organisationData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(organisationData, data);
                        MapOrganisationViewData(data, organisationData);
                    }
                }
            }
            catch (Exception ex)
            {
                organisationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapOrganisationViewData(JToken data, OrganisationDTO organisationData)
        {
            if (data[nameof(OrganisationDTO.OrganisationProfile)]?.Count() > 0)
            {
                organisationData.OrganisationProfile = new OrganisationModel
                {
                    OrganisationName = (string)data[nameof(OrganisationDTO.OrganisationProfile)][nameof(OrganisationModel.OrganisationName)],
                    OrganisationDomain = (string)data[nameof(OrganisationDTO.OrganisationProfile)][nameof(OrganisationModel.OrganisationDomain)],
                    AddedON = (DateTimeOffset)data[nameof(OrganisationDTO.OrganisationProfile)][nameof(OrganisationModel.AddedON)],
                };
            }
            organisationData.ErrCode = (ErrorCode)(int)data[nameof(OrganisationDTO.ErrCode)];
        }



        public async Task MapAndSaveOrganisationAsync(DataSyncModel result, JToken data)
        {
            try
            {
                OrganisationDTO userData = new OrganisationDTO
                {
                    Organisations = MapOrganisations(data, nameof(DataSyncDTO.Organisations))
                };
                if (GenericMethods.IsListNotEmpty(userData.Organisations))
                {
                    await new OrganisationDatabase().SaveOrganisationDataAsync(userData).ConfigureAwait(false);
                    result.RecordCount = userData.Organisations?.Count ?? 0;
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        protected List<OrganisationModel> MapOrganisations(JToken data, string collactionName)
        {
            return (data[collactionName]?.Count() > 0)
                ? (from dataItem in data[collactionName]
                   select new OrganisationModel
                   {
                       OrganisationID = GetDataItem<long>(dataItem, nameof(OrganisationModel.OrganisationID)),
                       OrganisationName = GetDataItem<string>(dataItem, nameof(OrganisationModel.OrganisationName)),
                       ParentID = GetDataItem<long>(dataItem, nameof(OrganisationModel.ParentID)),
                       DepartmentID = GetDataItem<long>(dataItem, nameof(OrganisationModel.DepartmentID)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(OrganisationModel.IsActive)),
                   }).ToList() : null;
        }

        /// <summary>
        /// Sync organisations from service
        /// </summary>
        /// <param name="organisationData">filter data</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>list of organsations and operation status</returns>
        public async Task SyncOrganisationsFromServerAsync(OrganisationDTO organisationData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_ORGANISATIONS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        {
                            Constants.SE_ORGANISATION_ID_FOR_SETUP_QUERY_KEY,
                            Convert.ToString(organisationData.OrganisationID, CultureInfo.InvariantCulture)
                        },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(organisationData.RecordCount, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                organisationData.ErrCode = httpData.ErrCode;
                if (organisationData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(organisationData, data);
                        MapOrganisationsData(data, organisationData);
                    }
                }
            }
            catch (Exception ex)
            {
                organisationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapOrganisationsData(JToken data, OrganisationDTO organisationData)
        {
            LibSettings.TryGetDateFormatSettings(organisationData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            organisationData.Organisations = (data[nameof(OrganisationDTO.Organisations)]?.Count() > 0)
                ? (from dataItem in data[nameof(OrganisationDTO.Organisations)]
                   select new OrganisationModel
                   {
                       OrganisationID = (long)dataItem[nameof(OrganisationModel.OrganisationID)],
                       OrganisationName = (string)dataItem[nameof(OrganisationModel.OrganisationName)],
                       OrganisationDomain = (string)dataItem[nameof(OrganisationModel.OrganisationDomain)],
                       IsActive = (bool)dataItem[nameof(OrganisationModel.IsActive)],
                       CurrentStatus = GetStatus((bool)dataItem[nameof(OrganisationModel.IsActive)]),
                       TaxNumber = (string)dataItem[nameof(OrganisationModel.TaxNumber)],
                       AddedON = (DateTimeOffset)dataItem[nameof(OrganisationModel.AddedON)],
                       AddedOnString = GenericMethods.GetLocalDateTimeBasedOnCulture((DateTimeOffset)dataItem[nameof(OrganisationModel.AddedON)], DateTimeType.Date, dayFormat, monthFormat, yearFormat),
                       NoOfEmployee = (long)dataItem[nameof(OrganisationModel.NoOfEmployee)],
                       NoOfPatient = (long)dataItem[nameof(OrganisationModel.NoOfPatient)],
                   }).ToList()
                 : null;

            organisationData.ErrCode = (ErrorCode)(int)data[nameof(OrganisationDTO.ErrCode)];
        }

        private string GetStatus(bool isActiveStatus)
        {
            string currentStatus = isActiveStatus ? Constants.ACTIVE_VARIABLE : Constants.INACTIVE_VARIABLE;
            string statusStyle;
            if (isActiveStatus)
            {
                statusStyle = "badge-done ";
            }
            else
            {
                statusStyle = "badge-error ";
            }
            return $"<label class ={statusStyle}>&nbsp{currentStatus}</label>";
        }
    }
}
