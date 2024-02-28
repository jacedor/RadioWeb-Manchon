using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Logica
{
    public class ResultadoRequest
    {
        public enum RESULTADO
        {
            SUCCESS,
            ERROR
        }
        public RESULTADO Resultado { get; set; }
        public string Mensaje { get; set; }
    }
}