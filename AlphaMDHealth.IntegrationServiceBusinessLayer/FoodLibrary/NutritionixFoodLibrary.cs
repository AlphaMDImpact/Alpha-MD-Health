using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class NutritionixFoodLibrary : BaseService, IFoodLibrary
    {
        private readonly List<LibraryServiceModel> _libraryServiceDetails;

        /// <summary>
        /// Nutritionx food library
        /// </summary>
        /// <param name="libraryServiceDetails">library service details client id, client secret and url</param>
        public NutritionixFoodLibrary(List<LibraryServiceModel> libraryServiceDetails)
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
            Uri requestUri = new Uri(string.Format(CultureInfo.InvariantCulture, libraryServiceData.ServiceTarget, search));
            await new HttpService().SendHttpRequestAsync(foodItemData, HttpMethod.Get, requestUri, string.Empty, GetHeaders(libraryServiceData.ServiceClientIdentifier, libraryServiceData.ServiceClientSecrete)).ConfigureAwait(false);
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
            HealthReadingDTO healthReadingData = new HealthReadingDTO();
            LibraryServiceModel libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == ServiceIdentifierFor.FoodNutrition.ToString());
            Uri requestUri = new Uri(libraryServiceData.ServiceTarget);
            await new HttpService().SendHttpRequestAsync(healthReadingData, HttpMethod.Post, requestUri, FoodDataToJsonAsync(foodIdentifier), GetHeaders(libraryServiceData.ServiceClientIdentifier, libraryServiceData.ServiceClientSecrete)).ConfigureAwait(false);
            if (healthReadingData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(healthReadingData.Response);
                if (data?.Count() > 0)
                {
                    MapFoodNutrientItems(data, healthReadingData);
                }
            }
            return healthReadingData;
        }

        private Dictionary<string, string> GetHeaders(string clientID, string clientSecret)
        {
            return new Dictionary<string, string>
            {
                { FoodConstants.NUTRITIONIX_X_APP_ID_TEXT, clientID },
                { FoodConstants.NUTRITIONIX_X_APP_KEY_TEXT, clientSecret }
            };
        }

        private void MapFoodItems(JToken data, FoodItemDTO foodItemData, bool isBarcode)
        {
            if (isBarcode)
            {
                foodItemData.FoodItems = (from dataitem in data[FoodConstants.FOODS_TEXT_KEY]
                                          select GetFoodItemData(dataitem)).ToList();
            }
            else
            {
                foodItemData.FoodItems = new List<FoodItemModel>();
                JArray foodItems = (JArray)data.SelectToken(FoodConstants.NIX_COMMON_TEXT_KEY);
                foodItems.Merge((JArray)data.SelectToken(FoodConstants.NIX_BRANDED_TEXT_KEY));
                foreach (JToken item in foodItems)
                {
                    foodItemData.FoodItems.Add(GetFoodItemData(item));
                }
            }
        }

        private FoodItemModel GetFoodItemData(JToken item)
        {
            return new FoodItemModel
            {
                FoodIdentifier = (string)item[FoodConstants.NIX_FOOD_NAME_TEXT_KEY],
                FoodTitle = (string)item[FoodConstants.NIX_FOOD_NAME_TEXT_KEY],
                Image = (string)item[FoodConstants.PHOTO_TEXT_KEY][FoodConstants.THUMB_TEXT_KEY]
            };
        }

        private string FoodDataToJsonAsync(string foodIdentifier)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName(FoodConstants.NUTRITIONIX_QUERY_PARAM_TEXT);
            writer.WriteValue(foodIdentifier);
            writer.WriteEndObject();
            return sw.ToString();
        }

        private void MapFoodNutrientItems(JToken data, HealthReadingDTO healthItemData)
        {
            ///map reading type depending on attribute id received from api
            ///use google sheet https://docs.google.com/spreadsheets/d/14ssR3_vFYrVAidDLJoio07guZM80SMR5nxdGpAX-1-A/edit#gid=0 for ref
            ///provided by Nutritionix Food

            healthItemData.HealthReadings = new List<HealthReadingModel>();
            JToken totalNutrients = data[FoodConstants.FOODS_TEXT_KEY][0][FoodConstants.FULL_NUTRIENTS_TEXT_KEY];
            foreach (var item in totalNutrients)
            {
                if (ReadingType.None != Mapping.ReadingTypeBinding(item[FoodConstants.ATTR_ID_TEXT_KEY].ToString()) && Convert.ToDouble(item[FoodConstants.VALUE_TEXT_KEY], CultureInfo.InvariantCulture) > 0)
                {
                    string readingUnit = Mapping.MappUnitForReadingType(item[FoodConstants.ATTR_ID_TEXT_KEY].ToString());
                    healthItemData.HealthReadings.Add(new HealthReadingModel
                    {
                        ReadingType = Mapping.ReadingTypeBinding(item[FoodConstants.ATTR_ID_TEXT_KEY].ToString()),
                        ReadingValue = Mapping.GetReadingValueAsPerUnit(Convert.ToDouble(item[FoodConstants.VALUE_TEXT_KEY], CultureInfo.InvariantCulture), readingUnit),
                        ReadingUnit = Mapping.ReadingUnitBinding(readingUnit)
                    });
                }
            }
        }
    }
}