using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RadioWeb.ViewModels
{
 
    public class VWFiltros
    {

        public VWFiltros()
        {
           
        }
        
        [DisplayName("FECHA")]
        public string FECHA { get; set; }

       
        [DisplayName("FECHA INICIO")]
        public string FECHA_INICIO { get; set; }

       
        [DisplayName("FECHA FIN")]
        public string FECHA_FIN { get; set; }

        [DataType("DAPARATO")]
        [DisplayName("APARATO")]
        public int IOR_APARATO { get; set; }

        [DataType("ESTADOEXPLORACION")]
        [DisplayName("ESTADO")]
        public int ESTADO { get; set; }

        [DataType("GAPARATO")]
        [DisplayName("GRUPO")]
        public int IOR_GRUPO { get; set; }

        [DataType("MUTUASLIST")]
        [DisplayName("MUTUA")]
        public int IOR_ENTIDADPAGADORA { get; set; }

        
        [DataType("CENTROS")]
        [DisplayName("CENTRO")]
        public int IOR_CENTRO { get; set; }

        [DataType("MUTUATIPO")]
        [DisplayName("TIPO")]
        public int IOR_TIPO { get; set; }

        [DataType("INFORMADA")]
        [DisplayName("INFORMADA")]
        public string INFORMADA { get; set; }

        [DataType("FACTURADO")]
        [DisplayName("FACTURADA")]
        public string FACTURADA { get; set; }


        [DataType("PAGADO")]
        [DisplayName("PAGADO")]
        public string PAGADO { get; set; }


        [DataType("MEDICO")]
        [DisplayName("MEDICO INFORMANTE")]
        public int IOR_MEDICO { get; set; }

        [DataType("COLEGIADO")]
        [DisplayName("MÉDICO REFERIDOR")]
        public int IOR_COLEGIADO { get; set; }

        [DisplayName("BORRADO")]
        public bool BORRADOS { get; set; }

        public bool BUSQUEDATOTAL { get; set; }

        public bool BUSQUEDATOTALPORMEDICO { get; set; }

        [DisplayName("NOMBRE DE PACIENTE")]
        public string PACIENTE { get; set; }

        public int IOR_PACIENTE { get; set; }

        public int OIDACTIVA { get; set; }


        [DataType("PIVOTTABLE")]
        [DisplayName("TABLA CRUZADA")]
        public int IORPIVOTTABLE { get; set; }
    }

   
}