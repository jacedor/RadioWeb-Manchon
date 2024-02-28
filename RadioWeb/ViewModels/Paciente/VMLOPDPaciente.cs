using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.ViewModels.Paciente
{
    public class VMLOPDPaciente
    {

        public VMLOPDPaciente()
        { }

        [DisplayName("EMAIL MÉDICO")]
        [DataType("BooleanString")]
        public string ENVIO_MEDICO { get; set; }

        [DisplayName("EMAIL RESULTADOS")]
        [DataType("BooleanString")]
        public string ENVIO_RESULTADOS { get; set; }

        [DisplayName("Notif. EMAIL")]
        [DataType("BooleanString")]
        public string ENVIO_MAIL { get; set; }

        [DisplayName("Notif. SMS")]
        [DataType("BooleanString")]
        public string ENVIO_SMS { get; set; }

        [DisplayName("PROPAGANDA")]
        [DataType("BooleanString")]
        public string ENVIO_PROPAGANDA { get; set; }

        [DisplayName("LLAMADA NOMBRE")]
        [DataType("BooleanString")]
        public string LLAMADA_NOMBRE { get; set; }

        [DisplayName("ACCESO WEB")]
        [DataType("BooleanString")]
        public string ACCESO_WEB { get; set; }

        [DisplayName("CONSULTA PREVIA")]
        [DataType("BooleanString")]
        public string CONSULTA_PREVIA { get; set; }

    }


}