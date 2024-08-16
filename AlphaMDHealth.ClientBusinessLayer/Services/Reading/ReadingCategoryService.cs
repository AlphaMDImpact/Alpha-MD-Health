using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading categories implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    private void MapCategoriesFilterOptions(PatientReadingDTO readingData, JToken data)
    {
        readingData.FilterOptions = MapCategoryRelations(data, nameof(PatientReadingDTO.FilterOptions));
        MapCategoryOptions(readingData);
    }

    private void MapCategoryOptions(PatientReadingDTO readingsData)
    {
        var relations = readingsData.FilterOptions;
        var categories = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_READING_CATEGORY_GROUP, string.Empty, false, readingsData.ReadingCategoryID);
        if (GenericMethods.IsListNotEmpty(categories) && GenericMethods.IsListNotEmpty(relations))
        {
            readingsData.FilterOptions = (from category in categories
                                          where relations.Any(x => x.OptionID == category.OptionID)
                                          select category).ToList();
            if (GenericMethods.IsListNotEmpty(readingsData.FilterOptions)) //&& readingsData.ReadingCategoryID < 1)
            {
                MapCategories(readingsData);
            }
        }
    }

    private void MapCategories(PatientReadingDTO readingsData)
    {
        if (GenericMethods.IsListNotEmpty(readingsData.FilterOptions))
        {
            if (readingsData.ReadingCategoryID < 1 || !readingsData.FilterOptions.Any(x => x.OptionID == Convert.ToDouble(readingsData.ReadingCategoryID)))
            {
                readingsData.ReadingCategoryID = Convert.ToInt16(readingsData.FilterOptions.FirstOrDefault().OptionID);
            }
            readingsData.FilterOptions.ForEach(selectedCategory =>
            {
                selectedCategory.IsSelected = selectedCategory.OptionID == readingsData.ReadingCategoryID;
            });
        }
    }

    private List<OptionModel> MapCategoryRelations(JToken data, string tokenValue)
    {
        return data[tokenValue].Any()
               ? (from dataItem in data[tokenValue]
                  select new OptionModel
                  {
                      OptionID = (long)dataItem[nameof(OptionModel.OptionID)]
                  }).ToList()
               : new List<OptionModel>();
    }
}
