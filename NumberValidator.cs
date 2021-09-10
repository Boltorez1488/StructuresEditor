using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StructuresEditor {
    public class NumberValidator : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string strValue = Convert.ToString(value);

            if (string.IsNullOrEmpty(strValue))
                return new ValidationResult(false, $"Value cannot be coverted to string.");

            if (Utils.IsMatchRegex(strValue, new Regex("(0x[A-Fa-f0-9]{0,8})|([A-Fa-f0-9]{1,8}h)|([0-9]+)")))
                return new ValidationResult(true, null);
            return new ValidationResult(false, "Input should be type of Int32");
        }
    }
}
