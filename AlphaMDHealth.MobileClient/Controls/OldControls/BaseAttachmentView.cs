using System.Globalization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Base Attachment View
    /// </summary>
    public abstract class BaseAttachmentView : ViewManager
    {
        /// <summary>
        /// Content Page reference property
        /// </summary>        
        protected readonly BasePage _parent;

        /// <summary>
        /// ImageBase64 String 
        /// </summary>
        protected string _base64String = ImageConstants.I_UPLOAD_ICON_PNG;

        /// <summary>
        /// Uploaded File Extension
        /// </summary>
        protected string _uploadedFileExtension;

        /// <summary>
        /// Base 64 prefix
        /// </summary>
        protected string _base64Prefix;

        /// <summary>
        /// File Name with Extension
        /// </summary>
        public string FileNameWithExtention { get; set; }
        /// <summary>
        /// File Name with Extension
        /// </summary>
        public Stream FileStream { get; set; }

        /// <summary>
        /// File Maximum Upload Size
        /// </summary>
        public string MaxFileUploadSize { get; set; }

        /// <summary>
        /// Message Attachment Byte Array
        /// </summary>
        public byte[] MessageAttachment { get; set; }

        /// <summary>
        /// Supported File Types 
        /// </summary>
        public string SupportedFileTypes { get; set; }

        /// <summary>
        /// Show Metadata
        /// </summary>
        public bool IsStreamSave { get; set; }

        private readonly string _base64PrefixFormat = "data:{0}/{1};base64,";

        /// <summary>
        /// Patient readings list
        /// </summary>
        /// <param name="page">Instance of base page</param>
        /// <param name="parameters">View parameters</param>
        protected BaseAttachmentView(BasePage page, object parameters) : base(page, parameters)
        {
            _parent = page;
        }

        /// <summary>
        /// Get Base64 with Prefix
        /// </summary>
        /// <returns></returns>
        public string GetBase64WithPrefix()
        {
            return _base64Prefix + _base64String;
        }

        /// <summary>
        /// Adds an action  
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns>List of Actions</returns>
        public List<string> Actionlist(FieldTypes controlType)
        {
            List<string> actions = new List<string>();
            if (controlType == FieldTypes.UploadControl)
            {
                actions.Add(_parent.GetResourceValueByKey(ResourceConstants.R_TAKE_PHOTO_FROM_CAMERA_TEXT_KEY));
                actions.Add(_parent.GetResourceValueByKey(ResourceConstants.R_CHOOSE_PHOTO_FROM_GALLERY_TEXT_KEY));
                actions.Add(_parent.GetResourceValueByKey(ResourceConstants.R_UPLOAD_DOCUMENT_TEXT_KEY));
            }
            else
            {
                if (controlType == FieldTypes.UploadControl)
                {
                    actions.Add(_parent.GetResourceValueByKey(ResourceConstants.R_TAKE_PHOTO_FROM_CAMERA_TEXT_KEY));
                    actions.Add(_parent.GetResourceValueByKey(ResourceConstants.R_CHOOSE_PHOTO_FROM_GALLERY_TEXT_KEY));
                }
                else
                {
                    if (controlType == FieldTypes.UploadControl)
                    {
                        actions.Add(_parent.GetResourceValueByKey(ResourceConstants.R_UPLOAD_DOCUMENT_TEXT_KEY));
                    }
                }
            }
            return actions;
        }

        /// <summary>
        /// Adds an Image Source to a Selection
        /// </summary>
        /// <param name="actions">List of actions</param>
        /// <returns></returns>
        public async Task ImageSourceSelectionAsync(List<string> actions)
        {
            var action = await Application.Current.MainPage.DisplayActionSheet(null,
               _parent.GetResourceValueByKey(ResourceConstants.R_CANCEL_ACTION_KEY), _base64String.Length > 100 ? _parent.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY) : null,
                actions.ToArray()).ConfigureAwait(true);
            if (action == _parent.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY))
            {
                DeleteUploads();
            }
            if (action != null && action != _parent.GetResourceValueByKey(ResourceConstants.R_CANCEL_ACTION_KEY))
            {
                App._essentials.SetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, true);
                await InitializeMediaCaptureAsync(action.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
            }
            else
            {
                _base64String = ImageConstants.I_UPLOAD_ICON_PNG;
            }
        }

        private async Task InitializeMediaCaptureAsync(string captureMethod)
        {
            //When camera is switched on app goes in background mode and to avoid MasterPage refreshing this property is set to true
            App._essentials.SetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, true);

            if (captureMethod == _parent.GetResourceValueByKey(ResourceConstants.R_CHOOSE_PHOTO_FROM_GALLERY_TEXT_KEY))
            {
                if (await PermissionHelper.CheckPermissionStatusAsync(Permission.Photos, _parent).ConfigureAwait(true))
                {
                    try
                    {
                        //todo
                        //MediaFile pickedFile = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions { }).ConfigureAwait(true);
                        //if (pickedFile != null)
                        //{
                        //    byte[] image = GetImage(pickedFile);

                        //    _base64String = Convert.ToBase64String(image);
                        //}
                    }
                    catch
                    {
                        await _parent.DisplayMessagePopupAsync($"{Permission.Storage}{ResourceConstants.R_PERMISSION_TEXT_KEY}", false, true, false).ConfigureAwait(true);
                        _base64String = ImageConstants.I_UPLOAD_ICON_PNG;
                    }
                }
            }
            else if (captureMethod == _parent.GetResourceValueByKey(ResourceConstants.R_UPLOAD_DOCUMENT_TEXT_KEY))
            {
                if (await PermissionHelper.CheckPermissionStatusAsync(Permission.Storage, _parent).ConfigureAwait(true))
                {
                    try
                    {
                        //todo
                        //FileData fileData = await CrossFilePicker.Current.PickFile().ConfigureAwait(true);
                        //if (fileData == null)
                        //{
                        //    //When camera is switched off app comes in foreground and to start MasterPage refreshing this property is set to false
                        //    _ = Task.Delay(300).ContinueWith((task) =>
                        //    {
                        //        //When camera is switched off app comes in foreground and to start MasterPage refreshing this property is set to false
                        //        App._essentials.SetPreferenceValue(LibStorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false);
                        //    });
                        //    return;
                        //}
                        //else
                        //{
                        //    _uploadedFileExtension = fileData.FileName?.Split(LibConstants.DOT_SEPARATOR).Last();
                        //    if (LibGenericMethods.IsExtensionSupported(SupportedFileTypes, _uploadedFileExtension))
                        //    {
                        //        GetPrefix(_uploadedFileExtension);
                        //        FileNameWithExtention = fileData.FileName;
                        //        MessageAttachment = fileData.DataArray;
                        //        if (IsStreamSave)
                        //        {
                        //            FileStream = fileData.GetStream();
                        //        }
                        //        _base64String = Convert.ToBase64String(fileData.DataArray);
                        //    }
                        //    else
                        //    {
                        //        await _parent.DisplayMessagePopupAsync(string.Format(CultureInfo.InstalledUICulture, _parent.GetResourceValueByKey(LibResourceConstants.R_SUPPORTED_UPLOAD_FILE_TYPE_KEY), SupportedFileTypes, CultureInfo.InvariantCulture), false, true, true).ConfigureAwait(true);
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        App._essentials.SetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false);
                        await _parent.DisplayMessagePopupAsync(ResourceConstants.R_CORRUPTED_DOCUMENT_TEXT_KEY, false, true, false).ConfigureAwait(false);
                        _base64String = ImageConstants.I_UPLOAD_ICON_PNG;
                        return;
                    }
                }
            }
            else
            {
                await UploadPictureAsync(captureMethod).ConfigureAwait(true);
            }
            AppHelper.ShowBusyIndicator = false;
            _ = Task.Delay(500).ContinueWith((task) =>
            {
                //When camera is switched off app comes in foreground and to start MasterPage refreshing this property is set to false
                App._essentials.SetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false);
            });
        }

        private async Task UploadPictureAsync(string captureMethod)
        {
            if (captureMethod == _parent.GetResourceValueByKey(ResourceConstants.R_TAKE_PHOTO_FROM_CAMERA_TEXT_KEY)
                && await PermissionHelper.CheckPermissionStatusAsync(Permission.Camera, _parent).ConfigureAwait(true)
                && await PermissionHelper.CheckPermissionStatusAsync(Permission.Storage, _parent).ConfigureAwait(true))
            {
                try
                {
                    //todo
                    //await CrossMedia.Current.Initialize().ConfigureAwait(true);
                    //MediaFile pickedFile = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    //{
                    //    RotateImage = LibGenericMethods.GetPlatformSpecificValue(false, true, false),
                    //    SaveMetaData = LibGenericMethods.GetPlatformSpecificValue(false, true, false),
                    //}).ConfigureAwait(true);
                    //if (pickedFile != null)
                    //{
                    //    byte[] image = GetImage(pickedFile);
                    //    _base64String = Convert.ToBase64String(image);
                    //}
                }
                catch
                {
                    await _parent.DisplayMessagePopupAsync($"{Permission.Camera}{ResourceConstants.R_PERMISSION_TEXT_KEY}", false, true, false).ConfigureAwait(false);
                    _base64String = ImageConstants.I_UPLOAD_ICON_PNG;
                }
            }
        }
        private void GetPrefix(string fileType)
        {
            switch (fileType)
            {
                case "pdf":
                    _base64Prefix = string.Format(CultureInfo.InvariantCulture, _base64PrefixFormat, Constants.APPLICATION_STRING, ImageConstants.PDF_TAG);
                    break;
                case "doc":
                    _base64Prefix = string.Format(CultureInfo.InvariantCulture, _base64PrefixFormat, Constants.APPLICATION_STRING, ImageConstants.DOC_TAG);
                    break;
                case "docx":
                    _base64Prefix = string.Format(CultureInfo.InvariantCulture, _base64PrefixFormat, Constants.APPLICATION_STRING, ImageConstants.DOCX_TAG);
                    break;
                case "xls":
                    _base64Prefix = string.Format(CultureInfo.InvariantCulture, _base64PrefixFormat, Constants.APPLICATION_STRING, ImageConstants.XLS_TAG);
                    break;
                case "xlsx":
                    _base64Prefix = string.Format(CultureInfo.InvariantCulture, _base64PrefixFormat, Constants.APPLICATION_STRING, ImageConstants.XLSX_TAG);
                    break;
                case "jpeg":
                case "jpg":
                case "png":
                    _base64Prefix = string.Format(CultureInfo.InvariantCulture, _base64PrefixFormat, "image", _uploadedFileExtension);
                    break;
                default:
                    // to be implemented
                    break;
            }
        }
        //todo
        //private byte[] GetImage(MediaFile mediaFile)
        //{
        //    if (mediaFile != null)
        //    {
        //        string[] fileNameWithExtension = mediaFile.Path.Split(LibConstants.SYMBOL_SLASH).Last().Split(LibConstants.DOT_SEPARATOR);
        //        FileNameWithExtention = fileNameWithExtension.First() + LibConstants.SYMBOL_DOT_STRING + fileNameWithExtension.Last();
        //        _uploadedFileExtension = fileNameWithExtension.Last();
        //        GetPrefix(_uploadedFileExtension);
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            var maxUploadSize = MaxFileUploadSize.Split('x');
        //            ICompression imageCompressionService = DependencyService.Get<ICompression>();
        //            mediaFile.GetStream().CopyTo(ms);
        //            return imageCompressionService.ResetImage(ms.ToArray(), Convert.ToSingle(maxUploadSize[0], CultureInfo.InvariantCulture), Convert.ToSingle(maxUploadSize[1], CultureInfo.InvariantCulture));
        //        }
        //    }
        //    else
        //    {
        //        return new byte[0];
        //    }
        //}

        /// <summary>
        /// Abstract Method to Delete uploads
        /// </summary>
        protected abstract void DeleteUploads();
    }
}