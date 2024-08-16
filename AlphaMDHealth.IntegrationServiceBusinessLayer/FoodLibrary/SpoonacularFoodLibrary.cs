using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    internal class SpoonacularFoodLibrary : BaseService, IFoodLibrary
    {
        private readonly List<LibraryServiceModel> _libraryServiceDetails;

        public SpoonacularFoodLibrary(List<LibraryServiceModel> libraryServiceDetails)
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
            Uri requestUri = new Uri(string.Format(CultureInfo.InvariantCulture, libraryServiceData.ServiceTarget, search, libraryServiceData.ServiceClientSecrete));
            await new HttpService().SendHttpRequestAsync(foodItemData, HttpMethod.Get, requestUri, string.Empty, null).ConfigureAwait(false);
            if (foodItemData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(foodItemData.Response);
                if (data?.Count() > 0)
                {
                    libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == ServiceIdentifierFor.FoodImage.ToString());
                    MapFoodItems(data, foodItemData, libraryServiceData.ServiceTarget, isBarcode);
                    if (imageRequired)
                    {
                        await UpdateImageBase64(foodItemData).ConfigureAwait(false);
                    }
                }
            }
            return foodItemData;
        }

        /// <summary>
        /// Get food nutrients for given food
        /// </summary>
        /// <param name="foodIdentifier">Identifier of the selected food item</param>
        /// <returns>Food item nutrition info</returns>
        public async Task<HealthReadingDTO> GetFoodDataAsync(string foodIdentifier)
        {
            HealthReadingDTO healthReadingData = new HealthReadingDTO();
            LibraryServiceModel libraryServiceData = _libraryServiceDetails.Find(x => x.IdentifierFor == ServiceIdentifierFor.FoodNutrition.ToString());
            Uri requestUri = new Uri(string.Format(CultureInfo.InvariantCulture, libraryServiceData.ServiceTarget, foodIdentifier, libraryServiceData.ServiceClientSecrete));
            await new HttpService().SendHttpRequestAsync(healthReadingData, HttpMethod.Get, requestUri, string.Empty, null).ConfigureAwait(false);
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

        private void MapFoodItems(JToken data, FoodItemDTO foodItemData, string imagePath, bool isBarcode)
        {
            if (isBarcode)
            {
                FoodItemModel foodItemModel = GetFoodItemData(data, imagePath, isBarcode);
                foodItemData.FoodItems.Add(foodItemModel);
            }
            else
            {
                foodItemData.FoodItems = (from dataItem in data[FoodConstants.RESULTS_TEXT_KEY]
                                          select GetFoodItemData(dataItem, imagePath, isBarcode)).ToList();
            }
        }

        private FoodItemModel GetFoodItemData(JToken data, string imagePath, bool isBarcode)
        {
            return new FoodItemModel
            {
                FoodIdentifier = (string)data[FoodConstants.SPOONACULAR_ID],
                FoodTitle = (string)data[FoodConstants.SPOONACULAR_TITLE],
                Image = imagePath + (isBarcode ? data[FoodConstants.SPOONACULAR_IMAGES][0] : data[FoodConstants.SPOONACULAR_IMAGE])
            };
        }

        private void MapFoodNutrientItems(JToken data, HealthReadingDTO healthReadingData)
        {
            healthReadingData.HealthReadings = new List<HealthReadingModel>();
            JArray bad = (JArray)data.SelectToken(FoodConstants.SPOONACULAR_BAD);
            JArray good = (JArray)data.SelectToken(FoodConstants.SPOONACULAR_GOOD);
            bad.Merge(good);
            foreach (JToken item in bad)
            {
                if (ReadingType.None != Mapping.ReadingTypeBinding(item[FoodConstants.SPOONACULAR_TITLE].ToString()))
                {
                    double readingValue = Convert.ToDouble(Regex.Match(item[FoodConstants.SPOONACULAR_AMOUNT].ToString(), @"\d+").Value, CultureInfo.InvariantCulture);
                    string readingUnit = new string(item[FoodConstants.SPOONACULAR_AMOUNT].ToString().Where(char.IsLetter).ToArray());
                    healthReadingData.HealthReadings.Add(new HealthReadingModel
                    {
                        ReadingType = Mapping.ReadingTypeBinding(item[FoodConstants.SPOONACULAR_TITLE].ToString()),
                        ReadingValue = Mapping.GetReadingValueAsPerUnit(readingValue, readingUnit),
                        ReadingUnit = Mapping.ReadingUnitBinding(readingUnit),
                    });
                }
            }
        }
    }
}