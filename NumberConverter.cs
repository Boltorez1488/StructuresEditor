using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StructuresEditor {
    public class NumberConverter : IValueConverter {
        // To HexString
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return "0x0";
            var intVal = (int)value;
            return "0x" + intVal.ToString("X");
        }

        // To Int
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var strVal = value?.ToString();
            if (strVal == null)
                return 0;

            if (Utils.IsMatchRegex(strVal, new Regex("0x[A-Fa-f0-9]{0,8}"))) {
                var toParse = strVal.Substring(2);
                return int.Parse(toParse, NumberStyles.HexNumber);
            }

            if (Utils.IsMatchRegex(strVal, new Regex("[A-Fa-f0-9]{1,8}h"))) {
                var toParse = strVal.TrimEnd('h');
                return int.Parse(toParse, NumberStyles.HexNumber);
            }

            if (Utils.IsMatchRegex(strVal, new Regex("[0-9]+"))) {
                return System.Convert.ToInt32(strVal);
            }

            return 0x0;
        }
    }
}
