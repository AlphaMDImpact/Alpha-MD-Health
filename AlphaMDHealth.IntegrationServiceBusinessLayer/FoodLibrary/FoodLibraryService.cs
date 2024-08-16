using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class FoodLibraryService : LibraryService
    {
        private readonly IFoodLibrary _foodLibrary;

        /// <summary>
        /// Initializes food library for the given source
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public FoodLibraryService(HttpContext httpContext) : base(httpContext)
        {
            switch (_libraryServiceData.LibraryInfo.ServiceType)
            {
                case ServiceType.Spoonacular:
                    _foodLibrary = new SpoonacularFoodLibrary(_libraryServiceData.LibraryDetails);
                    break;
                case ServiceType.Nutritionix:
                    _foodLibrary = new NutritionixFoodLibrary(_libraryServiceData.LibraryDetails);
                    break;
                case ServiceType.Edamam:
                    _foodLibrary = new EdamamFoodLibrary(_libraryServiceData.LibraryDetails);
                    break;
                case ServiceType.FatSecret:
                    _foodLibrary = new FatSecretFoodLibrary(_libraryServiceData.LibraryDetails);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Search food item data
        /// </summary>
        /// <param name="search">search string based on which food items are to be found</param>
        /// <param name="isBarcode">true if User sends barcode string as search parameter </param>
        /// <param name="imageRequired">true if image is required to be fetched</param>
        /// <param name="forApplication">for application</param>
        /// <returns>Food items matching the given search string</returns>
        public async Task<FoodItemDTO> SearchFoodItemAsync(string search, bool isBarcode, bool imageRequired, string forApplication)
        {
            FoodItemDTO foodItem = new FoodItemDTO();
            try
            {
                if (_libraryServiceData?.LibraryInfo != null && forApplication == _libraryServiceData.LibraryInfo.ForApplication)
                {
                    foodItem = await _foodLibrary.SearchFoodItemAsync(search, isBarcode, imageRequired).ConfigureAwait(false);
                    if (_libraryServiceData.LibraryInfo.LogCalls)
                    {
                        _ = SaveServiceCallLogsAsync(_libraryServiceData.LibraryInfo.OrganisationServiceID, foodItem.ErrCode.ToString(), foodItem.ErrorDescription).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                foodItem.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                foodItem.ErrorDescription = ex.Message;
                LogError(ex.Message, ex.StackTrace);
            }
            return foodItem;
        }

        /// <summary>
        /// Get food nutrients for given food
        /// </summary>
        /// <param name="foodIdentifier">Identifier of the selected food item</param>
        /// <param name="forApplication">for application</param>
        /// <returns>Food item nutrition info</returns>
        public async Task<HealthReadingDTO> GetFoodDataAsync(string foodIdentifier, string forApplication)
        {
            HealthReadingDTO healthReading = new HealthReadingDTO();
            try
            {
                if (_libraryServiceData?.LibraryInfo != null && forApplication == _libraryServiceData.LibraryInfo.ForApplication)
                {
                    healthReading = await _foodLibrary.GetFoodDataAsync(foodIdentifier).ConfigureAwait(false);
                    if (_libraryServiceData.LibraryInfo.LogCalls)
                    {
                        _ = SaveServiceCallLogsAsync(_libraryServiceData.LibraryInfo.OrganisationServiceID, healthReading.ErrCode.ToString(), healthReading.ErrorDescription).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                healthReading.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                healthReading.ErrorDescription = ex.Message;
                LogError(ex.Message, ex.StackTrace);
            }
            return healthReading;
        }
    }
}