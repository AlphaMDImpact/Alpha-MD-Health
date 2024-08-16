using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading food/nutrition implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    /// <summary>
    /// Search food data async
    /// </summary>
    /// <param name="foodData">search item text</param>
    /// <returns>List of searched food items</returns>
    public async Task SearchFoodAsync(PatientReadingDTO foodData)
    {
        await SyncSearchedFoodAsync(foodData).ConfigureAwait(false);
        await ConvertReadingUnitsAsync(foodData);
    }

    /// <summary>
    /// Get food nutrition data
    /// </summary>
    /// <param name="foodData">object to get nutrition data</param>
    /// <returns>Food nutrition data</returns>
    public async Task GetFoodNutritionDataAsync(PatientReadingDTO foodData)
    {
        await SyncFoodNutritionDataAsync(foodData).ConfigureAwait(false);
        await ConvertReadingUnitsAsync(foodData);
    }

    /// <summary>
    /// Search food data async
    /// </summary>
    /// <param name="foodData">search item text</param>
    /// <returns>List of searched food items</returns>
    public async Task SyncSearchedFoodAsync(PatientReadingDTO foodData)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                PathWithoutBasePath = UrlConstants.GET_SEARCH_FOOD_ITEM_ASYNC,
                QueryParameters = new NameValueCollection { { Constants.SE_SEARCH_TEXT_QUERY_KEY, foodData.ErrorDescription } }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            foodData.ErrCode = httpData.ErrCode;
            if (foodData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    //// Map Available food options for given text
                    foodData.SummaryDataOptions = (data[nameof(PatientReadingDTO.SummaryDataOptions)]?.Count() > 0)
                        ? (from dataItem in data[nameof(PatientReadingDTO.SummaryDataOptions)]
                           select new OptionModel
                           {
                               GroupName = GetDataItem<string>(dataItem, nameof(OptionModel.GroupName)),
                               OptionText = GetDataItem<string>(dataItem, nameof(OptionModel.OptionText)),
                               ParentOptionText = GetDataItem<string>(dataItem, nameof(OptionModel.ParentOptionText)),
                           }).ToList() : null;
                    if (GenericMethods.IsListNotEmpty(foodData.SummaryDataOptions) && foodData.SummaryDataOptions.Count == 1)
                    {
                        FoodNutritionData(foodData, data);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            foodData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    /// <summary>
    /// Get food nutrition data
    /// </summary>
    /// <param name="foodData">object to get nutrition data</param>
    /// <returns>Food nutrition data</returns>
    public async Task SyncFoodNutritionDataAsync(PatientReadingDTO foodData)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                PathWithoutBasePath = UrlConstants.GET_FOOD_NUTRITION_DATA_ASYNC,
                QueryParameters = new NameValueCollection { { Constants.SE_GET_FOOD_NUTRITIONS_QUERY_KEY, foodData.AddedBy } }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            foodData.ErrCode = httpData.ErrCode;
            if (foodData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    FoodNutritionData(foodData, data);
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            foodData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

}
