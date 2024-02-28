using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.ViewModels.Paciente
{
    public class VMHistoriaClinica
    {
       

        public int OID { get; set; }
        public int OIDPACIENTE { get; set; }
        public int CID { get; set; }

        public DateTime MODIF{ get; set; }

        public VMHistoriaClinica()
        { }


        [NotMapped]
        [AllowHtml]
        public string TEXTOHTML { get; set; }




    }


}