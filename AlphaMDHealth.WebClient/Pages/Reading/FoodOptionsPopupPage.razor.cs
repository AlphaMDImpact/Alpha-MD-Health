using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;
public partial class FoodOptionsPopupPage : BasePage
{
    /// <summary>
    /// Program ID
    /// </summary>
    [Parameter]
    public PatientReadingDTO ReadingData { get; set; }

    /// <summary>
    /// Callback Event to execute code after Edit page is closed
    /// </summary>
    [Parameter]
    public EventCallback<PatientReadingDTO> PopUpClosed { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isDataFetched = true;
        await Task.Run(() => { }).ConfigureAwait(true);
    }

    private async Task OnOptionSelectedAsync(OptionModel optionModel)
    {
        Success = Error = string.Empty;
        ReadingData.AddedBy = optionModel.GroupName;
        ReadingData.Title = ReadingData.SummaryDataOptions.FirstOrDefault(x => x.GroupName == optionModel.GroupName).OptionText;
        await PopUpClosed.InvokeAsync(ReadingData);
    }

    private void OnCloseIconClicked(bool isPopupShowing)
    {
        if (!isPopupShowing)
        {
            OnCancelClick();
        }
    }

    private void OnCancelClick()
    {
        PopUpClosed.InvokeAsync(null);
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
           new TableDataStructureModel{DataField= nameof(OptionModel.GroupName), IsKey= true, IsSearchable= false, IsHidden= true, IsSortable= false},
           new TableDataStructureModel{ImageSrc= nameof(OptionModel.ParentOptionText) , HasImage=true, ImageFieldType=FieldTypes.SquareWithBackgroundImageControl, ImageHeight=AppImageSize.ImageSizeL, ImageWidth=AppImageSize.ImageSizeL},
           new TableDataStructureModel{DataField= nameof(OptionModel.OptionText)},
        };
    }
}
