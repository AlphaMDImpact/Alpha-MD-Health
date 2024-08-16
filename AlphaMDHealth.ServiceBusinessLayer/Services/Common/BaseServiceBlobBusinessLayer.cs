using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public partial class BaseServiceBusinessLayer
    {
        /// <summary>
        /// Perform documents upload operation
        /// </summary>
        /// <param name="documents">documents to upload</param>
        /// <returns>Operation status and documents data</returns>
        protected async Task<FileUploadDTO> UploadDocumentsToBlobAsync(FileUploadDTO documents)
        {
            return await DocumentBlobOperationAsync(documents, UrlConstants.UPLOAD_FILE_STORAGE_ASYNC).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch dummy cdn link with actual replacement link from blod storage service
        /// </summary>
        /// <param name="documents">documents to upload</param>
        /// <returns>Operation status and documents data</returns>
        protected async Task<FileUploadDTO> GetDocumentsCdnLinksAsync(FileUploadDTO documents)
        {
            return await DocumentBlobOperationAsync(documents, UrlConstants.GET_FILE_STORAGE_REPLACEMENT_CDN_LINK_ASYNC).ConfigureAwait(false);
        }

        private async Task<FileUploadDTO> DocumentBlobOperationAsync(FileUploadDTO documents, string pathWithoutBasePath)
        {
            documents.Settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, 0, default, 0, documents.OrganisationID, false).ConfigureAwait(false)).Settings;
            if (GenericMethods.IsListNotEmpty(documents.Settings))
            {
                string clientIdentifier = GetSettingValueByKey(documents.Settings, SettingsConstants.S_BLOB_MICRO_SERVICE_KEY);
                var httpData = new HttpServiceModel<FileUploadDTO>
                {
                    BaseUrl = new Uri(UrlConstants.MICRO_SERVICE_PATH),
                    PathWithoutBasePath = pathWithoutBasePath,
                    AuthType = AuthorizationType.Basic,
                    ClientIdentifier = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[1],
                    ClientSecret = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[2],
                    ForApplication = clientIdentifier.Split(Constants.SYMBOL_PIPE_SEPERATOR)[0],
                    ContentToSend = documents
                };
                await new HttpLibService(new HttpService()).PostAsync(httpData).ConfigureAwait(false);
                if (httpData.ErrCode == ErrorCode.OK)
                {
                    documents = JsonConvert.DeserializeObject<FileUploadDTO>(httpData.Response);
                }
                documents.ErrCode = httpData.ErrCode;
            }
            else
            {
                documents.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return documents;
        }

        protected FileDataModel CreateFileObject(string recordID, string file, bool hasMultiple)
        {
            return new FileDataModel
            {
                HasMultiple = hasMultiple,
                Base64File = file,
                RecordID = recordID,
            };
        }

        protected FileUploadDTO CreateFileDataObject(FileTypeToUpload fileType, string ID = null)
        {
            var files = new FileUploadDTO
            {
                FileTypeUploading = fileType,
                FileContainers = new List<FileContainerModel>()
            };
            if (!string.IsNullOrWhiteSpace(ID))
            {
                files.FileContainers.Add(new FileContainerModel
                {
                    ID = ID,
                    FileData = new List<FileDataModel>()
                });
            }
            return files;
        }

        protected FileUploadDTO CreateFileDataObjectWithFileName(FileTypeToUpload fileType, string FileName, string ID = null)
        {
            var files = new FileUploadDTO
            {
                FileTypeUploading = fileType,
                FileName = FileName,
                FileContainers = []
            };
            if (!string.IsNullOrWhiteSpace(ID))
            {
                files.FileContainers.Add(new FileContainerModel
                {
                    ID = ID,
                    FileData = []
                });
            }
            return files;
        }

        protected FileUploadDTO CreateSingleFileDataObject(FileTypeToUpload fileType, string ID, string image)
        {
            return new FileUploadDTO
            {
                FileTypeUploading = fileType,
                FileContainers = new List<FileContainerModel> {
                    new FileContainerModel {
                        ID = ID,
                        FileData =  new List<FileDataModel> {
                            CreateFileObject(ID, image, false)
                        }
                    }
                }
            };
        }

        protected string GetFirstBase64File(FileUploadDTO files)
        {
            return files.FileContainers.FirstOrDefault().FileData.FirstOrDefault()?.Base64File ?? string.Empty;
        }

        protected string GetBase64FileFromFirstContainer(FileUploadDTO files, string recordID)
        {
            return files.FileContainers[0].FileData.FirstOrDefault(x => x.RecordID == recordID)?.Base64File ?? string.Empty;
        }

        protected async Task<string> ReplaceCDNLinkAsync(string inputStr, BaseDTO cdnCacheData)
        {
            System.Text.RegularExpressions.Regex tagRegex = new(Constants.HTML_TAG_CHECK);
            bool hasTags = tagRegex.IsMatch(inputStr); //Check if content is HTML 
            if (hasTags)
            {
                HtmlAgilityPack.HtmlDocument doc = new();
                doc.LoadHtml(inputStr);
                var imgs = doc.DocumentNode.SelectNodes(@"//img[@src]");
                if (imgs != null)
                {
                    foreach (var img in imgs)
                    {
                        string orig = img.Attributes["src"].Value;
                        var link = await ReplaceCDNLinkAsync(orig, cdnCacheData);
                        //do replacements on orig to a new string, newsrc
                        inputStr = inputStr.Replace(orig, link);
                    }
                }
                return inputStr;
            }
            else
            {
                var data = await GetBlobStorageCdnLinkFromCache(inputStr);
                if (data != null)
                {
                    cdnCacheData.AddedBy = data.AddedBy;
                    cdnCacheData.LastModifiedBy = data.LastModifiedBy;
                    cdnCacheData.EmailID = data.EmailID;
                }
                inputStr = $"{inputStr}?{cdnCacheData.EmailID}";
                return GenericMethods.ReplaceString(inputStr, cdnCacheData.AddedBy, cdnCacheData.LastModifiedBy);
            }
        }
    }
}