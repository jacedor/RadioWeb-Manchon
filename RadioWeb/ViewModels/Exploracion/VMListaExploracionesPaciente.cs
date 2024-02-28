using RadioWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.ViewModels.Exploracion
{
    public class VMListaExploracionesPaciente
    {
        public bool VECANTIDADES { get; set; }
        public bool ESPERFILMANRESA { get; set; }
        public List<LISTADIA> EXPLORACIONES{ get; set; }
    }
}