using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StructuresEditor {
    class PathValidator : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string strValue = Convert.ToString(value);

            if (string.IsNullOrEmpty(strValue))
                return new ValidationResult(true, null);

            if(strValue.StartsWith(".") || strValue.StartsWith(":"))
                return new ValidationResult(false, "Path is bad");

            if (Utils.IsMatchRegex(strValue, new Regex(@"((\.{0,1}|:{0,2})[A-z]{1}[A-z0-9]{0,})+")))
                return new ValidationResult(true, null);
            return new ValidationResult(false, "Path is bad");
        }
    }
}
