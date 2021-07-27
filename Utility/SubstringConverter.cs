using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Utility
{
    public class SubstringConverter : IValueConverter
    {
        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    string text = (string)value;
        //    return text.Substring(0, 5);
        //}
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object aa = null;
            try
            {
                string text = (string)value;
                if (!string.IsNullOrEmpty(text))
                {
                    aa = text.Substring(0, 5);
                }
                else
                {
                    aa = value;
                }
                return aa;

            }
            catch (Exception ex)
            {
                //LogFile.ErrorLog(ex);
                return aa;

            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
