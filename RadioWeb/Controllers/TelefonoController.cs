using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Models.Utilidades;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    public class TelefonoController : Controller
    {

        //
        // GET: /Direccion/
       [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult List(string oidPaciente)
        {
            return PartialView("List2", TelefonoRepositorio.Obtener(int.Parse(oidPaciente)));
        }

       public string GetMovilPacienteFromExploracion(int oidExploracion)
       {
           string resultado = "";
           EXPLORACION oExplo = ExploracionRepositorio.Obtener(oidExploracion);
           foreach (TELEFONO item in TelefonoRepositorio.Obtener(oExplo.IOR_PACIENTE))
           {
               if (item.NUMERO.StartsWith("6"))
               {
                   resultado= item.NUMERO;
               }
           }
           return resultado;
       }

        [HttpPost]
        public ActionResult EditarCampo(string name, int pk, string value)
        {
            TELEFONO oTelefono = null;
            if (pk == -1)
            {
                oTelefono = new TELEFONO { NUMERO = value,OWNER= int.Parse( name) };
                TelefonoRepositorio.Insertar(oTelefono);
            }
            else {
               
                if (string.IsNullOrEmpty(value))
                {
                    TelefonoRepositorio.Delete(pk.ToString());
                }
                else {
                    oTelefono = TelefonoRepositorio.Get(pk);
                    oTelefono.NUMERO = value;
                    TelefonoRepositorio.Editar(oTelefono);
                }
                
            }
            
            
            

            return new HttpStatusCodeResult(200);

        }

        public ActionResult GetItemTemplate()
       {
           TELEFONO oNewTelefono = new TELEFONO { OID = -1, LOCALIZACION = "", NUMERO="" };
           return PartialView("Nuevo", oNewTelefono);
       }

        public void Delete(int oid)
        {
            TelefonoRepositorio.Delete(oid.ToString());
            
        }

   

    }
}
