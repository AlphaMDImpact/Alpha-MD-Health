using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class EducationCategoryViewCell : Grid
{
    public EducationCategoryViewCell()
    {
        CustomLabelControl title = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft);
        title.SetBinding(CustomLabelControl.TextProperty, Constants.CNT_NAME_TEXT);
        CustomLabelControl seeMore = new CustomLabelControl(LabelType.PrimaryAppExtraSmallRight);
        seeMore.SetBinding(CustomLabelControl.TextProperty, nameof(EducationCategoryGroupModel.SubHeader));
        Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE];
        Padding = MobileConstants.IsTablet ? new Thickness(0, 20, 0, 10) : new Thickness(0, GenericMethods.GetPlatformSpecificValue(20, 10, 0), 0, 0);
        RowDefinitions = new RowDefinitionCollection
        {
                new RowDefinition { Height =  new GridLength(30, GridUnitType.Absolute)}
        };
        ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
        };
        this.Add(title, 0, 0);
        this.Add(seeMore, 1, 0);
        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += OnHeaderTap;
        GestureRecognizers.Add(tapGestureRecognizer);
    }

    private async void OnHeaderTap(object sender, EventArgs e)
    {
        var item = (sender as Grid).BindingContext as EducationCategoryGroupModel;
        await ShellMasterPage.CurrentShell.BaseContentPageInstance.PushPageByTargetAsync(Pages.EducationCategoriesPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id), "0", item[0].EducationCategoryID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
    }
}