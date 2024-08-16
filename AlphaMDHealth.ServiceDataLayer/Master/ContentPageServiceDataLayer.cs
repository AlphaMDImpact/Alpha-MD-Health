using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class ContentPageServiceDataLayer : BaseServiceDataLayer
	{
		/// <summary>
		/// Get Content Pages
		/// </summary>
		/// <param name="contentPagesData">Reference object to return list of Content Pages</param>
		/// <returns>List of Content Pages Data</returns>
		public async Task GetContentPagesAsync(ContentPageDTO contentPagesData, bool isBasic)
		{
			GetContentPermission(contentPagesData.RecordCount, contentPagesData.SelectedUserID, contentPagesData.Page.PageID, contentPagesData.Page.IsEducation, contentPagesData.IsActive, isBasic, out string permissioncheck, out string permissionRequest);
			using var connection = ConnectDatabase();
			connection.Open();
			DynamicParameters parameters = new DynamicParameters();
			parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), contentPagesData.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameters.Add(ConcateAt(nameof(ContentPageModel.PageID)), contentPagesData.Page.PageID, DbType.Int64, ParameterDirection.Input);
			parameters.Add(ConcateAt(nameof(ContentPageModel.IsEducation)), contentPagesData.Page.IsEducation, DbType.Boolean, ParameterDirection.Input);
			parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), contentPagesData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
			parameters.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), contentPagesData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
			parameters.Add(ConcateAt(nameof(BaseDTO.OrganisationID)), contentPagesData.OrganisationID, DbType.Int64, direction: ParameterDirection.Output);
			parameters.Add(ConcateAt(Constants.FOR_PROVIDER_CONSTANT), contentPagesData.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
			AddDateTimeParameter(nameof(BaseDTO.FromDate), contentPagesData.FromDate, parameters, ParameterDirection.Input);
			AddDateTimeParameter(nameof(BaseDTO.ToDate), contentPagesData.ToDate, parameters, ParameterDirection.Input);
			MapCommonSPParameters(contentPagesData, parameters, permissioncheck, permissionRequest);
			SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_CONTENT_PAGES, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			if (result.HasRows())
			{
				await MapContentPagesViewDataAsync(contentPagesData, result).ConfigureAwait(false);
				await MapReturnPermissionsAsync(contentPagesData, result).ConfigureAwait(false);
			}
			contentPagesData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
			contentPagesData.OrganisationID = parameters.Get<long>(ConcateAt(nameof(BaseDTO.OrganisationID)));
		}

		internal async Task MapContentPagesViewDataAsync(ContentPageDTO contentPagesData, SqlMapper.GridReader result)
		{
			if (contentPagesData.RecordCount == -2)
			{
				contentPagesData.Pages = (await result.ReadAsync<ContentPageModel>().ConfigureAwait(false))?.ToList();
				if (!result.IsConsumed)
				{
					contentPagesData.PageDetails = (await result.ReadAsync<ContentDetailModel>().ConfigureAwait(false))?.ToList();
				}
			}
			else if (contentPagesData.SelectedUserID == 0)
			{
				await MapResultsAsync(contentPagesData, result).ConfigureAwait(false);
			}
			else
			{
				await MapPatientEducationsAsync(contentPagesData, result).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Save Content Page to database
		/// </summary>
		/// <param name="contentPage">Content Page details data to be saved</param>
		/// <param name="isAfterImageUpload">Flag representing image data is uploaded in blob storage or not</param>
		/// <returns>Result of operation</returns>
		public async Task SaveContentPagesAsync(ContentPageDTO contentPage, bool isAfterImageUpload)
		{
			using var connection = ConnectDatabase();
			DynamicParameters parameter = new DynamicParameters();
			parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), contentPage.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameter.Add(ConcateAt(nameof(contentPage.Page.PageID)), contentPage.Page.PageID, DbType.Int64, ParameterDirection.InputOutput);
			parameter.Add(ConcateAt(nameof(contentPage.Page.IsEducation)), contentPage.Page.IsEducation, DbType.Boolean, ParameterDirection.Input);
			parameter.Add(ConcateAt(nameof(contentPage.Page.PageTags)), contentPage.Page.PageTags, DbType.String, ParameterDirection.Input);
			parameter.Add(ConcateAt(nameof(contentPage.Page.IsLink)), contentPage.Page.IsLink, DbType.Boolean, ParameterDirection.Input);
			parameter.Add(ConcateAt(nameof(contentPage.Page.IsPdf)), contentPage.Page.IsPdf, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(contentPage.Page.IsPublished)), contentPage.Page.IsPublished, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(contentPage.Page.ImageName)), isAfterImageUpload ? contentPage.Page.ImageName : string.Empty, DbType.String, ParameterDirection.Input);
			parameter.Add(ConcateAt(nameof(contentPage.Page.PDFName)), isAfterImageUpload ? contentPage.Page.PDFName : string.Empty, DbType.String, ParameterDirection.Input);
			parameter.Add(ConcateAt(nameof(contentPage.Page.EducationCategoryID)), contentPage.Page.EducationCategoryID, DbType.Int64, ParameterDirection.Input);
			parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), ConvertContentPagesToTable(contentPage, isAfterImageUpload).AsTableValuedParameter());
			MapCommonSPParameters(contentPage, parameter, GetSaveContentPermission(contentPage.Page.IsEducation, false));
			await connection.QueryAsync<SaveResultModel>(SPNameConstants.USP_SAVE_CONTENT_PAGE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			contentPage.Page.PageID = parameter.Get<long>(ConcateAt(nameof(contentPage.Page.PageID)));
			contentPage.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
		}

        /// <summary>
        /// Save Content Page Publish/unpublish status to database
        /// </summary>
        /// <param name="contentPage">Content Page details data to be marked as published or unpublished</param>
        /// <returns>Result of operation</returns>
        public async Task PublishContentPageAsync(BaseDTO contentPage)
        {
			ContentPageModel contentPageModel = new ContentPageModel();
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), contentPage.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ContentPageModel.PageID)), contentPage.RecordCount, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ContentPageModel.IsPublished)), contentPage.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(contentPage, parameter, GetSaveContentPermission(contentPageModel.IsEducation, true));
            await connection.QueryAsync<SaveResultModel>(SPNameConstants.USP_PUBLISH_CONTENT_PAGE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            contentPage.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }

        /// <summary>
        /// Save Content Page status to database
        /// </summary>
        /// <param name="contentPage">Content Page details data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task SaveEducationStatusAsync(ContentPageDTO contentPage)
		{
			using var connection = ConnectDatabase();
			DynamicParameters parameter = new DynamicParameters();
			parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), contentPage.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), ConvertContentPagesStatusToTable(contentPage).AsTableValuedParameter());
			MapCommonSPParameters(contentPage, parameter, string.Empty);
			await connection.QueryAsync<SaveResultModel>(SPNameConstants.USP_SAVE_EDUCATION_STATUS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			contentPage.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
		}

		private async Task MapResultsAsync(ContentPageDTO contentPagesData, SqlMapper.GridReader result)
		{
			if (contentPagesData.Page?.PageID > 0 || contentPagesData.RecordCount > -1)
			{
				contentPagesData.Pages = (await result.ReadAsync<ContentPageModel>().ConfigureAwait(false))?.ToList();
			}
			await MapPageDetailsAsync(contentPagesData, result).ConfigureAwait(false);
		}

		private async Task MapPageDetailsAsync(ContentPageDTO contentPagesData, SqlMapper.GridReader result)
		{
			if (contentPagesData.RecordCount == -1 && !result.IsConsumed)
			{
				contentPagesData.PageDetails = (await result.ReadAsync<ContentDetailModel>().ConfigureAwait(false))?.ToList();
				if (!result.IsConsumed)
				{
					contentPagesData.EducationCategory = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
				}
				if (!result.IsConsumed)
				{
					contentPagesData.PageTypes = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
				}
			}
		}

		private async Task MapPatientEducationsAsync(ContentPageDTO contentPagesData, SqlMapper.GridReader result)
		{
			/*
             Patient education Add - When RecordCount = -1 and SelectedUserID > 0 and PageID = 0
             Patient education Edit - When RecordCount = -1 and SelectedUserID > 0 and PageID > 0
             Patient education Preview -  When RecordCount = -2 and SelectedUserID > 0 and PageID > 0
             */
			if (contentPagesData.RecordCount == -1)
			{
				//Patient education add/edit
				await MapPatientEducationAndCategoryAsync(contentPagesData, result).ConfigureAwait(false);
			}
			else if (contentPagesData.RecordCount == -2)
			{
				//Data for patient education preview
				if (!result.IsConsumed)
				{
					contentPagesData.Pages = (await result.ReadAsync<ContentPageModel>().ConfigureAwait(false))?.ToList();
				}
			}
			else
			{
				if (!result.IsConsumed)
				{
					contentPagesData.PatientEducations = (await result.ReadAsync<PatientEducationModel>().ConfigureAwait(false))?.ToList();
				}
			}
		}

		private async Task MapPatientEducationAndCategoryAsync(ContentPageDTO contentPagesData, SqlMapper.GridReader result)
		{
			if (!result.IsConsumed)
			{
				//Map education category data
				contentPagesData.EducationTypes = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
			}
			if (!result.IsConsumed)
			{
				contentPagesData.Educations = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
			}
			if (contentPagesData.Page?.PageID > 0 && !result.IsConsumed)
			{
				contentPagesData.Pages = (await result.ReadAsync<ContentPageModel>().ConfigureAwait(false))?.ToList();
			}
		}

		private void GetContentPermission(long recordCount, long selectedUserID, long pageID, bool isEducation, bool forProvider, bool isBasic, out string permissioncheck, out string permissionRequest)
		{
			if (isBasic)
			{
				permissioncheck = AppPermissions.StaticContentsView.ToString();
				permissionRequest = "";
			}
			//add / edit operation
			else if (recordCount == -3)
			{
				permissioncheck = AppPermissions.MyEducationsView.ToString();
				permissionRequest = string.Empty;
			}
			else if (recordCount == -1)
			{
				if (selectedUserID == 0)
				{
					permissioncheck = isEducation ? AppPermissions.EducationsView.ToString() : AppPermissions.StaticContentsView.ToString();
					permissionRequest = isEducation ? $"{AppPermissions.EducationPublish},{AppPermissions.EducationAddEdit}" : $"{AppPermissions.StaticContentPublish},{AppPermissions.StaticContentAddEdit}";
				}
				else
				{
					permissioncheck = AppPermissions.PatientEducationsView.ToString();
					permissionRequest = $"{AppPermissions.PatientEducationAddEdit},{AppPermissions.PatientEducationDelete}";
				}
			}
			else if (forProvider && selectedUserID < 1)
			{
				if (recordCount == -2)
				{
					permissioncheck = AppPermissions.UserConsentsView.ToString();
					permissionRequest = "";
				}
				else
				{
					permissioncheck = AppPermissions.MyEducationsView.ToString();
					permissionRequest = AppPermissions.EducationPreview.ToString();
				}
			}
			//Patient education list
			else if (selectedUserID > 0)
			{
				permissioncheck = AppPermissions.PatientEducationsView.ToString();
				permissionRequest = $"{AppPermissions.PatientEducationAddEdit},{AppPermissions.EducationPreview}";
			}
			//Education and static page list
			else
			{
				permissioncheck = isEducation ? AppPermissions.EducationsView.ToString() : AppPermissions.StaticContentsView.ToString();
				permissionRequest = isEducation ? AppPermissions.EducationAddEdit.ToString() : AppPermissions.StaticContentAddEdit.ToString();
			}
		}

		private DataTable ConvertContentPagesToTable(ContentPageDTO contentPage, bool isAfterImageUpload)
		{
			DataTable dataTable = CreateGenericTypeTable();
			if(GenericMethods.IsListNotEmpty(contentPage.PageDetails))
			{
                foreach (ContentDetailModel record in contentPage.PageDetails)
                {
                    dataTable.Rows.Add(record.PageID, Guid.Empty, record.LanguageID, record.PageHeading, record.Description, isAfterImageUpload ? record.PageData : string.Empty);
                }
            }
			return dataTable;
		}

		private DataTable ConvertContentPagesStatusToTable(ContentPageDTO contentPage)
		{
			DataTable dataTable = CreateGenericTypeTable();
			foreach (PatientEducationModel record in contentPage.PatientEducations)
			{
				dataTable.Rows.Add(record.PatientEducationID, Guid.Empty, 0, record.Status, string.Empty, string.Empty);
			}
			return dataTable;
		}

		private string GetSaveContentPermission(bool isPublishUnpublish, bool isEducation)
		{
			if (isEducation)
			{
				return isPublishUnpublish ? AppPermissions.EducationPublish.ToString() : AppPermissions.EducationAddEdit.ToString();
			}
			return isPublishUnpublish ? AppPermissions.StaticContentPublish.ToString() : AppPermissions.StaticContentAddEdit.ToString();
		}
	}
}