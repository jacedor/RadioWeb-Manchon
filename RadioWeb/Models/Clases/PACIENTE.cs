//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------


namespace RadioWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web.Mvc;
    using RadioWeb.Models.Validation;
    using RadioWeb.Models.Repos;

    public partial class PACIENTE
    {
        
        public PACIENTE()
        {
            this.DIRECCIONES = new HashSet<DIRECCION>();
            this.TELEFONOS = new HashSet<TELEFONO>();
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            this.MODULO_LOPD = oConfig.ObtenerValor("LOPD_PACIENTE").Equals("T");
        }

        [DisplayName("Id")]
        public int OID { get; set; }

        public Nullable<int> VERS { get; set; }

        [MutuaObsoleta(ErrorMessage = "La mutua es obsoleta")]
        [DataType("MUTUASLIST")]
        [EnteroPositivo(ErrorMessage = "Debe asignar una mutua")]
        [DisplayName("MUTUA")]
        public Nullable<int> CID { get; set; }
        public string CANAL { get; set; }
        public Nullable<int> OWNER { get; set; }
        public string USERNAME { get; set; }
        public Nullable<System.DateTime> MODIF { get; set; }
        public Nullable<int> IOR_EMPRESA { get; set; }
        public string BORRADO { get; set; }
        [CampoManchon(Busqueda = true)]
        [DisplayName("HC")]
        public string COD_PAC { get; set; }

        [CampoManchon(Busqueda = true)]
        [DisplayName("PACIENTE")]
        [Required, StringLength(40)]
        [ContieneComa(ErrorMessage = "El nombre debe contener una coma")]
        [Column("PACIENTE")]
        public string PACIENTE1 { get; set; }

        
        [DisplayName("FECHA NAC.")]        
        [DataType("FECHAMASK")]
        [Required]
        public Nullable<System.DateTime> FECHAN { get; set; }

        [DisplayName("SEXO")]
        [DataType("SEXO")]
        [Required]
        public string SEXO { get; set; }

        [CampoManchon(Busqueda = true)]
        [DisplayName("PROFESION")]
        [StringLength(15, ErrorMessage = "El campo {0} debe tener un máximo de {2} carácteres.")]
        public string PROFESION { get; set; }


        [DataType("TIPO_DOC")]
        [Display(Name = "TIPO DOC")]
        public int TIPO_DOC { get; set; }

        [DisplayName("DNI")]
        [CampoManchon(Busqueda = true)]
        [StringLength(12, ErrorMessage = "El campo {0} debe tener un máximo de {2} carácteres.")]
        public string DNI { get; set; }

        [DisplayName("QREPORT")]
        [DataType("BooleanString")]
        public string COMPARTIR { get; set; }


        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string EMAIL { get; set; }

        [DisplayName("ENLACE")]
        [DataType("BooleanString")]
        public string ENLACE { get; set; }      

        public string EDAD { get; set; }

        [DisplayName("DNI RESPONSABLE")]
        [StringLength(12, ErrorMessage = "El campo {0} debe tener un máximo de {2} carácteres.")]
        public string DNIRESPONSABLE { get; set; }

        [DisplayName("TRAT.")]
        [DataType("TRATAMIENTOS")]
        public Nullable<short> TRAC { get; set; }

        public string MAILING { get; set; }
        public string OTROS1 { get; set; }

        [DisplayName("LOPD")]
        [DataType("BooleanString")]
        public string OTROS2 { get; set; }

        public string RUTA_LOPD { get; set; }

        [DisplayName("LOPD")]
        [DataType("BooleanString")]
        public string NUEVA_LOPD { get; set; }

        [DisplayName("DIFUSIÓ")]
        [DataType("BooleanString")]
        public string OTROS3 { get; set; }

        [DisplayName("IDIOMA")]
        [DataType("IDIOMAS")]
        public string OTROS4 { get; set; }
        public string OTROS5 { get; set; }

        [StringLength(30,ErrorMessage ="El campo {0} debe tener un máximo de {1} carácteres.")]
        public string POLIZA { get; set; }
        [StringLength(40, ErrorMessage = "El campo {0} debe tener un máximo de {1} carácteres.")]
        public string RESPONSABLE { get; set; }
        public string HORAMOD { get; set; }

        [DisplayName("CIP")]
        [CampoManchon(Busqueda = true)]
        public string CIP { get; set; }

        [DataType("BooleanString")]
        public string AVISO { get; set; }
        public string MAILING1 { get; set; }
        public string MAILING2 { get; set; }
        public string MAILING3 { get; set; }
        public string MAILING4 { get; set; }

        [DataType("StringMultiline")]
        public string COMENTARIO { get; set; }

        [DataType("StringMultiline")]
        public string TEXTO { get; set; }

        [StringLength(30, ErrorMessage = "El campo {0} debe tener un máximo de {2} carácteres.")]
        public string TARJETA { get; set; }

        [DataType("BooleanString")]
        public string VIP { get; set; }

        [DataType("BooleanString")]
        public string RIP { get; set; }
        public string CODMUTUA { get; set; }
        public string DESCMUTUA { get; set; }
        public string TRATAMIENTODESC { get; set; }

        [NotMapped]
        public DateTime? ULTIMAVISITAREALIZADA { get; set; }

        [NotMapped]
        public int CUENTAVISITAS { get; set; }

        [NotMapped]
        public DateTime? PROXIMACITA { get; set; }

        [NotMapped]
        public string HAYCAMBIOSPACIENTE { get; set; }

        [NotMapped]
        public string URLPREVIAPACIENTE { get; set; }

        [NotMapped]
        public string MOVILPACIENTE { get; set; }

        [UIHint("DIRECCION")]
        public virtual ICollection<DIRECCION> DIRECCIONES { get; set; }
        [UIHint("TELEFONO")]
        public virtual ICollection<TELEFONO> TELEFONOS { get; set; }
        public virtual ICollection<LISTADIA> EXPLORACIONES { get; set; }
        public virtual ICollection<INFORMES> INFORMES { get; set; }


        // Campos LOPD permisos de paciente
        [DisplayName("EMAIL MÉDICO")]
        [DataType("BooleanStringLOPD")]
        public string ENVIO_MEDICO { get; set; }

        [DisplayName("EMAIL RESULTADOS")]
        [DataType("BooleanStringLOPD")]
        public string ENVIO_RESULTADOS { get; set; }

        [DisplayName("Notif. EMAIL")]
        [DataType("BooleanStringLOPD")]
        public string ENVIO_MAIL { get; set; }

        [DisplayName("Notif. SMS")]
        [DataType("BooleanStringLOPD")]
        public string ENVIO_SMS { get; set; }

        [DisplayName("PROPAGANDA")]
        [DataType("BooleanString")]
        public string ENVIO_PROPAGANDA { get; set; }

        [DisplayName("LLAMADA NOMBRE")]
        [DataType("BooleanString")]
        public string LLAMADA_NOMBRE { get; set; }

        [DisplayName("ACCESO WEB")]
        [DataType("BooleanString")]
        public string ACCESO_WEB { get; set; }

        [DisplayName("CONSULTA PREVIA")]
        [DataType("BooleanString")]
        public string CONSULTA_PREVIA { get; set; }





        //Booleano que guarda si el modulo de la LOPD esta activado o no.
        [NotMapped]
        public bool MODULO_LOPD { get; set; }
    }




}