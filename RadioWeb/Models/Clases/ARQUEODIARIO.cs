using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models
{

    public class ARQUEODIARIO
    {
        public DateTime FECHA { get; set; }
        public string PACIENTE { get; set; }
        public string DESCRIPCION { get; set; }
        public string EXPLO { get; set; }
        public DateTime DEUDA_FECHA { get; set; }
        public string APLAZADO { get; set; }
        public double CANTIDAD { get; set; }
        public int CID { get; set; }
        public string TIPOPAGO { get; set; }
        public string SIMBOLO { get; set; }

    }
    }
