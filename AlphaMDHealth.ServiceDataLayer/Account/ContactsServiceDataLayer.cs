using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class ContactsServiceDataLayer : BaseServiceDataLayer
	{
		/// <summary>
		/// Get Contacts
		/// </summary>
		/// <param name="contacts">Reference object to return contacts</param>
		/// <returns>Returns contacts in reference object</returns>
		public async Task GetContactsAsync(ContactDTO contacts)
		{
			GetContactsPermissionCheck(contacts.ContactType, out string permissionCheck, out string permissionRequest);
			using var connection = ConnectDatabase();
			connection.Open();
			DynamicParameters parameters = new DynamicParameters();
			parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), contacts.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ContactModel.ContactID), contacts.Contact.ContactID, DbType.Guid, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(contacts.SelectedUserID), contacts.SelectedUserID, DbType.Int64, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), GenericMethods.ApplyUtcDateTimeFormatToUtcValue(contacts.LastModifiedON.Value), DbType.DateTimeOffset, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(contacts.ContactType), contacts.ContactType, DbType.Byte, ParameterDirection.Input);
			MapCommonSPParameters(contacts, parameters, permissionCheck, permissionRequest);
			SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_CONTACTS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			if (result.HasRows())
			{
				await MapContactsAsync(contacts, result).ConfigureAwait(false);
				await MapReturnPermissionsAsync(contacts, result).ConfigureAwait(false);
			}
			contacts.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
		}

		/// <summary>
		/// Save contacts
		/// </summary>
		/// <param name="contacts">Object which contains contacts to save</param>
		/// <returns>Operation Status</returns>
		public async Task SaveContactsAsync(ContactDTO contacts)
		{
			SaveContactsPermissionCheck(contacts.ContactType, contacts.IsActive, out string permissionCheck);
			using var connection = ConnectDatabase();
			connection.Open();
			DynamicParameters parameters = new DynamicParameters();
			parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), contacts.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(contacts.SelectedUserID), contacts.SelectedUserID, DbType.Int64, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(contacts.ContactType), contacts.ContactType, DbType.Byte, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertDataToContactTable(contacts.Contacts).AsTableValuedParameter());
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS_2, ConvertDataToContactDetailsTable(contacts.ContactDetails).AsTableValuedParameter());
			MapCommonSPParameters(contacts, parameters, permissionCheck);
			contacts.SaveResults = (await connection.QueryAsync<SaveResultModel>(SPNameConstants.USP_SAVE_CONTACTS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false)).ToList();
			contacts.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
		}

		private async Task MapContactsAsync(ContactDTO contacts, SqlMapper.GridReader result)
		{
			if (contacts.RecordCount == -1)
			{
				contacts.Contact = contacts.Contact.ContactID == Guid.Empty ? new ContactModel() : (await result.ReadAsync<ContactModel>().ConfigureAwait(false)).FirstOrDefault();
				await MapContactPageDataAsync(contacts, result).ConfigureAwait(false);
			}
			else
			{
				contacts.Contacts = (await result.ReadAsync<ContactModel>().ConfigureAwait(false)).ToList();
				if ((contacts.ContactType == ContactType.Patient || contacts.ContactType == ContactType.User) && !result.IsConsumed)
				{
					contacts.ContactDetails = (await result.ReadAsync<ContactModel>().ConfigureAwait(false)).ToList();
				}
			}
		}

		private async Task MapContactPageDataAsync(ContactDTO contacts, SqlMapper.GridReader result)
		{
			if (!result.IsConsumed)
			{
				contacts.ContactDetails = (await result.ReadAsync<ContactModel>().ConfigureAwait(false)).ToList();
			}
			if (!result.IsConsumed)
			{
				contacts.ContactTypeOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
			}
			if (!result.IsConsumed)
			{
				contacts.ContactTypeIsOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
			}
			if (!result.IsConsumed)
			{
				contacts.CountryCodes = (await result.ReadAsync<CountryModel>().ConfigureAwait(false))?.ToList();
			}
		}

		private DataTable ConvertDataToContactTable(List<ContactModel> contactDetails)
		{
			DataTable dataTable = new DataTable
			{
				Locale = CultureInfo.InvariantCulture,
				Columns =
				{
					new DataColumn(nameof(ContactModel.ContactID), typeof(Guid)),
					new DataColumn(nameof(ContactModel.ContactTypeID), typeof(int)),
					new DataColumn(nameof(ContactModel.ContactTypeIsID), typeof(int)),
					new DataColumn(nameof(ContactModel.IsActive),typeof(bool)),
					new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(byte))
				}
			};
			int count = 0;
			if (GenericMethods.IsListNotEmpty(contactDetails))
			{
				foreach (var item in contactDetails)
				{
					dataTable.Rows.Add(item.ContactID, item.ContactTypeID, item.ContactTypeIsID, item.IsActive, ++count);
				}
			}
			return dataTable;
		}

		private DataTable ConvertDataToContactDetailsTable(List<ContactModel> contactDetails)
		{
			DataTable dataTable = CreateGenericTypeTable();
			if (GenericMethods.IsListNotEmpty(contactDetails))
			{
				foreach (var item in contactDetails)
				{
					dataTable.Rows.Add(0, item.ContactID, item.LanguageID, item.ContactValue, string.Empty, string.Empty);
				}
			}
			return dataTable;
		}

		private void GetContactsPermissionCheck(ContactType ContactType, out string permissionCheck, out string permissionRequest)
		{
			if (ContactType == ContactType.Organisation)
			{
				permissionCheck = AppPermissions.OrganisationContactsView.ToString();
				permissionRequest = $"{AppPermissions.OrganisationContactDelete},{AppPermissions.OrganisationContactAddEdit}";
			}
			else if (ContactType == ContactType.Branch)
			{
				permissionCheck = AppPermissions.BranchContactsView.ToString();
				permissionRequest = $"{AppPermissions.BranchContactDelete},{AppPermissions.BranchContactAddEdit}";
			}
			else if (ContactType == ContactType.User)
			{
				permissionCheck = AppPermissions.UserContactsView.ToString();
				permissionRequest = $"{AppPermissions.UserContactDelete},{AppPermissions.UserContactAddEdit}";
            }
			else
			{
				MapPatientPermission(out permissionCheck, out permissionRequest);
			}
		}

		private void MapPatientPermission(out string permissionCheck, out string permissionRequest)
		{
			permissionCheck = AppPermissions.PatientContactsView.ToString();
			permissionRequest = $"{AppPermissions.PatientContactDelete},{AppPermissions.PatientContactAddEdit}";
		}

		private void SaveContactsPermissionCheck(ContactType ContactType, bool isActive, out string permissionCheck)
		{
			if (ContactType == ContactType.Organisation)
			{
				permissionCheck = isActive ? AppPermissions.OrganisationContactAddEdit.ToString() : AppPermissions.OrganisationContactDelete.ToString();
			}
			else if (ContactType == ContactType.Branch)
			{
				permissionCheck = isActive ? AppPermissions.BranchContactAddEdit.ToString() : AppPermissions.BranchContactDelete.ToString();
			}
			else if (ContactType == ContactType.User)
			{
				permissionCheck = isActive ? AppPermissions.UserContactAddEdit.ToString() : AppPermissions.UserContactDelete.ToString();
			}
			else
			{
				permissionCheck = isActive ? AppPermissions.PatientContactAddEdit.ToString() : AppPermissions.PatientContactDelete.ToString();
			}
		}
	}
}