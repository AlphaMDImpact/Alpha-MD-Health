using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class FilesServiceBusinessLayer : BaseServiceBusinessLayer
	{
		/// <summary>
		/// File service
		/// </summary>
		/// <param name="httpContext">Instance of HttpContext</param>
		public FilesServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
		{
		}

		/// <summary>
		/// Get Files from database
		/// </summary>
		/// <param name="languageID">user's selected language</param>
		/// <param name="permissionAtLevelID">permission at level id</param>
		/// <param name="organisationID">organisation id</param>
		/// <param name="selectedUserID">user id</param>
		/// <param name="fileID">file id</param>
		/// <param name="recordCount">number of record count to fetch</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
		/// <returns>Operation status and list of patient files</returns>
		public async Task<BaseDTO> GetFilesAndDocumentsAsync(byte languageID, long permissionAtLevelID, long organisationID, long selectedUserID, Guid fileID, long recordCount, string fromDate, string toDate)
		{
			FileDTO fileData = new FileDTO();
			try
			{
				if (languageID < 1 || permissionAtLevelID < 1 || (recordCount != -1 && selectedUserID < 1))
				{
					fileData.ErrCode = ErrorCode.InvalidData;
					return fileData;
				}
				if (AccountID < 1)
				{
					fileData.ErrCode = ErrorCode.Unauthorized;
					return fileData;
				}
				if (await GetFilesAndDocumentsResourceAndSettingsAsync(fileData, organisationID, languageID).ConfigureAwait(false))
				{
					fileData.RecordCount = recordCount;
					fileData.PermissionAtLevelID = permissionAtLevelID;
					fileData.FromDate = fromDate;
					fileData.ToDate = toDate;
					fileData.File = new FileModel
					{
						FileID = fileID,
						UserID = selectedUserID,
					};
					fileData.FeatureFor = FeatureFor;
					await new FilesServiceDataLayer().GetFilesAsync(fileData).ConfigureAwait(false);
					await MapFilesAndDocumentsDependancyAsync(fileData);
				}
			}
			catch (Exception ex)
			{
				fileData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
				LogError(ex.Message, ex);
			}
			return fileData;
		}

		internal async Task MapFilesAndDocumentsDependancyAsync(FileDTO fileData)
		{
			if (fileData.ErrCode == ErrorCode.OK)
			{
				await Task.WhenAll(
					ReplaceFileCatergoryImageCdnLinkAsync(fileData),
					ReplaceFileAttachmentImageCdnLinkAsync(fileData.FileDocuments)
				);
			}
		}

		internal async Task<bool> GetFilesAndDocumentsResourceAndSettingsAsync(FileDTO fileData, long organisationID, byte languageID)
		{
			fileData.AccountID = AccountID;
			fileData.OrganisationID = organisationID;
			fileData.LanguageID = languageID;
			if (await GetSettingsResourcesAsync(fileData, true, GroupConstants.RS_COMMON_GROUP,
					$"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}"
				).ConfigureAwait(false))
			{
				fileData.ErrCode = ErrorCode.OK;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Save Files to database
		/// </summary>
		/// <param name="permissionAtLevelID">permission at level id</param>
		/// <param name="filesData">files data to be saved</param>
		/// <returns>Operation status</returns>
		public async Task<BaseDTO> SaveFilesAsync(long permissionAtLevelID, FileDTO filesData)
		{
			try
			{
				if (permissionAtLevelID < 0 || AccountID < 1 || filesData.File == null)
				{
					filesData.ErrCode = ErrorCode.InvalidData;
					return filesData;
				}
				if (filesData.File.IsActive)
				{
					if (!GenericMethods.IsListNotEmpty(filesData.FileDocuments))
                    {
                        filesData.ErrCode = ErrorCode.InvalidData;
						return filesData;
					}
                    filesData.LanguageID = 1;
					if (await GetSettingsResourcesAsync(filesData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PATIENT_DOCUMNET_GROUP}").ConfigureAwait(false))
					{
						if (!await ValidateDataAsync(filesData.File, filesData.Resources))
						{
                            filesData.ErrCode = ErrorCode.InvalidData;
							return filesData;
						}
						var documentTextResource = LibResources.GetResourceByKey(filesData.Resources, ResourceConstants.R_DOCUMENT_TEXT_KEY);
                        if (documentTextResource.IsRequired && (filesData.FileDocuments.Count < documentTextResource.MinLength ||
                            filesData.FileDocuments.Count > documentTextResource.MaxLength))
						{
                            filesData.ErrCode = ErrorCode.InvalidData;
                            return filesData;
                        }
					}
					else
					{
						return filesData;
					}
				}
				filesData.AccountID = AccountID;
				filesData.PermissionAtLevelID = permissionAtLevelID;
				List<FileModel> newFilesToSendNotification = new List<FileModel>();
				filesData.FeatureFor = FeatureFor;
				await new FilesServiceDataLayer().SaveFilesAsync(filesData).ConfigureAwait(false);
				if (filesData.ErrCode == ErrorCode.OK)
				{
					newFilesToSendNotification = filesData.NewFiles;
					await UploadFileAttachmentsAsync(filesData).ConfigureAwait(false);
					if (filesData.ErrCode == ErrorCode.OK)
					{
						await new FilesServiceDataLayer().SaveFilesAsync(filesData).ConfigureAwait(false);
					}
				}
				if (GenericMethods.IsListNotEmpty(newFilesToSendNotification))
				{
					_ = Task.WhenAll(from file in newFilesToSendNotification
									 select SendUserNotificationAsync(file, filesData));
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				filesData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return filesData;
		}

		private async Task SendUserNotificationAsync(FileModel file, FileDTO filesData)
		{
			TemplateDTO communicationDto = new TemplateDTO();
			communicationDto.AccountID = AccountID;
			communicationDto.SelectedUserID = file.UserID;
			communicationDto.LanguageID = filesData.LanguageID;
			communicationDto.OrganisationID = filesData.OrganisationID;
			communicationDto.NotificationTags = $"{Constants.USER_TAG_PREFIX}{file.UserID}";
			communicationDto.Response = file.FileID.ToString();
			communicationDto.TemplateData = new TemplateModel
			{
				TemplateName = TemplateName.ENewFile,
				IsExternal = false,

			};
			await SendCommunicationAsync(communicationDto).ConfigureAwait(false);
		}

		/// <summary>
		/// Delete patient files from database
		/// </summary>
		/// <param name="permissionAtLevelID">permission at level id</param>
		/// <param name="fileID">File Id to be deleted</param>
		/// <returns>Operation status</returns>
		public async Task<BaseDTO> DeleteFilesAsync(long permissionAtLevelID, Guid fileID, DateTimeOffset lastModifiedOn)
		{
			BaseDTO resultDTO = new BaseDTO();
			try
			{
				if (permissionAtLevelID < 0 || fileID == Guid.Empty)
				{
					resultDTO.ErrCode = ErrorCode.InvalidData;
					return resultDTO;
				}
				if (AccountID < 1)
				{
					resultDTO.ErrCode = ErrorCode.Unauthorized;
					return resultDTO;
				}
				resultDTO.AccountID = AccountID;
				resultDTO.PermissionAtLevelID = permissionAtLevelID;
				resultDTO.ErrorDescription = fileID.ToString();
				resultDTO.LastModifiedON = lastModifiedOn;
				resultDTO.FeatureFor = FeatureFor;
				await new FilesServiceDataLayer().DeleteFileAsync(resultDTO).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				resultDTO.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return resultDTO;
		}

		/// <summary>
		/// To mark document as read
		/// </summary>
		/// <param name="permissionAtLevelID"></param>
		/// <param name="fileDocumentID"></param>
		/// <returns>Operation status</returns>
		public async Task<BaseDTO> UpdateDocumentStatusAsync(long permissionAtLevelID, Guid fileDocumentID)
		{
			BaseDTO response = new BaseDTO();
			try
			{
				if (permissionAtLevelID < 0 || fileDocumentID == Guid.Empty)
				{
					response.ErrCode = ErrorCode.InvalidData;
					return response;
				}
				if (AccountID < 1)
				{
					response.ErrCode = ErrorCode.Unauthorized;
					return response;
				}
				response.AccountID = AccountID;
				response.PermissionAtLevelID = permissionAtLevelID;
				response.FeatureFor = FeatureFor;
				response.ErrorDescription = fileDocumentID.ToString();
				await new FilesServiceDataLayer().UpdateDocumentStatusAsync(response).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				response.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return response;
		}

		/// <summary>
		/// Save Task/Note Documents
		/// </summary>
		/// <param name="file">File Document Data</param>
		/// <returns>Operation status</returns>
		public async Task<BaseDTO> SaveNoteDocumentsAsyncs(FileDTO file)
		{
			try
			{
				file.FeatureFor = FeatureFor;
				await new FilesServiceDataLayer().SaveNoteDocumentsAsync(file).ConfigureAwait(false);
				UpdateFileID(file);
				if (file.ErrCode == ErrorCode.OK)
				{
					await UploadNotesDocumentsAsync(file);
				}
				if (file.ErrCode == ErrorCode.OK)
				{
					await new FilesServiceDataLayer().SaveNoteDocumentsAsync(file).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				file.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return file;
		}

		internal async Task ReplaceFileAttachmentImageCdnLinkAsync(List<FileDocumentModel> fileDocuments)
		{
			if (GenericMethods.IsListNotEmpty(fileDocuments))
			{
				BaseDTO cdnCacheData = new BaseDTO();
				foreach (var attachment in fileDocuments)
				{
					if (!string.IsNullOrWhiteSpace(attachment.DocumentName))
					{
						attachment.DocumentName = await ReplaceCDNLinkAsync(attachment.DocumentName, cdnCacheData);
					}
				}
			}
		}

		private void UpdateFileID(FileDTO noteData)
		{
			if (noteData.FileDocuments.Any(x => x.FileID == Guid.Empty) && GenericMethods.IsListNotEmpty(noteData.SaveFileDocuments))
			{
				noteData.FileDocuments.Where(x => x.IsActive)?.ToList().ForEach(doc =>
				{
					if (doc.FileID == Guid.Empty && noteData.SaveFileDocuments.Any(x => x.ClientGuid == doc.FileDocumentID))
					{
						doc.FileID = noteData.SaveFileDocuments.FirstOrDefault(x => x.ClientGuid == doc.FileDocumentID).ServerGuid;
					}
				});
			}
		}

		private async Task UploadNotesDocumentsAsync(FileDTO noteData)
		{
			FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.FileAndDocuments, null);

			files.FileContainers.AddRange(from fileDocGroup in noteData.FileDocuments.GroupBy(x => x.FileID)
										  let attachments = (from fileDoc in fileDocGroup.ToList()
															 where fileDoc.IsActive && !string.IsNullOrWhiteSpace(fileDoc.DocumentName)
															 select CreateFileObject(fileDoc.DocumentName, fileDoc.DocumentName, false)).ToList()
										  where attachments?.Count > 0
										  select new FileContainerModel
										  {
											  ID = Convert.ToString(fileDocGroup.Key, CultureInfo.InvariantCulture),
											  FileData = attachments,
										  });

			if (files.FileContainers?.Count > 0)
			{
				files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
				noteData.ErrCode = files.ErrCode;
				if (files.ErrCode == ErrorCode.OK)
				{
					noteData.FileDocuments.ForEach(attachment =>
					{
						if (attachment.IsActive)
						{
							var file = files.FileContainers.FirstOrDefault(x => x.ID == Convert.ToString(attachment.FileID, CultureInfo.InvariantCulture))?.FileData?.FirstOrDefault(x => x.RecordID == attachment.DocumentName);
							if (file != null)
							{
								attachment.DocumentName = file.Base64File;
							}
						}
					});
				}
			}
		}

		private async Task ReplaceFileCatergoryImageCdnLinkAsync(FileDTO fileData)
		{
			if (GenericMethods.IsListNotEmpty(fileData.Files))
			{
				BaseDTO cdnCacheData = new BaseDTO();
				foreach (var file in fileData.Files)
				{
					if (!string.IsNullOrWhiteSpace(file.FileImage))
					{
						file.FileImage = await ReplaceCDNLinkAsync(file.FileImage, cdnCacheData);
					}
				}
			}
		}

		private async Task UploadFileAttachmentsAsync(FileDTO fileData)
		{
			FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.FileAndDocuments, null);
			files.FileContainers.AddRange(from file in fileData.Files
										  where file.IsActive && !fileData.SaveFiles.Any(y => y.ClientGuid == file.FileID && y.ErrCode != ErrorCode.OK)
										  let attachments = (from fileDoc in fileData.FileDocuments
															 where fileDoc.FileID == file.FileID
																 && fileDoc.IsActive
																 && !string.IsNullOrWhiteSpace(fileDoc.DocumentName)
																 && fileData.SaveFileDocuments.Any(x =>
																	 x.ClientGuid == fileDoc.FileDocumentID &&
																	 x.ErrCode != ErrorCode.DuplicateGuid &&
																	 x.ErrCode != ErrorCode.DuplicateData)
															 select CreateFileObject(fileDoc.DocumentName, fileDoc.DocumentName, false)).ToList()
										  where attachments?.Count > 0
										  select new FileContainerModel
										  {
											  ID = Convert.ToString(file.FileID, CultureInfo.InvariantCulture),
											  FileData = attachments,
										  });
			if (files.FileContainers?.Count > 0)
			{
				files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
				fileData.ErrCode = files.ErrCode;
				if (files.ErrCode == ErrorCode.OK)
				{
					fileData.FileDocuments.ForEach(attachment =>
					{
						if (attachment.IsActive)
						{
							var file = files.FileContainers.FirstOrDefault(x => x.ID == Convert.ToString(attachment.FileID, CultureInfo.InvariantCulture))?.FileData?.FirstOrDefault(x => x.RecordID == attachment.DocumentName);
							if (file != null)
							{
								attachment.DocumentName = file.Base64File;
							}
						}
					});
				}
			}
		}

		//private bool IsFilesInvalid(List<FileModel> files)
		//{
		//	return files == null || files.Count < 1 || files.Any(x => x.FileID == Guid.Empty || (x.IsActive && x.FileCategoryID < 1));
		//}

		//private bool IsFileDocumentsInvalid(List<FileDocumentModel> fileDocuments)
		//{
		//	return fileDocuments == null || fileDocuments.Count < 1 ||
		//		fileDocuments.Any(x => x.FileDocumentID == Guid.Empty || x.FileID == Guid.Empty
		//		|| (x.IsActive && (string.IsNullOrWhiteSpace(x.DocumentName))));
		//}
	}
}
