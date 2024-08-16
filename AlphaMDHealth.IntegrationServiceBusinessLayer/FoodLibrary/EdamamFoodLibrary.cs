using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class EdamamFoodLibrary : BaseService, IFoodLibrary
    {
        private readonly List<LibraryServiceModel> _libraryServiceDetails;

        public EdamamFoodLibrary(List<LibraryServiceModel> libraryServiceDetails)
        {
            _libraryServiceDetails = libraryServiceDetails;
        }

        /// <summary>
        /// Search food item data
        /// </summary>
        /// <param name="search">search string based on which food items are to be found</param>
        /// <param name="isBarcode">true if User sends barcode string as search parameter </param>
        /// <param name="imageRequired">true if image is required to be fetched</param>
        /// <returns>Food items matching the given search string</returns>
        public async Task<FoodItemDTO> SearchFoodItemAsync(string search, bool isBarcode, bool imageRequired)
        {
            FoodItemDTO foodItemData = new FoodItemDTO
            {
                FoodItems = new List<FoodItemModel>()
            };
            LibraryServiceModel libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == (isBarcode ? ServiceIdentifierFor.FoodBarcode : ServiceIdentifierFor.FoodSearch).ToString());
            Uri requestUri = new Uri(string.Format(CultureInfo.InvariantCulture, libraryServiceData.ServiceTarget, libraryServiceData.ServiceClientIdentifier, libraryServiceData.ServiceClientSecrete, search));
            await new HttpService().SendHttpRequestAsync(foodItemData, HttpMethod.Get, requestUri, string.Empty, null).ConfigureAwait(false);
            if (foodItemData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(foodItemData.Response);
                if (data?.Count() > 0)
                {
                    MapFoodItems(data, foodItemData, isBarcode);
                    if (imageRequired)
                    {
                        await UpdateImageBase64(foodItemData).ConfigureAwait(false);
                    }
                }
            }
            return foodItemData;
        }

        /// <summary>
        /// Get food nutrients data
        /// </summary>
        /// <param name="foodIdentifier">food identifier</param>
        /// <returns>Operation status and food nutrients data</returns>
        public async Task<HealthReadingDTO> GetFoodDataAsync(string foodIdentifier)
        {
            bool isBarcode;
            HealthReadingDTO healthReadingData = new HealthReadingDTO();
            Uri requestUri;
            LibraryServiceModel libraryServiceData;
            if (Convert.ToBoolean(foodIdentifier.Split(Constants.SYMBOL_HASH)[0], CultureInfo.InvariantCulture))
            {
                libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == ServiceIdentifierFor.FoodNutrition.ToString());
                requestUri = new Uri(string.Format(CultureInfo.InvariantCulture, libraryServiceData.ServiceTarget, libraryServiceData.ServiceClientIdentifier,
                    libraryServiceData.ServiceClientSecrete, foodIdentifier.Split(Constants.SYMBOL_HASH)[1]));
                isBarcode = true;
            }
            else
            {
                libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == ServiceIdentifierFor.FoodSearch.ToString());
                requestUri = new Uri(string.Format(CultureInfo.InvariantCulture, libraryServiceData.ServiceTarget + FoodConstants.EDAMAM_SEARCH_TO_NUTRITION_END_POINT,
                    libraryServiceData.ServiceClientIdentifier, libraryServiceData.ServiceClientSecrete, foodIdentifier.Split(Constants.SYMBOL_HASH)[1]));
                isBarcode = false;
            }
            await new HttpService().SendHttpRequestAsync(healthReadingData, HttpMethod.Get, requestUri, string.Empty, null).ConfigureAwait(false);
            if (healthReadingData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(healthReadingData.Response);
                if (data?.Count() > 0)
                {
                    MapFoodNutrientItems(data, healthReadingData, isBarcode);
                }
            }
            return healthReadingData;
        }

        private void MapFoodItems(JToken data, FoodItemDTO foodItemData, bool isBarcode)
        {
            foodItemData.FoodItems = (from dataitem in isBarcode ? data[FoodConstants.HINTS_TEXT_KEY] : data[FoodConstants.HITS_TEXT_KEY]
                                      select GetFoodItem(dataitem, isBarcode)).ToList();
        }

        private FoodItemModel GetFoodItem(JToken dataitem, bool isBarcode)
        {
            dataitem = isBarcode ? dataitem[FoodConstants.FOOD_TEXT_KEY] : dataitem[FoodConstants.RECIPE_TEXT_KEY];
            return new FoodItemModel
            {
                FoodIdentifier = $"{isBarcode}#" + (string)dataitem[FoodConstants.LABEL_TEXT_KEY],
                FoodTitle = (string)dataitem[FoodConstants.LABEL_TEXT_KEY],
                Image = (string)dataitem[FoodConstants.IMAGE_TEXT_KEY]
            };
        }

        private void MapFoodNutrientItems(JToken data, HealthReadingDTO healthReadingData, bool isBarcode)
        {
            healthReadingData.HealthReadings = new List<HealthReadingModel>();
            if (isBarcode)
            {
                if (data[FoodConstants.TOTALNUTRIENTS_TEXT_KEY].HasValues)
                {
                    GetNutritionData(healthReadingData, (JObject)data[FoodConstants.TOTALNUTRIENTS_TEXT_KEY]);
                }
            }
            else
            {
                if (data[FoodConstants.HITS_TEXT_KEY][0].HasValues)
                {
                    GetNutritionData(healthReadingData, (JObject)data[FoodConstants.HITS_TEXT_KEY][0][FoodConstants.RECIPE_TEXT_KEY][FoodConstants.TOTALNUTRIENTS_TEXT_KEY]);
                }
            }
        }

        private void GetNutritionData(HealthReadingDTO healthReadingData, JObject totalNutrients)
        {
            foreach (var nutrients in totalNutrients.Properties())
            {
                var item = totalNutrients[nutrients.Name];
                if (ReadingType.None != Mapping.ReadingTypeBinding(item[FoodConstants.LABEL_TEXT_KEY].ToString()))
                {
                    healthReadingData.HealthReadings.Add(GetNutrientsItem(item));
                }
            }
        }

        private HealthReadingModel GetNutrientsItem(JToken item)
        {
            return new HealthReadingModel
            {
                ReadingType = Mapping.ReadingTypeBinding(item[FoodConstants.LABEL_TEXT_KEY].ToString()),
                ReadingValue = Mapping.GetReadingValueAsPerUnit(Convert.ToDouble(item[FoodConstants.QUANTITY_TEXT_KEY]), item[FoodConstants.UNIT_TEXT_KEY].ToString()),
                ReadingUnit = Mapping.ReadingUnitBinding(item[FoodConstants.UNIT_TEXT_KEY].ToString()),
            };
        }
    }
}