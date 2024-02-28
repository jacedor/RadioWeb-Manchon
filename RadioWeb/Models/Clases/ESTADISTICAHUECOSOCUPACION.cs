using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Clases
{
    public class ESTADISTICAHUECOSOCUPACION
    {
        public String HORA { get; set; }
        public int NUMERO_HORALIBRE { get; set; }
        public int RECUENTO_TOTALHORAS { get; set; }

        public decimal PORCENTAJE { get; set; }
    }
}