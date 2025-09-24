using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Termino.Domain.Enums;

namespace Termino.App.Converters
{
    public class TermRowBrushConverter : IMultiValueConverter
    {
        private static readonly Brush Green = new SolidColorBrush( Color.FromRgb(210, 245, 210 )); // ≤ 7 дней

        private static readonly Brush Blue = new SolidColorBrush( Color.FromRgb(210, 230, 250 )); // 8–15 дней

        private static readonly Brush Gray = new SolidColorBrush( Color.FromRgb(230, 230, 230 )); // Completed

        private static readonly Brush Normal = Brushes.Transparent;                             // > 15

        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            if ( values is { Length: 2 } && values[0] 
                is DateTimeOffset due && values[1] 
                is TermStatus status )
            {
                if ( status == TermStatus.Completed )
                    return Gray;

                var days = ( due - DateTimeOffset.Now ).TotalDays;

                if ( days <= 7 ) 
                    return Green;

                if ( days <= 15 )
                    return Blue;

                return Normal;
            }
            return Normal;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
            => throw new NotSupportedException();
    }
}
