using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels
{
    public class VWImprimirJustificante
    {
        

        public int OID { get; set; }       
        public string HORA { get; set; }
        public string HORA_LL { get; set; }
       
        public string TEXTO { get; set; }

     
        public string PACIENTE { get; set; }
        
        public int TRAC { get; set; }
        public DateTime FECHA { get; set; }

        public string NOMBRECENTRO { get; set; }
        public string DIRECCIONCENTRO { get; set; }
        public string CPCENTRO { get; set; }
        public string CIUTATCENTRO { get; set; }        


        

        
        public string TELEFONOCENTRO { get; internal set; }

        
    }

      
}