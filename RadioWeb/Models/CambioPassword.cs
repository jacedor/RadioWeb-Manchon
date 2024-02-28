using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace RadioWeb.Models
{
    public class CambioPassword
    {
        public int IDUSER { get; set; }
        public string LOGIN { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Su contraseña actual")]
        public string PasswordOld { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Su nueva contraseña")]
        public string Password1 { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Repita su nueva contraseña")]
        public string Password2 { get; set; }
    }
}