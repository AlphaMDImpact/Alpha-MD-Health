using System.Globalization;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// 
    /// </summary>
    public class BoolToIntegerConvertor : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool targetValue = (bool)value;
            if (parameter != null)
            {
                switch (parameter.ToString())
                {
                    case "RowSpan": return GetRowSpan(targetValue);
                    case "ColumnSpan": return GetColumnSpan(targetValue);
                    case "LeftCellHeader": return GetLeftCellHeader(targetValue);
                    case "TopCellHeader": return GetTopCellHeader(targetValue);
                    case "LeftCellDescription": return GetLeftCellDescription(targetValue);
                    case "TopCellDescription": return GetTopCellDescription(targetValue);
                    case "TopContentGrid": return GetTopContentGrid(targetValue);
                    case "LeftContentGrid": return GetLeftContentGrid(targetValue);
                    default: return default(bool);
                }
            }
            else
            {
                return default(bool);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private int GetLeftContentGrid(bool targetValue)
        {
            return targetValue ? 0 : 1;
        }

        private int GetTopContentGrid(bool targetValue)
        {
            return targetValue ? 1 : 0;
        }

        private int GetTopCellDescription(bool targetValue)
        {
            return targetValue ? 2 : 1;
        }

        private int GetLeftCellDescription(bool targetValue)
        {
            return targetValue ? 0 : 1;
        }

        private int GetTopCellHeader(bool targetValue)
        {
            return targetValue ? 1 : 0;
        }

        private int GetLeftCellHeader(bool targetValue)
        {
            return targetValue ? 0 : 1;
        }

        private int GetColumnSpan(bool targetValue)
        {
            return targetValue ? 2 : 1;
        }

        private int GetRowSpan(bool targetValue)
        {
            return targetValue ? 1 : 3;
        }
    }
}
