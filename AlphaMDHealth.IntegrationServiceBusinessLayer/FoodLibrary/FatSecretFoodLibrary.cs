using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Web;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    internal class FatSecretFoodLibrary : BaseService, IFoodLibrary
    {
        private readonly List<LibraryServiceModel> _libraryServiceDetails;

        public FatSecretFoodLibrary(List<LibraryServiceModel> libraryServiceDetails)
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
            LibraryServiceModel libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == ServiceIdentifierFor.Authentication.ToString());
            Tuple<string, string> tokenSecret = new FatSecretApiAuthentication(libraryServiceData.ServiceClientIdentifier, libraryServiceData.ServiceClientSecrete, libraryServiceData.ServiceTarget).CreateTokenAndSecretKey();
            string urlBase;
            if (isBarcode)
            {
                foodItemData.ErrCode = ErrorCode.InvalidData;
                return foodItemData;
            }
            else
            {
                libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == ServiceIdentifierFor.FoodSearch.ToString());
                urlBase = string.Format(CultureInfo.InvariantCulture, libraryServiceData.ServiceTarget, search);
            }
            var signatureData = new FatSecretOAuthBase().GenerateSignature(new Uri(urlBase), libraryServiceData.ServiceClientIdentifier, libraryServiceData.ServiceClientSecrete, tokenSecret.Item1, tokenSecret.Item2);
            await new HttpService().SendHttpRequestAsync(foodItemData, HttpMethod.Post,
                new Uri(signatureData.Item1 + Constants.SYMBOL_QUESTION_MARK + signatureData.Item2 + Constants.SYMBOL_AMPERSAND + FoodConstants.OAUTH_SIGNATURE + Constants.SYMBOL_EQUAL + HttpUtility.UrlEncode(signatureData.Item3)),
                string.Empty, null).ConfigureAwait(false);
            if (foodItemData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(foodItemData.Response);
                MapFoodItems(data, foodItemData);
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
            HealthReadingDTO healthItemData = new HealthReadingDTO { HealthReadings = new List<HealthReadingModel>() };
            LibraryServiceModel libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == ServiceIdentifierFor.Authentication.ToString());
            Tuple<string, string> tokenSecret = new FatSecretApiAuthentication(libraryServiceData.ServiceClientIdentifier, libraryServiceData.ServiceClientSecrete, libraryServiceData.ServiceTarget).CreateTokenAndSecretKey();
            libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == ServiceIdentifierFor.FoodNutrition.ToString());
            var signatureData = new FatSecretOAuthBase().GenerateSignature(new Uri(string.Format(CultureInfo.InvariantCulture, libraryServiceData.ServiceTarget, foodIdentifier)), libraryServiceData.ServiceClientIdentifier,
                               libraryServiceData.ServiceClientSecrete, tokenSecret.Item1, tokenSecret.Item2);
            await new HttpService().SendHttpRequestAsync(healthItemData, HttpMethod.Post,
                new Uri(signatureData.Item1 + Constants.SYMBOL_QUESTION_MARK + signatureData.Item2 + Constants.SYMBOL_AMPERSAND + FoodConstants.OAUTH_SIGNATURE + Constants.SYMBOL_EQUAL + HttpUtility.UrlEncode(signatureData.Item3)),
                string.Empty, null).ConfigureAwait(false);
            if (healthItemData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(healthItemData.Response);
                MapFoodNutrientItems(data, healthItemData);
            }
            return healthItemData;
        }

        private void MapFoodItems(JToken data, FoodItemDTO foodItemData)
        {
            foodItemData.FoodItems = (from dataitem in data[FoodConstants.FOODS_TEXT_KEY][FoodConstants.FOOD_TEXT_KEY]
                                      select new FoodItemModel
                                      {
                                          FoodIdentifier = (string)dataitem[FoodConstants.FATSECRET_FOOD_ID],
                                          FoodTitle = (string)dataitem[FoodConstants.FATSECRET_FOOD_NAME]
                                      }).ToList();
        }

        private void MapFoodNutrientItems(JToken data, HealthReadingDTO healthItemData)
        {
            healthItemData.HealthReadings = new List<HealthReadingModel>();
            JToken serving = data[FoodConstants.FOOD_TEXT_KEY][FoodConstants.FATSECRET_SERVINGS_TEXT_KEY][FoodConstants.FATSECRET_SERVING_TEXT_KEY];
            JObject j = (JObject)(serving is JArray ? serving[0] : serving);
            foreach (var item in j.Properties())
            {
                if (ReadingType.None != Mapping.ReadingTypeBinding(item.Name) && Convert.ToDouble(item.Value) > 0)
                {
                    HealthReadingModel readingModel = new HealthReadingModel
                    {
                        ReadingType = Mapping.ReadingTypeBinding(item.Name),
                        ReadingValue = Mapping.GetReadingValueAsPerUnit(Convert.ToDouble(item.Value), Mapping.MappUnitForReadingType(item.Name)),
                        ReadingUnit = Mapping.ReadingUnitBinding(Mapping.MappUnitForReadingType(item.Name)),
                    };
                    healthItemData.HealthReadings.Add(readingModel);
                }
            }
        }
    }
}