using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientDataLayer
{
    /// <summary>
    /// File category database
    /// </summary>
    public class FileCategoryDatabase : BaseDatabase
    {
        /// <summary>
        /// insert or update File Categories in the database
        /// </summary>
        /// <param name="categoryData">Object with modified resources and refence for operation status</param>
        /// <returns>Operation Status</returns>
        public async Task SaveFileCategoriesAsync(FileCategoryDTO categoryData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(categoryData.FileCategories))
                {
                    foreach (var category in categoryData.FileCategories)
                    {
                        transaction.InsertOrReplace(category);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// insert or update File Category Details in the database
        /// </summary>
        /// <param name="categoryData">Object with modified resources and refence for operation status</param>
        /// <returns>Operation Status</returns>
        public async Task SaveFileCategoryDetailsAsync(FileCategoryDTO categoryData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(categoryData.FileCategoryDetails))
                {
                    foreach (var categoryDetail in categoryData.FileCategoryDetails)
                    {
                        transaction.InsertOrReplace(categoryDetail);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get file categories option list
        /// </summary>
        /// <param name="fileData">Reference object to return category options data</param>
        /// <returns>return File category options</returns>
        public async Task GetFileCategoryOptionsAsync(FileDTO fileData)
        {
            if (fileData.RecordCount == -1)
            {
                fileData.CategoryOptions = await SqlConnection.QueryAsync<OptionModel>(
                    "SELECT A.FileCategoryID AS OptionID, B.Name AS OptionText, B.Description AS GroupName " +
                    "FROM FileCategoryModel A " +
                    "JOIN FileCategoryDetailModel B ON A.FileCategoryID = B.FileCategoryID AND B.LanguageID = ? AND B.IsActive = 1 " +
                    "WHERE A.IsActive = 1 AND A.FileCategoryID NOT IN (SELECT FileCategoryID FROM FileModel WHERE FileID <> ? AND UserID = ? AND IsActive = 1)"
                    , fileData.LanguageID, fileData.File.FileID, fileData.File.UserID
                ).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get Education category details from database
        /// </summary>
        /// <param name="contentPages">Reference object to return category data</param>
        /// <returns>Category data</returns>
        public async Task GetFileCategoriesToSyncImagesAsync(FileCategoryDTO fileCategoryData)
        {
            fileCategoryData.FileCategories = await SqlConnection.QueryAsync<FileCategoryModel>(
                "SELECT FileCategoryID, ImageName FROM FileCategoryModel WHERE IsDataDownloaded = 0 AND IsActive = 1 "
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Update file category image download status
        /// </summary>
        /// <param name="fileCategoryData">Reference object to return category data</param>
        /// <returns>Operation status</returns>
        public async Task UpdateFileCategoriesImageSyncStatusAsync(FileCategoryDTO fileCategoryData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                if (GenericMethods.IsListNotEmpty(fileCategoryData.FileCategories))
                {
                    foreach (FileCategoryModel category in fileCategoryData.FileCategories)
                    {
                        transaction.Execute(
                            "UPDATE FileCategoryModel SET IsDataDownloaded = 1, ImageName = ?  WHERE FileCategoryID = ?"
                            , category.ImageName, category.FileCategoryID);
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}