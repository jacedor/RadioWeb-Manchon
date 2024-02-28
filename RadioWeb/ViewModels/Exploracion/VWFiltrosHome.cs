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
    public class VWFiltrosHome
    {
        public VWFiltrosHome()
        {
            //this.TRATAMIENTOS = DataBase.Tratamientos();
        }
        public bool MOSTRARRESUMEN { get; set; }
        [UIHint("FILTROS")]
        public VWFiltros FILTROS { get; set; }

    }
}