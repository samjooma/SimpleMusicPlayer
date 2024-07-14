using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace SimpleMusicPlayer.ValueConverters
{
    public class StaticMarkup<T> : MarkupExtension where T : new()
    {
        private static readonly T _instance = new T();
        public override object ProvideValue(IServiceProvider ServiceProvider) => _instance!;
    }

    public class ObjectIntoArray : StaticMarkup<ObjectIntoArray>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FileNameWithoutExt : StaticMarkup<FileNameWithoutExt>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetFileNameWithoutExtension((value as string) ?? "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanConverter : MarkupExtension, IValueConverter
    {
        public string FalseValue { get; set; } = "False";
        public string TrueValue { get; set; } = "True";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return FalseValue!;
            return (bool)value ? TrueValue! : FalseValue!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            return value.Equals(TrueValue);
        }

        public override object ProvideValue(IServiceProvider ServiceProvider)
        {
            BooleanConverter Result = new();
            Result.FalseValue = FalseValue;
            Result.TrueValue = TrueValue;
            return Result;
        }
    }

    public class TimeSpanToSeconds : StaticMarkup<TimeSpanToSeconds>, IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            if (Value is TimeSpan Time) return Time.TotalSeconds;
            return 0;
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            if (Value is double Seconds) return TimeSpan.FromSeconds(Seconds);
            return TimeSpan.Zero;
        }
    }

    public class MultiplyValue : StaticMarkup<MultiplyValue>, IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            if (Value is not double a)
            {
                throw new FormatException("The type of the value being converted must be double");
            }
            var b = double.Parse((string)Parameter);
            return a * b;
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            if (Value is not double a)
            {
                throw new FormatException("The type of the value being converted must be double");
            }
            var b = double.Parse((string)Parameter);
            return a / b;
        }
    }
}
