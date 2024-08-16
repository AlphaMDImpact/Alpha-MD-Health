using AlphaMDHealth.Utility;
using DevExpress.Maui.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateChartStyle()
    {
        CreateDevExpressChartStyle();
    }

    private void CreateDevExpressChartStyle()
    {
        Style lineSeriesStyle = new Style(typeof(LineSeriesStyle))
        {
            Setters =
            {
                new Setter{Property = LineSeriesStyle.StrokeThicknessProperty , Value = 2 },
                new Setter{Property = LineSeriesStyle.MarkerSizeProperty , Value = 8},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LINE_SERIES_STYLE, lineSeriesStyle);
        Style barSeriesStyle = new Style(typeof(BarSeriesStyle))
        {
            Setters =
            {
                new Setter{Property = BarSeriesStyle.StrokeThicknessProperty , Value = 2 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_BAR_SERIES_STYLE, barSeriesStyle);

        Style chartViewStyle = new Style(typeof(AxisStyle))
        {
            Setters =
            {
                new Setter{Property = AxisStyle.LineThicknessProperty , Value = 1 },
                new Setter{Property = AxisStyle.LineColorProperty , Value = Colors.LightGray},
                //new Setter{Property = Legend.OrientationProperty , Value = LegendOrientation.LeftToRight},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_CHART_VIEW_STYLE, chartViewStyle);
    }
}


