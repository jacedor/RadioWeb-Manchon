using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Models.Validation
{
    public class ContieneComa : ValidationAttribute, IClientValidatable
    {

        public ContieneComa()
        : base("El campo debe contener una coma {0}")
        {
            HayComa = false;
        }

        public bool HayComa { get; set; }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            //string errorMessage = this.FormatErrorMessage(metadata.DisplayName);
            var rule = new ModelClientValidationRule();
            rule.ErrorMessage = FormatErrorMessage(metadata.GetDisplayName());
            rule.ValidationParameters.Add("contienecoma", HayComa);
            rule.ValidationType = "contienecoma";
            yield return rule;
        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {


            var isValid = value != null ? value.ToString().Contains(",") : true;
            if (isValid)
            {
                return ValidationResult.Success;

            }
            else
            {
                return new ValidationResult(
                   FormatErrorMessage(validationContext.DisplayName)
               );
            }
            

        }
    }
}