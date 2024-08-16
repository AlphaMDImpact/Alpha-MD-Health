using AlphaMDHealth.IntegrationServiceBusinessLayer;
using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.IntegrationServiceLayer.Controllers
{
    /// <summary>
    /// Controller for food library service
    /// </summary>
    [Route("api/FoodLibraryService")]
    [ApiController]
    public class FoodLibraryServiceController : ControllerBase
    {
        /// <summary>
        /// Search food items
        /// </summary>
        /// <param name="search">search string based on which food items are to be found</param>
        /// <param name="isBarcode">true if User sends barcode string as search parameter </param>
        /// <param name="imageRequired">true if image is required to be fetched</param>
        /// <param name="forApplication">for application</param>
        /// <returns>Food items matching the given search string</returns>
        [Route("SearchFoodItemAsync")]
        [HttpGet]
        public async Task<FoodItemDTO> SearchFoodItemAsync(string search, bool isBarcode, bool imageRequired, string forApplication)
        {
            return await new FoodLibraryService(HttpContext).SearchFoodItemAsync(search, isBarcode, imageRequired, forApplication).ConfigureAwait(false);
        }

        /// <summary>
        /// Get food nutrients for given food
        /// </summary>
        /// <param name="foodIdentifier">Identifier of the selected food item</param>
        /// <param name="forApplication">for application</param>
        /// <returns>Food item nutrition info</returns>
        [Route("GetFoodDataAsync")]
        [HttpGet]
        public async Task<HealthReadingDTO> GetFoodDataAsync(string foodIdentifier, string forApplication)
        {
            return await new FoodLibraryService(HttpContext).GetFoodDataAsync(foodIdentifier, forApplication).ConfigureAwait(false);
        }
    }
}
