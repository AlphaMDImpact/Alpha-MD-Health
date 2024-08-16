using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public class BillingDataBase : BaseDatabase
    {
        /// <summary>
        /// Get Patient bills to display in list
        /// </summary>
        /// <param name="billingsData">billingsData object which is used to return list of Patients bills </param>
        /// <returns>Patient bills</returns>
        public async Task GetPatientBillingsAsync(BillingItemDTO billingsData)
        {
            string pColor;
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            if(roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
            {
                pColor = "G.ProgramGroupIdentifier";
            }
            else
            {
                pColor = "D.ProgramGroupIdentifier";
            }
            billingsData.PatientBills = await SqlConnection.QueryAsync<PatientBillModel>(
                $"SELECT DISTINCT A.BillDateTime, A.PatientBillID, D.Name AS ProgramName, A.TotalPaid, A.Amount, F.Name AS PaymentMode, A.ProgramID, {pColor} AS ProgramColor " +
                "FROM PatientBillModel A " +
                "LEFT JOIN UserModel B ON A.PatientID = B.UserID " +
                "LEFT JOIN UserModel C ON A.ProviderID= C.UserID " +
                "LEFT JOIN ProgramModel D ON A.ProgramID = D.ProgramID " +
                "LEFT JOIN BillPaymentModel E ON E.PaymentModeID = A.PaymentModeID " +
                "LEFT JOIN PaymentModeI18NModel F ON E.PaymentModeID = F.PaymentModeID " +
                "LEFT JOIN PatientProgramModel G ON G.PatientID = A.PatientID and A.ProgramID = G.ProgramID " +
                "WHERE A.PatientID = ? AND A.IsActive = 1 " +
                "ORDER BY A.BillDateTime DESC", billingsData.SelectedUserID);
            if (roleID == (int)RoleName.CareTaker && billingsData.PatientBills?.Count > 0)
            {
                List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(billingsData).ConfigureAwait(false);
                billingsData.PatientBills = sharedPrograms?.Count > 0
                    ? billingsData.PatientBills.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ProgramID))?.ToList()
                    : null;
            }
            if (billingsData.RecordCount > 0)
            {
                billingsData.PatientBills = billingsData.PatientBills?.Take((int)billingsData.RecordCount)?.ToList();
            }
        }

        /// <summary>
        /// Get Patient bill to display in add edit Page
        /// </summary>
        /// <param name="billingsData">billingsData object which is used to return data for Add Edit Patient Bills </param>
        /// <returns>Patient bills</returns>
        public async Task GetPatientBillingDataAsync(BillingItemDTO billingsData)
        {
            if (billingsData.IsBillDetailViews)
            {
                //var data = await SqlConnection.FindWithQueryAsync<UserModel>(
                //   "SELECT A.FirstName " +
                //   "FROM UserModel A " +
                //   "WHERE A.UserID=? ", 46128);

                billingsData.PatientBillItem = await SqlConnection.FindWithQueryAsync<PatientBillModel>(
                    "SELECT A.PatientBillID, A.BillDateTime, B.FirstName || ' ' || B.LastName AS PatientName, C.FirstName || ' ' || C.LastName as ProviderName, D.Name AS ProgramName, E.OrganisationName, A.GrossTotal, A.Discount, A.TotalPaid, F.Name AS PaymentMode, A.IsActive,A.ProviderID " +
                    "FROM PatientBillModel A " +
                    "LEFT JOIN UserModel B ON A.PatientID = B.UserID  " +
                    "LEFT JOIN UserModel C ON A.ProviderID= C.UserID " +
                    "LEFT JOIN ProgramModel D ON A.ProgramID = D.ProgramID  " +
                    "LEFT JOIN OrganisationModel E ON A.OrganisationID = E.OrganisationID  " +
                    "LEFT JOIN PaymentModeI18NModel F ON A.PaymentModeID= F.PaymentModeID " +
                    "WHERE A.OrganisationID=? AND A.PatientBillID=? ", billingsData.OrganisationID, billingsData.PatientBillItem.PatientBillID);

                billingsData.PatientBillItems = await SqlConnection.QueryAsync<PatientBillItemModel>(
                    "SELECT DISTINCT A.PatientBillID, A.BillingItemID, B.Name, A.Amount, A.IsActive " +
                    "FROM PatientBillItemModel A " +
                    "JOIN BillingItemsI18NModel B ON A.BillingItemID = B.BillingItemID  WHERE A.PatientBillID =? AND  A.IsActive = 1", billingsData.PatientBillItem.PatientBillID);

                byte languageID = (byte)billingsData.LanguageID;
                ContactDTO contactData = new ContactDTO { LanguageID = languageID };
                await new ContactDatabase().GetOrganisationContactsAsync(contactData);

                if (contactData?.Contacts?.Count > 0)
                {
                    var OrganisationAddress = contactData.Contacts.Find(x => x.ContactTypeID == 183);
                    if (OrganisationAddress != null)
                    {
                        billingsData.PatientBillItem.OrganisationAddress = OrganisationAddress.ContactValue;
                    }

                    var OrganisationContact = contactData.Contacts.Find(x => x.ContactTypeID == 185);
                    if (OrganisationContact != null)
                    {
                        billingsData.PatientBillItem.OrganisationContact = OrganisationContact.ContactValue;
                    }
                }
            }
            else
            {
                billingsData.PatientProgramOptionList = await SqlConnection.QueryAsync<OptionModel>(
                    "SELECT DISTINCT A.IsActive AS IsDisabled, A.ProgramID AS OptionID, B.Name AS OptionText " +
                    "FROM PatientProgramModel A " +
                    "JOIN ProgramModel B ON A.ProgramId = B.ProgramID WHERE A.PatientID = ?", billingsData.SelectedUserID);

                billingsData.PatientProvidersOptionList = await SqlConnection.QueryAsync<OptionModel>(
                   "SELECT DISTINCT A.CareGiverID AS OptionID, C.FirstName || ' ' || C.LastName AS OptionText , B.ProgramID AS GroupName " +
                    "FROM CaregiverModel A " +
                    "JOIN PatientProgramModel B ON A.ProgramID = B.ProgramID " +
                    $"JOIN UserModel C ON A.CareGiverID = C.UserID WHERE B.PatientID = ?", billingsData.SelectedUserID).ConfigureAwait(false);

                billingsData.PaymentModeOptionList = await SqlConnection.QueryAsync<OptionModel>(
                    "SELECT DISTINCT A.PaymentModeID AS OptionID, B.Name AS OptionText " +
                    "FROM BillPaymentModel A " +
                    "JOIN PaymentModeI18NModel B ON A.PaymentModeID = B.PaymentModeID WHERE A.IsActive = 1");

                billingsData.PatientBillingItems = await SqlConnection.QueryAsync<OptionModel>(
                    "SELECT DISTINCT A.BillingItemID AS OptionID, D.Name AS OptionText, A.Amount AS ParentOptionID, A.ProgramID AS GroupName " +
                    "FROM ProgramBillingModel A " +
                    "JOIN PatientProgramModel B On A.ProgramID = B.ProgramID " +
                    "JOIN BillItemModel C ON C.BillingItemID = A.BillingItemID " +
                    "JOIN BillingItemsI18NModel  D ON C.BillingItemID= D.BillingItemID " +
                    "WHERE B.PatientID=? ", billingsData.SelectedUserID);

                if (billingsData.PatientBillItem?.PatientBillID != Guid.Empty)
                {                  
                    billingsData.PatientBillItem = await SqlConnection.FindWithQueryAsync<PatientBillModel>(
                        "SELECT A.ProgramID, A.BillDateTime,A.PatientBillID, A.PatientID, A.OrganisationID, A.ProviderID, A.TotalPaid, A.Discount, A.PaymentModeID, A.GrossTotal, A.IsActive " +
                        "FROM PatientBillModel A WHERE A.PatientBillID = ?  ", billingsData.PatientBillItem?.PatientBillID);

                    billingsData.PatientBillItems = await SqlConnection.QueryAsync<PatientBillItemModel>(
                        "SELECT DISTINCT A.PatientBillID, A.BillingItemID, A.Amount, A.IsActive " +
                        "FROM PatientBillItemModel A WHERE A.PatientBillID =? AND A.IsActive= 1", billingsData.PatientBillItem.PatientBillID);            
                }
            }
        }

        /// <summary>
        /// Get patient billing data from database for sending it to server
        /// </summary>
        /// <param name="patientBillData">object to get patient billing data</param>
        /// <returns>patient billing data with operation status</returns>
        public async Task GetPatientBillingsForSyncToServerAsync(BillingItemDTO patientBillData)
        {
            patientBillData.PatientBills = await SqlConnection.QueryAsync<PatientBillModel>("SELECT * FROM PatientBillModel WHERE IsSynced = 0 ");
            patientBillData.PatientBillItems = await SqlConnection.QueryAsync<PatientBillItemModel>("SELECT * FROM PatientBillItemModel WHERE IsSynced = 0");
        }

        /// <summary>
        /// Save Program Billing Items
        /// </summary>
        /// <param name="billingData">Program billing data</param>
        /// <returns>return BillingItemDTO</returns>
        public async Task SaveProgramBillingItemsAsync(BillingItemDTO billingData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (ProgramBillingModel programBillItem in billingData.ProgramBillingItems)
                {
                    if (transaction.FindWithQuery<ProgramBillingModel>("SELECT 1 FROM ProgramBillingModel WHERE ProgramBillingItemID=? AND  OrganisationID=?", programBillItem.ProgramBillingItemID, programBillItem.OrganisationID) == null)
                    {
                        transaction.Execute("INSERT INTO ProgramBillingModel(ProgramBillingItemID, ProgramID, OrganisationID, BillingItemID, Amount, IsActive) VALUES(?, ?, ?, ?, ?, ?) "
                         , programBillItem.ProgramBillingItemID, programBillItem.ProgramID, programBillItem.OrganisationID, programBillItem.BillingItemID, programBillItem.Amount, programBillItem.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE ProgramBillingModel SET ProgramID=?, BillingItemID=?, Amount=?, IsActive=? WHERE ProgramBillingItemID=? AND OrganisationID=?",
                          programBillItem.ProgramID, programBillItem.BillingItemID, programBillItem.Amount, programBillItem.IsActive, programBillItem.ProgramBillingItemID, programBillItem.OrganisationID);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save Payment Mode  
        /// </summary>
        /// <param name="paymentData"> paymentData DTO Which holds Data to save </param>
        /// <returns> paymentData</returns>
        public async Task SavePaymentModesAsync(PaymentModeDTO paymentData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (BillPaymentModel paymentMode in paymentData.BillPaymentModes)
                {
                    if (transaction.FindWithQuery<BillPaymentModel>("SELECT 1 FROM BillPaymentModel WHERE PaymentModeID=? ", paymentMode.PaymentModeID) == null)
                    {
                        transaction.Execute("INSERT INTO BillPaymentModel(PaymentModeID,  IsActive) VALUES(?, ?) "
                            , paymentMode.PaymentModeID, paymentMode.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE BillPaymentModel SET PaymentModeID=?,  IsActive=? WHERE PaymentModeID=?",
                            paymentMode.PaymentModeID, paymentMode.IsActive, paymentMode.PaymentModeID);
                    }
                }
            });
        }

        /// <summary>
        /// Save Payment ModeI18N 
        /// </summary>
        /// <param name="paymentData"> paymentData DTO Which holds Data to save </param>
        /// <returns> paymentData</returns>
        public async Task SavePaymentModeI18NAsync(PaymentModeDTO paymentData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (PaymentModeI18NModel paymentMode in paymentData.PaymentModeI18N)
                {
                    if (transaction.FindWithQuery<PaymentModeI18NModel>("SELECT 1 FROM PaymentModeI18NModel WHERE PaymentModeID=? ", paymentMode.PaymentModeID) == null)
                    {
                        transaction.Execute("INSERT INTO PaymentModeI18NModel(PaymentModeID, LanguageID, Name, IsActive) VALUES(?, ?, ?, ?) "
                            , paymentMode.PaymentModeID, paymentMode.LanguageID, paymentMode.Name, paymentMode.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE PaymentModeI18NModel SET LanguageID=?, Name=?, IsActive=? WHERE PaymentModeID=?",
                            paymentMode.LanguageID, paymentMode.Name, paymentMode.IsActive, paymentMode.PaymentModeID);
                    }
                }
            });
        }

        /// <summary>
        /// Save BillingItem s
        /// </summary>
        /// <param name="billingData">billingData which holds data to save </param>
        /// <returns>billingData</returns>
        public async Task SaveBillItemsAsync(BillingItemDTO billingData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (BillItemModel billItem in billingData.BillItems)
                {
                    if (transaction.FindWithQuery<BillItemModel>("SELECT 1 FROM BillItemModel WHERE BillingItemID=?", billItem.BillingItemID) == null)
                    {
                        transaction.Execute("INSERT INTO BillItemModel(BillingItemID, IsActive) VALUES(?, ?) ", billItem.BillingItemID, billItem.IsActive);
                    }
                    else
                    {
                        transaction.Execute("UPDATE BillItemModel SET IsActive=? WHERE BillingItemID=?", billItem.IsActive, billItem.BillingItemID);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save BillingI18N data 
        /// </summary>
        /// <param name="billingData">billingData which holds data to save </param>
        /// <returns>billingData</returns>
        public async Task SaveBillingItemsI18NAsync(BillingItemDTO billingData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (BillingItemsI18NModel billingItemsI18N in billingData.BillingItemsI18N)
                {
                    if (transaction.FindWithQuery<BillingItemsI18NModel>("SELECT 1 FROM BillingItemsI18NModel WHERE BillingItemID=? ", billingItemsI18N.BillingItemID) == null)
                    {
                        transaction.Execute("INSERT INTO BillingItemsI18NModel(BillingItemID, IsActive, LanguageID, Name) " +
                            "VALUES(?, ?, ?, ?)", billingItemsI18N.BillingItemID, billingItemsI18N.IsActive, billingItemsI18N.LanguageID, billingItemsI18N.Name);
                    }
                    else
                    {
                        transaction.Execute("UPDATE BillingItemsI18NModel SET IsActive=?, LanguageID=?, Name=? WHERE BillingItemID=? ",
                            billingItemsI18N.IsActive, billingItemsI18N.LanguageID, billingItemsI18N.Name, billingItemsI18N.BillingItemID);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// update is sync status and Patient bill id
        /// </summary>
        /// <param name="patientBillData">bill data to update </param>
        /// <returns>update issynced and Patient bill id</returns>
        public async Task UpdatePatientBillsSyncStatusAsync(BillingItemDTO patientBillData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (PatientBillModel bill in patientBillData.PatientBills)
                {
                    SaveResultModel result = patientBillData.SavePatientBills?.FirstOrDefault(x => x.ClientGuid == bill.PatientBillID);
                    bill.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                    switch (bill.ErrCode)
                    {
                        case ErrorCode.OK:
                            // Data is successfully synced, so only update sync flag
                            transaction.Execute("UPDATE PatientBillModel SET IsSynced = 1, ErrCode = ? WHERE PatientBillID = ?", bill.ErrCode, bill.PatientBillID);
                            transaction.Execute("UPDATE PatientBillItemModel SET IsSynced = 1 WHERE PatientBillID = ?", bill.PatientBillID);
                            break;
                        case ErrorCode.DuplicateGuid:
                            // Update with new Guid
                            Guid newPatientBillID = GenerateNewGuid(transaction);
                            transaction.Execute("UPDATE PatientBillModel SET PatientBillID = ?, IsSynced = 0 WHERE PatientBillID = ?", newPatientBillID, bill.PatientBillID);
                            transaction.Execute("UPDATE PatientBillItemModel SET PatientBillID = ?, IsSynced = 0 WHERE PatientBillID = ?", newPatientBillID, bill.PatientBillID);
                            bill.PatientBillID = newPatientBillID;
                            break;
                        default:
                            // Mark record with the received error code
                            transaction.Execute("UPDATE PatientBillModel SET ErrCode = ? WHERE PatientBillID = ?", bill.ErrCode, bill.PatientBillID);
                            break;
                    }
                }
            });
        }

        /// <summary>
        /// Save Pateint Bills  and Patient Billing  items data 
        /// </summary>
        /// <param name="billData">patientBillingData obejct Which holds Data to save </param>
        /// <returns>patientBillingData</returns>
        public async Task SavePatientBillsAsync(BillingItemDTO billData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SavePatientBillItems(billData, transaction);
                SavePatientBills(billData, transaction);
            }).ConfigureAwait(false);
        }

        private void SavePatientBills(BillingItemDTO billData, SQLiteConnection transaction)
        {
            foreach (PatientBillModel bill in billData.PatientBills)
            {
                if (transaction.FindWithQuery<PatientBillModel>("SELECT 1 FROM PatientBillModel WHERE PatientBillID=? ", bill.PatientBillID) == null)
                {
                    transaction.Execute(
                        "INSERT INTO PatientBillModel(PatientBillID, BillDateTime, OrganisationID, PatientID, ProgramID, ProviderID, GrossTotal, Discount, TotalPaid, PaymentModeID, IsActive, AddedOn, AddedById, ErrCode, IsSynced) " +
                        "VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        bill.PatientBillID, bill.BillDateTime, bill.OrganisationID, bill.PatientID, bill.ProgramID, bill.ProviderID, bill.GrossTotal,
                        bill.Discount, bill.TotalPaid, bill.PaymentModeID, bill.IsActive, bill.AddedOn, bill.AddedById, bill.ErrCode, bill.IsSynced);
                }
                else
                {
                    transaction.Execute(
                        "UPDATE PatientBillModel SET BillDateTime=?, OrganisationID=?, PatientID=?, ProgramID=?, ProviderID=?, GrossTotal=?, Discount=?, TotalPaid=?, PaymentModeID=?, IsActive=?, ErrCode=?, IsSynced=?,AddedOn=? WHERE PatientBillID=?",
                        bill.BillDateTime, bill.OrganisationID, bill.PatientID, bill.ProgramID, bill.ProviderID, bill.GrossTotal,
                        bill.Discount, bill.TotalPaid, bill.PaymentModeID, bill.IsActive, bill.ErrCode, bill.IsSynced, bill.AddedOn, bill.PatientBillID);
                }
            }
        }

        private void SavePatientBillItems(BillingItemDTO patientBillingData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(patientBillingData.PatientBillItems))
            {
                foreach (PatientBillItemModel billItem in patientBillingData.PatientBillItems)
                {
                    if (transaction.FindWithQuery<PatientBillItemModel>(
                        "SELECT 1 FROM PatientBillItemModel WHERE PatientBillID=? AND BillingItemID=?", billItem.PatientBillID, billItem.BillingItemID) == null)
                    {
                        transaction.Execute(
                            "INSERT INTO PatientBillItemModel(PatientBillID, BillingItemID, Amount, IsActive, IsSynced, AddedOn) VALUES(?, ?, ?, ?, ?, ?) ",
                            billItem.PatientBillID, billItem.BillingItemID, billItem.Amount, billItem.IsActive, billItem.IsSynced, billItem.AddedOn);
                    }
                    else
                    {
                        transaction.Execute(
                            "UPDATE PatientBillItemModel SET Amount=?, IsActive=?, IsSynced=?,AddedOn=? WHERE PatientBillID=? AND BillingItemID=?",
                            billItem.Amount, billItem.IsActive, billItem.IsSynced, billItem.AddedOn, billItem.PatientBillID, billItem.BillingItemID);
                    }
                }
            }
        }

        private Guid GenerateNewGuid(SQLiteConnection transaction)
        {
            Guid newGuid = GenericMethods.GenerateGuid();
            while (transaction.ExecuteScalar<int>("SELECT 1 FROM PatientBillModel WHERE PatientBillID = ?", newGuid) > 0)
            {
                newGuid = GenericMethods.GenerateGuid();
            }
            return newGuid;
        }

    }
}