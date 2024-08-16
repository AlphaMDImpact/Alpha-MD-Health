using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;
using System.Globalization;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// Content pages database
    /// </summary>
    public class ContentPageDatabase : BaseDatabase
    {
        #region Save Content Data

        /// <summary>
        /// Save ContentPage to database
        /// </summary>
        /// <param name="contentData">ContentPage to be saved</param>
        /// <returns>ContentPage data and Operation result</returns>
        public async Task SaveContentPagesAsync(ContentPageDTO contentData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveContentPages(contentData, transaction);
                SaveContentPageDetails(contentData, transaction);
                SavePatientEducations(contentData, transaction);
            }).ConfigureAwait(false);
        }

        private void SaveContentPages(ContentPageDTO contentData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(contentData.Pages))
            {
                foreach (ContentPageModel contentpage in contentData.Pages)
                {
                    if (!string.IsNullOrWhiteSpace(contentpage.ImageName))
                    {
                        contentpage.IsDataDownloaded = false;
                    }
                    transaction.InsertOrReplace(contentpage);
                }
                contentData.RecordCount += contentData.Pages.Count;
            }
        }

        private void SaveContentPageDetails(ContentPageDTO contentData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(contentData.PageDetails))
            {
                foreach (ContentDetailModel detail in contentData.PageDetails)
                {
                    if (detail.PageID > 0 && detail.LanguageID > 0)
                    {
                        if (detail.PageData?.Contains(Constants.IMAGE_START_TAG) == true)
                        {
                            detail.IsDataDownloaded = false;
                        }
                        if (transaction.FindWithQuery<ContentDetailModel>("SELECT 1 FROM ContentDetailModel WHERE LanguageID = ? AND PageName = ? AND PageID = ? "
                            , detail.LanguageID, PageType.ContentPage, detail.PageID) == null)
                        {
                            transaction.Execute("INSERT INTO ContentDetailModel (PageID, LanguageID, PageHeading, PageData, IsDataDownloaded, IsActive, PageName, Description) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                                detail.PageID, detail.LanguageID, detail.PageHeading, detail.PageData, detail.IsDataDownloaded, detail.IsActive, detail.PageName, detail.Description);
                        }
                        else
                        {
                            transaction.Execute("UPDATE ContentDetailModel SET PageHeading = ?, PageData = ?, IsActive = ?, IsDataDownloaded = ?, PageName = ?, Description = ? WHERE PageID = ? AND LanguageID = ?",
                                detail.PageHeading, detail.PageData, detail.IsActive, detail.IsDataDownloaded, detail.PageName, detail.Description, detail.PageID, detail.LanguageID);
                        }
                    }
                }
                contentData.RecordCount += contentData.PageDetails.Count;
            }
        }

        private void SavePatientEducations(ContentPageDTO contentData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(contentData.PatientEducations))
            {
                foreach (PatientEducationModel education in contentData.PatientEducations)
                {
                    transaction.InsertOrReplace(education);
                }
                contentData.RecordCount += contentData.PatientEducations.Count;
            }
        }

        public async Task SavePatientEducationAsync(PatientEducationModel education)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                transaction.InsertOrReplace(education);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update Education page Image and status
        /// </summary>
        /// <param name="contentData">page data</param>
        /// <returns>operation status</returns>
        public async Task UpdateContentPagesSyncImageStatusAsync(ContentPageDTO contentData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                UpdateContentPages(contentData, transaction);
                UpdateContentPageDetails(contentData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update education Status on PatientEducationID
        /// </summary>
        /// <param name="contentData">Reference object to hold patient education</param>
        /// <returns>Operation Status</returns>
        public async Task UpdateEducationStatusAsync(ContentPageDTO contentData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (PatientEducationModel education in contentData.PatientEducations)
                {
                    transaction.Execute(
                        "UPDATE PatientEducationModel SET Status = ? WHERE PatientEducationID = ? "
                        , education.Status, education.PatientEducationID);
                }
            }).ConfigureAwait(false);
        }

        private void UpdateContentPageDetails(ContentPageDTO contentData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(contentData.PageDetails))
            {
                foreach (var page in contentData.PageDetails)
                {
                    transaction.Execute("UPDATE ContentDetailModel SET IsDataDownloaded = 1, PageData = ? WHERE PageID = ? AND LanguageID = ?"
                        , page.PageData, page.PageID, page.LanguageID);
                }
            }
        }

        private void UpdateContentPages(ContentPageDTO contentData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(contentData.Pages))
            {
                foreach (var page in contentData.Pages)
                {
                    transaction.Execute("UPDATE ContentPageModel SET IsDataDownloaded = 1, ImageName = ?  WHERE PageID = ?"
                        , page.ImageName, page.PageID);
                }
            }
        }

        #endregion

        #region Fetch Content Data

        /// <summary>
        /// Get All the Education and Status to sync
        /// </summary>
        /// <param name="contentData">Reference object to hold Education ID and Status</param>
        /// <returns>Operation Status</returns>
        public async Task GetEducationStatusAsync(ContentPageDTO contentData)
        {
            contentData.PatientEducations = await SqlConnection.QueryAsync<PatientEducationModel>(
                "SELECT PatientEducationID, Status FROM PatientEducationModel ");
        }

        /// <summary>
        /// Get Content Page from database
        /// </summary>
        /// <param name="contentData">Reference object to return ContentPage</param>
        /// <returns>Content Page data</returns>
        public async Task GetContentPagesAsync(ContentPageDTO contentData)
        {
            if (contentData.RecordCount == -2)
            {
                await GetTaskRelatedDataAsync(contentData);
                contentData.Page = await SqlConnection.FindWithQueryAsync<ContentPageModel>(
                    "SELECT A.PageID, B.PageHeading AS Title, B.PageData, A.IsLink, A.EducationCategoryID  " +
                    "FROM ContentPageModel A " +
                    "JOIN ContentDetailModel B ON A.PageID = B.PageID " +
                    "WHERE B.PageID = ? AND B.LanguageID = ?"
                    , contentData.Page.PageID, contentData.LanguageID
                ).ConfigureAwait(false);
            }

            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            if (contentData.SelectedUserID == 0)
            {
                var orderRandom = "";
                if (contentData.RecordCount > 0)
                {
                    orderRandom = $" ORDER BY RANDOM() ";
                }
                GetConditionalQueries(contentData, out string selectCols, out string eductaionCatergoryJoin, out string categoryIDcondition);
                string roleBasedQuery = GetRoleBasedQuery(contentData.CreatedByID);
                string pColor;
                if (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                {
                    pColor = "PPM.ProgramGroupIdentifier";
                }
                else
                {
                    pColor = "P.ProgramGroupIdentifier";
                }
                contentData.Pages = await SqlConnection.QueryAsync<ContentPageModel>(
                                "SELECT DISTINCT PEM.PatientEducationID AS EducationID, PEM.ProgramID, P.Name AS ProgramName, PEM.Status AS Status, " +
                                    "A.PageID, B.PageHeading AS Title, A.PageTags, A.ImageName, C.PageHeading AS CategoryName, PEM.FromDate, PEM.ToDate, " +
                                    $"{pColor} AS ProgramColor, {selectCols} A.EducationCategoryID " +
                                "FROM ContentPageModel A " +
                                $"JOIN ContentDetailModel B ON A.PageID = B.PageID AND B.LanguageID = ? AND B.PageName = ? {eductaionCatergoryJoin}" +
                                $"LEFT JOIN EducationCategoryDetailModel C ON C.PageID = A.EducationCategoryID AND C.LanguageID = ? {roleBasedQuery}" +
                                "LEFT JOIN ProgramModel P ON PEM.ProgramID = P.ProgramID " +
                                "LEFT JOIN PatientProgramModel PPM ON PPM.ProgramID = P.ProgramID AND PPM.PatientID = ? " +
                                "WHERE A.IsEducation = ? AND A.IsPublished = 1 " +
                                $"{categoryIDcondition} {orderRandom} "
                                , contentData.LanguageID, PageType.ContentPage, contentData.LanguageID, contentData.CreatedByID, contentData.Page.IsEducation
                            ).ConfigureAwait(false);
            }
            else
            {
                contentData.Pages = await SqlConnection.QueryAsync<ContentPageModel>(
                    "SELECT  PEM.PatientEducationID AS PageID, B.PageHeading AS Title, A.PageTags, C.PageHeading AS CategoryName, PEM.ProgramID, A.ImageName, " +
                        "A.EducationCategoryID, PEM.Status, PM.Name AS ProgramName, PM.ProgramGroupIdentifier AS ProgramColor, PEM.FromDate, PEM.ToDate  " +
                    "FROM ContentPageModel A " +
                    "JOIN ContentDetailModel B ON B.PageID = A.PageID AND B.LanguageID = ? AND A.IsEducation = ? AND A.IsPublished = 1 " +
                    "JOIN PatientEducationModel PEM ON PEM.PageID = A.PageID AND PEM.UserID = ? AND PEM.IsActive = 1 " +
                    "LEFT JOIN ProgramModel PM ON PEM.ProgramID = PM.ProgramID " +
                    "LEFT JOIN EducationCategoryDetailModel C ON C.PageID = A.EducationCategoryID AND C.LanguageID = B.LanguageID  " +
                    "ORDER BY RANDOM() "
                    , contentData.LanguageID, contentData.Page.IsEducation, contentData.SelectedUserID
                ).ConfigureAwait(false);
            }

            if (roleID == (int)RoleName.CareTaker && contentData.Pages?.Count > 0)
            {
                bool notSelectedUserID = contentData.SelectedUserID < 1;
                if (notSelectedUserID)
                {
                    contentData.SelectedUserID = Preferences.Get(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0);
                }
                List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(contentData).ConfigureAwait(false);
                contentData.Pages = sharedPrograms?.Count > 0
                    ? contentData.Pages.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ProgramID))?.ToList()
                    : null;
                if (notSelectedUserID)
                {
                    contentData.SelectedUserID = 0;
                }
            }

            if (contentData.RecordCount > 0)
            {
                contentData.Pages = contentData.Pages?.Take((int)contentData.RecordCount)?.ToList();
            }
        }

        /// <summary>
        /// Get Content Page Details from database
        /// </summary>
        /// <param name="contentData">Reference object to return ContentPage</param>
        /// <returns>Content Page data</returns>
        public async Task GetContentPagesStatusAsync(ContentPageDTO contentData)
        {
            contentData.Pages = await SqlConnection.QueryAsync<ContentPageModel>(
                "SELECT PageID, ImageName, PageData FROM ContentPageModel WHERE  IsDataDownloaded = 0 ").ConfigureAwait(false);

            contentData.PageDetails = await SqlConnection.QueryAsync<ContentDetailModel>(
                "SELECT A.PageID, B.PageData,B.LanguageID FROM ContentPageModel A " +
                "JOIN ContentDetailModel B ON A.PageID = B.PageID WHERE A.IsLink = 0 AND B.IsDataDownloaded = 0 ").ConfigureAwait(false);
        }

        /// <summary>
        /// Get Content Page Details
        /// </summary>
        /// <param name="pageData">Reference object to return ContentPage page data in resource</param>
        /// <returns>Content Page data</returns>
        public async Task GetContentDetailsAsync(BaseDTO pageData)
        {
            pageData.LanguageID = (byte)pageData.LanguageID;
            pageData.Resources = await SqlConnection.QueryAsync<ResourceModel>(
                "SELECT A.PageID AS ResourceKey, A.PageHeading AS ResourceValue, A.PageData AS InfoValue, A.LanguageID, B.IsLink AS IsWebLink " +
                "FROM ContentDetailModel A " +
                "JOIN ContentPageModel B ON A.PageID =  B.PageID " +
                "WHERE A.PageID = ? AND A.LanguageID = ?", pageData.AddedBy, pageData.LanguageID).ConfigureAwait(false);

            if (pageData.CreatedByID > 0 && pageData.LastModifiedBy == PageType.ContentPage.ToString())
            {
                pageData.Response = (await SqlConnection.FindWithQueryAsync<TaskModel>(
                    "SELECT Status FROM TaskModel WHERE PatientTaskID = ?", pageData.CreatedByID).ConfigureAwait(false)).Status;
            }
        }

        private async Task GetTaskRelatedDataAsync(ContentPageDTO contentData)
        {
            if (contentData.PermissionAtLevelID > 0)
            {
                var taskData = await SqlConnection.FindWithQueryAsync<TaskModel>(
                    "SELECT * FROM TaskModel WHERE PatientTaskID = ?", contentData.PermissionAtLevelID);
                if (taskData != null)
                {
                    contentData.Page.PageID = taskData.ItemID;
                    contentData.AddedBy = taskData.Status;
                    ////to get education id if the education data is not present in database.
                    contentData.ErrorDescription = taskData.ItemID.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private string GetRoleBasedQuery(long createdBy)
        {
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            return (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
                ? " JOIN PatientEducationModel PEM ON A.PageID = PEM.PageID AND PEM.IsActive = 1 " +
                        $"AND PEM.UserID = {createdBy} " +
                    $"LEFT JOIN TaskModel TM ON TM.ItemID = A.PageID AND TM.TaskType = 'EducationKey' AND TM.UserID = {createdBy} "
                : $"JOIN PatientEducationModel PEM ON A.PageID = PEM.PageID AND PEM.IsActive = 1 AND PEM.UserID = {createdBy} ";
        }

        private void GetConditionalQueries(ContentPageDTO contentData, out string selectCols, out string eductaionCatergoryJoin, out string categoryIDcondition)
        {
            selectCols = "";
            eductaionCatergoryJoin = "";
            categoryIDcondition = "";
            if (contentData.Page?.EducationCategoryID > 0)
            {
                selectCols = "C.PageData AS CategoryDetails, D.ImageName AS CategoryImage, ";
                eductaionCatergoryJoin = "LEFT JOIN EductaionCatergoryModel D ON D.EducationCategoryID = A.EducationCategoryID ";
                categoryIDcondition = $"AND A.EducationCategoryID = {contentData.Page.EducationCategoryID} ";
            }
            else
            {
                selectCols = "C.PageID AS EducationCategoryID, ";
            }
        }

        #endregion
    }
}