using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Informes
{
    public class VMInformesNoRevisados
    {
        public VMInformesNoRevisados()
        {
            ULTIMOSREVISADOS = new List<INFORMES>();
        }

        [DisplayName("Id")]
        public int OID { get; set; }     
        public bool BORRADO { get; set; }

        [DisplayName("Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> FECHA { get; set; }

        [DisplayName("F. Entrega")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> FECHAMAXIMA { get; set; }

        [DisplayName("Titulo")]
        public string TITULO { get; set; }

        [DisplayName("Validación")]
        public string VALIDACION { get; set; }

        [DisplayName("Hora")]
        public string HORA { get; set; }

        [DisplayName("Paciente")]
        public string PACIENTE { get; set; }

        [DisplayName("Mut")]
        public string COD_MUT { get; set; }

        [DisplayName("Apa")]
        public string COD_FIL { get; set; }

        [DisplayName("Exp")]
        public string FIL { get; set; }

        [DisplayName("Col")]
        public string COD_MED { get; set; }

        [DisplayName("In")]
        public bool INFORMADO  { get; set; }

        [DisplayName("Med")]
        public string MEDINFO { get; set; }

        [DisplayName("CENTRO")]
        public string CENTRO { get; set; }

        [DisplayName("Comen")]
        public string TEXTO { get; set; }

        public int IOR_EXPLORACION { get; set; }
        //Virtual MONEDA

        public int IOR_PACIENTE { get; set; }
        //Virtual MONEDA

        public static IEnumerable<INFORMES> ULTIMOSREVISADOS { get; set; }

        

    }
}