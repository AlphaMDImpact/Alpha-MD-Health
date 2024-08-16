using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient
{
    public class PatientFileView : BaseAttachmentView
    {
        private readonly CustomButtonControl _deleteButton;
        private readonly CustomButtonControl _uploadButton;
        private readonly CustomLabelControl _detailsSubHeader;
        private readonly CustomBindablePickerControl _categoriesPicker;
        private readonly CustomLabelControl _filesSubHeader;
        private readonly Label _documentDescriptionHeaderLabel;
        private readonly CustomLabelControl _documentDescription;
        private readonly CustomListView _filesList;
        internal readonly BasePage _parentPage;
        private readonly CustomMessageControl _emptyListView;
        private readonly List<FileDocumentModel> _removedDocumentList;
        private readonly FileDTO _documentData = new FileDTO { File = new FileModel() };
        private bool _isAttachmentDeleteClicked;
        private bool _isProviderLogin;
        private string _selectedID;
        private readonly Grid _bodyView;

        /// <summary>
        /// View to display profile page components
        /// </summary>
        /// <param name="page">Instance of base page</param>
        /// <param name="parameters">View parameters</param>
        public PatientFileView(BasePage page, object parameters) : base(page, parameters)
        {
            _parentPage = page;
            _parentPage.PageService = new FileService(App._essentials);
            _removedDocumentList = new List<FileDocumentModel>();
            _deleteButton = new CustomButtonControl(ButtonType.TransparentWithMargin)
            {
                TextColor = Color.FromArgb(StyleConstants.ERROR_COLOR),
                IsVisible = false,
                VerticalOptions = LayoutOptions.End
            };
            _emptyListView = new CustomMessageControl(false);
            _uploadButton = new CustomButtonControl(ButtonType.PrimaryWithMargin) { VerticalOptions = LayoutOptions.End };
            _detailsSubHeader = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft) { HeightRequest = 60 };
            _filesSubHeader = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft) { HeightRequest = 60 };
            _categoriesPicker = new CustomBindablePickerControl
            {
                ControlResourceKey = ResourceConstants.R_SELECT_File_CATEGORY_KEY,
                IsUnderLine = true
            };
            _documentDescriptionHeaderLabel = new Label
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_FORMATTED_LABEL_SECONDARY_TEXT_COLOR_SMALL_STYLE],
                StyleId = "HeaderLabel"
            };
            _documentDescription = new CustomLabelControl(LabelType.PrimarySmallLeft)
            {
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 5),
            };
            CustomCellModel filesModel = new CustomCellModel
            {
                CellID = nameof(FileDocumentModel.ClientFileDocumentID),
                CellHeader = nameof(FileDocumentModel.DocumentDescription),
                CellDescription = nameof(FileDocumentModel.FormattedDate),
                //todo:CellLeftSourceIcon = nameof(FileDocumentModel.ImageSource),
                CellLeftDefaultIcon = nameof(FileDocumentModel.DocumentImage),
                ShowUnreadBadge = nameof(FileDocumentModel.ShowUnreadBadge),
                IsUnreadHeader = nameof(FileDocumentModel.IsUnreadHeader),
                CellRightContentHeader = nameof(FileDocumentModel.ShowRemoveButtonText),
                ShowRemoveButton = nameof(FileDocumentModel.ShowRemoveButton)
            };
            _filesList = new CustomListView(ListViewCachingStrategy.RetainElement)
            {
                Style = (Style)App.Current.Resources[StyleConstants.ST_CustomListView_2_ROW_HEIGHT_STYLE],
                Margin = new Thickness(0, 0, 0, 15),
                SelectionMode = ListViewSelectionMode.Single,
                Footer = null,
                ItemTemplate = new DataTemplate(() =>
                {
                    ResponsiveView view = new ResponsiveView(filesModel) { Margin = new Thickness(0, 0, 0, new OnIdiom<double> { Phone = 10, Tablet = 0 }) };
                    view.OnItemClicked += DeleteDocumentClicked;
                    return new ViewCell { View = view };
                })
            };
            _bodyView = new Grid
            {
                Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
                RowDefinitions =
                {
                    //Details
                    new RowDefinition { Height = GridLength.Auto },
                    //Name Entry
                    new RowDefinition { Height = GridLength.Auto },
                    //Description
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    //Files
                    new RowDefinition { Height = GridLength.Auto },
                    //Upload Icon
                    new RowDefinition { Height = GridLength.Auto },
                    //List
                    new RowDefinition { Height =  new GridLength(1 ,GridUnitType.Star) },
                    //Separator
                    new RowDefinition { Height = GridLength.Auto },
                    //Delete button
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) },
                }
            };
            _bodyView.Add(_detailsSubHeader, 0, 0);
            _bodyView.Add(_categoriesPicker, 0, 1);
            _bodyView.Add(_documentDescriptionHeaderLabel, 0, 2);
            _bodyView.Add(_documentDescription, 0, 3);
            _bodyView.Add(_filesSubHeader, 0, 4);
            _bodyView.Add(_uploadButton, 0, 5);
            _bodyView.Add(_filesList, 0, 6);
            _bodyView.Add(_deleteButton, 0, 8);
            //used for popup page from tablet
            Content = new ScrollView
            {
                Content = _bodyView
            };
        }

        /// <summary>
        /// Load UI data of view
        /// </summary>
        /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
        /// <returns>Returns true if required view is found, else return false</returns>
        public override async Task LoadUIAsync(bool isRefreshRequest)
        {
            _isProviderLogin = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(BaseDTO.IsActive)));
            if (App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
            {
                return;
            }
            _documentData.RecordCount = -1;
            _documentData.File.FileID = Guid.Parse(GenericMethods.MapValueType<string>(GetParameterValue(nameof(FileModel.FileID))));
            _documentData.LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
            await (_parentPage.PageService as FileService).GetFilesAsync(_documentData).ConfigureAwait(true);
            _parentPage.PageData = _parent.PageData = _parentPage.PageService.PageData;
            if (_documentData.ErrCode == ErrorCode.OK)
            {
                _uploadButton.Clicked += UploadNewClicked;
                _emptyListView.PageResources = _categoriesPicker.PageResources = _parentPage.PageData;
                _detailsSubHeader.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_DETAILS_KEY);
                _filesSubHeader.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_DOCUMENT_TEXT_KEY);
                _uploadButton.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_UPLOAD_DOCUMENT_TEXT_KEY);
                _deleteButton.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY);
                _documentDescriptionHeaderLabel.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_FILE_DESCRIPTION_KEY);
                _categoriesPicker.ItemSource = _documentData.CategoryOptions;
                if (_documentData.File.FileID != Guid.Empty)
                {
                    if (HasDeletePermission() && _documentData.File.AddedByID == App._essentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, (long)0))
                    {
                        if (DeviceInfo.Idiom == DeviceIdiom.Tablet && IsPatientPage())
                        {
                            var separator = _parentPage.CreateLinkSeperator(true);
                            separator.Margin = new Thickness(-(double)App.Current.Resources[StyleConstants.ST_APP_PADDING], 0);
                            _bodyView.Add(separator, 0, 7);
                        }
                        _deleteButton.Clicked += DeleteFileClicked;
                        _deleteButton.IsVisible = true;
                    }
                    SelectCategory(_documentData.File.FileCategoryID);
                }
                RenderFileDocumentsList();
                _filesList.ItemTapped += FilesList_ItemTapped;
                _categoriesPicker.SelectedValuesChanged += OnCategoriesPickerSelectedValuesChanged;
            }
            else
            {
                await RenderErrorAsync();
            }
        }

        private void SelectCategory(long categoryID)
        {
            var selectedCategory = _documentData.CategoryOptions.FirstOrDefault(x => x.OptionID == categoryID);
            if (selectedCategory != null && selectedCategory.OptionID > 0)
            {
                _categoriesPicker.SelectedValue = selectedCategory.OptionID;
                _documentDescription.Text = selectedCategory.GroupName;
                _categoriesPicker.IsEnabled = false;
            }
        }

        private void OnCategoriesPickerSelectedValuesChanged(object sender, EventArgs e)
        {
            var selectedCategory = _documentData.CategoryOptions?[((sender) as CustomBindablePicker).SelectedIndex];
            if (selectedCategory != null)
            {
                _documentDescription.Text = selectedCategory.GroupName;
            }
        }

        private async Task RenderErrorAsync()
        {
            if (IsPatientPage() || MobileConstants.IsDevicePhone)
            {
                if (MobileConstants.IsDevicePhone)
                {
                    await _parentPage.DisplayMessagePopupAsync(_documentData.ErrCode.ToString(), OnPopupActionClicked);
                }
                else
                {
                    InvokeListRefresh(_documentData.ErrCode.ToString(), new EventArgs());
                    //todo:await Navigation.PopAllPopupAsync();
                }
            }
            else
            {
                InvokeListRefresh(_documentData.ErrCode.ToString(), new EventArgs());
            }
        }

        private async void OnPopupActionClicked(object sender, int e)
        {
            _parentPage.OnClosePupupAction(sender, e);
            await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
        }

        public override async Task UnloadUIAsync()
        {
            if (App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
            {
                return;
            }
            _filesList.ItemTapped -= FilesList_ItemTapped;
            _uploadButton.Clicked -= UploadNewClicked;
            if (_deleteButton.IsVisible)
            {
                _deleteButton.Clicked -= DeleteFileClicked;
            }
            await Task.CompletedTask;
        }

        public async Task SaveDocumentDataAsync()
        {
            if (await SaveDocumentAsync().ConfigureAwait(true))
            {
                InvokeListRefresh(Guid.Empty, new EventArgs());
                //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Action to close popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void OnPupupActionClicked()
        {
            _parentPage.MessagePopup.PopCustomMessageControlAsync();
            _parentPage.MessagePopup.IsVisible = false;
            if (MobileConstants.IsDevicePhone)
            {
                await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
            }
            else
            {
                if (ShellMasterPage.CurrentShell.CurrentPage is PatientsPage)
                {
                    //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
                }
                InvokeListRefresh(Guid.Empty, new EventArgs());
            }
        }

        private void ShowErrorForFile(bool showError)
        {
            _filesList.Footer = showError
                ? _filesList.Footer ?? new CustomLabelControl(LabelType.ClientErrorLabel)
                {
                    Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_FILE_ATTACHMENT_ERROR_MESSAGE_TEXT_KEY)
                }
                : null;
        }

        private bool HasDeletePermission()
        {
            return _parentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientFileDelete.ToString());
        }

        private void RenderFileDocumentsList()
        {
            if (GenericMethods.IsListNotEmpty(_documentData.FileDocuments))
            {
                AssignListItems();
            }
            else
            {
                _emptyListView.ControlResourceKey = ResourceConstants.R_DOCUMENT_FILE_EMPTY_VIEW_KEY;
                _filesList.Header = _emptyListView;
                _filesList.HeightRequest = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture);
                _filesList.ItemsSource = null;
            }
        }

        private async void DeleteDocumentClicked(object sender, EventArgs e)
        {
            if (!_isAttachmentDeleteClicked)
            {
                _isAttachmentDeleteClicked = true;
                _selectedID = (sender as CustomLabelControl).ClassId;
                await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeDeleteDocumentsActionClicked, true, true, false).ConfigureAwait(true);
            }
        }

        private void AssignListItems()
        {
            _filesList.ItemsSource = null;
            _filesList.Header = null;
            _filesList.ItemsSource = _documentData.FileDocuments;
            _filesList.HeightRequest = (_filesList.RowHeight + new OnIdiom<int> { Phone = 10, Tablet = 0 }) * (_documentData.FileDocuments?.Count ?? 1);
            if (GenericMethods.IsListNotEmpty(_documentData.FileDocuments))
            {
                ShowErrorForFile(false);
            }
        }

        private async void FilesList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            _filesList.ItemTapped -= FilesList_ItemTapped;
            FileDocumentModel document = ((sender as CustomListView).SelectedItem as FileDocumentModel);
            if (document != null && !string.IsNullOrWhiteSpace(document.DocumentName))
            {
                CustomAttachmentModel attachment = new CustomAttachmentModel
                {
                    ID = document.ClientFileDocumentID,
                    AttachmentBase64 = document.DocumentName,
                    FileName = document.FileDocumentName,
                    FileType = GetFileExtension(document.FileDocumentName),
                    //todo:AttachmentSource = document.ImageSource,
                    DefaultIcon = document.DocumentImage,
                    IsActive = false,
                    Text = document.DocumentDescription,
                    IsDisabledSaveButton = !document.ShowRemoveButton,
                    ControlResourceKey = ResourceConstants.R_DOCUMENT_CAPTION_KEY
                };
                await ShowAttachmentViewAsync(attachment).ConfigureAwait(true);
                if (document.ShowUnreadBadge)
                {
                    if (_isProviderLogin)
                    {
                        if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY))
                        {
                            await UnreadBadgeSyncCallAsync(document).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        await UnreadBadgeSyncCallAsync(document).ConfigureAwait(true);
                    }
                }
            }
            AssignListItems();
            _filesList.ItemTapped += FilesList_ItemTapped;
        }

        private async Task UnreadBadgeSyncCallAsync(FileDocumentModel document)
        {
            ErrorCode result = await (_parentPage.PageService as FileService).UpdateDocumentStatusAsync(document).ConfigureAwait(true);
            if (result == ErrorCode.OK)
            {
                var updatedocument = _documentData.FileDocuments.FirstOrDefault(x => x.ClientFileDocumentID == document.ClientFileDocumentID);
                updatedocument.IsUnreadHeader = updatedocument.ShowUnreadBadge = false;
                updatedocument.DocumentStatus = ResourceConstants.R_COMPLETED_STATUS_KEY;
                if (MobileConstants.IsTablet)
                {
                    InvokeListRefresh(_documentData.File.FileID, new EventArgs());
                }
                _ = _parentPage.SyncDataWithServerAsync(Pages.PatientFilePage, ServiceSyncGroups.RSSyncToServerGroup, DataSyncFor.Files, DataSyncFor.Files.ToString(), App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
            }
        }

        private void AttachmentPage_OnSendButtonClicked(object sender, EventArgs e)
        {
            CustomAttachmentModel attachment = sender as CustomAttachmentModel;
            if (!attachment.IsDisabledSaveButton)
            {
                FileDocumentModel document = new FileDocumentModel
                {
                    ClientFileDocumentID = attachment.ID == Guid.Empty ? GenericMethods.GenerateGuid() : attachment.ID,
                    DocumentName = attachment.AttachmentBase64,
                    FileDocumentName = attachment.FileName,
                    DocumentDescription = attachment.Text,
                    IsUnreadHeader = true,
                    IsActive = true,
                    ShowRemoveButton = true,
                    IsDownloaded = true,
                    ShowRemoveButtonText = _parentPage.GetResourceValueByKey(ResourceConstants.R_DELETE_ACTION_KEY),
                    AddedOn = GenericMethods.GetUtcDateTime.ToLocalTime(),
                    FormattedDate = string.Concat(_parentPage.GetResourceValueByKey(ResourceConstants.R_DOCUMENT_FILE_UPLOADEDBY_TEXT_KEY), Constants.STRING_SPACE, _parentPage.GetResourceValueByKey(ResourceConstants.R_DOCUMENT_FILE_YOU_TEXT_KEY), Constants.STRING_SPACE, DateTime.Now.ToString(Constants.DEFAULT_DATE_FORMAT, CultureInfo.InvariantCulture))
                };
                (_parentPage.PageService as FileService).GetFileIcon(document);
                if (attachment.ID == Guid.Empty)
                {
                    _documentData.FileDocuments.Insert(0, document);
                }
                else
                {
                    _documentData.FileDocuments.Find(x => x.ClientFileDocumentID == document.ClientFileDocumentID)
                      .DocumentDescription = attachment.Text?.Trim();
                    _documentData.FileDocuments.Find(x => x.ClientFileDocumentID == document.ClientFileDocumentID)
                      .IsSynced = false;
                }
                AssignListItems();
            }
        }

        private async Task ShowAttachmentViewAsync(CustomAttachmentModel attachment)
        {
            CustomAttachmentPopupPage attachmentPage = new CustomAttachmentPopupPage(attachment, _parentPage);
            attachmentPage.OnSendButtonClicked += AttachmentPage_OnSendButtonClicked;
            //todo:await Navigation.PushPopupAsync(attachmentPage).ConfigureAwait(true);
        }

        private async void UploadNewClicked(object sender, EventArgs e)
        {
            MaxFileUploadSize = _parentPage.GetSettingsValueByKey(SettingsConstants.S_LARGE_IMAGE_RESOLUTION_KEY);
            SupportedFileTypes = _parentPage.GetSettingsValueByKey(SettingsConstants.S_UPLOAD_SUPPORTED_FILE_TYPE_KEY);
            List<string> actions = Actionlist(FieldTypes.UploadControl);
            await ImageSourceSelectionAsync(actions).ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(_base64String) && _base64String != ImageConstants.I_UPLOAD_ICON_PNG)
            {
                if (GenericMethods.IsExtensionSupported(SupportedFileTypes, _uploadedFileExtension))
                {
                    CustomAttachmentModel attachment = new CustomAttachmentModel
                    {
                        AttachmentBase64 = GetBase64WithPrefix(),
                        FileName = FileNameWithExtention,
                        ControlResourceKey = ResourceConstants.R_DOCUMENT_CAPTION_KEY,
                        FileType = _uploadedFileExtension.ToEnum<AppFileExtensions>()
                    };
                    await ShowAttachmentViewAsync(attachment).ConfigureAwait(true);
                }
                else
                {
                    await _parentPage.DisplayMessagePopupAsync(
                           _parentPage.GetResourceValueByKey(ResourceConstants.R_SUPPORTED_UPLOAD_FILE_TYPE_KEY).Replace("{0}", SupportedFileTypes),
                           _parentPage.OnPupupActionClicked, false, true, true).ConfigureAwait(true);
                }
                _base64String = ImageConstants.I_UPLOAD_ICON_PNG;
            }
        }

        private async void DeleteFileClicked(object sender, EventArgs e)
        {
            if (_isProviderLogin)
            {
                if (await _parentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY))
                {
                    await DeleteMessagePopupCallAsync().ConfigureAwait(true);
                }
            }
            else
            {
                await DeleteMessagePopupCallAsync().ConfigureAwait(true);
            }

        }

        private async Task DeleteMessagePopupCallAsync()
        {
            _deleteButton.Clicked -= DeleteFileClicked;
            await _parentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeDeleteFilesActionClicked, true, true, false).ConfigureAwait(true);
            _deleteButton.Clicked += DeleteFileClicked;
        }

        private async void OnMessgeDeleteFilesActionClicked(object sender, int e)
        {
            switch (e)
            {
                case 1:
                    _parentPage.MessagePopup.PopCustomMessageControlAsync();
                    _parentPage.MessagePopup.IsVisible = false;
                    await DeleteFileAsync().ConfigureAwait(true);
                    break;
                case 2:
                    _parentPage.MessagePopup.PopCustomMessageControlAsync();
                    _parentPage.MessagePopup.IsVisible = false;
                    break;
                default:// to do
                    break;
            }
        }

        private void OnMessgeDeleteDocumentsActionClicked(object sender, int e)
        {
            switch (e)
            {
                case 1:
                    _parentPage.OnClosePupupAction(sender, e);
                    DeleteDocument();
                    break;
                case 2:
                    _parentPage.OnClosePupupAction(sender, e);
                    break;
                default:// to do
                    break;
            }
            _selectedID = "";
            _isAttachmentDeleteClicked = false;
        }

        private void DeleteDocument()
        {
            FileDocumentModel document = _documentData.FileDocuments.FirstOrDefault(x => x.ClientFileDocumentID == new Guid(_selectedID));
            if (document != null)
            {
                _documentData.FileDocuments.Remove(document);
                if (document.FileDocumentID != Guid.Empty)
                {
                    document.IsActive = false;
                    document.IsSynced = false;
                    _removedDocumentList.Add(document);
                }
                RenderFileDocumentsList();
            }
        }

        protected override void DeleteUploads()
        {
            throw new NotSupportedException();
        }

        private AppFileExtensions GetFileExtension(string file)
        {
            return file.Split(Constants.DOT_SEPARATOR)[1].ToEnum<AppFileExtensions>();
        }

        private async Task DeleteFileAsync()
        {
            await (_parentPage.PageService as FileService).DeleteFileAsync(_documentData).ConfigureAwait(true);
            await HandelSaveResponseAsync(true);
        }

        public async Task<bool> SaveDocumentAsync()
        {
            if (_parentPage.IsFormValid(this))
            {
                if (!GenericMethods.IsListNotEmpty(_documentData.FileDocuments))
                {
                    ShowErrorForFile(true);
                    return false;
                }
                _documentData.File.FileCategoryID = _categoriesPicker.SelectedValue;
                _documentData.File.IsActive = true;
                _documentData.File.ErrCode = ErrorCode.OK;
                if (GenericMethods.IsListNotEmpty(_removedDocumentList))
                {
                    foreach (FileDocumentModel removedDocument in _removedDocumentList)
                    {
                        _documentData.FileDocuments.Add(removedDocument);
                    }
                }
                await (_parentPage.PageService as FileService).SaveDocumentAsync(_documentData).ConfigureAwait(true);
                return await HandelSaveResponseAsync(false);
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> HandelSaveResponseAsync(bool isDeleteFlow)
        {
            if (_documentData.ErrCode == ErrorCode.OK)
            {
                await _parentPage.SyncDataWithServerAsync(Pages.FilePage, false, App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
                if (MobileConstants.IsDevicePhone)
                {
                    await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
                }
                else
                {
                    InvokeListRefresh(Guid.Empty, new EventArgs());
                    //todo: if (isDeleteFlow && PopupNavigation.Instance.PopupStack?.Count > 0)
                    {
                        //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
                    }
                }
                return true;
            }
            else
            {
                _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(_documentData.ErrCode.ToString()));
                return false;
            }
        }
    }
}