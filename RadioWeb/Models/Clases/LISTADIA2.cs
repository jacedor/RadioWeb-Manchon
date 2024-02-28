using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADPM.Common.Web.DataAnnotations;
namespace RadioWeb.Models
{
   
    public class LISTADIA2
    {
       
        [Key]
        public int OID { get; set; }


       
        public Nullable<int> CID { get; set; }

     
        public int CENTRO { get; set; }

        public string CENTROD { get; set; }
        public string GRUPO { get; set; }

       
        public Nullable<int> VERS { get; set; }

       
        public int IOR_EMPRESA { get; set; }

       
        public int IOR_GPR { get; set; }

       
        public int IOR_PACIENTE { get; set; }

       
        public Nullable<int>  IOR_MEDICO { get; set; }
        public string CIRUJANO{ get; set; }

       

        public string INTOCABLE { get; set; }
       
        public string DIA { get; set; }

        [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}")]
        public DateTime FECHA { get; set; }
        public string HORA { get; set; }

        [Display(Name = "INF")]    
        public string INFORMADA { get; set; }

       
        public string HORA_LL { get; set; }

       
        public string HORA_EX { get; set; }


       
        public string PACIENTE { get; set; }

       
        public Nullable<int> PRIVADO { get; set; }

       
        public Nullable<int> IOR_MASTER { get; set; }

       
        public string NUMEROS { get; set; }

       
        public string COD_MUT { get; set; }


        public string MUTUA { get; set; }

        public string NHCAP { get; set; }

        public string COD_FIL { get; set; }

       
        public string ESPERA { get; set; }


        public string FIL { get; set; }


        public string EXPLO { get; set; }

       
        public string TECNICO { get; set; }


        public double? CANTIDAD { get; set; }

       
        public string SIMBOLO { get; set; }


        public string APLAZADO { get; set; }

       
        public string PAGAR { get; set; }

       
        public string PAGADO { get; set; }

        [DataType("FACTURADO")]
        public string FACTURADA { get; set; }

       
        public string NUM_FAC { get; set; }


        public string ESTADO { get; set; }

       
        public int GRUPOAPA { get; set; }

       
        public string HAYCOMEN { get; set; }

        [Display(Name = "NF")]
        public string NOFACTURAB { get; set; }

        //[TableIcon(FaIconTrue = "fa  fa-eyedropper", ColorTrue = "blue")]
       
        public string HAYCONSUMIBLE { get; set; }

       
        public string MEDICO { get; set; }
        public string COLEGIADO { get; set; }

       
        public string VIP { get; set; }


        public string TEXTO { get; set; }

       
        public Nullable<int> CANCELADO { get; set; }


        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FECHA_IDEN { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? FECHAMAXENTREGA { get; set; }

        public Nullable<int> IOR_CENTROEXTERNO { get; set; }
        public Nullable<int> IOR_BOLSAPETICIONES { get; set; }
        public string FIRMA_CONSEN { get; set; }
        public string LOPD { get; set; }
        public string QRCOMPARTIRCASO { get; set; }
        public string TELEFONOPETICION { get; set; }
        public string PRIORIDAD { get; set; }
       





       
        public string COD_MED { get; set; }

       
        public string COLOR { get; set; }
        public string USERNAME { get; set; }

       
        public string HORA_HORARIO { get; set; }

       
        public string SUBTEXTO { get; set; }

       
        public string RUTAIMAGEN { get; set; }

       
        public List<EXP_CONSUM> CONSUMIBLES { get; set; }

       
        public List<PAGOS> PAGOS { get; set; }
    }
}