using AlphaMDHealth.Model;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    interface IFoodLibrary
    {
        /// <summary>
        /// Search food item data
        /// </summary>
        /// <param name="search">search string based on which food items are to be found</param>
        /// <param name="isBarcode">true if User sends barcode string as search parameter </param>
        /// <param name="imageRequired">true if image is required to be fetched</param>
        /// <returns>Food items matching the given search string</returns>
        Task<FoodItemDTO> SearchFoodItemAsync(string search, bool isBarcode, bool imageRequired);

        /// <summary>
        /// Get food nutrients for given food
        /// </summary>
        /// <param name="foodIdentifier">Identifier of the selected food item</param>
        /// <returns>Food item nutrition info</returns>
        Task<HealthReadingDTO> GetFoodDataAsync(string foodIdentifier);
    }
}