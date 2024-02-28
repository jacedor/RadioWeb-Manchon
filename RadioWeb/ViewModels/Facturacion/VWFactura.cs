using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RadioWeb.Models;
using System.ComponentModel.DataAnnotations;
using RadioWeb.Models.Repos;
using System.ComponentModel;

namespace RadioWeb.ViewModels
{
    public class VWArqueo
    {
        public VWArqueo()
        {
           
          
        }

        public string Fecha { get; set; }
        public string Aplazado { get; set; }
        public string DescripcionConcepto { get; set; }
        public int  CuentaConcepto { get; set; }
        public string CantidadSumaConcepto { get; set; }

    }
}