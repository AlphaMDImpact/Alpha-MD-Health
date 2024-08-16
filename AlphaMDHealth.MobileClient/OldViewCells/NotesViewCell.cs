using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Note view cell
/// </summary>
public class NotesViewCell : ViewCell
{
    private readonly PatientProviderNotesView _page;

    /// <summary>
    /// Default constructor
    /// </summary>
    public NotesViewCell(PatientProviderNotesView page)
    {
        _page = page;
        HtmlWebViewSource instructionWebViewSource = new HtmlWebViewSource();
        Label label = new Label();
        CustomWebView CostumWebView = new CustomWebView
        {
            IsAutoIncreaseHeight = true,
            ShowBusyIndicator = false,
            Source = instructionWebViewSource,
        };
        CostumWebView.WebviewLoadedCompleted += CostumWebView_WebviewLoadedCompleted;
        Grid mainGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowSpacing = 10,
            VerticalOptions = LayoutOptions.FillAndExpand,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
            }
        };
        CostumWebView.SetBinding(CustomWebView.ClassIdProperty, nameof(OptionModel.ParentOptionText));
        this.SetBinding(ViewCell.ClassIdProperty, nameof(OptionModel.ParentOptionText));
        instructionWebViewSource.SetBinding(HtmlWebViewSource.HtmlProperty, nameof(OptionModel.OptionText));
        mainGrid.Add(CostumWebView, 0, 0);
        mainGrid.Add(label, 0, 0);
        if (!_page._isPatientLogin && !_page._isDashboard)
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            label.GestureRecognizers.Add(tapGestureRecognizer);
        }
        else
        {
            CostumWebView.IsEnabled = false;
        }
        View = mainGrid;
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
       var item = (sender as Label).BindingContext as OptionModel;
       _page.OnNoteClicked(item);
    }

    private void CostumWebView_WebviewLoadedCompleted(object sender, EventArgs e)
    {
        var webviewObject = sender as CustomWebView;
        var hieght = webviewObject.HeightRequest;
        var parent = (webviewObject.Parent).Parent;
        if (parent is ViewCell cell)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if(cell.ClassId == (webviewObject).ClassId)
                {
                   if (cell.Height == hieght)
                   {
                       return;
                   }
                    cell.Height = hieght;

                    if (_page.GetCellHieght.ContainsKey(cell.ClassId))
                    {
                        _page.GetCellHieght[cell.ClassId] = hieght;
                    }
                    else
                    {
                        _page.GetCellHieght.Add(cell.ClassId, hieght);
                    }
                    cell.ForceUpdateSize();
                    _page.NotesViewCell_ListCompleted();
                }
               
            });
        }
    }
}