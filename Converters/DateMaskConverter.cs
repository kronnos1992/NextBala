using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace NextBala.Converters
{
    public class DateMaskConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return string.Empty;

                string texto = value.ToString();

                // Remove tudo que não é número
                string numeros = new string(texto.Where(char.IsDigit).ToArray());

                if (numeros.Length > 8)
                    numeros = numeros.Substring(0, 8);

                if (numeros.Length >= 5)
                    return numeros.Insert(4, "/").Insert(2, "/");

                if (numeros.Length >= 3)
                    return numeros.Insert(2, "/");

                return numeros;
            }
            catch
            {
                // Nunca deixa o WPF quebrar o pipeline de input
                return value?.ToString() ?? string.Empty;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new DateMaskConverter();
        }
    }
}