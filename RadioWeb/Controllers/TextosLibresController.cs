using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using RadioWeb.Utils;
using RadioWeb.ViewModels.Paciente;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class TextosLibresController : Controller
    {
        private RadioDBContext db = new RadioDBContext();

        [HttpGet]
        public string Obtener(string Numero)
        {
            Numero = Numero.ToUpper();
            TEXTOSLIBRES oTextoLibre = db.TextosLibres.FirstOrDefault(h => h.NUMERO == Numero);          
            //Si ya existe una historia clinica
            if (oTextoLibre != null)
            {                
                oTextoLibre.TEXTO = DataBase.convertRtfToPlainText(oTextoLibre.DES_TEX);
            }
            else
            {
                oTextoLibre.TEXTO = "";
            }
            return oTextoLibre.TEXTO;
        }

     

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}