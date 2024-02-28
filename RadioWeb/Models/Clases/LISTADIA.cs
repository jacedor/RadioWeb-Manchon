using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADPM.Common.Web.DataAnnotations;
namespace RadioWeb.Models
{
    [Table("LISTADIA")]
    public class LISTADIA
    {
        [HiddenInput]
        [Key]
        public int OID { get; set; }
        [NotMapped]
        public string COMENT { get; set; }
        [NotMapped]
        public string URLDOCUMENT { get; set; }
        [NotMapped]
        public int? OIDDOCUMENTOBOLSA { get; set; }


        [TableColumn(Exclude = true)]
        public Nullable<int> CID { get; set; }

        [TableColumn(Exclude = true)]
        public int CENTRO { get; set; }

        [NotMapped]
        public string CENTRONAME { get; set; }

        [TableColumn(Exclude = true)]
        public Nullable<int> VERS { get; set; }

        [TableColumn(Exclude = true)]
        public int IOR_EMPRESA { get; set; }

        [TableColumn(Exclude = true)]
        public int IOR_GPR { get; set; }

        [TableColumn(Exclude = true)]
        public int IOR_PACIENTE { get; set; }

        [TableColumn(Exclude = true)]
        public int IOR_MEDICO { get; set; }

        [TableColumn(Exclude = true)]

        [NotMapped]
        [DefaultValue(false)]
        public bool INTOCABLE { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public string DIA { get; set; }

        [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}")]
        public DateTime FECHA { get; set; }

        [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}")]
        [NotMapped]
        public DateTime FECHA_IDEN { get; set; }
        public string HORA { get; set; }

        [Display(Name = "INF")]
        [TableIcon(Action = "Test", Controller = "Controller",
            FaIconTrue = "fa fa-clipboard", FaIconFalse = "fa fa-clipboard",
            ColorTrue = "green", ColorFalse = "red")]

        //[NotMapped]
        //public bool INFORMADA { get; set; }
        public string INFORMADA { get; set; }

        [TableColumn(Exclude = true)]
        public string HORA_LL { get; set; }

        [TableColumn(Exclude = true)]
        public string HORA_EX { get; set; }


        [TableColumn(Exclude = true)]
        public string PACIENTE { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public int PRIVADO { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public int IOR_MASTER { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public string NUMEROS { get; set; }

        [TableColumn(Exclude = true)]
        public string COD_MUT { get; set; }


        public string MUTUA { get; set; }

        public int IOR_ENTIDADPAGADORA { get; set; }


        [NotMapped]
        public string NHCAP { get; set; }

        public string COD_FIL { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public string ESPERA { get; set; }


        public string FIL { get; set; }


        public string EXPLO { get; set; }

        [TableColumn(Exclude = true)]
        public string TECNICO { get; set; }


        public double? CANTIDAD { get; set; }

        [TableColumn(Exclude = true)]
        public string SIMBOLO { get; set; }

        [NotMapped]
        public bool APLAZADO { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public bool PAGAR { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public bool PAGADO { get; set; }

        [DataType("FACTURADO")]
        [NotMapped]
        public bool FACTURADA { get; set; }

        [TableColumn(Exclude = true)]
        public string NUM_FAC { get; set; }


        public string ESTADO { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public int GRUPOAPA { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public bool HAYCOMEN { get; set; }

        [Display(Name = "NF")]
        [NotMapped]
        public bool NOFACTURAB { get; set; }

        //[TableIcon(FaIconTrue = "fa  fa-eyedropper", ColorTrue = "blue")]
        [TableColumn(Exclude = true)]
        [NotMapped]
        public bool HAYCONSUMIBLE { get; set; }

        [TableColumn(Exclude = true)]
        public string MEDICO { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public bool VIP { get; set; }


        public string TEXTO { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public int CANCELADO { get; set; }



        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? FECHAMAXENTREGA { get; set; }

        public int IOR_COLEGIADO { get; set; }
        public Nullable<int> IOR_CENTROEXTERNO { get; set; }

        [NotMapped]
        public string CENTROEXTERNO { get; set; }
        [NotMapped]
        public int IOR_BOLSAPETICIONES { get; set; }
        [NotMapped]
        public bool FIRMA_CONSEN { get; set; }
        [NotMapped]
        public bool LOPD { get; set; }
        [NotMapped]
        public bool QREPORT { get; set; }
        [NotMapped]
        public bool RECORDED { get; set; }

        [NotMapped]
        public bool QRCOMPARTIRCASO { get; set; }
        [NotMapped]
        public string TELEFONOPETICION { get; set; }
        [NotMapped]
        public string PRIORIDAD { get; set; }
       





        [TableColumn(Exclude = true)]
        public string COD_MED { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public string COLOR { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public string HORA_HORARIO { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public string SUBTEXTO { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public string RUTAIMAGEN { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public List<EXP_CONSUM> CONSUMIBLES { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public List<PAGOS> PAGOS { get; set; }

        public Nullable<int> IOR_MOTDESPROG { get; set; }

        [TableColumn(Exclude = true)]
        [NotMapped]
        public Nullable<int> IOR_INFORME { get; set; }

        [NotMapped]
        public bool ENTREGA_PAPEL { get; set; }
    }
}