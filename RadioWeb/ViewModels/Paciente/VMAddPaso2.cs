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
    public class VMAddPaso2
    {
       

        public PACIENTE PACIENTEALTA { get; set; }
        public int IOR_BOLSA { get; set; }
      
        public List<HUECO> HUECOS { get; set; }
        public VMAddPaso2()
        { }


    




    }


}