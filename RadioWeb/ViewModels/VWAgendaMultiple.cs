using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RadioWeb.Models;
using System.Web.Mvc;

namespace RadioWeb.ViewModels
{
    public class VWAgendaMultiple
    {
        public VWAgendaMultiple()
        {
            if (System.Web.HttpContext.Current.Application["GrupoAparatos"] != null)
            {
                List<GAPARATOS> oListTemp = (List<GAPARATOS>)System.Web.HttpContext.Current.Application["GrupoAparatos"];
                this.GruposAparatos = Utils.DropDownList<GAPARATOS>.LoadItems(oListTemp, "OID", "COD_GRUP");
            }
        }
        public DateTime Fecha { get; set; }
        public string DiaSemana { get; set; }
        public int oidAparato { get; set; }
        public List<LISTADIAAMBFORATS> ListaDia { get; set; }
        public string TextoAgenda { get; set; }
        public string HuecosLibres { get; set; }
        public List<string> ExploracionesDadas { get; set; }
        public SelectList GruposAparatos { get; set; }

    }
}