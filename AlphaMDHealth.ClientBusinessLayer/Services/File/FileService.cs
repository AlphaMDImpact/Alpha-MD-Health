using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AppPermissions = AlphaMDHealth.Utility.AppPermissions;

namespace AlphaMDHealth.ClientBusinessLayer
{
    /// <summary>
    /// File service class
    /// </summary>
    public partial class FileService : BaseService
    {
        public FileService(IEssentials essentials) : base(essentials)
        {

        }
        /// <summary>
        /// Get Files data
        /// </summary>
        /// <param name="fileData">Object containing documents</param>
        /// <returns>return documents Datat</returns>
        public async Task GetFilesAsync(FileDTO fileData)
        {
            try
            {
                fileData.File.UserID = fileData.SelectedUserID = GetUserID();
                if (MobileConstants.IsMobilePlatform)
                {
                    await Task.WhenAll(
                        GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                        GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_PATIENT_DOCUMNET_GROUP, GroupConstants.RS_CONTACT_PAGE_GROUP),
                        GetFeaturesAsync(AppPermissions.PatientFileAddEdit.ToString(), AppPermissions.PatientFileDelete.ToString(), AppPermissions.PatientFilesView.ToString()),
                        new FileCategoryDatabase().GetFileCategoryOptionsAsync(fileData),
                        fileData.RecordCount == -1
                            ? new FilesDatabase().GetFileAsync(fileData)
                            : new FilesDatabase().GetFilesAsync(fileData)
                        ).ConfigureAwait(false);
                }
                else
                {
                    await SyncFilesFromServerAsync(fileData, CancellationToken.None).ConfigureAwait(false);
                }
                GetFilesUIData(fileData);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                fileData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// get file icon 
        /// </summary>
        /// <param name="document">FileDocumentModel to map image source and default icon</param>
        public void GetFileIcon(FileDocumentModel document)
        {
            //todo:
            //document.DocumentImage = GetFileIcon(GetFileExtension(document.FileDocumentName), document.DocumentName, out ImageSource source);
            //document.ImageSource = source;
        }

        /// <summary>
        /// Update Document read status to database
        /// </summary>
        /// <param name="fileDocument">ID to update status of file</param>
        /// <returns>Operation status</returns>
        public async Task<ErrorCode> UpdateDocumentStatusAsync(FileDocumentModel fileDocument)
        {
            try
            {
                await new FilesDatabase().UpdateDocumentStatusAsync(fileDocument).ConfigureAwait(false);
                return ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return ErrorCode.ErrorWhileSavingRecords;
            }
        }

        /// <summary>
        /// Delete document from database
        /// </summary>
        /// <param name="fileData">Object containing file ID to delete</param>
        /// <returns>Operation status</returns>
        public async Task DeleteFileAsync(FileDTO fileData)
        {
            try
            {
                if (fileData.File != null)
                {
                    fileData.LastModifiedON = GenericMethods.GetUtcDateTime;
                }
                if (MobileConstants.IsMobilePlatform)
                {
                    await new FilesDatabase().DeleteFileAsync(fileData.File.FileID, fileData.LastModifiedON).ConfigureAwait(false);
                    fileData.ErrCode = ErrorCode.OK;
                }
                else
                {
                    await SyncDeletedFileToServerAsync(fileData, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                fileData.ErrCode = ErrorCode.ErrorWhileDeletingRecords;
            }
        }

        /// <summary>
        /// Generates GUID in case of add and handles all save cases
        /// </summary>
        /// <param name="documentData">Data to be saved</param>
        /// <returns>Operation status</returns>
        public async Task SaveDocumentAsync(FileDTO documentData, bool isQuestionaData = false)
        {
            try
            {
                documentData.File.UserID = GetUserID();
                documentData.File.AddedOn = GenericMethods.GetUtcDateTime; //it will also use in edit case also for LastmodifiedON Date
                if (documentData.File.FileID == Guid.Empty)
                {
                    documentData.File.FileID = GenericMethods.GenerateGuid();
                    if (MobileConstants.IsMobilePlatform)
                    {
                        documentData.File.AddedByID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                    }
                }
                documentData.File.IsSynced = false;
                documentData.Files = new List<FileModel> { documentData.File };
                FormatDocumentsData(documentData);
                if (isQuestionaData == false)
                {
                    if (MobileConstants.IsMobilePlatform)
                    {
                        await new FilesDatabase().SaveFileAndDocumentsAsync(documentData).ConfigureAwait(false);
                    }
                    else
                    {
                        await SyncFilesToServerAsync(documentData, CancellationToken.None).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                documentData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Prearing file data for saving against question
        /// </summary>
        /// <param name="docModel">File data for questionnaire data</param>
        public void MapQuestionnaireFileData(long categoryID, FileDocumentModel docModel)
        {
            docModel.DocumentStatus = ResourceConstants.R_NEW_STATUS_KEY;
            docModel.IsDownloaded = true;
            docModel.IsSynced = false;
            docModel.FileCategoryID = categoryID;
            if (docModel.FileDocumentID == Guid.Empty)
            {
                FormatDocumentData(docModel.FileID, docModel);
            }
        }

        private void GetFilesUIData(FileDTO fileData)
        {
            if (fileData.ErrCode == ErrorCode.OK)
            {
                if (!MobileConstants.IsMobilePlatform)
                {
                    SetResourcesAndSettings(fileData);
                }
                else if (fileData.RecordCount == -1)
                {
                    AddPlaceHolder(fileData.CategoryOptions);
                }
                LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                string deleteValue = LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DELETE_ACTION_KEY);
                if (GenericMethods.IsListNotEmpty(fileData.Files))
                {
                    fileData.Files.ForEach(file =>
                    {
                        file.FormattedNumberOfFiles = Convert.ToInt32(file.FormattedNumberOfFiles, CultureInfo.InvariantCulture) > 0
                            ? string.Concat(file.FormattedNumberOfFiles, Constants.STRING_SPACE, LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_FILES_TEXT_KEY))
                            : string.Empty;
                        file.IsUnreadHeader = Convert.ToInt32(file.NumberOfFiles, CultureInfo.InvariantCulture) > 0;
                        file.NumberOfFiles = file.IsUnreadHeader
                            ? file.NumberOfFiles
                            : string.Empty;
                        file.FormattedDate = string.Format(LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_UPLOADED_AT_TEXT_KEY), GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(file.AddedOn), DateTimeType.Date, dayFormat, monthFormat, yearFormat)) ;
                        //file.FileImage = MobileConstants.IsMobilePlatform
                        //    ? ImageConstants.I_FILES_ICON_SVG
                        //    : ImageConstants.I_FILES_ICON_WEB_SVG;
                        if (MobileConstants.IsMobilePlatform)
                        {
                            if (!string.IsNullOrWhiteSpace(file.FileImage) && !file.FileImage.Contains(Constants.HTTP_TAG_PREFIX))
                            {
                                //todo:
                                //file.ImageSource = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(file.FileImage));
                            }
                            else
                            {
                                file.FileImage = ImageConstants.I_FILES_ICON_SVG;
                            }
                        }
                        file.FileImage = ImageConstants.PATIENT_FILES_SVG;
                    });
                }
                MapFileDocumentData(fileData, dayFormat, monthFormat, yearFormat, deleteValue);
            }
        }

        private AppFileExtensions GetFileExtension(string file)
        {
            return string.IsNullOrWhiteSpace(file)
                ? AppFileExtensions.none
                : file.Split(Constants.DOT_SEPARATOR)[1].ToEnum<AppFileExtensions>();
        }

        private void MapFileDocumentData(FileDTO fileAndDocumentData, string dayFormat, string monthFormat, string yearFormat, string deleteValue)
        {
            if (GenericMethods.IsListNotEmpty(fileAndDocumentData.FileDocuments))
            {
                fileAndDocumentData.FileDocuments.ForEach(document =>
                {
                    FormatDocumentData(dayFormat, monthFormat, yearFormat, deleteValue, document);
                });
            }
            else
            {
                fileAndDocumentData.FileDocuments = new List<FileDocumentModel>();
            }
        }

        public void FormatDocumentData(string dayFormat, string monthFormat, string yearFormat, string deleteValue, FileDocumentModel document)
        {
            document.ClientFileDocumentID = document.FileDocumentID;
            document.IsUnreadHeader = document.DocumentStatus == ResourceConstants.R_NEW_STATUS_KEY;
            var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
            document.ShowUnreadBadge = document.IsUnreadHeader
                && (MobileConstants.IsMobilePlatform
                && (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                    ? document.AddedForAccountID != document.AddedByID
                    : document.AddedForAccountID == document.AddedByID);
            document.ShowRemoveButton = document.AddedByID == _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
            document.FormattedDate = string.Concat(
                LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DOCUMENT_FILE_UPLOADEDBY_TEXT_KEY), Constants.STRING_SPACE,
                document.ShowRemoveButton ? LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DOCUMENT_FILE_YOU_TEXT_KEY) : document.AddedByUserName, Constants.STRING_SPACE,
                GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(document.AddedOn), DateTimeType.Date, dayFormat, monthFormat, yearFormat));
            document.FormattedStyle = string.Concat(
                LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DOCUMENT_FILE_UPLOADEDBY_TEXT_KEY), Constants.STRING_SPACE,
                document.ShowRemoveButton ? LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_DOCUMENT_FILE_YOU_TEXT_KEY) : document.AddedByUserName);
            document.AddedByUserName = $"<div class='margin-horizontal-md col-6 px-0 d-flex flex-column'>" +
                              $"<label class='lbl-primary-text-body-large-semi-bold text-start truncate' style='max-width: 500px;'>{{0}}</label>" +
                              $"<label class='lbl-secondary-text-body-medium-regular text-start truncate' style='line-height:1.5;'>{document.FormattedDate}</label> " +
                              $" </div> ";
            document.ShowRemoveButtonText = document.ShowRemoveButton ? deleteValue : string.Empty;
            GetFileIcon(document);
        }
    }
}