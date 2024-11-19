using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace RadioWeb.Models
{
    [Table("UCCADUSER")]
    public class USUARIO
    {
        public USUARIO()
        {
            MainMenus = new List<UCCADMENU>();
            SubMenus = new List<UCCADMENU>();
            
        }

        [Key]
        public int IDUSER { get; set; }

        public string ACTIVO { get; set; }

        [Display(Name = "NOMBRE")]
        public string NOME { get; set; }      
       
        [Required]
        [Display(Name = "USUARIO")]
        public string LOGIN { get; set; }

        public string SENHA { get; set; }

        public string EMAIL { get; set; }

        public int? PRIVILEGIADO { get; set; }

        public string TIPO { get; set; }

        [DataType("ROLES")]
        public int? PERFIL { get; set; }

        public string PASSWORD { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "PASSWORD")]      
        public string PASSWORDWEB { get; set; }

        public string ENCRIPTADO { get; set; }
        public int? CENTROASOCIADO { get; set; }

   

        [Display(Name = "VINCULAR A MEDICO REFERIDOR")]
        [DataType("COLEGIADO")]
        public int? IOR_COLEGIADO { get; set; }


        public Nullable<System.DateTime> FECHA_PASSWORD { get; set; }

        [Display(Name = "VINCULAR A MUTUA REFERIDORA?")]
        [DataType("MUTUASLIST")]
        public int? IOR_ENTIDADPAGADORA { get; set; }

        [Display(Name = "VINCULAR A CENTRO EXTERNO")]
        [DataType("CENTROEXTERNO")]
        public int? IOR_CENTROEXTERNO { get; set; }

        [Display(Name = "PERMITIR ACCESO A CITA ONLINE")]
        [DataType("BooleanString")]
        public string CITACIONONLINE { get; set; }

        [Display(Name = "PERMITIR ACCESO A PETICIONES")]
        [DataType("BooleanString")]
        public string CREARPETICIONES { get; set; }
        public int? LOGIN_ATTEMPTS { get; set; }

        [Display(Name = "ACCESO BLOQUEADO")]
        [DataType("BooleanString")]
        public string BLOQUEADO { get; set; }

        [NotMapped]
        [DataType("PERSONAL")]
        [Display(Name = "VINCULAR CON PERSONAL DEL CENTRO")]
        public int IOR_PERSONAL { get; set; }

      

        [NotMapped]
        public string DESCPERFILWEB { get; set; }
        [NotMapped]
        public int PERFILWEB { get; set; }
       
      
        [NotMapped]
        public string CARTELERADEFECTO { get; set; }
        [NotMapped]
        public int VECANTIDADES { get; set; }
       

        [NotMapped]
        public PERSONAL PERSONAL { get; set; }
        [NotMapped]
        public MUTUAS MUTUA { get; set; }
        [NotMapped]
        public CENTROSEXTERNOS CENTROEXTERNO { get; set; }

        [NotMapped]
        public bool ESMEDICO { get; set; }
        [NotMapped]
        public List<UCCADMENU> MainMenus{ get; set; }
        [NotMapped]
        public List<UCCADMENU> SubMenus { get; set; }
        [NotMapped]
        public static List<USUARIO> Roles { get; set; }

    }
}