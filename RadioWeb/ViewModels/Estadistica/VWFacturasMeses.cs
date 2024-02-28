using RadioWeb.Models;
using RadioWeb.Models.Estadistica;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Estadistica
{
    public class VWFacturasMeses
    {
        public VWFacturasMeses()
        {
            //this.TRATAMIENTOS = DataBase.Tratamientos();
        }

        public List<ItemFacturasMes> TablaEstadistica { get; set; }
        public VWFiltros FILTROS { get; set; }

    }
}