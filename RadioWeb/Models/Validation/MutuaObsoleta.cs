using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Models.Validation
{
    public class MutuaObsoleta : ValidationAttribute, IClientValidatable
    {

        public MutuaObsoleta()
        : base("La mutua es obsoleta. {0}")
        {
            mutuaObsoleta = false;
        }

        public bool mutuaObsoleta { get; set; }
        

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            //string errorMessage = this.FormatErrorMessage(metadata.DisplayName);
            var rule = new ModelClientValidationRule();
            rule.ErrorMessage = FormatErrorMessage(metadata.GetDisplayName());
            rule.ValidationParameters.Add("mutuaobsoleta", mutuaObsoleta);
            rule.ValidationType = "mutuaobsoleta";
            yield return rule;
        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            int mutuaID = Convert.ToInt32(value);
            var mutua = MutuasRepositorio.Obtener(mutuaID);
            
            var isValid = (mutua.VERS == 0 ? true : false);
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