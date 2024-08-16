using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient
{
    public partial class PatientFilePage : BasePage
    {
        private readonly FileDTO _fileData = new()
        {
            RecordCount = -1,
            FileDocuments = new List<FileDocumentModel>(),
        };
        private bool _hideDeletedConfirmationPopup = true;
        private string _description = string.Empty;
        private FileService _serviceObject;
        private List<ButtonActionModel> _actionData;
        private List<AttachmentModel> _files = new();
        private bool _isEditable;
        /// <summary>
        /// Document ID parameter
        /// </summary>
        [Parameter]
        public Guid FileID { get; set; }
        private IList<ButtonActionModel> _actionButtons = null;

        protected override async Task OnInitializedAsync()
        {
            _serviceObject = new FileService(AppState.webEssentials);
            _fileData.File = new FileModel
            {
                FileID = FileID,
            };

            await SendServiceRequestAsync(_serviceObject.GetFilesAsync(_fileData), _fileData).ConfigureAwait(true);

            if (_fileData.ErrCode == ErrorCode.OK)
            {
                if (GenericMethods.IsListNotEmpty(_fileData.FileDocuments))
                {
                    foreach (var fileDocument in _fileData.FileDocuments)
                    {
                        if (!string.IsNullOrWhiteSpace(fileDocument.DocumentName))
                        {
                            bool isDeleteAllowed = fileDocument.AddedByID == AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0) && LibPermissions.HasPermission(_fileData.FeaturePermissions, AppPermissions.PatientFileDelete.ToString());
                            var file = new AttachmentModel();
                            file.FileValue = fileDocument.DocumentName;
                            file.FileDescription = fileDocument.DocumentDescription;
                            file.FileID = fileDocument.FileDocumentID;
                            file.IsActive = true;
                            file.AddedBy = fileDocument.AddedByUserName;
                            file.IsDeleteAllowed = isDeleteAllowed;
                            if (IsPatientMobileView)
                            {
                                file.AddedBy = fileDocument.FormattedStyle;
                                var statusStyle = Constants.ERROR_COLOR;
                                var value = LibResources.GetResourceValueByKey(_fileData?.Resources, ResourceConstants.R_REMOVE_TEXT_KEY);
                                file.Text = isDeleteAllowed ? $"<label style ='{statusStyle}'><b>{value}</b></label>" : "";
                            }
                            _files.Add(file);
                        }
                    }
                }
                _isEditable = LibPermissions.HasPermission(_fileData.FeaturePermissions, AppPermissions.PatientFileAddEdit.ToString());
                _isDataFetched = true;
                if (IsPatientMobileView)
                {
                    _actionButtons ??= new List<ButtonActionModel>();
                    if (_isEditable)
                    {
                        _actionButtons.Add(new ButtonActionModel
                        {
                            ButtonResourceKey = ResourceConstants.R_SAVE_ACTION_KEY,
                            ButtonAction = () => { OnSaveClickedAsync(); },
                            Icon = ImageConstants.I_SAVE_ICON_RESPONSIVE
                        });
                    }
                    _actionButtons.Add(new ButtonActionModel
                    {
                        ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                        ButtonAction = () => { OnCancelClickedAsync(); },
                        Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
                    });
                    if (_fileData.File.FileID != Guid.Empty && _fileData.File.AddedByID == AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0)
               && LibPermissions.HasPermission(_fileData.FeaturePermissions, AppPermissions.PatientFileDelete.ToString()))
                    {
                        _actionButtons.Add(new ButtonActionModel
                        {
                            ButtonResourceKey = ResourceConstants.R_DELETE_ACTION_KEY,
                            FieldType = FieldTypes.DeleteBorderTransparentButtonControl,
                            ButtonAction = () => { OnRemoveClick(); },
                        });
                    }
                }
            }
            else
            {
                await OnClose.InvokeAsync(_fileData.ErrCode.ToString());
            }
        }

        private void OnImageChange(AttachmentModel attachment)
        {
            var existingFile = attachment.FileID == Guid.Empty
                ? _fileData.FileDocuments.FirstOrDefault(x => x.FileDocumentName == attachment.FileName)
                : _fileData.FileDocuments.FirstOrDefault(x => x.FileDocumentID == attachment.FileID);

            if (_fileData.FileDocuments.Contains(existingFile))
            {
                var index = _fileData.FileDocuments.IndexOf(existingFile);
                _fileData.FileDocuments[index].FileID = _fileData.File.FileID;
                _fileData.FileDocuments[index].DocumentDescription = attachment.FileDescription;
                _fileData.FileDocuments[index].FileDocumentName = attachment.IsActive ? attachment.FileName : string.Empty;
                _fileData.FileDocuments[index].DocumentName = attachment.IsActive ? attachment.FileValue : string.Empty;
                _fileData.FileDocuments[index].IsActive = attachment.IsActive;
            }
            else
            {
                _fileData.FileDocuments.Add(new FileDocumentModel
                {
                    FileID = _fileData.File.FileID,
                    FileDocumentID = Guid.Empty,
                    FileDocumentName = attachment.IsActive ? attachment.FileName : string.Empty,
                    DocumentDescription = attachment.FileDescription,
                    DocumentName = attachment.IsActive ? attachment.FileValue : string.Empty,
                    IsActive = attachment.IsActive
                });
            }
        }

        private async Task OnSaveClickedAsync()
        {
            if (IsValid())
            {
                _fileData.File.FileCategoryID = _fileData.CategoryOptions.FirstOrDefault(x => x.IsSelected).OptionID;
                _fileData.File.IsActive = true;
                await SendServiceRequestAsync(_serviceObject.SaveDocumentAsync(_fileData), _fileData).ConfigureAwait(true);
                if (_fileData.ErrCode == ErrorCode.OK)
                {
                    await OnClose.InvokeAsync(_fileData.ErrCode.ToString());
                }
                else
                {
                    Error = _fileData.ErrCode.ToString();
                    StateHasChanged();
                }
            }
        }

        private void OnRemoveClick()
        {
            _actionData = new List<ButtonActionModel>
            {
                new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey =ResourceConstants.R_OK_ACTION_KEY  },
                new ButtonActionModel{ ButtonID = Constants.NUMBER_TWO, ButtonResourceKey =ResourceConstants.R_CANCEL_ACTION_KEY  },
            };
            _hideDeletedConfirmationPopup = false;
        }

        private async Task OnDeleteConfirmationPopUpClickedAsync(object sequenceNo)
        {
            _hideDeletedConfirmationPopup = true;
            if (sequenceNo != null && Convert.ToInt64(sequenceNo) == 1)
            {
                await SendServiceRequestAsync(_serviceObject.DeleteFileAsync(_fileData), _fileData).ConfigureAwait(true);
                if (_fileData.ErrCode == ErrorCode.OK)
                {
                    await OnClose.InvokeAsync(_fileData.ErrCode.ToString());
                }
                else
                {
                    Error = _fileData.ErrCode.ToString();
                }
            }
        }

        private async Task OnCancelClickedAsync()
        {
            await OnClose.InvokeAsync(string.Empty);
        }

        private static List<TableDataStructureModel> GenerateTableStructure()
        {
            return new List<TableDataStructureModel>
            {
                new TableDataStructureModel{DataField=nameof(FileDocumentModel.ClientFileDocumentID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
                new TableDataStructureModel{DataField=nameof(FileDocumentModel.DocumentDescription),DataHeader=ResourceConstants.R_BILLING_REPORTS_KEY },
                new TableDataStructureModel{DataField=nameof(FileDocumentModel.FormattedDate),DataHeader=ResourceConstants.R_START_DATE_KEY} ,
            };
        }
    }
}