using RadioWeb.Models;
using System;


namespace RadioWeb.ViewModels
{
    public class VWPagoRapido
    {
        public int OID { get; set; }
     
        public int OWNER { get; set; }      
        public DateTime FECHA { get; set; }
        public double CANTIDAD { get; set; }
        public DateTime DEUDA_FECHA { get; set; }
        public Nullable<double> DEUDA_CANTIDAD { get; set; }
        public string APLAZADO { get; set; }
        public string TIPOPAGO { get; set; }       
        public string BORRADO { get; set; }
        public string ACTUALIZADO { get; set; }
        public int IOR_MONEDA { get; set; }

        public EXPLORACION EXPLORACION { get; set; }
    }
}