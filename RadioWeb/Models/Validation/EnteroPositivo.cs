using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Models.Validation
{
    public class EnteroPositivo : ValidationAttribute, IClientValidatable
    {
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            //string errorMessage = this.FormatErrorMessage(metadata.DisplayName);
            var rule = new ModelClientValidationRule();
            rule.ErrorMessage = FormatErrorMessage(metadata.GetDisplayName());
            rule.ValidationParameters.Add("enteropositivo", false);
            rule.ValidationType = "enteropositivo";
            yield return rule;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var valor = value;
            if (valor!=null && (int)valor > 0)
            {
                return ValidationResult.Success;

            }
            else
            {
                return new ValidationResult(
                   FormatErrorMessage(validationContext.DisplayName)
               );
            }

           ;

        }
    }
}