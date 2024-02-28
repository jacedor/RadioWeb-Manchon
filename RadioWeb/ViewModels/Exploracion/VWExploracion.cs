using RadioWeb.Models;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Exploracion
{
    public class VWExploracion
    {
        public VWExploracion()
        {
            this.TRATAMIENTOS = DataBase.Tratamientos();
        }

        public Dictionary<string, string> TRATAMIENTOS { get; set; }
        public PACIENTE PACIENTE { get; set; }
        //public string ESTADO { get; set; }
        public EXPLORACION EXPLORACION { get; set; }
       // public List<LISTADIA> EXPLORACIONES { get; internal set; }
        //public List<INFORMES> INFORMES { get; internal set; }
    }
}