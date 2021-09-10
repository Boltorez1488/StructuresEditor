using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StructuresEditor {
    class VarValidator : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string strValue = Convert.ToString(value);

            if (string.IsNullOrEmpty(strValue))
                return new ValidationResult(false, "Value cannot be coverted to string.");

            if (Utils.IsMatchRegex(strValue, new Regex(@"([:A-z0-9_ ]{1,}[ *&]{0,}\([A-z0-9_]{0,}[ *]{1,}[A-z0-9_]{1,}\)[ ]{0,}\(.*\))|([A-z]{1,}.*[:]{0,}[\*, ]{1,}[A-z_]{1,}([0-9,A-z].*){0,}(\[.*\]){0,})")))
                return new ValidationResult(true, null);
            return new ValidationResult(false, "Variable is bad");
        }
    }
}
