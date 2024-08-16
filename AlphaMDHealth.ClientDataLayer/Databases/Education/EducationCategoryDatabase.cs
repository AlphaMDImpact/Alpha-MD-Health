using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public class EducationCategoryDatabase : BaseDatabase
    {
        /// <summary>
        /// Save Education Categories
        /// </summary>
        /// <param name="educationCategory">Education category data</param>
        /// <returns>operation status</returns>
        public async Task SaveEducationCategoryAsync(EducationCategoryDTO educationCategory)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (EductaionCatergoryModel category in educationCategory.EductaionCatergories)
                {
                    if (!string.IsNullOrWhiteSpace(category.ImageName))
                    {
                        category.IsDataDownloaded = false;
                    }
                    transaction.InsertOrReplace(category);
                }
                if (GenericMethods.IsListNotEmpty(educationCategory.CategoryDetails))
                {
                    SaveEducationCategoryDetails(educationCategory, transaction);
                }
            }).ConfigureAwait(false);

        }

        private void SaveEducationCategoryDetails(EducationCategoryDTO educationCategory, SQLiteConnection transaction)
        {
            foreach (EducationCategoryDetailModel categoryDetails in educationCategory.CategoryDetails)
            {
                if (categoryDetails.PageData.Contains(Constants.IMAGE_START_TAG))
                {
                    categoryDetails.IsDataDownloaded = false;
                }
                if (transaction.Query<EducationCategoryDetailModel>("SELECT * FROM EducationCategoryDetailModel")
                   .FindAll(x => x.LanguageID == categoryDetails.LanguageID && x.PageID == categoryDetails.PageID)?.Count == 0)
                {
                    transaction.Execute("INSERT INTO EducationCategoryDetailModel (PageID, LanguageID, PageHeading, PageData , IsDataDownloaded) VALUES (?, ?, ?, ?, ?)",
                        categoryDetails.PageID, categoryDetails.LanguageID, categoryDetails.PageHeading, categoryDetails.PageData, categoryDetails.IsDataDownloaded);
                }
                else
                {
                    transaction.Execute("UPDATE EducationCategoryDetailModel SET PageHeading = ?, PageData = ?, IsActive = ?, IsDataDownloaded = ? WHERE PageID = ? AND LanguageID = ?",
                         categoryDetails.PageHeading, categoryDetails.PageData, categoryDetails.IsActive, categoryDetails.IsDataDownloaded, categoryDetails.PageID, categoryDetails.LanguageID);
                }
            }
        }

        /// <summary>
        /// Update Education category page Image and status
        /// </summary>
        /// <param name="pageData">education category data</param>
        /// <returns>operation status</returns>
        public async Task UpdateContentPagesSyncImageStatusAsync(EducationCategoryDTO categoryData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(categoryData.EductaionCatergories))
                {
                    foreach (var page in categoryData.EductaionCatergories)
                    {
                        transaction.Execute("UPDATE EductaionCatergoryModel SET IsDataDownloaded = 1, ImageName = ?  WHERE EducationCategoryID = ?", page.ImageName, page.EducationCategoryID);
                    }
                }
                if (GenericMethods.IsListNotEmpty(categoryData.CategoryDetails))
                {
                    foreach (var page in categoryData.CategoryDetails)
                    {
                        transaction.Execute("UPDATE EducationCategoryDetailModel SET IsDataDownloaded = 1 ,PageData = ? WHERE PageID = ? AND LanguageID=? ", page.PageData, page.PageID, page.LanguageID);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Education category details from database
        /// </summary>
        /// <param name="contentPages">Reference object to return Education category data</param>
        /// <returns>Education category data</returns>
        public async Task GetEducationCategoryStatusAsync(EducationCategoryDTO category)
        {
            category.EductaionCatergories = await SqlConnection.QueryAsync<EductaionCatergoryModel>("SELECT EducationCategoryID, ImageName FROM EductaionCatergoryModel WHERE  IsDataDownloaded = 0 ").ConfigureAwait(false);
            category.CategoryDetails = await SqlConnection.QueryAsync<EducationCategoryDetailModel>("SELECT B.PageID, B.PageData,B.LanguageID FROM EductaionCatergoryModel A INNER JOIN EducationCategoryDetailModel B ON A.EducationCategoryID = B.PageID WHERE B.IsDataDownloaded = 0 ").ConfigureAwait(false);
        }
    }
}
