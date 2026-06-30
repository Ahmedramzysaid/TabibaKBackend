using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Validations
{
    public class ValidationsResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public ValidationsResult(bool isValid,string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
    }
}
