using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Clases
{
    public class LOPD_ENVIO_PAC
    {
        [NotMapped]
        public bool ENVIO_MEDICO { get; set; }
        [NotMapped]
        public bool ENVIO_RESULTADOS { get; set; }
        [NotMapped]
        public bool ENVIO_MAIL { get; set; }
        [NotMapped]
        public bool ENVIO_SMS { get; set; }
        [NotMapped]
        public bool ENVIO_PROPAGANDA { get; set; }

    }
}