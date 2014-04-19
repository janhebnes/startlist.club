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

            ValidationParameters["min"] = minValue.ToString("yyyy-MM-dd");
            ValidationParameters["max"] = maxValue.ToString("yyyy-MM-dd");
        }
    }
}