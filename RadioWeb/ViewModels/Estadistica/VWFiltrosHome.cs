using RadioWeb.Models;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Estadistica
{
    public class VWFiltrosResumenFacturacion
    {
        public VWFiltrosResumenFacturacion()
        {
            //this.TRATAMIENTOS = DataBase.Tratamientos();
        }
      
        [UIHint("FILTROS")]
        public VWFiltros FILTROS { get; set; }

    }
}