using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Informes
{
    public class VMExploNoInformadas
    {
        public VMExploNoInformadas()
        {
            ULTIMOSINFORMES = new List<INFORMES>();
            PENDIENTESREVISAR = new List<VMInformesNoRevisados>();
        }

        [DisplayName("Id Expl")]
        public int OID { get; set; }     
        public bool BORRADO { get; set; }

        [DisplayName("Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> FECHA { get; set; }

        [DisplayName("C")]
        public int CID { get; set; }


        [DisplayName("F. Entrega")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> FECHAMAXIMA { get; set; }

        [DisplayName("Hora")]
        public string HORA { get; set; }

        [DisplayName("Paciente")]
        public string PACIENTE { get; set; }

        [DisplayName("Mut")]
        public string COD_MUT { get; set; }

        [DisplayName("Mut")]
        public string DES_MUT { get; set; }

        [DisplayName("Centro")]
        public string CENTRO { get; set; } 

        [DisplayName("Apa")]
        public string COD_FIL { get; set; }

        [DisplayName("Exp")]
        public string FIL { get; set; }

        [DisplayName("Col")]
        public string COD_MED { get; set; }


        [DisplayName("NomMed")]
        public string NOM_MED { get; set; }

        [DisplayName("In")]
        public bool INFORMADO  { get; set; }

        [DisplayName("Med")]
        public string MEDINFO { get; set; }

        [DisplayName("Revisión")]
        public string MEDREV { get; set; }

        [DisplayName("Status")]
        public string STATUS { get; set; }

        [DisplayName("Cs")]
        public bool HAYCONSUMIBLE { get; set; }

        [DisplayName("Comen")]
        public string TEXTO { get; set; }

        [DisplayName("Autorizacion")]
        public string AUTORIZACION { get; set; }

        public int IOR_INFORME { get; set; }

        [DisplayName("Recogida")]
        public int DIAS_ENTREGA { get; set; }

        [DisplayName("Id Pac")]
        public int IOR_PACIENTE { get; set; }


        public int IOR_MASTER { get; set; }

        public static IEnumerable<INFORMES> ULTIMOSINFORMES { get; set; }

        public static List<VMInformesNoRevisados> PENDIENTESREVISAR { get; set; }



    }
}