using System.Globalization;
using System.Windows.Data;
using Core;


namespace UIWPF
{
    /// <summary>
    /// Преобразует CellState в отображаемую строку.
    /// </summary>
    public class CellStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CellState state)
            {
                return state switch
                {
                    CellState.X => GameConstants.PlayerXSymbol,
                    CellState.O => GameConstants.PlayerOSymbol,
                    _ => GameConstants.EmptySymbol
                };
            }
            return GameConstants.EmptySymbol;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
