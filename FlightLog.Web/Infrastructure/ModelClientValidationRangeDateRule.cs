using System;
using System.Web.Mvc;

namespace FlightLog.Infrastructure
{
    public class ModelClientValidationRangeDateRule : ModelClientValidationRule
    {
        public ModelClientValidationRangeDateRule(string errorMessage, DateTime minValue, DateTime maxValue)
        {
            ErrorMessage = errorMessage;
            ValidationType = "rangedate";

            ValidationParameters["min"] = minValue.ToString("dd-MM-yyyy");
            ValidationParameters["max"] = maxValue.ToString("dd-MM-yyyy");
        }
    }
}